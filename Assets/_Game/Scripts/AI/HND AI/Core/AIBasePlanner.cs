using UnityEngine;
using System.Collections.Generic;
using System;
using Hedronoid.HNDFSM;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public class AIBasePlanner : HNDFiniteStateMachine, IAIPlanner
    {
        [SerializeField]
        protected Dictionary<EStates,FSMState> m_AIPlannerStates = new Dictionary<EStates, FSMState>();
        [SerializeField]
        protected AIBaseNavigation m_Navigation;
        [SerializeField]
        protected AIBaseSensor m_Sensor;
        [SerializeField]
        protected AIBaseMotor m_Motor;

        public enum EStates
        {
            Repel,
            Attraction,
            Attack,
            Interact,
            Navigation,

            Highest
        }

        protected override void Awake()
        {
            base.Awake();

            /*m_AIPlannerStates.Add(EStates.Repel, CreateState(EStates.Repel, OnRepelUpdate, null, null));
            m_AIPlannerStates.Add(EStates.Attraction, CreateState(EStates.Attraction,OnAttractionUpdate, null, null));
            m_AIPlannerStates.Add(EStates.Attack, CreateState(EStates.Attack,OnAttackUpdate, null, null));
            m_AIPlannerStates.Add(EStates.Interact, CreateState(EStates.Interact,OnInteractUpdate, null, null));
            m_AIPlannerStates.Add(EStates.Navigation, CreateState(EStates.Navigation,OnNavigationUpdate, null, null));

            ChangeState(EStates.Navigation);
            */

            if (!m_Navigation) m_Navigation = GetComponent<AIBaseNavigation>();
            if (!m_Sensor) m_Sensor = GetComponent<AIBaseSensor>();
            if (!m_Motor) m_Motor = GetComponent<AIBaseMotor>();
            
        }

        public virtual void CheckRules()
        {
            throw new NotImplementedException();
        }       

        public virtual void OnRepelUpdate() { }
        public virtual void ResumeRepel() { }
        public virtual void PauseRepel() { }
        public virtual void CancelRepel() { }

        public virtual void OnAttractionUpdate() { }
        public virtual void ResumeAttraction() { }
        public virtual void PauseAttraction() { }
        public virtual void CancelAttraction() { }

        public virtual void OnAttackUpdate() { }
        public virtual void ResumeAttack() { }
        public virtual void PauseAttack() { }
        public virtual void CancelAttack() { }

        public virtual void OnInteractUpdate() { }
        public virtual void ResumeInteract() { }
        public virtual void PauseInteract() { }
        public virtual void CancelInteract() { }

        public virtual void SetNavigationTarget(Transform target = null)
        {
            if (m_Navigation)
                m_Navigation.SetTarget(target);
        }
        public virtual void OnNavigationUpdate()
        {
            throw new NotImplementedException();
        }
        public virtual void ResumeNavigation()
        {
            throw new NotImplementedException();
        }
        public virtual void PauseNavigation()
        {
            throw new NotImplementedException();
        }
        public virtual void CancelNavigation()
        {
            throw new NotImplementedException();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}
