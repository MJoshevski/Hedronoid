using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Conditions/Has Not Landed")]
    public class HasNotLanded : Condition
    {
        public override void InitCondition(PlayerStateManager state)
        {
        }

        public override bool CheckCondition(PlayerStateManager state)
        {
            float m_timeDifference = Time.realtimeSinceStartup - state.timeSinceJump;
            if (m_timeDifference > 0.5f)
            {
                bool result = !state.isGrounded;
                return result;
            }
            return false;            
        }
    }
}
