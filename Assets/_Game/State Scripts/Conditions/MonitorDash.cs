using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName ="Conditions/Monitor Dash")]
    public class MonitorDash : Condition
    {
        public StateActions onTrueAction;
        private DashVariables dashVariables;

        public override void InitCondition(PlayerStateManager state)
        {
            onTrueAction.Execute_Start(state);
            dashVariables = state.dashVariables;
        }

        public override bool CheckCondition(PlayerStateManager state)
        {
            dashVariables.DashMade = dashVariables.DashMade && !state.dashReleased;

            bool dashMade = dashVariables.DashMade;

            if (dashMade && !state.jumpReleased && !state.dashPressed)
                dashMade = dashVariables.DashMade = false;

            if (state.dashPressed && !dashMade &&
                dashVariables.DashesMade < dashVariables.MaxDashes)
            {
                onTrueAction.Execute(state);
                return true;
            }
            return false;
        }
    }
}
