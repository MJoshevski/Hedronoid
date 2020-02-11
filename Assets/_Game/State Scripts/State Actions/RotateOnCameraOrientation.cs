using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Rotate On Camera Rotation")]
    public class RotateOnCameraOrientation : StateActions
    {
        public TransformVariable cameraTransform;
        public float speed = 8;

        public override void Execute_Start(PlayerStateManager states)
        {
        }

        public override void Execute(PlayerStateManager states)
        {
            if (cameraTransform.value == null)
                return;

            Vector3 targetDirection = states.movementVariables.MoveDirection;

            if (targetDirection == Vector3.zero)
                targetDirection = states.Transform.forward;

            Quaternion tr = Quaternion.LookRotation(targetDirection);

            Quaternion targetRotation = Quaternion.Slerp(
                states.Rigidbody.rotation,
                tr,
                states.delta * states.movementVariables.MoveAmount * speed);

            states.Rigidbody.rotation = targetRotation;


            //Apply adequate rotation
            //if (states.Rigidbody.transform.up != states.gravityService.GravityUp)
            //{
            //    states.Rigidbody.rotation = Quaternion.Slerp(
            //       states.Rigidbody.rotation,
            //       states.gravityService.GravityRotation,
            //       states.gravityVariables.GravityRotationMultiplier * states.delta);
            //}
        }
    }
}
