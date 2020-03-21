using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Rotate To Collision Normal")]
    public class RotateToCollisionNormal : StateActions
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
                states.Transform.rotation,
                tr,
                states.delta * states.gravityVariables.GravityRotationMultiplier);

            states.Transform.rotation = targetRotation;
        }
    }
}
