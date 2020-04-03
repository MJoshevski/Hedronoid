using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Player Gravity Applier")]
    public class PlayerGravityApplier : StateActions
    {

        public override void Execute_Start(PlayerStateManager states)
        {
        }

        public override void Execute(PlayerStateManager states)
        {
            var gravityService = GravityService.Instance;

            states.Rigidbody.AddForce(
                gravityService.GravityDirection *
                gravityService.Gravity *
                states.gravityVariables.GravityForceMultiplier);;
        }
    }
}

