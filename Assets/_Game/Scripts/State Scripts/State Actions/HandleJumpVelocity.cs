using UnityEngine;
using System.Collections;

namespace Hedronoid
{
    [CreateAssetMenu(menuName ="Actions/State Actions/Handle Jump Velocity")]
    public class HandleJumpVelocity : StateActions
    {
        private PhysicalForceSettings firstJumpSettings, secondJumpSettings;
        private JumpVariables jumpVariables;
        public override void Execute_Start(PlayerStateManager states)
        {
            jumpVariables = states.jumpVariables;

            firstJumpSettings = jumpVariables.firstJumpForceSettings;
            secondJumpSettings = jumpVariables.secondJumpForceSettings;
        }

        public override void Execute(PlayerStateManager states)
        {
            //states.Rigidbody.drag = 0;
            //states.timeSinceJump = Time.realtimeSinceStartup;
            //states.isGrounded = false;

            //if (states.jumpPhase > 1 && states.jumpPhase < states.maxAirJumps)
            //{
            //    states.Animator.CrossFade(states.animHashes.DoubleJump, 0.2f);
            //}
            //else if (states.jumpPhase == 1)
            //{
            //    if (states.movementVariables.MoveAmount > 0.1f)
            //    {
            //        states.Animator.CrossFade(states.animHashes.JumpForward, 0.2f);
            //    }
            //    else
            //    {
            //        states.Animator.CrossFade(states.animHashes.JumpIdle, 0.2f);
            //    }
            //}
        }
    }
}
