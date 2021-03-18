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
            float angle = Mathf.Deg2Rad * coneAngle;
            coneRadius = maxDistance * Mathf.Tan(angle);

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

            float angle = Mathf.Deg2Rad * coneAngle;
            coneRadius = maxDistance * Mathf.Tan(angle / 2f);

            Vector3 sphereLocation = transform.position + (transform.forward * maxDistance);
            Popcron.Gizmos.Sphere(sphereLocation, coneRadius, Color.yellow);
        }
    }

}
