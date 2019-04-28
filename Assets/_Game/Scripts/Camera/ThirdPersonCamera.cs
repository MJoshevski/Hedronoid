using System;
using UnityEngine;
using UnityStandardAssets.Cameras;
using UnityStandardAssets.CrossPlatformInput;

namespace MDKShooter
{
    public class ThirdPersonCamera : PivotBasedCameraRig
    {
        // This script is designed to be placed on the root object of a camera rig,
        // comprising 3 gameobjects, each parented to the next:

        // 	Camera Rig
        // 		Pivot
        // 			Camera

        [SerializeField] private float m_MoveSpeed = 1f;                      // How fast the rig will move to keep up with the target's position.
        [Range(0f, 10f)] [SerializeField] private float m_TurnSpeed = 1.5f;   // How fast the rig will rotate from user input.
        [SerializeField] private float m_TurnSmoothing = 0.0f;                // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
        [SerializeField] private float m_TiltMax = 75f;                       // The maximum value of the x axis rotation of the pivot.
        [SerializeField] private float m_TiltMin = 45f;                       // The minimum value of the x axis rotation of the pivot.
        [SerializeField] private bool m_VerticalAutoReturn = false;           // set wether or not the vertical axis should auto return

        private float m_LookAngle;                    // The rig's y axis rotation.
        private float m_TiltAngle;                    // The pivot's x axis rotation.
        private const float k_LookDistance = 100f;    // How far in front of the pivot the character's look target is.
        private Vector3 m_PivotEulers;
        private Quaternion m_PivotTargetRot;
        private Quaternion m_TransformTargetRot;

        protected override void Awake()
        {
            base.Awake();

            m_PivotEulers = m_Pivot.rotation.eulerAngles;

            m_PivotTargetRot = m_Pivot.transform.localRotation;
            m_TransformTargetRot = transform.localRotation;
        }


        protected void Update()
        {
            HandleRotationMovement();
            //Debug.Log(m_Target.rotation.eulerAngles.ToString());
        }

        protected override void FollowTarget(float deltaTime)
        {
            if (m_Target == null) return;
            // Move the rig towards target position.
            transform.position = Vector3.Lerp(transform.position, m_Target.position, deltaTime * m_MoveSpeed);
        }


        private void HandleRotationMovement()
        {
            if (Time.timeScale < float.Epsilon)
                return;

            float x;
            float y;

            // Adjust the look angle by an amount proportional to the turn speed and horizontal input.
            if (CharacterController.Instance.GravityUp.x == 0)
            {
                // Read the user input
                x = CrossPlatformInputManager.GetAxis("Mouse X");
                y = CrossPlatformInputManager.GetAxis("Mouse Y");

                m_LookAngle += x * m_TurnSpeed * m_Target.up.y;

                // Rotate the rig (the root object) around Y-Z axis only:
                m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, m_Target.rotation.eulerAngles.z);
            }
            else
            {
                // Read the user input
                y = CrossPlatformInputManager.GetAxis("Mouse X");
                x = CrossPlatformInputManager.GetAxis("Mouse Y");

                m_LookAngle += y * m_TurnSpeed * m_Target.up.x;

                // Rotate the rig (the root object) around Y-Z axis only:
                if (CharacterController.Instance.GravityUp.x > 0)
                    m_TransformTargetRot = Quaternion.Euler(m_LookAngle, 0f, -90);
                else
                    m_TransformTargetRot = Quaternion.Euler(m_LookAngle, 0f, 90);
            }

            if (m_VerticalAutoReturn)
            {
                // For tilt input, we need to behave differently depending on whether we're using mouse or touch input:
                // on mobile, vertical input is directly mapped to tilt value, so it springs back automatically when the look input is released
                // we have to test whether above or below zero because we want to auto-return to zero even if min and max are not symmetrical.
                m_TiltAngle = y > 0 ? Mathf.Lerp(0, -m_TiltMin, y) : Mathf.Lerp(0, m_TiltMax, -y);
            }
            else
            {
                // on platforms with a mouse, we adjust the current angle based on Y mouse input and turn speed
                if (CharacterController.Instance.GravityUp.x == 0) m_TiltAngle -= y * m_TurnSpeed;
                else m_TiltAngle -= x * m_TurnSpeed;

                // and make sure the new value is within the tilt range
                if (CharacterController.Instance.GravityUp.x == 0) m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);
                else m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);
            }

            if (m_PivotEulers.z > 0) m_PivotEulers.z *= -1;
            m_PivotTargetRot = Quaternion.Euler(m_TiltAngle, m_PivotEulers.y, m_PivotEulers.z);

            if (m_TurnSmoothing > 0)
            {
                m_Pivot.localRotation = Quaternion.Slerp(m_Pivot.localRotation, m_PivotTargetRot, m_TurnSmoothing * Time.deltaTime);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, m_TransformTargetRot, m_TurnSmoothing * Time.deltaTime);
            }
            else
            {
                m_Pivot.localRotation = m_PivotTargetRot;
                transform.localRotation = m_TransformTargetRot;
            }
        }
    }
}
