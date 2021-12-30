using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Gizmos = Popcron.Gizmos;
using Hedronoid.HNDFSM;
using Hedronoid.Core;
using Hedronoid.Events;
using Hedronoid.Weapons;
using UnityEngine.SceneManagement;
using Hedronoid.Health;
using Hedronoid.Particle;
using Hedronoid.AI;
using CameraShake;

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
        [Tooltip("How much faster do we want to fall?")]
        [Range(1, 100)]
        public float fallMultiplier = 25f;
        [Tooltip("What is the limit of how high the fall multiplier goes?")]
        [Range(1, 100)]
        public float fallMultiplierThreshold = 5f;
        [Tooltip("How much gravity do we apply on low jumps?")]
        [Range(1, 100)]
        public float lowJumpMultiplier = 10f;
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
        [Tooltip("Distance of the ground contact probe.")]
        [Min(0f)]
        public float dashProbeDistance = 5f;
        [Tooltip("Maximum allowed angle for walking on ground.")]
        [Range(0, 90)]
        public float maxGroundAngle = 25f;
        [Tooltip("Maximum allowed angle for walking on stairs.")]
        [Range(0, 90)]
        public float maxStairsAngle = 50f;
        public bool IsMoving { get { return isMoving; } }
        private bool isMoving = false;


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
        [Tooltip("Bullet raycast ignore layers")]
        public LayerMask bulletIgnoreLayers;
        public bool IsShooting { get { return isShooting; } }
        private bool isShooting = false;

        [Header("Visual")]
        [SerializeField]
        private GameObject playerModel;
        public ParticleList.ParticleSystems DeathParticle = ParticleList.ParticleSystems.NONE;
        public ParticleList.ParticleSystems DashStartParticle = ParticleList.ParticleSystems.NONE;
        public ParticleList.ParticleSystems DashTrailParticle = ParticleList.ParticleSystems.NONE;
        public List<ParticleList.ParticleSystems> MuzzleFlashParticles = new List<ParticleList.ParticleSystems>();

        public List<ParticleList.ParticleSystems> JumpParticles = new List<ParticleList.ParticleSystems>();
        public List<ParticleList.ParticleSystems> DoubleJumpParticles = new List<ParticleList.ParticleSystems>();
        public List<ParticleList.ParticleSystems> LandParticles = new List<ParticleList.ParticleSystems>();

        [Header("FMOD Audio Data")]
        public PlayerAudioData m_playerAudioData;

        [HideInInspector]
        public bool desiredJump, desiredDash;
        #endregion

        #region PRIVATE/HIDDEN VARS
        // GENERAL REFS
        [HideInInspector]
        public Rigidbody Rigidbody, connectedRb, prevConnectedRb;
        private Camera orbitCameraObject;
        private OrbitCamera orbitCamera;
        private Animator Animator;
        private DamageHandler m_damageHandler;
        private HealthBase m_healthBase;
        private AnimatorHashes animHashes;
        private AnimatorData animData;

        // ACTION FLAGS
        private float timeSinceJump;

        // SHOOTING
        private float lastFired_Auto = 0f;
        private float lastFired_Shotgun = 0f;
        private float lastFired_Rail = 0f;
        [HideInInspector]
        public Ray LookRay;
        [HideInInspector]
        public RaycastHit RayHit;
        private System.Action onDespawnAction;
        private BulletPoolManager.BulletConfig bulletConf;

        // EFFECTS
        private AfterImageEffect m_afterImageEffect;

        // CONTROLS/BINDINGS
        private PlayerActionSet PlayerActions;
        private float MouseHorizontalSensitivity { get; set; }
        private float MouseVerticalSensitivity { get; set; }

        // PHYSICS VARS
        private Vector3 upAxis, rightAxis, forwardAxis;
        private Vector3 velocity, gravityAlignedVelocity, desiredVelocity, connectionVelocity;
        [SerializeField]
        private bool OnGround => groundContactCount > 0;
        private bool OnSteep => steepContactCount > 0;
        private int groundContactCount, steepContactCount;
        private Vector3 contactNormal, steepNormal;
        [SerializeField]
        private int jumpPhase;
        private int stepsSinceLastGrounded, stepsSinceLastJump;
        private Vector3 connectionWorldPosition, connectionLocalPosition;
        private float minGroundDotProduct, minStairsDotProduct;
        private bool inVacuum;
        [SerializeField]
        private float secondaryGravityMultiplier = 1f;

        //DASH VARS
        private Vector3 posBeforeDash;
        private float timeOnDashEnter;

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

            if (!orbitCameraObject) GameplaySceneContext.OrbitCamera.TryGetComponent(out orbitCameraObject);
            if (!orbitCamera) orbitCamera = GameplaySceneContext.OrbitCamera;
            if (!Rigidbody) TryGetComponent(out Rigidbody);
            if (!m_healthBase) TryGetComponent(out m_healthBase);
            if (!m_damageHandler) TryGetComponent(out m_damageHandler);
            if (!m_afterImageEffect) TryGetComponent(out m_afterImageEffect);

            Animator = GetComponentInChildren<Animator>();
            animHashes = new AnimatorHashes();
            animData = new AnimatorData(Animator);
            bulletConf = new BulletPoolManager.BulletConfig();
            secondaryGravityMultiplier = 1f;

            PlayerActions = InputManager.Instance.PlayerActions;

            lastFired_Auto = lastFired_Rail = lastFired_Shotgun = 0;

            AddHNDEventListeners();
            CreateFSMStates();
            CreateFMODEvents();

            HNDEvents.Instance.AddListener<KillEvent>(OnKilled);
        }

        protected override void Start()
        {
            base.Start();
            ChangeState(EPlayerStates.GROUND_MOVEMENT);
        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetButton("Fire1"))
            {
                Shoot(true);
            }

            if (Input.GetButton("Fire2"))
            {
                Shoot(false);
            }

            if (Input.GetButton("Fire1") || Input.GetButton("Fire2"))
                isShooting = true;
            else isShooting = false;


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
                orbitCameraObject.ScreenPointToRay(
                new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Physics.Raycast(LookRay, out RayHit, 10000f);

            if (orbitCameraObject.transform)
            {
                rightAxis =
                    VectorExtensions.ProjectDirectionOnPlane(orbitCameraObject.transform.right, upAxis);
                forwardAxis =
                    VectorExtensions.ProjectDirectionOnPlane(orbitCameraObject.transform.forward, upAxis);
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

            //if (GravityService.CurrentGravity == Vector3.zero)
            //    ChangeState(EPlayerStates.FLYING);

            if (desiredDash &&
                (Time.realtimeSinceStartup - timeOnDashEnter) >= dashVariables.DashCooldown &&
                 (dashVariables.DashesMade < dashVariables.MaxDashes))
            {
                ChangeState(EPlayerStates.DASHING);
            }
            else desiredDash = false;

            if (desiredJump && jumpPhase <= maxAirJumps)
            {
                if (jumpPhase == 0)
                    ChangeState(EPlayerStates.JUMPING);
                else if (maxAirJumps > 0)
                    ChangeState(EPlayerStates.AIR_JUMPING);
            }
            else desiredJump = false;

            velocity += 
                GravityService.CurrentGravity * 
                Time.fixedDeltaTime * 
                gravityVariables.GravityForceMultiplier *
                secondaryGravityMultiplier;

            Rigidbody.velocity = velocity;

            gravityAlignedVelocity = transform.InverseTransformDirection(velocity);

            // ROTATE TO GRAVITY
            Vector3 targetDirection = movementVariables.MoveDirection;

            if (targetDirection == Vector3.zero)
                targetDirection = forwardAxis;

            Quaternion targetRotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(targetDirection, upAxis),
                Time.fixedDeltaTime * gravityVariables.GravityRotationMultiplier);

            transform.rotation = targetRotation;

            if (Rigidbody.velocity.sqrMagnitude > 1)
                isMoving = true;
            else isMoving = false;

            ClearState();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            HNDEvents.Instance.RemoveListener<KillEvent>(OnKilled);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            HNDEvents.Instance.RemoveListener<KillEvent>(OnKilled);
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

            if (!OnGround && !OnSteep)
            {
                ChangeState(EPlayerStates.FALLING);
            }

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
            Jump(GravityService.CurrentGravity);
            desiredJump = false;

            if (JumpParticles != null && JumpParticles.Count > 0)
            {
                int index = Random.Range(0, JumpParticles.Count);
                ParticleHelper.PlayParticleSystem(JumpParticles[index], cachedTransform.position, -cachedTransform.up);
            }

            FMODUnity.RuntimeManager.PlayOneShot(m_playerAudioData.jump, transform.position);
            secondaryGravityMultiplier = 1f;
            //contactNormal = upAxis;

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

            if (gravityAlignedVelocity.y < -0.001)
                ChangeState(EPlayerStates.FALLING);
            else if (gravityAlignedVelocity.y > 0 && !Input.GetButton("Jump"))
            {
                secondaryGravityMultiplier += (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
            }
        }

        private void OnExitJumping(FSMState fromState)
        {
            secondaryGravityMultiplier = 1f;
        }
        #endregion

        #region STATE: AIR_JUMPING
        private void OnEnterAirJumping(FSMState fromState)
        {
            Jump(GravityService.CurrentGravity);
            desiredJump = false;
            secondaryGravityMultiplier = 1f;

            if (JumpParticles != null && DoubleJumpParticles.Count > 0)
            {
                int index = Random.Range(0, DoubleJumpParticles.Count);
                ParticleHelper.PlayParticleSystem(DoubleJumpParticles[index], cachedTransform.position, -cachedTransform.up);
            }

            FMODUnity.RuntimeManager.PlayOneShot(m_playerAudioData.doubleJump, transform.position);
            //contactNormal = upAxis;

            Animator.CrossFade(animHashes.DoubleJump, 0.2f);
        }

        private void OnUpdateAirJumping()
        {
        }

        private void OnFixedUpdateAirJumping()
        {
            Move();

            if (gravityAlignedVelocity.y < -0.001)
                ChangeState(EPlayerStates.FALLING);
            else if (gravityAlignedVelocity.y > 0 && !Input.GetButton("Jump"))
            {
                secondaryGravityMultiplier += (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
            }
        }

        private void OnExitAirJumping(FSMState fromState)
        {
            secondaryGravityMultiplier = 1f;
        }
        #endregion

        #region STATE: DASHING
        private void OnEnterDashing(FSMState fromState)
        {
            timeOnDashEnter = Time.realtimeSinceStartup;

            Vector3 moveDirection = movementVariables.MoveDirection;

            if (moveDirection.sqrMagnitude < .25f)
                moveDirection = transform.forward;

            Vector3 forceDirection = moveDirection;

            forceDirection.Normalize();

            // Zero out vertical velocity on dash
            Rigidbody.velocity = Vector3.zero;

            dashVariables.DashesMade++;
            posBeforeDash = transform.position;
            desiredDash = false;
            Animator.CrossFade(animHashes.Dash, 0.2f);

            StopAllCoroutines();
            StartCoroutine(LerpCameraFov(100, 120));

            // SFX
            FMODUnity.RuntimeManager.PlayOneShotAttached(m_playerAudioData.dash, gameObject);

            // VFX
            orbitCamera.camerShakeCollection.PlayCameraShake(CameraShakeType.BounceShake2);
            m_afterImageEffect.Play();
            ParticleHelper.PlayParticleSystem(DashStartParticle, cachedTransform.position, -cachedTransform.forward, 3f);
            ParticleHelper.PlayParticleSystem(DashTrailParticle, cachedTransform.position, -cachedTransform.up, 2f, false, cachedTransform);

            secondaryGravityMultiplier = 1f; ;
            Rigidbody.ApplyForce(forceDirection * dashVariables.PhysicalForce.Multiplier, dashVariables.PhysicalForce.ForceMode);
        }

        private void OnUpdateDashing()
        {
        }

        private void OnFixedUpdateDashing()
        {
            if (OnGround || OnSteep)
            {
                SnapPlayerToGround();
            }
            else
            {
                gravityAlignedVelocity.y = 0f;
            }

            if (Vector3.Distance(posBeforeDash, transform.position) > dashVariables.MaxDistance ||
                Time.realtimeSinceStartup - timeOnDashEnter >= dashVariables.MaxTime)
            {
                Rigidbody.velocity = Vector3.zero;

                if (OnGround || OnSteep)
                    ChangeState(EPlayerStates.GROUND_MOVEMENT);
                else ChangeState(EPlayerStates.FALLING);
            }
        }

        private void OnExitDashing(FSMState fromstate)
        {
            secondaryGravityMultiplier = 1f;

            // Dead stop on dash-end
            Rigidbody.velocity = Vector3.zero;

            StopAllCoroutines();
            StartCoroutine(LerpCameraFov(120, 100));

            Animator.CrossFade(animHashes.LandRun, 0.2f);
        }

        IEnumerator LerpCameraFov(float from, float to)
        {
            float t = 0;

            while (from != to)
            {
                GameplaySceneContext.OrbitCamera.orbitCamera.fieldOfView =
                    Mathf.Lerp(from, to, t);

                t += Time.fixedDeltaTime * 4f;
                yield return null;
            }

            yield return null;
        }
        #endregion

        #region STATE: FALLING
        private void OnEnterFalling(FSMState fromState)
        {
            Animator.CrossFade(animHashes.Falling, 0.2f);
            secondaryGravityMultiplier = 1f;
        }

        private void OnUpdateFalling()
        {
        }

        private void OnFixedUpdateFalling()
        {
            Move();

            secondaryGravityMultiplier += (fallMultiplier - 1f) * Time.fixedDeltaTime;
            secondaryGravityMultiplier = 
                Mathf.Clamp(secondaryGravityMultiplier, 1f, fallMultiplierThreshold);

            if (OnGround || OnSteep)
            {
                ChangeState(EPlayerStates.LANDING);
            }
            //else if (GravityService.CurrentGravity == Vector3.zero)
            //{
            //    ChangeState(EPlayerStates.FLYING);
            //}

        }

        private void OnExitFalling(FSMState fromState)
        {
            secondaryGravityMultiplier = 1f;
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

            if (LandParticles != null && LandParticles.Count > 0)
            {
                int index = Random.Range(0, LandParticles.Count);
                ParticleHelper.PlayParticleSystem(LandParticles[index], cachedTransform.position, -cachedTransform.up);
            }

            FMODUnity.RuntimeManager.PlayOneShot(m_playerAudioData.land, transform.position);

            Animator.SetBool(animHashes.IsGrounded, OnGround);
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

        #region STATE: FLYING
        private void OnEnterFlying(FSMState fromState)
        {
            upAxis = orbitCameraObject.transform.up;
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

            if (OnGround || OnSteep)
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

        FMOD.Studio.EventInstance Run;
        FMOD.Studio.EventInstance BulletPrimary;
        private void CreateFMODEvents()
        {
            Run = FMODUnity.RuntimeManager.CreateInstance(m_playerAudioData.footsteps);
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(Run, transform, GetComponent<Rigidbody>());
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

        [SerializeField]
        float timeBetweenFtstp = 0.3f;
        float timeFromLastFtstp;

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

            if (desiredVelocity.magnitude > 0.1f && OnGround)
            {              
                if (timeFromLastFtstp > timeBetweenFtstp)
                {
                    FMODUnity.RuntimeManager.PlayOneShot(m_playerAudioData.footsteps, transform.position);
                    timeFromLastFtstp = 0f;
                }
                timeFromLastFtstp += Time.fixedDeltaTime;
            }
            else
            {
                timeFromLastFtstp = 0f;
            }
           
            //ANIMATION PURPOSE 
            float moveAmount =
                Mathf.Clamp01(Mathf.Abs(movementVariables.Horizontal) + Mathf.Abs(movementVariables.Vertical));
            movementVariables.MoveAmount = moveAmount;

            //DIRECTION GIZMO
            Vector3 moveDirection =
                movementVariables.Vertical * orbitCameraObject.transform.forward +
                movementVariables.Horizontal * orbitCameraObject.transform.right;
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

                if (stepsSinceLastJump > 2)
                {
                    jumpPhase = 0;

                    if(dashVariables.DashesMade != 0)
                        dashVariables.DashesMade--;
                }
                if (groundContactCount > 1)
                {
                    contactNormal.Normalize();
                }
            }
            else
            {
                contactNormal = upAxis;
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
                // Uncomment this, if you want the double jump to not be performed when falling,
                // i.e. if you are fell from a platform, you can only single jump.
                //
                //if (jumpPhase == 0)
                //{
                //    jumpPhase = 1;
                //}

                jumpDirection = contactNormal;
            }
            else
            {
                return;
            }

            // Zero out the previous up-velocity 
            gravityAlignedVelocity.y = 0;

            stepsSinceLastJump = 0;
            jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
            jumpDirection = (jumpDirection + upAxis + forwardAxis).normalized;
            float alignedSpeed = Vector3.Dot(velocity, jumpDirection);

            if (alignedSpeed > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }          

            // Transform the aligned velocity to world space
            velocity = transform.InverseTransformDirection(gravityAlignedVelocity);

            // Apply jump force after zeroing out local y-velocity
            velocity += jumpDirection * jumpSpeed;
        }   
        public void Shoot(bool primaryFire)
        {
            Vector3 shootDirection;
            Vector3 rayHitPos = Vector3.zero;
            Ray ray = Camera.main.ViewportPointToRay(new Vector3 (0.5f, 0.5f, 0f));

            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, ~bulletIgnoreLayers);
            List<float> distances = new List<float>();

            for (int i = 0; i < hits.Length; i++)
            {
                float distance = Vector3.Distance(hits[i].transform.position, transform.position);
                distances.Add(distance);
            }

            float minDistance = 1000000f;
            int minDistanceIdx = 0;
            for (int i = 0; i < distances.Count; i++)
            {
                if (minDistance > distances[i])
                {
                    minDistance = distances[i];
                    minDistanceIdx = i;
                }
            }

            rayHitPos = hits[minDistanceIdx].point;

            if (rayHitPos == Vector3.zero)
            {
                rayHitPos = ray.GetPoint(10000f);
                //Debug.LogError("HITTIN ZERO");
                //Debug.LogError("HITS: " + hits.Length);
            }

            shootDirection = (rayHitPos - bulletOrigin.position).normalized;


            Popcron.Gizmos.Line(bulletOrigin.position, rayHitPos, Color.black);

            if (primaryFire && Time.realtimeSinceStartup - lastFired_Auto > fireRatePrimary)
            {
                bulletConf.Prefab = bulletPrimary;
                bulletConf.Position = bulletOrigin.position;
                bulletConf.Rotation = Quaternion.identity;
                bulletConf.Parent = null;
                bulletConf.Duration = 5f;

                GameObject auto = GameplaySceneContext.BulletPoolManager.GetBulletToFire(bulletConf);
                Rigidbody rb_auto = auto.GetComponent<Rigidbody>();
                rb_auto.velocity = Vector3.zero;
                rb_auto.AddForce(shootDirection * shootForcePrimary);
                lastFired_Auto = Time.realtimeSinceStartup;
                rb_auto.transform.forward = shootDirection;

                //FX
                orbitCamera.camerShakeCollection.PlayCameraShake(CameraShakeType.KickShake);
                for (int i = 0; i < MuzzleFlashParticles.Count; i++)
                    ParticleHelper.PlayParticleSystem(MuzzleFlashParticles[i], bulletOrigin.transform.position, shootDirection);
                FMODUnity.RuntimeManager.PlayOneShot(m_playerAudioData.bulletPrimary[0], transform.position);
            }
            else if (!primaryFire && Time.realtimeSinceStartup - lastFired_Shotgun > fireRateSecondary)
            {
                bulletConf.Prefab = bulletSecondary;
                bulletConf.Position = bulletOrigin.position;
                bulletConf.Rotation = Quaternion.identity;
                bulletConf.Parent = null;
                bulletConf.Duration = 5f;

                GameObject shot = GameplaySceneContext.BulletPoolManager.GetBulletToFire(bulletConf);

                Rigidbody rb_shot = shot.GetComponent<Rigidbody>();
                rb_shot.velocity = Vector3.zero;
                rb_shot.transform.LookAt(shootDirection);
                rb_shot.AddForce(shootDirection * shootForceSecondary);
                lastFired_Shotgun = Time.realtimeSinceStartup;

                orbitCamera.camerShakeCollection.PlayCameraShake(CameraShakeType.BounceShake);
                FMODUnity.RuntimeManager.PlayOneShot(m_playerAudioData.bulletSecondary, transform.position);
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

        void SnapPlayerToGround()
        {
            if (!Physics.Raycast(
                Rigidbody.position, -upAxis, out RaycastHit hit,
                dashProbeDistance, probeMask
            ))
            {
                return;
            }

            float upDot = Vector3.Dot(upAxis, hit.normal);
            if (upDot < GetMinDot(hit.collider.gameObject.layer))
            {
                return;
            }
            Rigidbody.position = hit.point;
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

        public IEnumerator WaitForSeconds(float duration)
        {
            yield return new WaitForSeconds(duration);
        }
        private void OnKilled(KillEvent e)
        {
            if (e.GOID != gameObject.GetInstanceID()) return;

            ParticleHelper.PlayParticleSystem(DeathParticle, cachedTransform.position, cachedTransform.forward, 2f);
            playerModel.SetActive(false);
            GameController.Instance.LoadScene(SceneManager.GetActiveScene().buildIndex);
            StopAllCoroutines();
            //enabled = false;
        }
        void OnCollisionEnter(Collision collision)
        {
            // HACKITY HACKITY HACK REMOVE ME ASAP
            if (!m_damageHandler.IsInvulnerable && 
                HNDAI.Settings.EnemyLayer == 
                (HNDAI.Settings.EnemyLayer | (1 << collision.gameObject.layer)))
            {
                FMODUnity.RuntimeManager.PlayOneShot(m_playerAudioData.recieveHit, transform.position);
                if (m_healthBase.CurrentHealth > 1 && collision.gameObject.tag != "Lava")
                {
                    HealthInfo healthInfo = new HealthInfo { amount = -1 };

                    m_healthBase.HealthInfoReciever(healthInfo);

                }
                else m_healthBase.InstaKill();
            }

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

