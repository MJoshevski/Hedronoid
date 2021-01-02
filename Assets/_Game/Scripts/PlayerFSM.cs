using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;
using Hedronoid.HNDFSM;
using Hedronoid.Core;
using Hedronoid.Events;

namespace Hedronoid.Player
{
    public class PlayerCreated : HNDBaseEvent
    {
        public PlayerFSM Player;
    }

    public class PlayerDestroyed : HNDBaseEvent
    {
        public PlayerFSM Player;
    }

    /// <summary>
    /// All possible player states.
    /// </summary>
    public enum EPlayerStates
    {
        GROUND_MOVEMENT,
        JUMPING,
        AIR_JUMPING,
        FALLING,
        LANDING,
        DASHING,
        FLYING,

        HIGHEST
    }

    public class PlayerFSM : HNDFiniteStateMachine, IGameplaySceneContextInjector
    {
        #region PUBLIC/VISIBLE VARS
        public GameplaySceneContext GameplaySceneContext { get; set; }

        [Header("Character Controller Settings")]
        public float health;
        [Tooltip("Maximum allowed acceleration/movement while on ground.")]
        [Range(0f, 100f)]
        public float maxAcceleration = 10f;
        [Tooltip("Maximum allowed acceleration/movement while NOT on ground.")]
        [Range(0f, 100f)]
        public float maxAirAcceleration = 1f;
        [Tooltip("Maximum allowed velocity while in vacuum.")]
        [Range(0f, 200f)]
        public float maxVelocityMagnitudeInVacuum = 80f;
        [Tooltip("Height of the jump.")]
        [Range(0f, 50f)]
        public float jumpHeight = 2f;
        [Tooltip("Number of jumps possible in air.")]
        [Range(0, 5)]
        public int maxAirJumps = 0;
        [Tooltip("Layer mask for ground contact probing and stairs.")]
        public LayerMask probeMask = -1;
        [Tooltip("Layer mask for detecting stairs.")]
        public LayerMask stairsMask = -1;
        [Tooltip("Speed of snapping player to ground.")]
        [Range(0f, 100f)]
        public float maxSnapSpeed = 100f;
        [Tooltip("Distance of the ground contact probe.")]
        [Min(0f)]
        public float probeDistance = 1f;
        [Tooltip("Maximum allowed angle for walking on ground.")]
        [Range(0, 90)]
        public float maxGroundAngle = 25f;
        [Tooltip("Maximum allowed angle for walking on stairs.")]
        [Range(0, 90)]
        public float maxStairsAngle = 50f;

        [Header("Custom Variables")]
        public MovementVariables movementVariables;
        public GravityVariables gravityVariables;
        public DashVariables dashVariables;

        [Header("Shooting Variables")]
        [Tooltip("Spawn position of each bullet.")]
        public Transform bulletOrigin;
        [Tooltip("Seconds between each shot.")]
        public float fireRatePrimary = 0.2f, fireRateSecondary = 2f, fireRateTertiary = 5f;
        [Tooltip("Force of each shot.")]
        public float shootForcePrimary = 8000f, shootForceSecondary = 5000f, shootForceTertiary = 100000f;
        [Tooltip("Prefab of the respective weapon's bullet.")]
        public GameObject bulletPrimary, bulletSecondary, bulletTertiary;

        //[HideInInspector]
        public bool desiredJump, desiredDash;
        #endregion

        #region PRIVATE/HIDDEN VARS
        // GENERAL REFS
        [HideInInspector]
        public Rigidbody Rigidbody, connectedRb, prevConnectedRb;
        private Camera orbitCamera;
        private Animator Animator;
        private AnimatorHashes animHashes;
        private AnimatorData animData;

        // ACTION FLAGS
        private bool isDashing, isGrounded, hasLanded;
        private float timeSinceJump;

        // SHOOTING
        private float lastFired_Auto = 0f;
        private float lastFired_Shotgun = 0f;
        private float lastFired_Rail = 0f;
        [HideInInspector]
        public Ray LookRay;
        [HideInInspector]
        public RaycastHit RayHit;
        private Rigidbody rb_auto;
        private System.Action onDespawnAction;

        // CONTROLS/BINDINGS
        private PlayerActionSet PlayerActions;
        private float MouseHorizontalSensitivity { get; set; }
        private float MouseVerticalSensitivity { get; set; }

