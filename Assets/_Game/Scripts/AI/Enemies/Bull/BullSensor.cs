using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.AI
{
    public class BullSensor : AIBaseSensor
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

        protected Collider[] m_colliderBuffer = new Collider[10];
        protected List<Transform> m_targetsInRange = new List<Transform>(10);

        [Header("Patrol Settings")]

        [SerializeField]
        [Tooltip("This is how far away we will detect the player or NPCs")]
        protected float m_sensorRange = 3f;
        public float SensorRange
        {
            get { return m_sensorRange; }
        }

        [SerializeField]
        [Tooltip("This is how far the cutoff will be once the player has already entered the sensor range")]
        protected float m_sensorCutoffRange = 20f;
        public float SensorCutoffRange
        {
            get { return m_sensorCutoffRange; }
        }

        [SerializeField]
        [Tooltip("This is how often we will check for new targets around us if we are patrolling.")]
        protected float m_sensorTimestep = 0.25f;
        public float SensorTimeStep
        {
            get { return m_sensorTimestep; }
        }

        public virtual Transform GetTargetWithinReach(float distance)
        {
            m_targetsInRange.Clear();
            // First check if we have any players in range
            var players = Physics.OverlapSphereNonAlloc(transform.position, distance, m_colliderBuffer, HNDAI.Settings.PlayerLayer);
            if (players > 0)
            {
                if (players == 1)
                {
                    return m_colliderBuffer[0].transform;
                }
            }
            return null;
        }
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

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, SensorRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, SensorCutoffRange);
        }
#endif
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
