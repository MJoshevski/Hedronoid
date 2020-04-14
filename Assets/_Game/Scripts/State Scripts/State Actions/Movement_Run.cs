using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Movement Forward")]
    public class Movement_Run : StateActions
    {
        private Transform cameraTransform;
        private Vector3 _movementVelocity;
        private MovementVariables moveVars;

        public override void Execute_Start(PlayerStateManager states)
        {
            moveVars = states.movementVariables;
            cameraTransform = states.camera.value;
        }

        public override void Execute(PlayerStateManager states)
        {
            float moveAmount =
                Mathf.Clamp01(Mathf.Abs(moveVars.Horizontal) + Mathf.Abs(moveVars.Vertical));
            moveVars.MoveAmount = moveAmount;

            Vector3 acceleration;
            Vector3 velocity = Vector3.zero;

            if (cameraTransform)
            {
                Vector3 forward = cameraTransform.forward;
                forward.y = 0f;
                forward.Normalize();
                Vector3 right = cameraTransform.right;
                right.y = 0f;
                right.Normalize();

                acceleration = 
                    (forward * moveVars.Vertical + right * moveVars.Horizontal) * 
                    moveVars.MovementSpeed;
            }
            else
            {
                acceleration = 
                    states.Transform.forward * 
                    states.movementVariables.MoveAmount *
                    moveVars.MovementSpeed;
            }

            Vector3 moveDirection =
                states.movementVariables.Vertical * states.camera.value.forward +
                states.movementVariables.Horizontal * states.camera.value.right;

            moveDirection = Vector3.ProjectOnPlane(moveDirection, GravityService.Instance.GravityUp);
            moveDirection.Normalize();

            Debug.DrawRay(states.Transform.position, moveDirection, Color.yellow);

            moveVars.MoveDirection = moveDirection;

            if (states.isGrounded)
            {
                if (states.gravityService.Direction == GravityDirections.DOWN ||
                states.gravityService.Direction == GravityDirections.UP)
                {
                    acceleration.y = 0;
                }
                else if (states.gravityService.Direction == GravityDirections.LEFT ||
                  states.gravityService.Direction == GravityDirections.RIGHT)
                {
                    acceleration.x = 0;
                }else if (states.gravityService.Direction == GravityDirections.FRONT ||
                states.gravityService.Direction == GravityDirections.BACK)
                {
                    acceleration.z = 0;
                }
            }

            if (states.gravityService.Direction == GravityDirections.DOWN ||
                states.gravityService.Direction == GravityDirections.UP)
                acceleration.y = states.Rigidbody.velocity.y;
            else if (states.gravityService.Direction == GravityDirections.LEFT ||
                states.gravityService.Direction == GravityDirections.RIGHT)
                acceleration.x = states.Rigidbody.velocity.x;
            else if (states.gravityService.Direction == GravityDirections.FRONT ||
                states.gravityService.Direction == GravityDirections.BACK)
                acceleration.z = states.Rigidbody.velocity.z;

            float maxSpeedChange = states.movementVariables.MaxAcceleration * states.delta;
            velocity.x = Mathf.MoveTowards(velocity.x, states.movementVariables.desiredVelocity.x, maxSpeedChange);
            velocity.z = Mathf.MoveTowards(velocity.z, states.movementVariables.desiredVelocity.z, maxSpeedChange);

            velocity += acceleration * states.delta;
            Vector3 displacement = velocity * states.delta;
            states.Transform.localPosition += displacement;
            //states.Rigidbody.velocity = targetVelocity;
        }
    }
}
