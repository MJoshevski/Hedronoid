using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HedronoidSP
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Anim_UpdateIsGrounded")]
    public class Anim_UpdateIsGrounded : StateActions
    {
        public override void Execute(StateManager states)
        {
            states.Animator.SetBool(states.animHashes.IsGrounded, states.isGrounded);
        }
    }
}
