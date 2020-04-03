using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/Camera/Camera Gravity Rotation")]
    public class CameraGravityRotation : Action
    {
        // This script is designed to be placed on the root object of a camera rig,
        // comprising 3 gameobjects, each parented to the next:

        // 	Camera Rig
        // 		Pivot
        // 			Camera

        public TransformVariable cameraTransform;
        public TransformVariable pivotTransform;

        // How fast the rig will adapt to the newly changed gravity
        [Range(100f, 1000f)] [SerializeField] private float m_GravityAdaptTurnSpeed = 500f;

        // The maximum value of the x axis rotation of the pivot.
        [SerializeField] private float m_TiltMax = 75f;                                     
        [SerializeField] private float m_TiltMin = 45f;

        public override void Execute_Start()
        {
        }

        public override void Execute()
        {
            if (Time.timeScale < float.Epsilon)
                return;

            var playerAction = InputManager.Instance.PlayerActions;
            var gravityService = GravityService.Instance;

            var x = playerAction.Look.X;
            var y = playerAction.Look.Y;


            var horizontalAngle = x * Time.deltaTime * InputManager.Instance.MouseHorizontalSensitivity;
            cameraTransform.value.Rotate(new Vector3(0, horizontalAngle, 0), Space.Self);

            float verticalAngle = -y * Time.deltaTime * InputManager.Instance.MouseVerticalSensitivity;
            pivotTransform.value.Rotate(new Vector3(verticalAngle, 0, 0), Space.Self);

            //TODO: add clamping
            // var angles = m_Pivot.localRotation.eulerAngles;
            // clampedX = Mathf.Clamp(angles.x, m_TiltMin, m_TiltMax);
            // m_Pivot.localRotation = Quaternion.Euler(angles);

            if (cameraTransform.value.up != gravityService.GravityUp)
            {
                cameraTransform.value.rotation = Quaternion.RotateTowards(
                    cameraTransform.value.rotation,
                    gravityService.GravityRotation,
                    m_GravityAdaptTurnSpeed * Time.deltaTime);
            }
        }
    }
}