        // PHYSICS VARS
        private Vector3 upAxis, rightAxis, forwardAxis;
        private Vector3 velocity, desiredVelocity, connectionVelocity;
        private bool OnGround => groundContactCount > 0;
        private bool OnSteep => steepContactCount > 0;
        private int groundContactCount, steepContactCount;
        private Vector3 contactNormal, steepNormal;
        private int jumpPhase;
        private int stepsSinceLastGrounded, stepsSinceLastJump;
        private Vector3 connectionWorldPosition, connectionLocalPosition;
        private Coroutine _forceApplyCoroutine = null;
        private float minGroundDotProduct, minStairsDotProduct;
        private bool inVacuum;

        // INPUT
        private Vector2 playerInput;

        // STATES
        private FSMState m_GroundMovementState;
        private FSMState m_JumpingState;
        private FSMState m_AirJumpingState;
        private FSMState m_FallingState;
        private FSMState m_LandingState;
        private FSMState m_DashingState;
        private FSMState m_FlyingState;
        #endregion

        #region CONSTANTS
        const string KEY_BINDINGS = "Bindings";
        const string KEY_MOUSE_HORIZONTAL_SENSITIVITY = "MouseHorizontalSensitivity";
        const string KEY_MOUSE_VERTICAL_SENSITIVITY = "MouseVerticalSensitivity";
        #endregion

        #region UNITY LIFECYCLE
        void OnValidate()
        {
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        }

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            orbitCamera = GameplaySceneContext.OrbitCamera.GetComponent<Camera>();
            Rigidbody = GetComponent<Rigidbody>();
            Animator = GetComponentInChildren<Animator>();
            animHashes = new AnimatorHashes();
            animData = new AnimatorData(Animator);

            PlayerActions = PlayerActionSet.CreateWithDefaultBindings();
            LoadBindings();
            LoadSensitivities();

            lastFired_Auto = lastFired_Rail = lastFired_Shotgun = 0;

            AddHNDEventListeners();
            CreateFSMStates();
        }

        protected override void Start()
        {
            base.Start();
            ChangeState(EPlayerStates.GROUND_MOVEMENT);
        }

        protected override void Update()
        {
            base.Update();

            Shoot();
            desiredDash |= Input.GetButtonDown("Dash");
            desiredJump |= Input.GetButtonDown("Jump");

            playerInput = new Vector2(movementVariables.Horizontal, movementVariables.Vertical);
            playerInput = Vector2.ClampMagnitude(playerInput, 1f);
            //Debug.LogError("VELO MAG: " + Rigidbody.velocity.magnitude);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            PlayerFSM player = GameplaySceneContext.Player;

            LookRay =
                orbitCamera.ScreenPointToRay(
                new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Physics.Raycast(LookRay, out RayHit, 10000f);

            if (orbitCamera.transform)
            {
                rightAxis =
                    VectorExtensions.ProjectDirectionOnPlane(orbitCamera.transform.right, upAxis);
                forwardAxis =
                    VectorExtensions.ProjectDirectionOnPlane(orbitCamera.transform.forward, upAxis);
            }
            else
            {
                rightAxis =
                    VectorExtensions.ProjectDirectionOnPlane(Vector3.right, upAxis);
                forwardAxis =
                    VectorExtensions.ProjectDirectionOnPlane(Vector3.forward, upAxis);
            }

            UpdateState();
            AdjustVelocity();

            GravityService.CurrentGravity = 
                GravityService.GetGravity(Rigidbody.position, out upAxis);

            if (GravityService.CurrentGravity == Vector3.zero)
                ChangeState(EPlayerStates.FLYING);

            if (desiredDash)
                ChangeState(EPlayerStates.DASHING);

            if (desiredJump)
            {
                if (CurrentState != EPlayerStates.JUMPING)
                    ChangeState(EPlayerStates.JUMPING);
                else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
                    ChangeState(EPlayerStates.AIR_JUMPING);
            }

            velocity += 
                GravityService.CurrentGravity * 
                Time.fixedDeltaTime * 
                gravityVariables.GravityForceMultiplier;

            Rigidbody.velocity = velocity;

            // ROTATE TO GRAVITY
            Vector3 targetDirection = movementVariables.MoveDirection;

            if (targetDirection == Vector3.zero)
                targetDirection = forwardAxis;

            Quaternion targetRotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(targetDirection, upAxis),
                Time.fixedDeltaTime * gravityVariables.GravityRotationMultiplier);

            transform.rotation = targetRotation;

            ClearState();          
        }

