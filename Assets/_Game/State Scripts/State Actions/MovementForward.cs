using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HedronoidSP
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Movement Forward")]
    public class MovementForward : StateActions
    {
        public float movementSpeed = 2;

        public override void Execute(StateManager states)
        {
            if(states.movementVariables.MoveAmount > 0.1f)
            {
                states.Rigidbody.drag = 0;
            }
            else
            {
                states.Rigidbody.drag = 4;
            }

            Vector3 targetVelocity = states.Transform.forward * states.movementVariables.MoveAmount *
                movementSpeed;
            targetVelocity.y = states.Rigidbody.velocity.y;
            states.Rigidbody.velocity = targetVelocity;

            //Vector3 targetVelocity = MoveDirection * CharacterMoveSettings.MoveSpeedMultiplier;
            //_movementVelocity = Vector3.Lerp(_movementVelocity, targetVelocity, Time.deltaTime * CharacterMoveSettings.MoveVeloctiyChangeRate);

            //transform.position += _movementVelocity * Time.deltaTime;
        }
    }
}
