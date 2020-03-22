using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Is Grounded")]
    public class IsGrounded : StateActions
    {
        public float _rayOnGroundDistance = 1.4f;
        public float _rayInAirDistance = 1f;
        public float _rayOriginOffset = 0.7f;
        public float _sphereCastRadius = 0.3f;

        public override void Execute_Start(PlayerStateManager states)
        {
        }

        public override void Execute(PlayerStateManager states)
        {
            Vector3 origin = states.RelativeTransform.GetNonRelativeTrans.relativePosition();
            //origin.y = (Mathf.Abs(origin.y) + _rayOriginOffset) * states.Transform.NonRelativeTransform.relYDir();
            //Debug.LogErrorFormat("X: {0} - Y: {1} - Z: {2}", states.RelativeTransform.GetNonRelativeTrans.relX(), states.RelativeTransform.GetNonRelativeTrans.relY(), states.RelativeTransform.GetNonRelativeTrans.relZ());
            //Debug.LogErrorFormat("XDir: {0} - YDir: {1} - ZDir: {2}", states.RelativeTransform.GetNonRelativeTrans.relXDir(), states.RelativeTransform.GetNonRelativeTrans.relYDir(), states.RelativeTransform.GetNonRelativeTrans.relZDir());
            //if (states.gravityService.Direction == GravityDirections.DOWN)
            //    origin.y += _rayOriginOffset;
            //else if (states.gravityService.Direction == GravityDirections.UP)
            //    origin.y -= _rayOriginOffset;
            //else if (states.gravityService.Direction == GravityDirections.LEFT)
            //    origin.x += _rayOriginOffset;
            //else if (states.gravityService.Direction == GravityDirections.RIGHT)
            //    origin.x -= _rayOriginOffset;
            //else if (states.gravityService.Direction == GravityDirections.FRONT)
            //    origin.z -= _rayOriginOffset;
            //else if (states.gravityService.Direction == GravityDirections.BACK)
            //    origin.z += _rayOriginOffset;

            Vector3 dir = -states.gravityService.GravityUp;

            float dis = _rayOnGroundDistance;

            if (!states.isGrounded)
                dis = _rayInAirDistance;

            RaycastHit hit;

            Debug.DrawRay(origin, dir * dis, Color.red);

            states.isGrounded = Physics.Raycast(
                origin,
                dir,
                out hit,
                dis,
                Layers._ignoreLayersController);
     
        }
    }
}
