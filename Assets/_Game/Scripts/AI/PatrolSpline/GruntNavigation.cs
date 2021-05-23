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
    // Grunts move 
    [RequireComponent(typeof(GruntDash))]
    [RequireComponent(typeof(GruntSensor))]
    [RequireComponent(typeof(NavMeshAgent))]

    public class GruntNavigation : AIBaseNavigation
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
        protected GruntSensor m_GruntSensor;
        protected GruntDash m_GruntDash;
        protected DamageHandler m_damageHandler;
        public bool m_GruntFreeze;

        protected DamageInfo damage;
        protected float remainingSensorTime;
        protected float m_targetEvaluationDistance = 3f;

        protected override void Awake()
        {
            base.Awake();

            m_GruntSensor = (GruntSensor) m_Sensor;
            m_GruntDash = GetComponent<GruntDash>();
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
            if (m_GruntDash.DashInProgress) return;

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

                if (m_Motor is GruntDash)
                {
                    (m_Motor as GruntDash).DoDash(m_Target);
                }
            }
        }

        public override void ChangeTarget()
        {
            base.ChangeTarget();
            var newTarget = m_GruntSensor.GetTargetWithinReach(m_GruntSensor.SensorRange);

            if (newTarget)
            {
                m_Target = newTarget;
                if (enemyEmojis)
                {
                    enemyEmojis.ChangeTarget(m_Target.gameObject);
                }
                lastEvaluationPosition = m_Target.position;
                if (!m_GruntDash.DashInProgress)
                {
                    ChangeState(EStates.GoToTarget);
                    return;
                }
            }
            else
            {
                remainingSensorTime = m_GruntSensor.SensorTimeStep;
            }
        }

        public override void ChangeTarget(Transform newTarget)
        {
            base.ChangeTarget();

            if (newTarget)
            {
                m_DefaultTarget = newTarget;
                m_Target = newTarget;
                lastEvaluationPosition = m_Target.position;
                ChangeState(EStates.GoToTarget);
                return;
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

        public override void OnGoToTargetUpdate()
        {
            if (m_isFrozen) return;

            if (m_Target)
            {
                var distanceToTaget = Vector3.Distance(transform.position, m_Target.position);

                // If we are within dash distance, change to the dash state
                if (distanceToTaget <= m_dashDistance)
                {
                    if (!dashed)
                    {
                        ChangeState(EGruntStates.DashToTarget);
                        dashed = true;
                        return;
                    }
                }

                if (distanceToTaget > m_GruntSensor.SensorRange)
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
                        if (distanceToTaget > m_GruntSensor.SensorRange)
                        {
                            m_Target = null;
                            ChangeState(EStates.DefaultMovement);
                        }
                        else if (!m_GruntDash.DashInProgress)
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