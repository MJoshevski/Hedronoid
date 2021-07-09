using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;
using Hedronoid.Health;
using Hedronoid.Events;
using static Hedronoid.Health.HealthBase;
using Hedronoid.Weapons;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    [RequireComponent(typeof(BullDash))]
    [RequireComponent(typeof(BullSensor))]
    [RequireComponent(typeof(NavMeshAgent))]

    public class BullNavigation : AIBaseNavigation
    {
        public enum EGruntStates
        {
            DashToTarget = EStates.Highest + 1,
        }

        [SerializeField]
        protected float m_dashDistance = 8f;

        public float DashDistance
        {
            get { return m_dashDistance; }
        }

        protected int nextWaypoint;

        protected EnemyEmojis enemyEmojis;

        protected Vector3 lastEvaluationPosition;

        protected bool dashed = false;

        [SerializeField]
        public float m_physicsCullRange = 30.0f;
        protected Transform[] m_playerTx = new Transform[2] { null, null };
        protected BullSensor m_BullSensor;
        protected BullDash m_BullDash;
        protected DamageHandler m_damageHandler;
        public bool m_GruntFreeze;

        protected DamageInfo damage;
        protected float remainingSensorTime;
        protected float m_targetEvaluationDistance = 3f;

        protected override void Awake()
        {
            base.Awake();

            m_BullSensor = (BullSensor) m_Sensor;
            m_BullDash = GetComponent<BullDash>();
            m_damageHandler = GetComponent<DamageHandler>();
            CreateState(EGruntStates.DashToTarget, OnDashUpdate, null, null);

            enemyEmojis = GetComponent<EnemyEmojis>();
            HNDEvents.Instance.AddListener<KillEvent>(OnKilled);

            agent.updateRotation = true;
            agent.updateUpAxis = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
            {
                damage = new DamageInfo();
                damage.sender = Target.gameObject;
                damage.Damage = 1;

                m_damageHandler.DoDamage(damage);
            }
        }

        private void OnKilled(KillEvent e)
        {
            if (e.GOID != gameObject.GetInstanceID()) return;
            StopAllCoroutines();
            this.enabled = false;
        }

        protected override void Start()
        {
            base.Start();

            ChangeState(EStates.DefaultMovement);
        }

        public override Vector3 GetDirection()
        {
            // This is not actually being used in the current code, but if someone wants it they will get the last direction moved.
            return transform.forward;
        }


        public override void ChangeState(System.Enum newState)
        {
            if (m_BullDash.DashInProgress) return;

            base.ChangeState(newState);

            if (IsInState(EStates.DefaultMovement))
            {
                // Pick a random waypoint and set that as the target
                nextWaypoint = UnityEngine.Random.Range(0, waypoints.Length - 1);
            }
            if (IsInState(EGruntStates.DashToTarget))
            {
                if (agent)
                {
                    agent.isStopped = true;
                    agent.updateRotation = false;
                    //agent.enabled = false;
                }

                if (m_Motor is BullDash)
                {
                    (m_Motor as BullDash).DoDash(m_Target);
                }
            }
        }
        public override void OnDefaultMovementUpdate()
        {
            if (m_isFrozen) return;

            // Decrease sensor time and check the sensor if nessecary
            // Note that we will not check for this is the agent is not on the NavMesh - i.e. in the air or somewhere else.
            if ((remainingSensorTime -= Time.deltaTime) <= 0 && agent.isOnNavMesh)
            {
                ChangeTarget();
            }

            // Check if we are close to the waypoint that we are moving for
            if (agent.isOnNavMesh && agent.remainingDistance < 1f)
            {
                nextWaypoint = (nextWaypoint + 1) % waypoints.Length;
                SetAgentDestination(waypoints[nextWaypoint].position);
            }
        }
        public override void OnReturnToDefaultUpdate()
        {
        }

        private IEnumerator WaitImpactDashDone()
        {
            while (true)
            {
                if (m_OnImpact)
                {
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    if (m_Target)
                    {
                        if (m_Target.gameObject.layer == LayerMask.NameToLayer("Ghost"))
                            m_Target = null;
                    }

                    if (agent)
                    {
                        agent.isStopped = false;
                        //agent.enabled = true;
                        agent.updateRotation = true;
                    }

                    if (m_Target)
                    {
                        // After the dash we will check if the player is still within range.
                        // If not we will lose him as a target.
                        var distanceToTaget = Vector3.Distance(transform.position, m_Target.position);
                        if (distanceToTaget > m_BullSensor.SensorRange)
                        {
                            m_Target = null;
                            ChangeState(EStates.DefaultMovement);
                        }
                        else if (!m_BullDash.DashInProgress)
                        {
                            SetAgentDestination(m_Target.position);
                            ChangeState(EStates.GoToTarget);
                        }
                    }
                    else
                    {
                        m_Target = null;
                        ChangeState(EStates.DefaultMovement);
                    }
                    yield break;
                }
            }
        }

        public void DashDone()
        {
            dashed = false;
            StartCoroutine(WaitImpactDashDone());
        }

        public void PointUpDone()
        {
            ChangeState(EStates.DefaultMovement);
        }

        public void OnDashUpdate()
        {
            // We are not doing anything here. It got started when we changed state.
        }

        public void OnSpearUpdate()
        {
            // We are not doing anything here. It got started when we changed state.
        }

        public override void OnFleeFromTargetUpdate()
        {
        }

        private Vector3 targetDirection;
        public override bool SetAgentDestination(Vector3 destination)
        {
            if (!agent.isOnNavMesh)
            {
                return false;
            }

            // First make sure that the destination is grounded
            var groundDetectStart = new Vector3(destination.x, destination.y + 1f, destination.z);
            RaycastHit rh;
            if (Physics.Raycast(groundDetectStart, -m_Rb.transform.up, out rh, GroundDetectDistance * 2, HNDAI.Settings.GroundLayer))
            {
                NavMeshHit nmh;
                NavMesh.SamplePosition(rh.point, out nmh, 3f, NavMesh.AllAreas);
                if (nmh.hit)
                {
                    agent.destination = nmh.position;
                    return true;
                }
            }
            return false;
        }

        public override void OnGoToTargetUpdate()
        {
            if (m_isFrozen) return;

            if (m_Target)
            {
                var distanceToTarget = Vector3.Distance(transform.position, m_Target.position);

                // If we are within dash distance, change to the dash state
                if (distanceToTarget <= m_dashDistance)
                {
                    if (!dashed)
                    {
                        ChangeState(EGruntStates.DashToTarget);
                        dashed = true;
                        return;
                    }
                }

                if (/*distanceToTarget > m_BullSensor.SensorCutoffRange ||*/
                    distanceToTarget > m_BullSensor.MaxConeDistance)
                {
                    // We can no longer see the target. Pick a waypoint
                    ChangeState(EStates.DefaultMovement);
                    return;
                }

                bool setAgentDestination = false;
                if (!agent.hasPath)
                {
                    setAgentDestination = true;
                }

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

        Transform newTarget;
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            newTarget = m_BullSensor.GetTargetInsideCone(m_Rb.transform.forward);

            //if (!newTarget)
            //    newTarget = m_BullSensor.GetTargetWithinReach(m_BullSensor.SensorRange);

            if (newTarget)
            {
                m_Target = newTarget.transform;
            }
        }


        public override void ChangeTarget()
        {
            if (newTarget)
            {
                m_Target = newTarget.transform;
                if (enemyEmojis)
                {
                    enemyEmojis.ChangeTarget(m_Target.gameObject);
                }
                lastEvaluationPosition = m_Target.position;
                if (!m_BullDash.DashInProgress)
                {
                    ChangeState(EStates.GoToTarget);
                    return;
                }
            }
            else
            {
                remainingSensorTime = m_BullSensor.SensorTimeStep;
            }
        }

        public override Vector3 CreateRandomWaypoint(Vector3 lastPos, float minRange, float maxRange)
        {
            Vector3 upAxis = m_Rb.transform.up;
            Vector3 forwardAxis = m_Rb.transform.forward;

            Vector3 newWayPointDirection = forwardAxis * UnityEngine.Random.Range(minRange, maxRange);
            Vector3 newWayPoint = lastPos + newWayPointDirection;

            bool found = false;
            int searchCount = 50;
            while (!found)
            {
                newWayPointDirection =
                    Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), upAxis) * newWayPointDirection;
                newWayPoint = lastPos + newWayPointDirection;

                Vector3 groundDetectStart = new Vector3(newWayPoint.x, newWayPoint.y + 0.2f, newWayPoint.z);

                RaycastHit rh;

                if (Physics.Raycast(
                    groundDetectStart,
                    -upAxis,
                    out rh,
                    GroundDetectDistance * 2,
                    HNDAI.Settings.GroundLayer))
                {
                    NavMeshHit nmh;
                    NavMesh.SamplePosition(rh.point, out nmh, 1f, NavMesh.AllAreas);
                    if (nmh.hit)
                    {
                        //agent.destination = nmh.position;
                        newWayPoint = nmh.position + upAxis * 0.2f;
                        found = true;
                    }
                }
                if (searchCount <= 0)
                {
                    return lastPos;
                }
                searchCount--;
            }
            return newWayPoint;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // if (!HNDAI.Settings.DrawGizmos)
            // {
            //     return;
            // }

            if (agent)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(agent.destination, 1f);
                Gizmos.color = (agent.hasPath) ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position, agent.destination);
            }

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