using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Movement Forward")]
    public class Movement_Run : StateActions
    {
        private Vector3 _movementVelocity;
        private MovementVariables moveVars;
        private PlayerActionSet playerActions;

        public override void Execute_Start(PlayerStateManager states)
        {
            moveVars = states.movementVariables;
            playerActions = states.PlayerActions;
        }

        public override void Execute(PlayerStateManager states)
        {
            moveVars.Horizontal = playerActions.Move.X;
            moveVars.Vertical = playerActions.Move.Y;

            float moveAmount =
                Mathf.Clamp01(Mathf.Abs(moveVars.Horizontal) + Mathf.Abs(moveVars.Vertical));
            moveVars.MoveAmount = moveAmount;

            Vector3 targetVelocity = states.Transform.forward * states.movementVariables.MoveAmount *
                moveVars.MovementSpeed;

            Vector3 moveDirection =
                playerActions.Move.Y * states.camera.value.forward +
                playerActions.Move.X * states.camera.value.right;

            moveDirection = Vector3.ProjectOnPlane(moveDirection, GravityService.Instance.GravityUp);
            moveDirection.Normalize();

            Debug.DrawRay(states.Transform.position, moveDirection, Color.yellow);

            moveVars.MoveDirection = moveDirection;

            if (states.isGrounded)
            {
                if (states.gravityService.Direction == GravityDirections.DOWN ||
                states.gravityService.Direction == GravityDirections.UP)
                {
                    targetVelocity.y = 0;
                }
                else if (states.gravityService.Direction == GravityDirections.LEFT ||
                  states.gravityService.Direction == GravityDirections.RIGHT)
                {
                    targetVelocity.x = 0;
                }else if (states.gravityService.Direction == GravityDirections.FRONT ||
                states.gravityService.Direction == GravityDirections.BACK)
                {
                    targetVelocity.z = 0;
                }
            }

            if (states.gravityService.Direction == GravityDirections.DOWN ||
                states.gravityService.Direction == GravityDirections.UP)
                targetVelocity.y = states.Rigidbody.velocity.y;
            else if (states.gravityService.Direction == GravityDirections.LEFT ||
                states.gravityService.Direction == GravityDirections.RIGHT)
                targetVelocity.x = states.Rigidbody.velocity.x;
            else if (states.gravityService.Direction == GravityDirections.FRONT ||
                states.gravityService.Direction == GravityDirections.BACK)
                targetVelocity.z = states.Rigidbody.velocity.z;

            states.Rigidbody.velocity = targetVelocity;
        }
    }
}
