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
        public CollisionVariables collisionVariables;
        public GravityVariables gravityVariables;
        public JumpVariables jumpVariables;
        public DashVariables dashVariables;
        public WallRunVariables wallRunVariables;
        public WallJumpVariables wallJumpVariables;

        [HideInInspector]
        public float delta;
        [HideInInspector]
        public Transform Transform;
        [HideInInspector]
        public new Rigidbody Rigidbody;

        [HideInInspector]
        public Animator Animator;
        public AnimatorHashes animHashes;
        public AnimatorData animData;

        [HideInInspector] public bool jumpPressed;
        [HideInInspector] public bool jumpReleased;
        [HideInInspector] public bool dashPressed;
        [HideInInspector] public bool dashReleased;

        //[HideInInspector]
        public bool isGrounded;
        [HideInInspector]
        public float timeSinceJump;

        public PlayerActionSet PlayerActions;
        public float MouseHorizontalSensitivity { get; set; }
        public float MouseVerticalSensitivity { get; set; }
        public new TransformVariable camera;

        [HideInInspector]
        public IGravityService gravityService;

        [HideInInspector]
        public Vector3 upAxis, rightAxis, forwardAxis;
        [HideInInspector]
        public Vector3 velocity, desiredVelocity;
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

        void OnValidate()
        {
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        }

        private void Start()
        {
            Transform = this.transform;
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
        }

        private void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;

            //DEBUG
            //
            if (Input.GetKeyDown(KeyCode.F3))
                Gizmos.Enabled = !Gizmos.Enabled;
            //
            //

            if (currentState != null)
            {
                currentState.FixedTick(this);
            }
        }

        public bool desiredJump;
        private void Update()
        {
            delta = Time.deltaTime;

            Vector2 playerInput = new Vector2(movementVariables.Horizontal, movementVariables.Vertical);
            playerInput = Vector2.ClampMagnitude(playerInput, 1f);

            if (currentState != null)
            {
                currentState.Tick(this);
            }

            if (camera.value)
            {
                rightAxis = VectorExtensions.ProjectDirectionOnPlane(camera.value.right, upAxis);
                forwardAxis =
                    VectorExtensions.ProjectDirectionOnPlane(camera.value.forward, upAxis);
            }
            else
            {
                rightAxis = VectorExtensions.ProjectDirectionOnPlane(Vector3.right, upAxis);
                forwardAxis = VectorExtensions.ProjectDirectionOnPlane(Vector3.forward, upAxis);
            }

            desiredVelocity =
                new Vector3(playerInput.x, 0f, playerInput.y) * 
                movementVariables.MaxAcceleration;

            desiredJump |= Input.GetButtonDown("Jump");

            //Debug.LogError("GRAVITY: " + gravityService.CurrentGravity.ToString());

            if (gravityService.CurrentGravity == Vector3.zero)
            {
                Rigidbody.velocity =
                    Vector3.ClampMagnitude(
                        Rigidbody.velocity,maxVelocityMagnitudeInVacuum);
            }

            //Debug.LogError("VELO MAG: " + Rigidbody.velocity.magnitude);

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
                }
                else if (upDot > -0.01f)
                {
                    steepContactCount += 1;
                    steepNormal += normal;
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
