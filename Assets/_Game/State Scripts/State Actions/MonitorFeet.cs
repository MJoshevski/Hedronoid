using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SO;

namespace SA
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Monitor Feet Position")]
    public class MonitorFeet : StateActions
    {
        public override void Execute(StateManager states)
        {
            //Vector3 rightFoot_relative =
            //    states.m_Transform.InverseTransformPoint(states.animData.rightFoot.position);

            //Vector3 leftFoot_relative =
            //    states.m_Transform.InverseTransformPoint(states.animData.leftFoot.position);

            //bool leftForward = false;

            //if (leftFoot_relative.z > rightFoot_relative.z)
            //    leftForward = true;

            //states.m_Animator.SetBool(states.animHashes.LeftFootForward, leftForward);
        }
    }
}
