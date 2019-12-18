using System.Collections;
using System.Collections.Generic;
using SO;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Rotate On Movement")]
    public class RotateOnMovement : StateActions
    {
        public override void Execute(StateManager states)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    states.movementVariables.MoveDirection,
                    GravityService.Instance.GravityUp);

            float h = states.movementVariables.Horizontal;
            float v = states.movementVariables.Vertical;

            Vector3 targetDirection = states.Transform.forward * v;
            targetDirection += states.Transform.right * h;
            targetDirection.Normalize();
            targetDirection.y = 0;

            states.Transform.rotation =
                Quaternion.Slerp(
                    states.Transform.rotation,
                    targetRotation,
                    states.movementVariables.TurnSpeedMultiplier * states.delta);

            states.Transform.rotation = targetRotation;
        }
    }
}
