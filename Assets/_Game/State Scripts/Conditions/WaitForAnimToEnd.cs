using UnityEngine;
using System.Collections;

namespace Hedronoid
{

    [CreateAssetMenu(menuName ="Conditions/Wait For Anim to End")]
    public class WaitForAnimToEnd : Condition    {

        public string targetBool = "isPlayingAnim";

        public override void InitCondition(PlayerStateManager state)
        {
            if (hasInitialized) return;
            hasInitialized = true;
        }

        public override bool CheckCondition(PlayerStateManager state)
        {
            bool retVal = !state.Animator.GetBool(targetBool);
            return retVal;
        }
    }
}
