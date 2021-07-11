using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System;
using System.Collections;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    // Leaders move using a NavMeshAgent instead of a Motor, as this gives the most reliable movement.
    public class RangedNavigation : AIBaseNavigation
    {
        public enum ERangedStates
        {
            ShootAtPlayer = EStates.Highest + 1,
        }

        [Header("Patrol Settings")]
        [SerializeField]
        [Tooltip("This is how far away we will detect the player or NPCs.")]
        private float m_sensorRange = 3f;
        [SerializeField]
        private float m_shootDistance = 30f;

        public float ShootDistance
        {
            get { return m_shootDistance; }
            set { m_shootDistance = value; }
        }

        [SerializeField]
        [Tooltip("This is how often we will check for new targets around us if we are patrolling.")]
        private float m_sensorTimestep = 0.25f;

        [SerializeField]
        private LayerMask m_groundLayer;

        private float m_targetEvaluationDistance = 35f;

        private int nextWaypoint;
        private float remainingSensorTime;
        private EnemyEmojis enemyEmojis;
        private float m_tempShootDistance;

        private Vector3 lastEvaluationPosition;

        protected override void Awake()
        {
            base.Awake();

            CreateState(ERangedStates.ShootAtPlayer, OnShootUpdate, null, null);

            enemyEmojis = GetComponent<EnemyEmojis>();
        }

        public void OnShootUpdate()
        {
            // We are not doing anything here. It got started when we changed state.
        }

        protected override void Start()
        {
            base.Start();
            m_tempShootDistance = m_shootDistance;


            ChangeState(EStates.DefaultMovement);
        }

        public override Vector3 GetDirection()
        {
            // This is not actually being used in the current code, but if someone wants it they will get the last direction moved.
            return transform.forward;
        }

        public override void ChangeState(System.Enum newState)
        {
            base.ChangeState(newState);

            if (IsInState(EStates.DefaultMovement))
            {
                // TOOD: Right now this enemy is using the same waypoint system as the Grunt. IF we do not want that we should do changes here.
                // Pick a random waypoint and set that as the target
                nextWaypoint = UnityEngine.Random.Range(0, waypoints.Length - 1);
                SetAgentDestination(waypoints[nextWaypoint].position);
                remainingSensorTime = UnityEngine.Random.Range(0, m_sensorTimestep);
            }
            if (IsInState(EStates.GoToTarget))
            {
                // agent.enabled = false;
            }
            if (IsInState(ERangedStates.ShootAtPlayer))
            {
                if (m_Motor is RangedShoot)
                {
                    (m_Motor as RangedShoot).DoShoot(m_Target);
                }
            }
        }

        public void ShotDone()
        {
            D.AILog("RangedNavigation ShotDone()");

            // After the shooting we will check if the player is still within range.
            // If not we will lose him as a target.
            if (m_Target && m_Target.gameObject.layer == LayerMask.NameToLayer("Ghost"))
                m_Target = null;

            if (!m_Target)
            {
                ChangeState(EStates.DefaultMovement);
                return;
            }
            var distanceToTaget = Vector3.Distance(transform.position, m_Target.position);
            if (distanceToTaget > m_sensorRange)
            {
                //m_Target = null;
                ChangeState(EStates.DefaultMovement);
                return;
            }
            else
            {
                SetAgentDestination(m_Target.position);
                ChangeState(EStates.GoToTarget);
            }
        }

        public override void ChangeTarget()
        {
            base.ChangeTarget();
            var newTarget = (m_Sensor as DetectPlayerSensor).GetTargetWithinReach(m_sensorRange);
            if (newTarget)
            {
                m_Target = newTarget;
                if (enemyEmojis)
                {
                    enemyEmojis.ChangeTarget(m_Target.gameObject);
                }
                lastEvaluationPosition = m_Target.position;
                ChangeState(EStates.GoToTarget);
                return;
            }
            else
            {
                remainingSensorTime = m_sensorTimestep;
            }
        }

        public override void OnDefaultMovementUpdate()
        {
            // TOOD: Right now this enemy is using the same waypoint system as the Grunt. IF we do not want that we should do changes here.
            // Decrease sensor time and check the sensor if nessecary
            if ((remainingSensorTime -= Time.deltaTime) <= 0)
            {
                ChangeTarget();
            }

            // Check if we are close to the waypoint that we are moving for
            //if (agent.isOnNavMesh && agent.remainingDistance < 1f)
            //{
            //    nextWaypoint = (nextWaypoint + 1) % waypoints.Length;
            //    SetAgentDestination(waypoints[nextWaypoint].position);
            //}
        }

        public override void OnGoToTargetUpdate()
        {
            // TODO: GoToTarget for this enemy is the same as "the player is in range - throw shit".
            // We should use the cooldown for spacing throws.

            if (m_Target)
            {
                var distanceToTaget = Vector3.Distance(transform.position, m_Target.position);

                //If the distance is smaller than something maybe we should flee? /OL note to self

                if (distanceToTaget < m_shootDistance)
                {
                    ChangeState(ERangedStates.ShootAtPlayer);
                    return;
                }

                if (distanceToTaget > m_sensorRange)
                {
                    // We can no longer see the target. Go back to moving.
                    ChangeState(EStates.DefaultMovement);
                    return;
                }

                bool setAgentDestination = false;

                // If the target has moved too far from where it originally was, update the destination
                if (Vector3.Distance(lastEvaluationPosition, m_Target.position) >= m_targetEvaluationDistance)
                {
                    setAgentDestination = true;
                }
                if (setAgentDestination)
                {
                    SetAgentDestination(m_Target.position);
                }
            }
            else
            {
                ChangeState(EStates.DefaultMovement);
            }
        }

        public override void OnReturnToDefaultUpdate()
        {
        }

        public override void OnFleeFromTargetUpdate()
        {
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            //if (agent)
            //{
            //    Gizmos.color = Color.yellow;
            //    Gizmos.DrawSphere(agent.destination, 1f);
            //    Gizmos.color = (agent.hasPath) ? Color.green : Color.red;
            //    Gizmos.DrawLine(transform.position, agent.destination);
            //}

            if (DefaultTarget && DefaultTarget.childCount > 1)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < DefaultTarget.childCount; i++)
                {
                    var waypoint = DefaultTarget.GetChild(i);
                    Gizmos.DrawWireSphere(waypoint.position, 0.5f);
                    if (i > 0)
                    {
                        Gizmos.DrawLine(DefaultTarget.GetChild(i - 1).position, waypoint.position);
                    }
                }
                Gizmos.DrawLine(DefaultTarget.GetChild(DefaultTarget.childCount - 1).position, DefaultTarget.GetChild(0).position);
            }
        }
#endif

    }
}