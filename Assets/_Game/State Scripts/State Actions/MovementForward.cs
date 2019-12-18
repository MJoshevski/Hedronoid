using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Movement Forward")]
    public class MovementForward : StateActions
    {
        private Vector3 _movementVelocity;

        public override void Execute(StateManager states)
        {
            MovementVariables moveVars = states.movementVariables;

            if(states.movementVariables.MoveAmount > 0.1f)
            {
                states.Rigidbody.drag = 0;
            }
            else
            {
                states.Rigidbody.drag = 4;
            }

            Vector3 targetVelocity = states.Transform.forward * states.movementVariables.MoveAmount *
                moveVars.MovementSpeed;

            //Vector3 targetVelocity = moveVars.MoveDirection * states.movementVariables.MoveAmount *
            //   moveVars.MovementSpeed;

            //_movementVelocity = Vector3.Lerp(_movementVelocity, targetVelocity, states.delta * moveVars.MoveVeloctiyChangeRate);

            //states.Transform.position += _movementVelocity * states.delta;

            if (states.isGrounded)
            {
                targetVelocity.y = 0;
            }

            targetVelocity.y = states.Rigidbody.velocity.y;

            states.Rigidbody.velocity = targetVelocity;
        }
    }
}
