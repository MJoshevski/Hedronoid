using Hedronoid.AI;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    public static class PhysicsExtensions
    {
        public static RaycastHit[] ConeCastAll(this Physics physics, Vector3 origin, float maxRadius, Vector3 direction, float maxDistance, float coneAngle)
        {
            RaycastHit[] sphereCastHits = Physics.SphereCastAll(
                origin - (direction * maxRadius),
                maxRadius,
                direction,
                maxDistance,
                HNDAI.Settings.PlayerLayer,
                QueryTriggerInteraction.Ignore);

            List<RaycastHit> coneCastHitList = new List<RaycastHit>();

            if (sphereCastHits.Length > 0)
            {
                for (int i = 0; i < sphereCastHits.Length; i++)
                {
                    Vector3 hitPoint = sphereCastHits[i].point;
                    Vector3 directionToHit = hitPoint - origin;
                    float angleToHit = Vector3.Angle(direction, directionToHit);

                    if (angleToHit < coneAngle)
                    {
                        coneCastHitList.Add(sphereCastHits[i]);
                    }
                }
            }

            RaycastHit[] coneCastHits = new RaycastHit[coneCastHitList.Count];
            coneCastHits = coneCastHitList.ToArray();

            return coneCastHits;
        }

        public static RaycastHit[] ConeCastNonAlloc(this Physics physics, Vector3 origin, float maxRadius, Vector3 direction, int hitBufferCount, float maxDistance, LayerMask layerMask, float coneAngle)
        {
            RaycastHit[] sphereCastHits = new RaycastHit[hitBufferCount];

            Physics.SphereCastNonAlloc(
                origin - (direction * maxRadius), 
                maxRadius, 
                direction, 
                sphereCastHits, 
                maxDistance,
                layerMask, 
                QueryTriggerInteraction.Ignore);

            List<RaycastHit> coneCastHitList = new List<RaycastHit>();

            if (sphereCastHits.Length > 0)
            {
                for (int i = 0; i < sphereCastHits.Length; i++)
                {
                    Vector3 hitPoint = sphereCastHits[i].point;
                    Vector3 directionToHit = hitPoint - origin;
                    float angleToHit = Vector3.Angle(direction, directionToHit);

                    if (angleToHit <= coneAngle)
                    {
                        coneCastHitList.Add(sphereCastHits[i]);
                    }
                }
            }

            RaycastHit[] coneCastHits = new RaycastHit[coneCastHitList.Count];
            coneCastHits = coneCastHitList.ToArray();

            return coneCastHits;
        }
    }
}