using System;
using MDKShooter;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace MDKShooter
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterInput : MonoSingleton<CharacterInput>
    {
        private CharacterController m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_Move;

        public Action<GravityDirections> OnPlatformSwitched = delegate { };

        private void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<CharacterController>();
        }



        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            var playerActions = InputManager.Instance.PlayerActions;
            // read inputs
            float h = playerActions.Move.X;
            float v = playerActions.Move.Y;

            //Gravity switch input for testing
            if (Input.GetKeyDown(KeyCode.Alpha1))
                OnPlatformSwitched(GravityDirections.DOWN);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                OnPlatformSwitched(GravityDirections.UP);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                OnPlatformSwitched(GravityDirections.LEFT);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                OnPlatformSwitched(GravityDirections.RIGHT);

            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                if (m_Character.GravityUp.x == 0)
                    m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                else
                    m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(0, 1, 1)).normalized;

                m_Move = v * m_CamForward + h * m_Cam.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v * Vector3.forward + h * Vector3.right;
            }
            // #if !MOBILE_INPUT
            //         // walk speed multiplier
            //         if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
            // #endif

            // bool crouch = Input.GetKey(KeyCode.C);
            bool crouch = false;
            bool jump = playerActions.Jump.WasPressed;
            // pass all parameters to the character control script
            m_Character.Move(m_Move, crouch, jump);
        }
    }
}