using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Conditions/Monitor Wall-Run")]
    public class MonitorWallRun : Condition
    {
        public StateActions onTrueAction;

        private WallRunVariables wallRunVariables;

        public override void InitCondition(PlayerStateManager state)
        {
            onTrueAction.Execute_Start(state);
            wallRunVariables = state.wallRunVariables;
        }

        public override bool CheckCondition(PlayerStateManager state)
        {
            if (wallRunVariables.WallRunning) /*onTrueAction.Execute(state);*/
                return true;
            else return false;
        }
    }
}

