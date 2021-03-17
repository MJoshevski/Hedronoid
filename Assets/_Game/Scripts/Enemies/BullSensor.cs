using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.AI
{
    public class BullSensor : GruntSensor
    {
        private Physics physics;
        private float m_maxRadius, m_maxDistance, m_coneAngle;
        private Vector3 m_direction;

        public Transform GetTargetInsideCone(float maxRadius, Vector3 direction, float maxDistance, float coneAngle)
        {
            m_maxRadius = maxRadius;
            m_maxDistance = maxDistance;
            m_coneAngle = coneAngle;
            m_direction = direction;

            // First check if we have any players in range
            RaycastHit[] players = physics.ConeCastNonAlloc(transform.position, maxRadius, direction, m_colliderBuffer.Length, maxDistance, coneAngle);

            if (players.Length > 0)
            {
                if (players.Length == 1)
                {
                    return players[0].collider.gameObject.transform;
                }
                //if (GetModifiedAggroValue(m_colliderBuffer[0].gameObject) > GetModifiedAggroValue(m_colliderBuffer[1].gameObject))
                //{
                //    return m_colliderBuffer[0].transform;
                //}
                //else
                //{
                //    return m_colliderBuffer[1].transform;
                //}
            }
            return null;
        }

        private void FixedUpdate()
        {
            Popcron.Gizmos.Enabled = true;
            Popcron.Gizmos.Cone(
                transform.position,
                transform.rotation,
                m_maxDistance,
                m_coneAngle,
                Color.yellow);
        }
    }

}
