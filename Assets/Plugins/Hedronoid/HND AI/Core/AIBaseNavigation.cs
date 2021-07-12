using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.AI;
using System.Collections;
using Hedronoid.Health;
using Hedronoid.HNDFSM;
using Pathfinding;
using Pathfinding.RVO;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public class AIBaseNavigation : HNDFiniteStateMachine, IAINavigation
    {
        public enum EStates
        {
            GoToTarget,
            FleeFromTarget,
            ReturnToDefault,
            DefaultMovement,

            Highest
        }

        [Header("Base Navigation")]
        [Tooltip("Object to return to: Area, point, object, etc.")]
        [SerializeField]
        protected Transform m_DefaultTarget;
        [SerializeField]
        protected Transform m_Target;

        [Header("Automatic Waypoint")]
        [SerializeField]
        private int m_MinWaypoints = 3;
        [SerializeField]
        private int m_MaxWaypoints = 7;
        [SerializeField]
        private float m_MinWayPointDistance = 2f;
        [SerializeField]
        private float m_MaxWayPointDistance = 4f;

        protected bool m_OnImpact = false;
        protected Coroutine m_RecoverRoutine;

        [SerializeField]
        private float m_RadiusForGroundDetection = .5f;
        [SerializeField]
        private float m_GroundDetectionDistance = .55f;

        private Transform m_tempTarget;

        protected bool m_isFrozen = false;
        public bool IsFrozen
        {
            get { return m_isFrozen; }
        }

        public Transform Target 
        {
            get { return m_Target; }
            set { m_Target = value; }
        }

        public Transform DefaultTarget 
        {
            get { return m_DefaultTarget; }
            set { m_DefaultTarget = value; }
        }

        public bool OnImpact
        {
            get { return m_OnImpact; }
            set { m_OnImpact = value; }
        }

        // Waypoints
        //[SerializeField]
        protected Transform[] waypoints;

        protected const float GroundDetectDistance = 1000f;

        // Components
        protected Animator [] m_Animators;
        protected AIBaseSensor m_Sensor;
        protected AIBaseMotor m_Motor;
        protected RVOController m_RVOController;
        protected Seeker m_Seeker;
        public Rigidbody m_Rb;
        protected HealthBase m_HealthBase;

        protected IAstarAI ai;
        protected override void Awake()
        {
            base.Awake();

            CreateState(EStates.DefaultMovement, OnDefaultMovementUpdate, null, null);

            CreateState(EStates.GoToTarget, OnGoToTargetUpdate, null, null);
            CreateState(EStates.FleeFromTarget, OnFleeFromTargetUpdate, null, null);
            CreateState(EStates.ReturnToDefault, OnReturnToDefaultUpdate, null, null);

            if (!DefaultTarget) DefaultTarget = transform;
            if (!m_Sensor) m_Sensor = GetComponent<AIBaseSensor>();
            if (!m_Motor) m_Motor = GetComponent<AIBaseMotor>();
            if (m_Animators == null || m_Animators.Length == 0) m_Animators = GetComponentsInChildren<Animator>();
            if (!m_Rb) m_Rb = GetComponent<Rigidbody>();
            if (!m_HealthBase) m_HealthBase = GetComponent<HealthBase>();
            m_RVOController = GetComponent<RVOController>();
            m_Seeker = GetComponent<Seeker>();
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            if (ai == null) ai = GetComponent<IAstarAI>();
            if (ai != null) ai.onSearchPath += Update;
        }

        protected override void OnDisable()
        {
            if (ai != null) ai.onSearchPath -= Update;
        }
        protected override void Start()
        {
            base.Start();
    
            if (DefaultTarget && DefaultTarget != transform)
            {
                if (DefaultTarget.childCount == 0)
                {
                    waypoints = new Transform[1] { DefaultTarget };
                }
                else
                {
                    waypoints = new Transform[DefaultTarget.childCount];
                    for (int i = 0; i < DefaultTarget.childCount; i++)
                    {
                        waypoints[i] = DefaultTarget.GetChild(i);
                    }
                }
            } 

            ChangeState(EStates.DefaultMovement);
        }

        public virtual void RecieveImpact(float RecoverTime = 0f, bool spin = true)
        {
            OnImpact = true;

            if (m_RecoverRoutine != null)
            {
                StopCoroutine(m_RecoverRoutine);
            }

            m_RecoverRoutine = StartCoroutine(RecoverImpact(RecoverTime));
        }

        protected virtual IEnumerator RecoverImpact(float time)
        {
            float RecoverTimeStamp = Time.time;
            yield return new WaitForSeconds(time);
            while (true)
            {
                RaycastHit hit;
                if (Physics.SphereCast(new Ray(cachedTransform.position  +Vector3.up * m_GroundDetectionDistance, Vector3.down), m_RadiusForGroundDetection, 
                                        out hit, m_GroundDetectionDistance * 2f, HNDAI.Settings.WalkableLayer, QueryTriggerInteraction.Ignore) ||
                    RecoverTimeStamp + 6f < Time.time)
                {
                    if (OnImpact)
                    {
                        OnImpact = false;
                    }
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        public virtual void StopImpact()
        {
            if (m_RecoverRoutine != null)
            {
                StopCoroutine(m_RecoverRoutine);
            }

            OnImpact = false;
        }

        public virtual void SetDefaultTarget(Transform target)
        {
            m_DefaultTarget = target;
        }

        public virtual void SetTarget(Transform target)
        {
            m_Target = target;
        }
        public virtual void SetAgentDestination(Vector3 destination)
        {
            throw new NotImplementedException();
        }
        public virtual Vector3 GetDirection()
        {
            throw new NotImplementedException();
        }

        public virtual void OnGoToTargetUpdate()
        {
            throw new NotImplementedException();
        }

        public virtual void OnFleeFromTargetUpdate()
        {
            throw new NotImplementedException();
        }

        public virtual void OnReturnToDefaultUpdate()
        {
            throw new NotImplementedException();
        }

        public virtual void OnDefaultMovementUpdate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Force a change of the current target. Used for attracting the attention of an enemy via yell.
        /// </summary>
        public virtual void ChangeTarget()
        {
        }

        public virtual void ChangeTarget(Transform trans)
        {
        }

    }
}