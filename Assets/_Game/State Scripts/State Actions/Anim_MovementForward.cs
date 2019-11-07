using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SO;

namespace SA
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Anim_MovementForward")]
    public class Anim_MovementForward : StateActions
    {
        public StateActions[] stateActions;
        public override void Execute(StateManager states)
        {
            if (stateActions != null)
            {
                for (int i = 0; i < stateActions.Length; i++)
                {
                    stateActions[i].Execute(states);
                }
            }

            states.m_Animator.SetFloat(
                states.animHashes.Vertical,
                states.movementVariables.MoveAmount,
                0.2f,
                states.delta);
        }
    }
}
