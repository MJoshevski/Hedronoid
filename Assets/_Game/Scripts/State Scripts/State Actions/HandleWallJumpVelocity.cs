using UnityEngine;
using System.Collections;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Handle Wall Jump Velocity")]
    public class HandleWallJumpVelocity : StateActions
    {

        private bool _wallRunning;
        private CollisionVariables collisionVariables;
        private WallRunVariables wallRunVariables;
        private WallJumpVariables wallJumpVariables;
        private Vector3 _collisionNormal;

        public override void Execute_Start(PlayerStateManager states)
        {
            collisionVariables = states.collisionVariables;
            wallRunVariables = states.wallRunVariables;
            wallJumpVariables = states.wallJumpVariables;
        }

        public override void Execute(PlayerStateManager states)
        {
            _collisionNormal = GetCollisionNormal();

            var forceDirection = Quaternion.FromToRotation(
                states.Transform.forward, _collisionNormal)
                * GravityService.Instance.GravityRotation
                * wallJumpVariables.WallJumpForce.Direction;

            forceDirection.Normalize();

            states.StartCoroutine(
                states.Rigidbody.ApplyForceContinuously(
                    forceDirection,
                    wallJumpVariables.WallJumpForce));
        }

        private Vector3 GetCollisionNormal()
        {
            var arrayFlags = collisionVariables.ArrayFlags;
            var arrayHits = collisionVariables.ArrayHits;

            for (int i = 2; i < arrayFlags.Length; i++)
            {
                if (arrayFlags[i])
                {
                    _collisionNormal = arrayHits[i].normal;
                    var dot = Vector3.Dot(GravityService.Instance.GravityUp, _collisionNormal);

                    if (Mathf.Approximately(0f, dot))
                        return _collisionNormal;
                }
            }
            return Vector3.zero;
        }
    }
}

