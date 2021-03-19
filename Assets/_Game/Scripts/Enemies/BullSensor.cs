using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.AI
{
    public class BullSensor : GruntSensor
    {
        [SerializeField]
        protected float maxDistance = 35f;
        [SerializeField]
        protected float coneAngle = 15f;

        private Physics physics;
        private float coneRadius;
        public Transform GetTargetInsideCone(Vector3 direction)
        {
            coneRadius = Mathf.Tan((coneAngle / 2f * 0.5f * Mathf.Deg2Rad)) * maxDistance;

            // First check if we have any players in range
            RaycastHit[] players = physics.ConeCastNonAlloc(transform.position, coneRadius, direction, m_colliderBuffer.Length, maxDistance, coneAngle);

            if (players.Length > 0)
            {
                if (players.Length == 1)
                {
                    return players[0].collider.transform;
                }
            }
            return null;
        }

        private void FixedUpdate()
        {
            Popcron.Gizmos.Enabled = true;

            Popcron.Gizmos.Cone(
                transform.position,
                transform.rotation,
                maxDistance,
                coneAngle,
                Color.yellow);
        }
    }

}
