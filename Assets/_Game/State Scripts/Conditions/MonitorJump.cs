using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName ="Conditions/Monitor Jump")]
    public class MonitorJump : Condition
    {
        public StateActions onTrueAction;

        public override void InitCondition(PlayerStateManager state)
        {
            if (hasInitialized) return;

            onTrueAction.Execute_Start(state);
            hasInitialized = true;
        }
        public override bool CheckCondition(PlayerStateManager state)
        {
            bool result = state.isJumping;

            if (state.isJumping &&
                state.jumpVariables.JumpsMade < state.jumpVariables.MaxJumps)
            {
                state.isJumping = false;
                state.jumpVariables.JumpsMade++;
                onTrueAction.Execute(state);
            }

            return result;
        }
    }
}
