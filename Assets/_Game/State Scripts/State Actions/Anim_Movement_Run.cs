﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Anim_Movement Run")]
    public class Anim_Movement_Run: StateActions
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

            states.Animator.SetFloat(
                states.animHashes.Vertical,
                states.movementVariables.MoveAmount,
                0.2f,
                states.delta);
        }
    }
}