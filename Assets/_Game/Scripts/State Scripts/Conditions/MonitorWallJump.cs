using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName ="Conditions/Monitor Wall Jump")]
    public class MonitorWallJump : Condition
    {
        public StateActions onTrueAction;

        public override void InitCondition(PlayerStateManager state)
        {
            onTrueAction.Execute_Start(state);
        }

        public override bool CheckCondition(PlayerStateManager state)
        {
            if (!state.jumpPressed)
                return false;
            if (!state.wallRunVariables.WallRunning)
                return false;

            onTrueAction.Execute(state);
            Debug.Log("Activated Wall Jump");

            return true;
        }
    }
}
