using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName ="Conditions/Monitor Jump")]
    public class MonitorJump : Condition
    {
        public StateActions onTrueAction;
        private bool jumpMade = false;
        public override void InitCondition(PlayerStateManager state)
        {
            if (hasInitialized) return;

            onTrueAction.Execute_Start(state);
            hasInitialized = true;
        }
        public override bool CheckCondition(PlayerStateManager state)
        {
            bool resultPressed = state.jumpPressed;
            bool resultReleased = state.jumpReleased;

            if (resultPressed && !jumpMade &&
                state.jumpVariables.JumpsMade < 
                state.jumpVariables.MaxJumps)
            {
                state.jumpVariables.JumpsMade++;
                resultPressed = false;
                jumpMade = true;
                onTrueAction.Execute(state);
            }

            if(resultReleased)
            {
                jumpMade = false;
                resultReleased = false;
            }
            return resultPressed;
        }
    }
}
