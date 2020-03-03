using UnityEngine;
using System.Collections;

namespace Hedronoid
{
    [CreateAssetMenu(menuName ="Actions/State Actions/Handle Wall-Run")]
    public class HandleWallRun : StateActions
    {
        [Header("DebugView")]
        [SerializeField] float _runCounter = 0f;
        [SerializeField] bool _wallRunning;

        IGravityService gravityService;
        WallRunVariables wallRunVariables;

        public bool WallRunning { get { return _wallRunning; } private set { _wallRunning = value; } }

        public override void Execute_Start(PlayerStateManager states)
        {
            gravityService = GravityService.Instance;
            wallRunVariables = states.wallRunVariables;
        }

        public override void Execute(PlayerStateManager states)
        {
            if (WallRunning)
            {
                _runCounter -= Time.fixedDeltaTime;

                if (_runCounter > 0f)
                {
                    //Applying negative gravity
                    states.Rigidbody.ApplyForce(
                        gravityService.GravityUp *
                        gravityService.Gravity *
                        wallRunVariables.GravityNegateMultiplier);
                }
            }
        }

        void CheckIfWallRunning(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                var contact = collision.contacts[i];
                var dot = Vector3.Dot(GravityService.Instance.GravityUp, contact.normal);
                if (Mathf.Approximately(0f, dot))
                {
                    WallRunning = true;
                }
            }
        }
    }
}