        #endregion

        #region STATE: GROUND_MOVEMENT
        private void OnEnterGroundMovement(FSMState fromState)
        {
        }

        private void OnUpdateGroundMovement()
        {
        }

        private void OnFixedUpdateGroundMovement()
        {
            Move();

            // RUNNING ANIM
            Animator.SetFloat(
                animHashes.Vertical,
                movementVariables.MoveAmount,
                0.2f,
                Time.fixedDeltaTime);
        }

        private void OnExitGroundMovement(FSMState toState)
        {
        }
        #endregion

        #region STATE: JUMPING
        private void OnEnterJumping(FSMState fromState)
        {
            Move();
            Jump(GravityService.CurrentGravity);
            desiredJump = false;

            contactNormal = upAxis;
            isGrounded = false;
            hasLanded = false;

            // JUMPS ANIMATION
            if (movementVariables.MoveAmount > 0.1f)
            {
                Animator.CrossFade(animHashes.JumpForward, 0.2f);
            }
            else
            {
                Animator.CrossFade(animHashes.JumpIdle, 0.2f);
            }               
        }

        private void OnUpdateJumping()
        {
        }

        private void OnFixedUpdateJumping()
        {
            Move();

            if (velocity.y < -0.001)
                ChangeState(EPlayerStates.FALLING);
        }

        private void OnExitJumping(FSMState fromState)
        {
        }
        #endregion

        #region STATE: AIR_JUMPING
        private void OnEnterAirJumping(FSMState fromState)
        {
            Jump(GravityService.CurrentGravity);
            desiredJump = false;

            contactNormal = upAxis;
            isGrounded = false;
            hasLanded = false;

            Animator.CrossFade(animHashes.DoubleJump, 0.2f);
        }

        private void OnUpdateAirJumping()
        {
        }

        private void OnFixedUpdateAirJumping()
        {
            Move();

            if (velocity.y < -0.01)
                ChangeState(EPlayerStates.FALLING);
        }

        private void OnExitAirJumping(FSMState fromState)
        {
        }
        #endregion

        #region STATE: FALLING
        private void OnEnterFalling(FSMState fromState)
        {
            Animator.CrossFade(animHashes.Falling, 0.2f);
        }

        private void OnUpdateFalling()
        {
        }

        private void OnFixedUpdateFalling()
        {
            Move(); 

            if (OnGround)
            {
                ChangeState(EPlayerStates.LANDING);
            }
        }

        private void OnExitFalling(FSMState fromState)
        {
        }
        #endregion

        #region STATE: LANDING
        private void OnEnterLanding(FSMState fromState)
        {
            if (movementVariables.MoveAmount > 0.3f)
            {
                Animator.CrossFade(animHashes.LandRun, 0.2f);
            }
            else
            {
                Animator.CrossFade(animHashes.LandFast, 0.2f);
            }
            hasLanded = true;
            isGrounded = true;
            Animator.SetBool(animHashes.IsGrounded, isGrounded);
            ChangeState(EPlayerStates.GROUND_MOVEMENT);
        }

        private void OnUpdateLanding()
        {
        }

        private void OnFixedUpdateLanding()
        {          
        }

        private void OnExitLanding(FSMState fromState)
        {
        }
        #endregion

        #region STATE: DASHING
        private void OnEnterDashing(FSMState fromState)
        {
            Dash();
            desiredDash = false;
            Animator.CrossFade(animHashes.Dash, 0.2f);
        }

        private void OnUpdateDashing()
        {
        }

        private void OnFixedUpdateDashing()
        {
            GravityService.CurrentGravity = Vector3.zero;
            velocity.y = 0;

            if (GravityService.CurrentGravity == Vector3.zero)
                ChangeState(EPlayerStates.FLYING);
            else if (OnGround) ChangeState(EPlayerStates.GROUND_MOVEMENT);
            else ChangeState(EPlayerStates.FALLING);
        }

        private void OnExitDashing(FSMState fromState)
        {
        }        
        #endregion

        #region STATE: FLYING
        private void OnEnterFlying(FSMState fromState)
        {
            upAxis = orbitCamera.transform.up;
            // FLYING ANIM
            Animator.CrossFade(animHashes.Flying, 0.2f);
        }

        private void OnUpdateFlying()
        {

        }

