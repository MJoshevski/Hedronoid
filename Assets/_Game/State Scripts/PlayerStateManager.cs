using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public WallRunVariables wallRunVariables;

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

        [HideInInspector]
        public bool isJumping;
        //[HideInInspector]
        public bool isGrounded;
        [HideInInspector]
        public float timeSinceJump;

        public PlayerActionSet PlayerActions;
        public float MouseHorizontalSensitivity { get; set; }
        public float MouseVerticalSensitivity { get; set; }
        public TransformVariable camera;

        [HideInInspector]
        public IGravityService gravityService;

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
        }

        private void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;

            if (currentState != null)
            {
                currentState.FixedTick(this);
            }

            //Apply adequate rotation
            if (Rigidbody.transform.up != gravityService.GravityUp)
            {
                Rigidbody.rotation = Quaternion.Slerp(
                   Rigidbody.rotation,
                   gravityService.GravityRotation,
                   gravityVariables.GravityRotationMultiplier * delta);
            }
        }

        private void Update()
        {
            delta = Time.deltaTime;

            if(currentState != null)
            {
                currentState.Tick(this);
            }
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

        const string KEY_BINDINGS = "Bindings";
        const string KEY_MOUSE_HORIZONTAL_SENSITIVITY = "MouseHorizontalSensitivity";
        const string KEY_MOUSE_VERTICAL_SENSITIVITY = "MouseVerticalSensitivity";
    }
}
