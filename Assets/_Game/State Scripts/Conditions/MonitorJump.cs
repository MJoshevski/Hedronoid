using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName ="Conditions/Monitor Jump")]
    public class MonitorJump : Condition
    {
        public StateActions onTrueAction;
        [SerializeField]
        private bool jumpMade = false;
        private JumpVariables jumpVariables;

        public override void InitCondition(PlayerStateManager state)
        {
            onTrueAction.Execute_Start(state);
            jumpVariables = state.jumpVariables;
        }

        public override bool CheckCondition(PlayerStateManager state)
        {
            bool isPressed = state.jumpPressed;
            jumpMade = jumpMade && !state.jumpReleased;

            if (isPressed && !jumpMade &&
                jumpVariables.JumpsMade < 
                jumpVariables.MaxJumps)
            {
                //Debug.LogError("JUMP MADE: " + jumpMade);

                onTrueAction.Execute(state);
                jumpVariables.JumpsMade++;
                jumpMade = true;
            } 

            return isPressed;
        }
    }
}
