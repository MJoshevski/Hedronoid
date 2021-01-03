using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public class ChaserPlanner : AIBasePlanner
    {
        [Header("Chaser Settings")]
        [SerializeField]
        protected float m_Reach = 10f;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void CheckRules()
        {
            if (!IsInState(EStates.Attack))
            {
                Transform target = ((ChaserSensor)m_Sensor).GetRandomTargetInReach(m_Reach);
                if (target)
                {
                    m_Navigation.SetTarget(target);
                    ResumeAttack();
                }
            }
            else
            {
                if (Vector3.Distance(m_Navigation.DefaultTarget.position, transform.position) > ((ChaserNavigation)m_Navigation).ReturnAreaSize / 2f)
                {
                    ResumeNavigation();
                }
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
            if (Random.Range(0f, 1f) > 0.7f)
                CheckRules();
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