        private void OnFixedUpdateFlying()
        {            
            Move();      

            Rigidbody.velocity =
                    Vector3.ClampMagnitude(
                        Rigidbody.velocity, maxVelocityMagnitudeInVacuum);

            if (OnGround)
                ChangeState(EPlayerStates.LANDING);
            else if (GravityService.CurrentGravity != Vector3.zero)
                ChangeState(EPlayerStates.FALLING);
        }       

        private void OnExitFlying(FSMState fromState)
        {
        }

        #endregion

        #region METHODS
        private void AddHNDEventListeners()
        {
        }

        private void CreateFSMStates()
        {
            m_GroundMovementState = CreateState(EPlayerStates.GROUND_MOVEMENT, OnUpdateGroundMovement, OnEnterGroundMovement, OnExitGroundMovement);
            m_GroundMovementState.onFixedUpdateState = OnFixedUpdateGroundMovement;
            m_JumpingState = CreateState(EPlayerStates.JUMPING, OnUpdateJumping, OnEnterJumping, OnExitJumping);
            m_JumpingState.onFixedUpdateState = OnFixedUpdateJumping;
            m_AirJumpingState = CreateState(EPlayerStates.AIR_JUMPING, OnUpdateAirJumping, OnEnterAirJumping, OnExitAirJumping);
            m_AirJumpingState.onFixedUpdateState = OnFixedUpdateAirJumping;
            m_FallingState = CreateState(EPlayerStates.FALLING, OnUpdateFalling, OnEnterFalling, OnExitFalling);
            m_FallingState.onFixedUpdateState = OnFixedUpdateFalling;
            m_LandingState = CreateState(EPlayerStates.LANDING, OnUpdateLanding, OnEnterLanding, OnExitLanding);
            m_LandingState.onFixedUpdateState = OnFixedUpdateLanding;
            m_FlyingState = CreateState(EPlayerStates.FLYING, OnUpdateFlying, OnEnterFlying, OnExitFlying);
            m_FlyingState.onFixedUpdateState = OnFixedUpdateFlying;
            m_DashingState = CreateState(EPlayerStates.DASHING, OnUpdateDashing, OnEnterDashing, OnExitDashing);
            m_DashingState.onFixedUpdateState = OnFixedUpdateDashing;
        }

        private void Move()
        {
            // MOVEMENT
            movementVariables.Horizontal = PlayerActions.Move.X;
            movementVariables.Vertical = PlayerActions.Move.Y;

            movementVariables.desiredVelocity =
               new Vector3(movementVariables.Horizontal, 0f, movementVariables.Vertical) *
                movementVariables.MovementSpeed;

            desiredVelocity =
               new Vector3(playerInput.x, 0f, playerInput.y) *
               movementVariables.MaxAcceleration;

            //ANIMATION PURPOSE 
            float moveAmount =
                Mathf.Clamp01(Mathf.Abs(movementVariables.Horizontal) + Mathf.Abs(movementVariables.Vertical));
            movementVariables.MoveAmount = moveAmount;

            //DIRECTION GIZMO
            Vector3 moveDirection =
                movementVariables.Vertical * orbitCamera.transform.forward +
                movementVariables.Horizontal * orbitCamera.transform.right;
            moveDirection = Vector3.ProjectOnPlane(moveDirection, upAxis);
            moveDirection.Normalize();
            Debug.DrawRay(transform.position, moveDirection, Color.yellow);
            movementVariables.MoveDirection = moveDirection;
        }

        void AdjustVelocity()
        {
            Vector3 relativeVelocity = velocity - connectionVelocity;
            Vector3 xAxis = VectorExtensions.ProjectDirectionOnPlane(rightAxis, contactNormal);
            Vector3 zAxis = VectorExtensions.ProjectDirectionOnPlane(forwardAxis, contactNormal);

            float currentX = Vector3.Dot(relativeVelocity, xAxis);
            float currentZ = Vector3.Dot(relativeVelocity, zAxis);

            float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            float maxSpeedChange = acceleration * Time.fixedDeltaTime;

            float newX =
                Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
            float newZ =
                Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

            velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }

        void ClearState()
        {
            groundContactCount = steepContactCount = 0;
            contactNormal = steepNormal = connectionVelocity = Vector3.zero;
            prevConnectedRb = connectedRb;
            connectedRb = null;
        }      

