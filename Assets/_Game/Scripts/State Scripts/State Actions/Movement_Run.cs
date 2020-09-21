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
        private PlayerStateManager states;

        public override void Execute_Start(PlayerStateManager states)
        {
            moveVars = states.movementVariables;
            cameraTransform = states.cameraTransform.value;
            this.states = states;
        }

        public override void Execute(PlayerStateManager states)
        {
            //ANIMATION PURPOSE 
            float moveAmount =
                Mathf.Clamp01(Mathf.Abs(moveVars.Horizontal) + Mathf.Abs(moveVars.Vertical));
            moveVars.MoveAmount = moveAmount;

            Vector3 gravity = GravityService.GetGravity(states.Rigidbody.position, out states.upAxis);
            GravityService.Instance.CurrentGravity = gravity;

            UpdateState();
            AdjustVelocity();

            if (states.desiredJump)
            {
                states.desiredJump = false;
                Jump(gravity);
            }
            
            states.velocity += gravity * states.delta * states.gravityVariables.GravityForceMultiplier;

            states.Rigidbody.velocity = states.velocity;

            ClearState();

            //DIRECTION GIZMO
            Vector3 moveDirection =
                states.movementVariables.Vertical * states.cameraTransform.value.forward +
                states.movementVariables.Horizontal * states.cameraTransform.value.right;
            moveDirection = Vector3.ProjectOnPlane(moveDirection, states.upAxis);
            moveDirection.Normalize();
            Debug.DrawRay(states.Transform.position, moveDirection, Color.yellow);
            moveVars.MoveDirection = moveDirection;
            //
        }

        void AdjustVelocity()
        {
            Vector3 xAxis = VectorExtensions.ProjectDirectionOnPlane(states.rightAxis, states.contactNormal);
            Vector3 zAxis = VectorExtensions.ProjectDirectionOnPlane(states.forwardAxis, states.contactNormal);

            float currentX = Vector3.Dot(states.velocity, xAxis);
            float currentZ = Vector3.Dot(states.velocity, zAxis);

            float acceleration = states.OnGround ? states.maxAcceleration : states.maxAirAcceleration;
            float maxSpeedChange = acceleration * Time.deltaTime;

            float newX =
                Mathf.MoveTowards(currentX, states.desiredVelocity.x, maxSpeedChange);
            float newZ =
                Mathf.MoveTowards(currentZ, states.desiredVelocity.z, maxSpeedChange);

            states.velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }

        void ClearState()
        {
            states.groundContactCount = states.steepContactCount = 0;
            states.contactNormal = states.steepNormal = Vector3.zero;
        }

        void UpdateState()
        {
            states.stepsSinceLastGrounded += 1;
            states.stepsSinceLastJump += 1;
            states.velocity = states.Rigidbody.velocity;
            if (states.OnGround || SnapToGround() || CheckSteepContacts())
            {
                states.stepsSinceLastGrounded = 0;
                states.isGrounded = true;
                if (states.stepsSinceLastJump > 1)
                {
                    states.jumpPhase = 0;
                }
                if (states.groundContactCount > 1)
                {
                    states.contactNormal.Normalize();
                }
            }
            else
            {
                states.contactNormal = states.upAxis;
                states.isGrounded = false;
            }
        }

        void Jump(Vector3 gravity)
        {
            Vector3 jumpDirection;
            if (states.OnGround)
            {
                jumpDirection = states.contactNormal;
            }
            else if (states.OnSteep)
            {
                jumpDirection = states.steepNormal;
                states.jumpPhase = 0;
            }
            else if (states.maxAirJumps > 0 && states.jumpPhase <= states.maxAirJumps)
            {
                if (states.jumpPhase == 0)
                {
                    states.jumpPhase = 1;
                }
                jumpDirection = states.contactNormal;
            }
            else
            {
                return;
            }

            states.stepsSinceLastJump = 0;
            states.jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * states.jumpHeight);
            jumpDirection = (jumpDirection + states.upAxis).normalized;
            float alignedSpeed = Vector3.Dot(states.velocity, jumpDirection);
            if (alignedSpeed > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }

            if(states.jumpPhase == 1)
            {
                if (states.movementVariables.MoveAmount > 0.1f)
                {
                    states.Animator.CrossFade(states.animHashes.JumpForward, 0.2f);
                }
                else
                {
                    states.Animator.CrossFade(states.animHashes.JumpIdle, 0.2f);
                }
            } else if (states.jumpPhase > 1)
                states.Animator.CrossFade(states.animHashes.DoubleJump, 0.2f);

            states.velocity += jumpDirection * jumpSpeed;
        }

        bool SnapToGround()
        {
            if (states.stepsSinceLastGrounded > 1 || states.stepsSinceLastJump <= 2)
            {
                return false;
            }
            float speed = states.velocity.magnitude;
            if (speed > states.maxSnapSpeed)
            {
                return false;
            }
            if (!Physics.Raycast(
                states.Rigidbody.position, -states.upAxis, out RaycastHit hit,
                states.probeDistance, states.probeMask
            ))
            {
                return false;
            }

            float upDot = Vector3.Dot(states.upAxis, hit.normal);
            if (upDot < states.GetMinDot(hit.collider.gameObject.layer))
            {
                return false;
            }

            states.groundContactCount = 1;
            states.contactNormal = hit.normal;
            float dot = Vector3.Dot(states.velocity, hit.normal);
            if (dot > 0f)
            {
                states.velocity = (states.velocity - hit.normal * dot).normalized * speed;
            }
            return true;
        }

        bool CheckSteepContacts()
        {
            if (states.steepContactCount > 1)
            {
                states.steepNormal.Normalize();
                float upDot = Vector3.Dot(states.upAxis, states.steepNormal);
                if (upDot >= states.minGroundDotProduct)
                {
                    states.steepContactCount = 0;
                    states.groundContactCount = 1;
                    states.contactNormal = states.steepNormal;
                    return true;
                }
            }
            return false;
        }
    }
}
