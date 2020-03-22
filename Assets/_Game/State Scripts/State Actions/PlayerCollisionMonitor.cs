using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Player Collision Monitor")]
    public class PlayerCollisionMonitor : StateActions
    {
        private GravityVariables gravityVariables;
        private CollisionVariables collisionVariables;

        public Dictionary<string, RaycastHit> raycastHitDictionary;
        public Dictionary<string, bool> raycastFlagDictionary;

        public RaycastHit[] arrayHits;
        public bool[] arrayFlags;

        private Transform nonRelativeTransform;

        private RaycastHit hit;
        private bool hitFlag;

        public override void Execute_Start(PlayerStateManager states)
        {
            raycastHitDictionary = new Dictionary<string, RaycastHit>();
            raycastFlagDictionary = new Dictionary<string, bool>();

            gravityVariables = states.gravityVariables;
            collisionVariables = states.collisionVariables;
            nonRelativeTransform = states.RelativeTransform.GetNonRelativeTrans;
        }

        public override void Execute(PlayerStateManager states)
        {
            var origin = nonRelativeTransform.position;
            Vector3 originOffseted = origin;

            //if (states.gravityService.Direction == GravityDirections.DOWN)
            //    originOffseted = new Vector3(origin.x, origin.y + 1f, origin.z);
            //else if (states.gravityService.Direction == GravityDirections.UP)
            //    originOffseted = new Vector3(origin.x, origin.y - 1f, origin.z);
            //else if (states.gravityService.Direction == GravityDirections.LEFT)
            //    originOffseted = new Vector3(origin.x + 1f, origin.y, origin.z);
            //else if (states.gravityService.Direction == GravityDirections.RIGHT)
            //    originOffseted = new Vector3(origin.x - 1f, origin.y, origin.z);
            //else if (states.gravityService.Direction == GravityDirections.FRONT)
            //    originOffseted = new Vector3(origin.x, origin.y, origin.z - 1f);
            //else if (states.gravityService.Direction == GravityDirections.BACK)
            //    originOffseted = new Vector3(origin.x, origin.y, origin.z + 1f);

            float raySize = collisionVariables.RaySize * 10;

            //for now useless - refactor when we introduce global velocity change
            float velMagnitudeClamped = Mathf.Clamp01(states.Rigidbody.velocity.sqrMagnitude);

            if (velMagnitudeClamped > 0)
                raySize = collisionVariables.RaySize;
            else raySize = raySize / 2;

            Gizmos.Line(originOffseted, originOffseted + (nonRelativeTransform.forward * raySize), Color.green);
            Gizmos.Line(originOffseted, originOffseted + (nonRelativeTransform.forward * -raySize), Color.green);
            Gizmos.Line(originOffseted, originOffseted + (nonRelativeTransform.up * raySize), Color.green);
            Gizmos.Line(originOffseted, originOffseted + (nonRelativeTransform.up * -raySize), Color.green);
            Gizmos.Line(originOffseted, originOffseted + (nonRelativeTransform.right * raySize), Color.green);
            Gizmos.Line(originOffseted, originOffseted + (nonRelativeTransform.right * -raySize), Color.green);

            raycastHitDictionary.Clear();
            raycastFlagDictionary.Clear();

            //----------UP----------
            hitFlag = Physics.Raycast(
                originOffseted,
                nonRelativeTransform.up,
                out hit,
                raySize,
                Layers._ignoreLayersController);

            raycastHitDictionary.Add("UP", hit);
            raycastFlagDictionary.Add("UP", hitFlag);

            //----------DOWN----------
            hitFlag = Physics.Raycast(
                originOffseted,
                nonRelativeTransform.up * -1,
                out hit,
                raySize,
                Layers._ignoreLayersController);

            raycastHitDictionary.Add("DOWN", hit);
            raycastFlagDictionary.Add("DOWN", hitFlag);

            //----------LEFT----------
            hitFlag = Physics.Raycast(
                originOffseted,
                nonRelativeTransform.right * -1,
                out hit,
                raySize,
                Layers._ignoreLayersController);
            raycastHitDictionary.Add("LEFT", hit);
            raycastFlagDictionary.Add("LEFT", hitFlag);

            //----------RIGHT----------
            hitFlag = Physics.Raycast(
                originOffseted,
                nonRelativeTransform.right,
                out hit,
                raySize,
                Layers._ignoreLayersController);

            raycastHitDictionary.Add("RIGHT", hit);
            raycastFlagDictionary.Add("RIGHT", hitFlag);

            //----------FORWARD----------
            hitFlag = Physics.Raycast(
                originOffseted,
                nonRelativeTransform.forward,
                out hit,
                raySize,
                Layers._ignoreLayersController);

            raycastHitDictionary.Add("FORWARD", hit);
            raycastFlagDictionary.Add("FORWARD", hitFlag);

            //----------BACK----------
            hitFlag = Physics.Raycast(
                originOffseted,
                nonRelativeTransform.forward * -1,
                out hit,
                raySize,
                Layers._ignoreLayersController);

            raycastHitDictionary.Add("BACK", hit);
            raycastFlagDictionary.Add("BACK", hitFlag);

            collisionVariables.RaycastHitDictionary = raycastHitDictionary;
            collisionVariables.RaycastFlagDictionary = raycastFlagDictionary;

            collisionVariables.ArrayHits = raycastHitDictionary.Values.ToArray();
            collisionVariables.ArrayFlags = raycastFlagDictionary.Values.ToArray();
        }
    }
}

