using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

namespace Hedronoid
{
    public class PlayerStateManager : MonoSingleton<PlayerStateManager>
    {
        public float health;
        
        public State currentState;

        public MovementVariables movementVariables;
        public GravityVariables gravityVariables;
        public JumpVariables jumpVariables;
        public DashVariables dashVariables;

        [HideInInspector]
        public float delta;
        [HideInInspector]
        public Transform Transform;
        [HideInInspector]
        public Rigidbody Rigidbody, connectedRb, prevConnectedRb;

        [HideInInspector]
        public Animator Animator;
        public AnimatorHashes animHashes;
        public AnimatorData animData;

        [HideInInspector] public bool jumpPressed;
        [HideInInspector] public bool jumpReleased;
        [HideInInspector] public bool dashPressed;
        [HideInInspector] public bool dashReleased;
        public bool isDashing;

        //[HideInInspector]
        public bool isGrounded;
        [HideInInspector]
        public float timeSinceJump;

        [HideInInspector]
        public Ray LookRay;
        [HideInInspector]
        public RaycastHit RayHit;

        public PlayerActionSet PlayerActions;
        public float MouseHorizontalSensitivity { get; set; }
        public float MouseVerticalSensitivity { get; set; }
        private Camera orbitCamera;

        [HideInInspector]
        public IGravityService gravityService;

        [HideInInspector]
        public Vector3 upAxis, rightAxis, forwardAxis;
        [HideInInspector]
        public Vector3 velocity, desiredVelocity, connectionVelocity;
        [Range(0f, 100f)]
        public float maxAcceleration = 10f, maxAirAcceleration = 1f;
        [Range(0f, 200f)]
        public float maxVelocityMagnitudeInVacuum = 80f;
        [HideInInspector]
        public bool OnGround => groundContactCount > 0;
        [HideInInspector]
        public bool OnSteep => steepContactCount > 0;
        [HideInInspector]
        public int groundContactCount, steepContactCount;
        [HideInInspector]
        public Vector3 contactNormal, steepNormal;
        [HideInInspector]
        public int jumpPhase;
        [Range(0, 5)]
        public int maxAirJumps = 0;
        [HideInInspector]
        public int stepsSinceLastGrounded, stepsSinceLastJump;
        [Range(0f, 100f)]
        public float maxSnapSpeed = 100f;
        [Min(0f)]
        public float probeDistance = 1f;
        [Range(0, 90)]
        public float maxGroundAngle = 25f, maxStairsAngle = 50f;

        [Range(0f, 50f)]
        public float jumpHeight = 2f;

        // SHOOTING
        [HideInInspector]
        public float lastFired_Auto = 0f;
        [HideInInspector]
        public float lastFired_Shotgun = 0f;
        [HideInInspector]
        public float lastFired_Rail = 0f;

        void OnValidate()
        {
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        }

        private void Start()
        {
            Transform = this.transform;
            orbitCamera = OrbitCamera.Instance.orbitCamera;
            Rigidbody = GetComponent<Rigidbody>();
            Animator = GetComponentInChildren<Animator>();
            animHashes = new AnimatorHashes();
            animData = new AnimatorData(Animator);

            PlayerActions = PlayerActionSet.CreateWithDefaultBindings();
            LoadBindings();
            LoadSensitivities();

            gravityService = GravityService.Instance;

            currentState.FixedTickStart(this);
            currentState.TickStart(this);

            lastFired_Auto = lastFired_Rail = lastFired_Shotgun = 0;

        }

        [HideInInspector]
        public bool desiredJump, desiredDash;
        private bool inVacuum;

