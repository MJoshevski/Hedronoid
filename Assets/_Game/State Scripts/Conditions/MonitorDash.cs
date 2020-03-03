using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName ="Conditions/Monitor Dash")]
    public class MonitorDash : Condition
    {
        public StateActions onTrueAction;
        [SerializeField]
        private bool dashMade = false;
        private DashVariables dashVariables;

        public override void InitCondition(PlayerStateManager state)
        {
            onTrueAction.Execute_Start(state);
            dashVariables = state.dashVariables;
        }

        public override bool CheckCondition(PlayerStateManager state)
        {
            bool isPressed = state.dashPressed;
            dashMade = dashMade && !state.dashReleased;

            if (isPressed && !dashMade &&
                dashVariables.DashesMade <
                dashVariables.MaxDashes)
            {
                onTrueAction.Execute(state);
                dashMade = true;
            }

            return isPressed;
        }
    }
}
