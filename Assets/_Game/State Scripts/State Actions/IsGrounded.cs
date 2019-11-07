using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Is Grounded")]
    public class IsGrounded : StateActions
    {
        public override void Execute(StateManager states)
        {
            Vector3 origin = states.m_Transform.position;

            origin.y += .7f;
            Vector3 dir = -Vector3.up;
            float dis = 1.4f;

            RaycastHit hit;

            if(Physics.Raycast(origin, dir, out hit, dis))
            {
                states.isGrounded = true;
            }
            else
            {
                states.isGrounded = false;
            }

            states.m_Animator.SetBool(states.animHashes.IsGrounded, states.isGrounded);
        }
    }
}