        void UpdateState()
        {
            stepsSinceLastGrounded += 1;
            stepsSinceLastJump += 1;
            velocity = Rigidbody.velocity;

            if (OnGround || SnapToGround() || CheckSteepContacts())
            {
                stepsSinceLastGrounded = 0;

                if (stepsSinceLastJump > 1)
                {
                    jumpPhase = 0;
                }
                if (groundContactCount > 1)
                {
                    contactNormal.Normalize();
                }
            }
            else
            {
                contactNormal = upAxis;
                isGrounded = false;
                hasLanded = false;
            }

            if (connectedRb)
            {
                if (connectedRb.isKinematic || connectedRb.mass >= Rigidbody.mass)
                {
                    UpdateConnectionState();
                }
            }
        }

        void UpdateConnectionState()
        {
            if (connectedRb == prevConnectedRb)
            {
                Vector3 connectionMovement =
                    connectedRb.transform.TransformPoint(connectionLocalPosition) -
                    connectionWorldPosition;

                connectionVelocity = connectionMovement / Time.deltaTime;
            }

            connectionWorldPosition = Rigidbody.position;
            connectionLocalPosition = connectedRb.transform.InverseTransformPoint(
                connectionWorldPosition);
        }

        void Jump(Vector3 gravity)
        {
            Vector3 jumpDirection;
            if (OnGround)
            {
                jumpDirection = contactNormal;
            }
            else if (OnSteep)
            {
                jumpDirection = steepNormal;
                jumpPhase = 0;
            }
            else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
            {
                if (jumpPhase == 0)
                {
                    jumpPhase = 1;
                }
                jumpDirection = contactNormal;
            }
            else
            {
                return;
            }

            // Zero out the previous up-velocity
            velocity.y = 0;

            stepsSinceLastJump = 0;
            jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
            jumpDirection = (jumpDirection + upAxis).normalized;
            float alignedSpeed = Vector3.Dot(velocity, jumpDirection);

            if (alignedSpeed > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }          

            velocity += jumpDirection * jumpSpeed;
        }

        public void Dash()
        {
            Vector3 moveDirection = movementVariables.MoveDirection;

            if (moveDirection.sqrMagnitude < .25f)
                moveDirection = transform.forward;

            Vector3 forceDirection = transform.forward;

            forceDirection.Normalize();

            if (dashVariables.ContinuousInput
            && _forceApplyCoroutine != null)
            {
                StopCoroutine(_forceApplyCoroutine);
                _forceApplyCoroutine = null;
                AfterApplyForce();
            }

            if (dashVariables.DashesMade >= dashVariables.MaxDashes)
                return;

            StartCoroutine(
                DoApplyForceOverTime(forceDirection, dashVariables.PhysicalForce));

        }

        IEnumerator DoApplyForceOverTime(Vector3 forceDirection, PhysicalForceSettings forceSettings)
        {
            dashVariables.DashesMade++;

            // Zero out vertical velocity on dash
            isDashing = true;
            velocity.y = 0;

            _forceApplyCoroutine = StartCoroutine(
                Rigidbody.ApplyForceContinuously(forceDirection, forceSettings));
            yield return _forceApplyCoroutine;

            // Dead stop on dash-end
            isDashing = false;
            velocity = Vector3.zero;

            AfterApplyForce();
        }

        void AfterApplyForce()
        {
            dashVariables.DashesMade--;
            _forceApplyCoroutine = null;
        }

        public void Shoot()
        {
            Gizmos.Line(bulletOrigin.position, RayHit.point, Color.yellow);

            Vector3 shootDirection;

            if (RayHit.point != Vector3.zero)
                shootDirection = RayHit.point - bulletOrigin.position;
            else shootDirection = LookRay.direction;

            if (Input.GetButton("Fire1") &&
                Time.realtimeSinceStartup - lastFired_Auto > fireRatePrimary)
            {
                GameObject auto = TrashMan.spawn(
                    bulletPrimary, bulletOrigin.position, Quaternion.identity);
                TrashMan.despawnAfterDelay(auto, 5f, () => onDespawnReset(auto));

                rb_auto = auto.GetComponent<Rigidbody>();
                rb_auto.AddForce(shootDirection.normalized * shootForcePrimary);
                lastFired_Auto = Time.realtimeSinceStartup;
            }
            else if (Input.GetButtonDown("Fire2") &&
                Time.realtimeSinceStartup - lastFired_Shotgun > fireRateSecondary)
            {
                GameObject shot = TrashMan.spawn(
                    bulletSecondary, bulletOrigin.position, Quaternion.identity);
                TrashMan.despawnAfterDelay(shot, 5f, () => onDespawnReset(shot));

                Rigidbody rb_shot = shot.GetComponent<Rigidbody>();
                rb_shot.AddForce(shootDirection.normalized * shootForceSecondary);
                lastFired_Shotgun = Time.realtimeSinceStartup;
            }
        }

