using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    [CreateAssetMenu(menuName = "Conditions/Has Landed")]
    public class HasLanded : Condition
    {
        public override bool CheckCondition(StateManager state)
        {
            if(Time.realtimeSinceStartup - state.timeSinceJump > 1f)
            {
                return state.isGrounded;
            }
            else
            {
                return false;
            }
            
        }
    }
}
