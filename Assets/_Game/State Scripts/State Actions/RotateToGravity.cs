using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Rotate To Gravity")]
    public class RotateToGravity : StateActions
    {
        public override void Execute_Start(PlayerStateManager states)
        {
        }

        public override void Execute(PlayerStateManager states)
        {
            Vector3 targetDirection = states.movementVariables.MoveDirection;

            if (targetDirection == Vector3.zero)
                targetDirection = states.Transform.forward;

            Quaternion tr =
                Quaternion.LookRotation(targetDirection, states.gravityService.GravityUp);

            Quaternion targetRotation = Quaternion.Slerp(
                states.Transform.localRotation,
                tr,
                states.delta * states.gravityVariables.GravityRotationMultiplier);

            states.Transform.localRotation = targetRotation;

            //Apply adequate rotation
            //if (states.Rigidbody.transform.up != states.gravityService.GravityUp)
            //{
            //    states.Transform.localRotation =
            //        Quaternion.Slerp(
            //        states.Transform.localRotation,
            //        states.gravityService.GravityRotation,
            //        states.gravityVariables.GravityRotationMultiplier * states.delta);
            //}
        }
    }
}
