using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName ="Conditions/Always True")]
    public class AlwaysTrue : Condition
    {
        public override void InitCondition(PlayerStateManager state)
        {
            if (hasInitialized) return;
            hasInitialized = true;
        }

        public override bool CheckCondition(PlayerStateManager state)
        {
            return true;
        }
    }
}
