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

        [Header("Patrol Settings")]
        [SerializeField]
        [Tooltip("This is how far away we will detect the player or NPCs")]
        protected float m_sensorRange = 3f;
        [SerializeField]
        [Tooltip("This is how far the cutoff will be once the player has already entered the sensor range")]
        protected float m_sensorCutoffRange = 20f;
        [SerializeField]
        protected float m_dashDistance = 8f;

        public float DashDistance
        {
            get { return m_dashDistance; }
        }

        public float SensorCutoffRange
        {
            get { return m_sensorCutoffRange; }
        }

        [SerializeField]
        [Tooltip("This is how often we will check for new targets around us if we are patrolling.")]
        protected float m_sensorTimestep = 0.25f;

        protected float m_targetEvaluationDistance = 3f;

        protected int nextWaypoint;
        protected float remainingSensorTime;
        protected EnemyEmojis enemyEmojis;

        protected Vector3 lastEvaluationPosition;

        protected bool dashed = false;

        [SerializeField]
        public float m_physicsCullRange = 30.0f;
        protected Camera[] m_playerCameras = new Camera[2] { null, null };
        protected Transform[] m_playerTx = new Transform[2] { null, null };
        protected GruntDash m_GruntDash;
        protected DamageHandler m_damageHandler;
        public bool m_GruntFreeze;

        protected DamageInfo damage;

        protected override void Awake()
        {
            base.Awake();
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
            var newTarget = (m_Sensor as BlockheadSensor).GetTargetWithinReach(m_sensorRange);

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
                remainingSensorTime = m_sensorTimestep;
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

        protected override void Update()
        {
            m_GruntFreeze = m_isFrozen;

            // TODO : disable when is not visible 
            // //Get player cameras
            // if (m_playerCameras[0] == null )
            // {
            //   RollPlayingGame.Characters.CharacterBase player = PlayerManager.Instance.GetPlayer(0);

            //   if( player )
            //   {
            //     m_playerTx[0]  = player.transform;
            //     m_playerCameras[0] = player.PlayerCamera.Camera;

            //     player = PlayerManager.Instance.GetPlayer(1);
            //     m_playerTx[1]  = player.transform;
            //     m_playerCameras[1] = player.PlayerCamera.Camera;              
            //   }              
            // }
            // else
            // {
            //   bool visible = System.Math.Min( Vector3.Distance( m_playerTx[0].position,transform.position),Vector3.Distance( m_playerTx[1].position,transform.position) ) < m_physicsCullRange || 
            //                  Vector3.Magnitude( m_rigidBody.velocity ) > 1.0f; 
            //   if( !visible )
            //   {
            //     //If it's not in range, check if a camera is seeing it
            //     Vector3 vp0 = m_playerCameras[0].WorldToViewportPoint(transform.position);
            //     Vector3 vp1 = m_playerCameras[1].WorldToViewportPoint(transform.position);
            //     visible = ( ( vp0.x > 0.0 && vp0.x < 1.0 && vp0.y > 0.0 && vp0.y < 1.0 && vp0.z > 0.0 && vp0.z < 150.0f ) || 
            //                 ( vp1.x > 0.0 && vp1.x < 1.0 && vp1.y > 0.0 && vp1.y < 1.0  && vp1.z > 0.0 && vp1.z < 150.0f  ) );               
            //   }

            //   if( visible && !m_isVisible)
            //   {
            //       //If grunt became visible enable physics.
            //       m_rigidBody.isKinematic = false;
            //       m_rigidBody.detectCollisions = true;
            //       m_rigidBody.WakeUp();
            //       m_Animator.enabled = true;

            //   }
            //   else if(!visible && m_isVisible)
            //   {
            //     //If grunt became invisible disable physics
            //     if (!CameraManager.Instance.IsCutscenePlaying)
            //         m_rigidBody.isKinematic = true;
            //     else
            //         m_rigidBody.isKinematic = false;
            //     m_rigidBody.detectCollisions = false;
            //     m_rigidBody.Sleep();
            //     m_Animator.enabled = false;
            //   }

            //   m_isVisible = visible;
            // }

            // if (m_isVisible || CameraManager.Instance.IsCutscenePlaying)
            // {
            // Only update if visible
            base.Update();
            // }
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

                if (distanceToTaget > m_sensorRange)
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
                        if (distanceToTaget > m_sensorRange)
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