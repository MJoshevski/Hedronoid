using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Movement Forward")]
    public class MovementForward : StateActions
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
            if (moveVars.MoveAmount > 0.1f)
            {
                states.Rigidbody.drag = 0;
            }
            else
            {
                states.Rigidbody.drag = 4;
            }

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
                targetVelocity.y = 0;
            }

            targetVelocity.y = states.Rigidbody.velocity.y;

            states.Rigidbody.velocity = targetVelocity;
        }
    }
}
