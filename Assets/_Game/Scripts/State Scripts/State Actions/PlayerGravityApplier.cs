using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Player Gravity Applier")]
    public class PlayerGravityApplier : StateActions
    {
        private IGravityService gravityService;
        private float floatDelay;

        public override void Execute_Start(PlayerStateManager states)
        {
            gravityService = GravityService.Instance;
        }

        public override void Execute(PlayerStateManager states)
        {
            if (states.gravityVariables.floatToSleep)
            {
                if (states.Rigidbody.IsSleeping())
                {
                    floatDelay = 0f;
                    return;
                }

                if (states.Rigidbody.velocity.sqrMagnitude < 0.0001f)
                {
                    floatDelay += states.delta;
                    if (floatDelay >= 1f)
                    {
                        return;
                    }
                }
                else floatDelay = 0f;
            }

            //states.Rigidbody.AddForce(
            //   GravityService.GetGravity(states.Rigidbody.velocity) *
            //   states.gravityVariables.GravityForceMultiplier, ForceMode.Acceleration);

            //states.Rigidbody.AddForce(
            //    gravityService.GravityDirection *
            //    gravityService.GravityAmount *
            //    states.gravityVariables.GravityForceMultiplier, ForceMode.Acceleration);
        }
    }
}

