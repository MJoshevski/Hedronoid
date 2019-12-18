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

        public override void Execute(StateManager states)
        {
            Vector3 origin = states.Transform.position;

            origin.y += _rayOriginOffset;
            Vector3 dir = -Vector3.up;

            float dis = _rayOnGroundDistance;

            if (!states.isGrounded)
                dis = _rayInAirDistance;

            RaycastHit hit;

            //if(Physics.Raycast(origin, dir, out hit, _rayDistance))
            Debug.DrawRay(origin, dir * dis, Color.red);
            if (Physics.SphereCast(origin, _sphereCastRadius, dir, out hit, dis,
                Layers._ignoreLayersController))
            {
                states.isGrounded = true;
            }
            else
            {
                states.isGrounded = false;
            }        
        }
    }
}
