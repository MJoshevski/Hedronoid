using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Anim_UpdateIsGrounded")]
    public class Anim_UpdateIsGrounded : StateActions
    {
        public override void Execute_Start(PlayerStateManager states)
        {
        }

        public override void Execute(PlayerStateManager states)
        {
            states.Animator.SetBool(states.animHashes.IsGrounded, states.isGrounded);
        }
    }
}
