using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Anim_Dash")]
    public class Anim_Dash: StateActions
    {
        public StateActions[] stateActions;

        public override void Execute_Start(PlayerStateManager states)
        {
        }

        public override void Execute(PlayerStateManager states)
        {
            if (stateActions != null)
            {
                for (int i = 0; i < stateActions.Length; i++)
                {
                    stateActions[i].Execute(states);
                }
            }

            states.Animator.CrossFade(
                states.animHashes.Dash,
                0.2f);
        }
    }
}
