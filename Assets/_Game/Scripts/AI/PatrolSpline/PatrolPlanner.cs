using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public class PatrolPlanner : AIBasePlanner
    {
        [Header("Patrol Settings")]
        [SerializeField]
        protected float m_Reach = 10f;
        [SerializeField]
        protected Transform m_LastTarget;

        private NPC targetNpcState;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void CheckRules()
        {
            if (!IsInState(EStates.Attack))
            {
                if (m_Sensor is BlockheadSensor)
                {
                    m_LastTarget = (m_Sensor as BlockheadSensor).GetTargetWithinReach(m_Reach);
                }
                if (m_LastTarget)
                {
                    targetNpcState = m_LastTarget.GetComponent<NPC>();
                    m_Navigation.SetTarget(m_LastTarget);
                    ResumeAttack();
                }
                else
                {
                    ResumeNavigation();
                }
            }
            else
            {
                ResumeNavigation();
            }
        }

        public override void ResumeAttack()
        {
            ChangeState(EStates.Attack);

            m_Navigation.ChangeState(AIBaseNavigation.EStates.GoToTarget);
        }

        public override void CancelAttack()
        {
            m_Navigation.Target = null;
        }

        public override void OnAttackUpdate()
        {
            // If the target is dead
            if (!m_LastTarget)
            {
                CheckRules();
            }
            else
            {
                if (targetNpcState)
                {
                    // If the target is now a zombie, check the rules.
                    if (targetNpcState.NpcType == NPC.NPCType.Zombie)
                    {
                        CheckRules();
                    }
                }
                else
                {
                    // If the player is out of reach, resume navigation.
                    if (Vector3.Distance(m_LastTarget.position, transform.position) > m_Reach)
                    {
                        CheckRules();
                    }
                }
            }
        }

        public override void OnNavigationUpdate()
        {
            if (Random.Range(0f, 1f) > 0.7f)
                CheckRules();
        }

        public override void ResumeNavigation()
        {
            m_Navigation.ChangeState(AIBaseNavigation.EStates.ReturnToDefault);
            CancelAttack();
            ChangeState(EStates.Navigation);
        }

        public override void PauseNavigation()
        {
        }

        public override void CancelNavigation()
        {
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_Reach);
        }
#endif
    }
}