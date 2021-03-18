using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.AI
{
    public class BullSensor : GruntSensor
    {
        [SerializeField]
        protected float maxRadius = 10f;
        [SerializeField]
        protected float maxDistance = 35f;
        [SerializeField]
        protected float coneAngle = 15f;

        private Physics physics;

        public Transform GetTargetInsideCone(Vector3 direction)
        {
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
                maxDistance,
                coneAngle,
                Color.yellow);
        }
    }

}
