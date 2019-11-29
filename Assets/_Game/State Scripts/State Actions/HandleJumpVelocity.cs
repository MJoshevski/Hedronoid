using UnityEngine;
using System.Collections;

namespace HedronoidSP
{
    [CreateAssetMenu(menuName ="Actions/State Actions/Handle Jump Velocity")]
    public class HandleJumpVelocity : StateActions
    {
        public float jumpSpeed = 4;

        public override void Execute(StateManager states)
        {
            states.Rigidbody.drag = 0;
            Vector3 currVelocity = states.Rigidbody.velocity;
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

            currVelocity += jumpSpeed * Vector3.up;            
            states.Rigidbody.velocity = currVelocity;
        }
    }
}