        private System.Action onDespawnReset(GameObject bullet)
        {
            bullet.SetActive(false);

            // Zero out previous velocity
            Rigidbody bulletRb = bullet.gameObject.GetComponent<Rigidbody>();
            if (bulletRb)
                bulletRb.velocity = Vector3.zero;

            // Return bullet pos/rot to origin
            bullet.transform.SetPositionAndRotation(bulletOrigin.position, Quaternion.identity);

            bullet.SetActive(true);

            return null;
        }

        bool SnapToGround()
        {
            if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
            {
                return false;
            }
            float speed = velocity.magnitude;
            if (speed > maxSnapSpeed)
            {
                return false;
            }
            if (!Physics.Raycast(
                Rigidbody.position, -upAxis, out RaycastHit hit,
                probeDistance, probeMask
            ))
            {
                return false;
            }

            float upDot = Vector3.Dot(upAxis, hit.normal);
            if (upDot < GetMinDot(hit.collider.gameObject.layer))
            {
                return false;
            }

            groundContactCount = 1;
            contactNormal = hit.normal;
            float dot = Vector3.Dot(velocity, hit.normal);
            if (dot > 0f)
            {
                velocity = (velocity - hit.normal * dot).normalized * speed;
            }

            connectedRb = hit.rigidbody;
            return true;
        }

        bool CheckSteepContacts()
        {
            if (steepContactCount > 1)
            {
                steepNormal.Normalize();
                float upDot = Vector3.Dot(upAxis, steepNormal);
                if (upDot >= minGroundDotProduct)
                {
                    steepContactCount = 0;
                    groundContactCount = 1;
                    contactNormal = steepNormal;
                    return true;
                }
            }
            return false;
        }

        public void SaveBindings()
        {
            var saveData = PlayerActions.Save();
            PlayerPrefs.SetString(KEY_BINDINGS, saveData);
            PlayerPrefs.SetFloat(KEY_MOUSE_HORIZONTAL_SENSITIVITY, MouseHorizontalSensitivity);
            PlayerPrefs.SetFloat(KEY_MOUSE_VERTICAL_SENSITIVITY, MouseVerticalSensitivity);
            PlayerPrefs.Save();
            Debug.Log("Bindings saved...");
        }

        public void ResetBindings()
        {
            PlayerActions = PlayerActionSet.CreateWithDefaultBindings();
            Debug.Log("Bindings reset...");

        }

        void LoadBindings()
        {
            if (PlayerPrefs.HasKey(KEY_BINDINGS))
            {
                var saveData = PlayerPrefs.GetString(KEY_BINDINGS);
                PlayerActions.Load(saveData);
                Debug.Log("Bindings loaded...");
            }
        }

        void LoadSensitivities()
        {
            MouseHorizontalSensitivity = PlayerPrefs.GetFloat(KEY_MOUSE_HORIZONTAL_SENSITIVITY, 50f);
            MouseVerticalSensitivity = PlayerPrefs.GetFloat(KEY_MOUSE_VERTICAL_SENSITIVITY, 50f);
        }

        public IEnumerator WaitForSeconds(float duration)
        {
            yield return new WaitForSeconds(duration);
        }
        void OnCollisionEnter(Collision collision)
        {
            EvaluateCollision(collision);
        }

        void OnCollisionStay(Collision collision)
        {
            EvaluateCollision(collision);
        }

        void EvaluateCollision(Collision collision)
        {
            float minDot = GetMinDot(collision.gameObject.layer);
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                float upDot = Vector3.Dot(upAxis, normal);
                if (upDot >= minDot)
                {
                    groundContactCount += 1;
                    contactNormal += normal;
                    connectedRb = collision.rigidbody;
                }
                else if (upDot > -0.01f)
                {
                    steepContactCount += 1;
                    steepNormal += normal;
                    if (groundContactCount == 0)
                    {
                        connectedRb = collision.rigidbody;
                    }
                }
            }
        }

        public float GetMinDot(int layer)
        {
            return (stairsMask & (1 << layer)) == 0 ?
                minGroundDotProduct : minStairsDotProduct;
        }
        #endregion
    }
}

