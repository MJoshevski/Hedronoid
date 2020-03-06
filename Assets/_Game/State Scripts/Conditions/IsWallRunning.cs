using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Conditions/Is Wall Running")]
    public class IsWallRunning : Condition
    {
        public float hardLandThreshold = 1.5f;
        public float maxLandThreshold = 4f;

        public State fastLandState;

        public override void InitCondition(PlayerStateManager state)
        {
        }

        public override bool CheckCondition(PlayerStateManager state)
        {
            float m_timeDifference = Time.realtimeSinceStartup - state.timeSinceJump;
            if (m_timeDifference > 0.5f)
            {
                bool result = state.isGrounded;

                if(result)
                {
                    if(m_timeDifference > hardLandThreshold &&
                        m_timeDifference < maxLandThreshold)
                    {
                        if(state.movementVariables.MoveAmount > 0.3f)
                        {
                            state.Animator.CrossFade(state.animHashes.LandRoll, 0.2f);
                        }
                        else
                        {
                            // Matej: This is a slight deviation from Strategy Pattern
                            //as we can't see this in States, but didn't matter for now
                            //can be reworked later.
                            state.Animator.SetBool(state.animHashes.IsPlayingAnim, true);
                            //
                            state.Animator.CrossFade(state.animHashes.LandHard, 0.2f);
                        }
                    }
                    else if (m_timeDifference > maxLandThreshold)
                    {
                        // Matej: This is a slight deviation from Strategy Pattern
                        //as we can't see this in States, but didn't matter for now
                        //can be reworked later.
                        state.Animator.SetBool(state.animHashes.IsPlayingAnim, true);
                        //
                        state.Animator.CrossFade(state.animHashes.LandHard, 0.2f);
                    }
                    else
                    {
                        state.Animator.CrossFade(state.animHashes.LandFast, 0.2f);
                    }

                    state.jumpVariables.JumpsMade = 0;
                }
                return result;
            }
            else
            {
                return false;
            }
            
        }
    }
}
