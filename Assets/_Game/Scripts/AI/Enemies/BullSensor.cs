using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.AI
{
    public class BullSensor : GruntSensor
    {
        [SerializeField]
        protected float maxDistance = 35f;
        public float MaxConeDistance { get { return maxDistance; } }
        [SerializeField]
        [Range(1f, 90f)]
        protected float coneAngle = 15f;

        private Physics physics;
        [SerializeField]
        private float coneRadius;

        public Transform GetTargetInsideCone(Vector3 direction)
        {
            coneRadius = maxDistance * Mathf.Abs(Mathf.Tan(coneAngle * Mathf.Deg2Rad));

            // First check if we have any players in the cone
            RaycastHit[] players = physics.ConeCastNonAlloc(
                transform.position, 
                coneRadius, 
                direction, 
                m_colliderBuffer.Length, 
                maxDistance, coneAngle);

            if (players.Length > 0)
            {
                if (players.Length == 1)
                {
                    return players[0].collider.transform;
                }
            }
            return null;
        }

        private void Update()
        {
            Popcron.Gizmos.Cone(
                transform.position,
                transform.rotation,
                maxDistance,
                coneAngle,
                Color.yellow);
        }
    }

}
