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
            onTrueAction.Execute_Start(state);
        }

        public override bool CheckCondition(PlayerStateManager state)
        {
            bool isPressed = state.jumpPressed;
            jumpMade = jumpMade && !state.jumpReleased;
            Debug.LogError("IS RELEASED: " + state.jumpReleased);

            Debug.LogError("JUMP MADE: "+ jumpMade);
            if (isPressed && !jumpMade &&
                state.jumpVariables.JumpsMade < 
                state.jumpVariables.MaxJumps)
            {
                //Debug.LogError("JUMP MADE: " + jumpMade);

                onTrueAction.Execute(state);
                state.jumpVariables.JumpsMade++;
                jumpMade = true;
            } 

            return isPressed;
        }
    }
}