        private void Update()
        {
            delta = Time.deltaTime;

            Vector2 playerInput = new Vector2(movementVariables.Horizontal, movementVariables.Vertical);
            playerInput = Vector2.ClampMagnitude(playerInput, 1f);

            // MOVEMENT
            movementVariables.Horizontal = PlayerActions.Move.X;
            movementVariables.Vertical = PlayerActions.Move.Y;

            movementVariables.desiredVelocity =
               new Vector3(movementVariables.Horizontal, 0f, movementVariables.Vertical) *
                movementVariables.MovementSpeed;
            //

            if (currentState != null)
            {
                currentState.Tick(this);
            }

            if (orbitCamera.transform)
            {
                rightAxis = VectorExtensions.ProjectDirectionOnPlane(orbitCamera.transform.right, upAxis);
                forwardAxis =
                    VectorExtensions.ProjectDirectionOnPlane(orbitCamera.transform.forward, upAxis);
            }
            else
            {
                rightAxis = VectorExtensions.ProjectDirectionOnPlane(Vector3.right, upAxis);
                forwardAxis = VectorExtensions.ProjectDirectionOnPlane(Vector3.forward, upAxis);
            }

            desiredVelocity =
                new Vector3(playerInput.x, 0f, playerInput.y) *
                movementVariables.MaxAcceleration;

            desiredDash |= Input.GetButtonDown("Dash");
            desiredJump |= Input.GetButtonDown("Jump");

            //RUNNING ANIM
            Animator.SetFloat(
                animHashes.Vertical, 
                movementVariables.MoveAmount,
                0.2f,
                Time.deltaTime);
            //

            //Debug.LogError("GRAVITY: " + gravityService.CurrentGravity.ToString());

            if (gravityService.CurrentGravity == Vector3.zero)
            {
                Rigidbody.velocity =
                    Vector3.ClampMagnitude(
                        Rigidbody.velocity, maxVelocityMagnitudeInVacuum);

                if (!inVacuum)
                {
                    inVacuum = true;
                    Animator.CrossFade(animHashes.Flying, 0.2f);
                }
            }
            else if (inVacuum == true)
            {
                inVacuum = false;
                Animator.CrossFade(animHashes.Falling, 0.2f);
            }


            //Debug.LogError("VELO MAG: " + Rigidbody.velocity.magnitude);

        }

        public Vector3 charRay;
        private void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;

            LookRay =
                orbitCamera.ScreenPointToRay(
                new Vector3(Screen.width / 2, Screen.height / 2, 0));

            Physics.Raycast(LookRay, out RayHit, 10000f);

            //DEBUG
            if (Input.GetKeyDown(KeyCode.F3))
                Gizmos.Enabled = !Gizmos.Enabled;
            //

            if (currentState != null)
            {
                currentState.FixedTick(this);
            }

            //ANIMATION PURPOSE 
            float moveAmount =
                Mathf.Clamp01(Mathf.Abs(movementVariables.Horizontal) + Mathf.Abs(movementVariables.Vertical));
            movementVariables.MoveAmount = moveAmount;

            Vector3 gravity = GravityService.GetGravity(Rigidbody.position, out upAxis);

            // In Vacuum Behaviour (get up-axis from camera instead of gravity)
            if (gravity == Vector3.zero)
            {
                upAxis = orbitCamera.transform.up;
            }

            if (desiredDash)
            {
                desiredDash = false;
                Dash();
            }

            if (isDashing) gravity = Vector3.zero;

            GravityService.Instance.CurrentGravity = gravity;

            UpdateState();
            AdjustVelocity();

            if (desiredJump)
            {
                desiredJump = false;
                Jump(gravity);
            }

            velocity += gravity * delta * gravityVariables.GravityForceMultiplier;

            Rigidbody.velocity = velocity;

            // ROTATE TO GRAVITY
            Vector3 targetDirection = movementVariables.MoveDirection;

            if (targetDirection == Vector3.zero)
                targetDirection = forwardAxis;

            Quaternion tr =
                Quaternion.LookRotation(targetDirection, upAxis);

            Quaternion targetRotation = Quaternion.Slerp(
                Transform.rotation,
                tr,
                delta * gravityVariables.GravityRotationMultiplier);

            Transform.rotation = targetRotation;
            //

            ClearState();

            //DIRECTION GIZMO
            Vector3 moveDirection =
                movementVariables.Vertical * orbitCamera.transform.forward +
                movementVariables.Horizontal * orbitCamera.transform.right;
            moveDirection = Vector3.ProjectOnPlane(moveDirection, upAxis);
            moveDirection.Normalize();
            Debug.DrawRay(Transform.position, moveDirection, Color.yellow);
            movementVariables.MoveDirection = moveDirection;
            //
        }

