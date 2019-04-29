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
        [SerializeField] private float m_TiltMax = 75f;                       // The maximum value of the x axis rotation of the pivot.
        [SerializeField] private float m_TiltMin = 45f;                       // The minimum value of the x axis rotation of the pivot.


        void Update()
        {
            if (Time.timeScale < float.Epsilon)
                return;

            var playerAction = InputManager.Instance.PlayerActions;
            var gravityService = GravityService.Instance;

            var x = playerAction.Look.X;
            var y = playerAction.Look.Y;


            var horizontalAngle = x * m_TurnSpeed * Time.deltaTime * InputManager.Instance.MouseHorizontalSensitivity;
            transform.Rotate(new Vector3(0, horizontalAngle, 0), Space.Self);

            float verticalAngle = -y * m_TurnSpeed * Time.deltaTime * InputManager.Instance.MouseVerticalSensitivity;
            m_Pivot.Rotate(new Vector3(verticalAngle, 0, 0), Space.Self);

            //TODO: add clamping
            // var angles = m_Pivot.localRotation.eulerAngles;
            // clampedX = Mathf.Clamp(angles.x, m_TiltMin, m_TiltMax);
            // m_Pivot.localRotation = Quaternion.Euler(angles);

            if (transform.up != gravityService.GravityUp)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    gravityService.GravityRotation,
                    m_TurnSpeed * Time.deltaTime);
            }
        }

        protected override void FollowTarget(float deltaTime)
        {
            if (m_Target == null)
                return;
            // Move the rig towards target position.
            transform.position = Vector3.Lerp(transform.position, m_Target.position, deltaTime * m_MoveSpeed);
        }
    }
}
