using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName ="Conditions/Monitor Dash")]
    public class MonitorDash : Condition
    {
        public StateActions onTrueAction;

        public override void InitCondition(PlayerStateManager state)
        {
            onTrueAction.Execute_Start(state);
        }

        public override bool CheckCondition(PlayerStateManager state)
        {
            bool isPressed = state.dashPressed;

            if (isPressed) onTrueAction.Execute(state);

            return isPressed;
        }
    }
}
