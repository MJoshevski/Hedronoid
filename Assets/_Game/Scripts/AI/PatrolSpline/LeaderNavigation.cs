using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    // Leaders move using a NavMeshAgent instead of a Motor, as this gives the most reliable movement.
    public class LeaderNavigation : AIBaseNavigation
    {
        public enum ELeaderStates
        {
            Slam,
            ShootAtPlayer
        }

        [Header("Leader Settings")]
        [SerializeField]
        [Tooltip("If we are within this range of our tower, select another tower")]
        protected float m_RangeToTarget = 5f;

        protected Vector3? lastEvaluationPosition;
        protected NavMeshPath path;
        private bool doWeHaveAPath;
        private int currentPathCorner;
        private Vector3 lastDirection;

        [SerializeField]
        private float m_TargetTimeMemory = 5f;
        [SerializeField]
        private float m_TargetDistanceMemory = 15f;
        [SerializeField]
        private float m_MaxDistanceToDefault = 20f;
        [SerializeField]
        protected float m_SlamDistance = 5f;
        [Header("Fred Settings")]
        [SerializeField]
        protected float m_RangedStartDistance;
        [SerializeField]
        private TelegraphShoot m_TelegraphShoot;
        [SerializeField]
        private ArcherShoot m_ArcherShoot;

        public bool CanAttack = false;
        protected float remainingSensorTime;
        protected EnemyEmojis enemyEmojis;
        [SerializeField]
        protected float m_MinDistanceToDefault = 0.82f;

        protected override void Awake()
        {
            base.Awake();
            CreateState(ELeaderStates.Slam, OnSlamUpdate, null, null);
            CreateState(ELeaderStates.ShootAtPlayer, OnShootUpdate, null, null);
            agent = GetComponent<NavMeshAgent>();
            m_TelegraphShoot = GetComponent<TelegraphShoot>();
            enemyEmojis = GetComponent<EnemyEmojis>();
        }

        public override Vector3 GetDirection()
        {
            // This is not actually being used in the current code, but if someone wants it they will get the last direction moved.
            return lastDirection;
        }

        public void OnShootUpdate()
        {
            if (m_Target)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation((m_Target.position - transform.position)), 0.6f);
            }
        }

        public void ShotDone()
        {
            agent.enabled = true;
            // After the shooting we will check if the player is still within range.
            // If not we will lose him as a target.
            if (m_Target && m_Target.gameObject.layer != LayerMask.NameToLayer("Players"))
                m_Target = null;

            if (!m_Target)
            {
                ChangeState(EStates.GoToTarget);
            }

            m_Target = null;
            ChangeState(EStates.GoToTarget);
        }

        public override void ChangeTarget()
        {
            base.ChangeTarget();
            var newTarget = (m_Sensor as BlockheadSensor).GetTargetWithinReach(m_RangeToTarget);
            if (newTarget)
            {
                m_Target = newTarget;
                lastEvaluationPosition = m_Target.position;
                SetAgentDestination(m_Target.position);
                if (enemyEmojis)
                {
                    enemyEmojis.ChangeTarget(m_Target.gameObject);
                }
                if (newTarget.CompareTag(HNDAI.Settings.PlayerTag))
                {
                    DoAction();
                }
                return;
            }
            else
            {
                remainingSensorTime = m_TargetTimeMemory;
            }
        }

        /// <summary>
        /// Note, we will always go look for the next tower. We do not have a default state.
        /// </summary>
        public override void OnGoToTargetUpdate()
        {
            // Check if the default target is too far away from us and return to it so we are nto getting kited
            if (m_DefaultTarget && Vector3.Distance(m_DefaultTarget.position, transform.position) > m_MaxDistanceToDefault)
            {
                ChangeState(EStates.ReturnToDefault);
                return;
            }

            if ((remainingSensorTime -= Time.deltaTime) <= 0 && agent.isOnNavMesh)
            {
                ChangeTarget();
            }
            else
            {
                if (m_Target && m_Target.CompareTag(HNDAI.Settings.PlayerTag))
                {
                    DoAction();
                }
            }
        }

        void DoAction()
        {
            if (!CanAttack)
            {
                return;
            }

            var dist = Vector3.Distance(transform.position, m_Target.position);
            if ((m_RangedStartDistance > dist))
            {
                if (dist < m_SlamDistance)
                {
                    ChangeState(ELeaderStates.Slam);
                }
            }
            else
            {
                ChangeState(ELeaderStates.ShootAtPlayer);
            }
        }

        public override void SetTarget(Transform target)
        {
            base.SetTarget(target);
        }

        public override void ChangeState(System.Enum newState)
        {
            if (!CanAttack)
            {
                return;
            }

            base.ChangeState(newState);

            if (IsInState(ELeaderStates.Slam))
            {
                agent.enabled = false;
                m_TelegraphShoot.CurrentStage = 0;
                m_TelegraphShoot.StartShooting();
            }
            if (IsInState(ELeaderStates.ShootAtPlayer))
            {
                agent.enabled = false;
                if (m_ArcherShoot && m_Target)
                {
                    if (!m_ArcherShoot.DoShoot(m_Target))
                    {
                        ChangeState(EStates.GoToTarget);
                    }
                }
            }
            if (IsInState(EStates.ReturnToDefault))
            {
                if (m_DefaultTarget)
                {
                    SetAgentDestination(m_DefaultTarget.position);
                }
            }
        }

        public override void OnFleeFromTargetUpdate()
        {
        }

        public override void OnReturnToDefaultUpdate()
        {
            if (!m_DefaultTarget || Vector3.Distance(m_DefaultTarget.position, transform.position) < m_MinDistanceToDefault)
            {
                ChangeState(EStates.GoToTarget);
            }
        }

        public override void OnDefaultMovementUpdate()
        {
            ChangeState(EStates.GoToTarget);
        }


        public void OnSlamUpdate()
        {
            if (m_Target)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation((m_Target.position - transform.position)), 0.6f);
            }
        }

        public void SlamDone()
        {
            // We are done with the slam. Return to normal proceedings.
            agent.enabled = true;
            m_Target = null;
            ChangeState(EStates.GoToTarget);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (agent)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(agent.destination, 1f);
                Gizmos.color = (agent.hasPath) ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position, agent.destination);
            }

            // Display the explosion radius when selected
            if (m_Target)
            {
                Gizmos.color = new Color(1, 1, 0, 0.75F);
                Gizmos.DrawSphere(m_Target.position, 3f);
            }
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, m_RangedStartDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_SlamDistance);
        }
#endif
    }
}