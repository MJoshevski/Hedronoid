using UnityEngine;
using System.Collections;

namespace SA
{
    [CreateAssetMenu(menuName ="Actions/State Actions/Handle Jump Velocity")]
    public class HandleJumpVelocity : StateActions
    {
        public float jumpSpeed = 4;

        public override void Execute(StateManager states)
        {
            states.m_Rb.drag = 0;
            Vector3 currVelocity = states.m_Rb.velocity;

            if (states.movementVariables.MoveAmount > 0.1f)
            {
                states.m_Animator.CrossFade(states.animHashes.JumpForward, 0.2f);                
            }
            else
            {
                states.m_Animator.CrossFade(states.animHashes.JumpIdle, 0.2f);
            }

            currVelocity += jumpSpeed * Vector3.up;            
            states.m_Rb.velocity = currVelocity;
        }
    }
}
