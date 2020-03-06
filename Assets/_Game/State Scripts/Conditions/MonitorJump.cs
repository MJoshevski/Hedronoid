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
            onTrueAction.Execute_Start(state);
        }

        public override bool CheckCondition(PlayerStateManager state)
        {
            bool isPressed = state.jumpPressed;

            if (isPressed && !state.jumpVariables.JumpMade &&
                state.jumpVariables.JumpsMade <
                state.jumpVariables.MaxJumps)
            {
                //Debug.LogError("JUMP MADE: " + jumpMade);

                onTrueAction.Execute(state);
            }
            else
            {
                state.jumpVariables.JumpMade =
                    state.jumpVariables.JumpMade && !state.jumpReleased;
            }

            return isPressed;
        }
    }
}