        void AdjustVelocity()
        {
            Vector3 relativeVelocity = velocity - connectionVelocity;
            Vector3 xAxis = VectorExtensions.ProjectDirectionOnPlane(rightAxis, contactNormal);
            Vector3 zAxis = VectorExtensions.ProjectDirectionOnPlane(forwardAxis, contactNormal);

            float currentX = Vector3.Dot(relativeVelocity, xAxis);
            float currentZ = Vector3.Dot(relativeVelocity, zAxis);

            float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            float maxSpeedChange = acceleration * Time.deltaTime;

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

        private bool hasLanded;
        private Vector3 connectionWorldPosition, connectionLocalPosition;

        void UpdateState()
        {
            stepsSinceLastGrounded += 1;
            stepsSinceLastJump += 1;
            velocity = Rigidbody.velocity;

            if (OnGround || SnapToGround() || CheckSteepContacts())
            {
                stepsSinceLastGrounded = 0;

                if (!hasLanded && !isGrounded)
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
                }

                isGrounded = true;
                Animator.SetBool(animHashes.IsGrounded, isGrounded);

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

            // JUMPS ANIMATION
            if (jumpPhase == 1)
            {
                if (movementVariables.MoveAmount > 0.1f)
                {
                    Animator.CrossFade(animHashes.JumpForward, 0.2f);
                }
                else
                {
                    Animator.CrossFade(animHashes.JumpIdle, 0.2f);
                }
            }
            else if (jumpPhase > 1)
                Animator.CrossFade(animHashes.DoubleJump, 0.2f);

            velocity += jumpDirection * jumpSpeed;
        }

        Coroutine _forceApplyCoroutine = null;
        public void Dash()
        {
            Vector3 moveDirection = movementVariables.MoveDirection;

            GravityService.Instance.CurrentGravity = Vector3.zero;

            if (moveDirection.sqrMagnitude < .25f)
                moveDirection = Transform.forward;

            Vector3 forceDirection = Transform.forward;

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

            dashVariables.DashMade = true;

            StartCoroutine(
                DoApplyForceOverTime(forceDirection, dashVariables.PhysicalForce));

        }

        IEnumerator DoApplyForceOverTime(Vector3 forceDirection, PhysicalForceSettings forceSettings)
        {
            dashVariables.DashesMade++;

            Animator.CrossFade(animHashes.Dash, 0.2f);

            //Zero out vertical velocity on dash
            isDashing = true;
            velocity.y = 0;

            _forceApplyCoroutine = StartCoroutine(
                Rigidbody.ApplyForceContinuously(forceDirection, forceSettings));
            yield return _forceApplyCoroutine;

            //Dead stop on dash-end
            isDashing = false;
            velocity = Vector3.zero;

            AfterApplyForce();
        }

        void AfterApplyForce()
        {
            dashVariables.DashesMade--;
            _forceApplyCoroutine = null;
        }

        public float fireRate_Auto = 0.2f;
        public float fireRate_Shotgun = 2f;
        public float fireRate_Rail = 5f;
        public float shootForce_Auto = 8000f;
        public float shootForce_Shotgun = 5000f;
        public float shootForce_Rail = 100000f;

        public GameObject bullets_Auto;
        public GameObject bullets_Shotgun;
        public GameObject bullets_Rail;

        public Transform bulletOrigin;
        [HideInInspector]
        public Rigidbody rb_auto;

        private System.Action onDespawnAction;

        public void Shoot()
        {
            Gizmos.Line(bulletOrigin.position, RayHit.point, Color.yellow);
            Vector3 shootDirection = RayHit.point - bulletOrigin.position;

            if (Input.GetButton("Fire1") &&
                Time.realtimeSinceStartup - lastFired_Auto > fireRate_Auto)
            {
                GameObject auto = TrashMan.spawn(
                    bullets_Auto, bulletOrigin.position, Quaternion.identity);
                TrashMan.despawnAfterDelay(auto, 5f, () => onDespawnReset(auto));

                rb_auto = auto.GetComponent<Rigidbody>();
                rb_auto.AddForce(shootDirection.normalized * shootForce_Auto);
                lastFired_Auto = Time.realtimeSinceStartup;
            }
            else if (Input.GetButtonDown("Fire2") &&
                Time.realtimeSinceStartup - lastFired_Shotgun > fireRate_Shotgun)
            {
                GameObject shot = TrashMan.spawn(
                    bullets_Shotgun, bulletOrigin.position, Quaternion.identity);
                TrashMan.despawnAfterDelay(shot, 5f, () => onDespawnReset(shot));

                Rigidbody rb_shot = shot.GetComponent<Rigidbody>();
                rb_shot.AddForce(shootDirection.normalized * shootForce_Shotgun);
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

        //Matej: Switch this to SP parameter (figure a way around coroutines)
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

        public LayerMask probeMask = -1, stairsMask = -1;
        [HideInInspector]
        public float minGroundDotProduct, minStairsDotProduct;
        public float GetMinDot(int layer)
        {
            return (stairsMask & (1 << layer)) == 0 ?
                minGroundDotProduct : minStairsDotProduct;
        }

        const string KEY_BINDINGS = "Bindings";
        const string KEY_MOUSE_HORIZONTAL_SENSITIVITY = "MouseHorizontalSensitivity";
        const string KEY_MOUSE_VERTICAL_SENSITIVITY = "MouseVerticalSensitivity";
    }
}
