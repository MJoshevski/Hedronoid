using UnityEngine;
using System.Collections;

namespace HedronoidSP
{

    [CreateAssetMenu(menuName ="Conditions/Wait For Anim to End")]
    public class WaitForAnimToEnd : Condition
    {

        public string targetBool = "isPlayingAnim";

        public override bool CheckCondition(StateManager state)
        {
            bool retVal = !state.Animator.GetBool(targetBool);
            return retVal;
        }
    }
}
