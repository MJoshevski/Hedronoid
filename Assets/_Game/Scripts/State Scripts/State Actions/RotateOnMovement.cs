using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Rotate On Movement")]
    public class RotateOnMovement : StateActions
    {
        public override void Execute_Start(PlayerStateManager states)
        {
        }

        public override void Execute(PlayerStateManager states)
        {
            MovementVariables movVars = states.movementVariables;

            if (movVars.MoveDirection.sqrMagnitude < .25f)
                return;

            var targetRotation = Quaternion.LookRotation(
                movVars.MoveDirection, GravityService.Instance.GravityUp);

            states.Transform.rotation =
                Quaternion.Slerp(
                    states.Transform.rotation,
                    targetRotation,
                    movVars.TurnSpeedMultiplier * states.delta);
        }
    }
}
