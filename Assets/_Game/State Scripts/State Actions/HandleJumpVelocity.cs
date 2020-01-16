using UnityEngine;
using System.Collections;

namespace Hedronoid
{
    [CreateAssetMenu(menuName ="Actions/State Actions/Handle Jump Velocity")]
    public class HandleJumpVelocity : StateActions
    {
        private PhysicalForceSettings forceSettings;

        public override void Execute_Start(PlayerStateManager states)
        {
            forceSettings = states.jumpVariables.physicalForce;
            IGravityService gravityService = GravityService.Instance;
        }

        public override void Execute(PlayerStateManager states)
        {
            states.Rigidbody.drag = 0;
            states.timeSinceJump = Time.realtimeSinceStartup;
            states.isGrounded = false;

            if (states.movementVariables.MoveAmount > 0.1f)
            {
                states.Animator.CrossFade(states.animHashes.JumpForward, 0.2f);
            }
            else
            {
                states.Animator.CrossFade(states.animHashes.JumpIdle, 0.2f);
            }      

            Vector3 moveDirection = states.movementVariables.MoveDirection;

            if (moveDirection.sqrMagnitude < .25f)
                moveDirection = states.Transform.forward;

            Vector3 forceDirection =
                Quaternion.FromToRotation(states.Transform.forward, moveDirection)
                * GravityService.Instance.GravityRotation
                * forceSettings.Direction;

            forceDirection.Normalize();

            states.StartCoroutine(
                states.Rigidbody.ApplyForceContinuously(forceDirection, forceSettings)
                );
        }
    }
}
