using System.Collections;
using System.Collections.Generic;
using SO;
using UnityEngine;

namespace HedronoidSP
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Rotate On Camera Rotation")]
    public class RotateOnCameraOrientation : StateActions
    {
        public TransformVariable cameraTransform;
        public float speed = 8;

        public override void Execute(StateManager states)
        {
            if (cameraTransform.value == null)
                return;

            float h = states.movementVariables.Horizontal;
            float v = states.movementVariables.Vertical;

            Vector3 targetDirection = cameraTransform.value.forward * v;
            targetDirection += cameraTransform.value.right * h;
            targetDirection.Normalize();
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero)
                targetDirection = states.m_Transform.forward;

            Quaternion tr = Quaternion.LookRotation(targetDirection);
            Quaternion targetRotation = Quaternion.Slerp(
                states.m_Transform.rotation,
                tr,
                states.delta * states.movementVariables.MoveAmount * speed);

            states.m_Transform.rotation = targetRotation;
        }
    }
}
