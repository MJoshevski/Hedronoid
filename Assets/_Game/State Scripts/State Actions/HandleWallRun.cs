using UnityEngine;
using System.Collections;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Handle Wall-Run")]
    public class HandleWallRun : StateActions
    {
        [Header("DebugView")]
        [SerializeField] float _runCounter = 0f;
        [SerializeField] bool _wallRunning;

        IGravityService gravityService;
        WallRunVariables wallRunVariables;

        public override void Execute_Start(PlayerStateManager states)
        {
            gravityService = GravityService.Instance;
            wallRunVariables = states.wallRunVariables;
        }

        public override void Execute(PlayerStateManager states)
        {
            _runCounter = wallRunVariables.Duration;
            _runCounter -= states.delta;

            if (_runCounter > 0f && wallRunVariables.WallRunning)
            {
                Debug.LogError("RUNNING - HANDLE");
                //Applying negative gravity
                states.
                    Rigidbody.
                    ApplyForce(
                    gravityService.GravityUp * 
                    gravityService.Gravity *
                    wallRunVariables.GravityNegateMultiplier);
            }
        }
    }
}

