using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.AI;
using System.Collections;
using Hedronoid.Health;
using Hedronoid.HNDFSM;

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
        protected NavMeshAgent agent;
        protected Coroutine m_RecoverRotine;

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
        public Rigidbody m_Rb;
        protected HealthBase m_HealthBase;

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

            agent = GetComponent<NavMeshAgent>();
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
            else
            {
                DefaultTarget = new GameObject().transform;
                DefaultTarget.transform.position = transform.position;
                DefaultTarget.gameObject.name = "AutomaticWaypoints " + gameObject.name;
                int waypointamount = UnityEngine.Random.Range(m_MinWaypoints, m_MaxWaypoints);
                waypoints = new Transform[waypointamount];

                GameObject wayGo = new GameObject();
                wayGo.transform.position = CreateRandomWaypoint(transform.position, m_MaxWayPointDistance, m_MaxWayPointDistance*1.3f);
                wayGo.transform.parent = DefaultTarget;
                wayGo.name = "AutomaticWaypoint" + gameObject.name + " (" + 0 + ")";
                waypoints[0] = wayGo.transform; 

                for(int i = 1; i < waypointamount; i++)
                {
                    wayGo = new GameObject();
                    wayGo.transform.position = CreateRandomWaypoint(waypoints[i - 1].position,m_MinWayPointDistance, m_MaxWayPointDistance);
                    wayGo.transform.parent = DefaultTarget;
                    wayGo.name = "AutomaticWaypoint" + gameObject.name + " (" + i + ")";
                    waypoints[i] = wayGo.transform;
                }
            }

            ChangeState(EStates.DefaultMovement);
        }

        public virtual void RecieveImpact(float RecoverTime = 0f, bool spin = true)
        {
            OnImpact = true;

            if (agent)
            {
                agent.enabled = false;
            }

            if (m_RecoverRotine != null)
            {
                StopCoroutine(m_RecoverRotine);
                StopCoroutine("Spin");
            }

            if (spin && !m_isFrozen)
            {
                StartCoroutine(Spin());
            }

            m_RecoverRotine = StartCoroutine(RecoverImpact(RecoverTime));
        }
        
        protected virtual IEnumerator Spin()
        {
            while (true)
            {
                if (!OnImpact || (agent && agent.enabled))
                {
                    cachedRigidbody.angularVelocity = Vector3.zero;
                    yield break;
                }
                cachedRigidbody.AddTorque(Vector3.up * 1000f, ForceMode.VelocityChange);
                yield return new WaitForFixedUpdate();
            }
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

                        if (agent)
                        {
                            agent.enabled = true;
                        }
                    }
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        public virtual void StopImpact()
        {
            if (m_RecoverRotine != null)
            {
                StopCoroutine(m_RecoverRotine);
            }

            OnImpact = false;

            if (agent)
            {
                agent.enabled = true;
            }
        }

        public virtual void SetDefaultTarget(Transform target)
        {
            m_DefaultTarget = target;
        }

        public virtual void SetTarget(Transform target)
        {
            m_Target = target;
        }

        public virtual Vector3 GetDirection()
        {
            throw new NotImplementedException();
        }

        public virtual void OnGoToTargetUpdate()
        {
            if (m_isFrozen) return;
            throw new NotImplementedException();
        }

        public virtual void OnFleeFromTargetUpdate()
        {
            if (m_isFrozen) return;
            throw new NotImplementedException();
        }

        public virtual void OnReturnToDefaultUpdate()
        {
            if (m_isFrozen) return;
            throw new NotImplementedException();
        }

        public virtual void OnDefaultMovementUpdate()
        {
            if (m_isFrozen) return;
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

        public void Freezing()
        {
            m_isFrozen = true;
            ChangeState(EStates.DefaultMovement);

            foreach (Animator anim in m_Animators)
            {
                anim.enabled = false;
            }

            if (Target) m_tempTarget = Target;
            Target = null;
            if (m_Sensor) m_Sensor.enabled = false;

            if (gameObject.layer != LayerMask.NameToLayer("Enemies"))
            {
                if (m_Rb) m_Rb.isKinematic = true;
            }

            else if (m_Rb) m_Rb.constraints = RigidbodyConstraints.FreezeAll;
            if (agent)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }
            if (m_HealthBase) m_HealthBase.CanBeOneShotted = true;
        }

        public void Thawing()
        {
            m_isFrozen = false;
            foreach (Animator anim in m_Animators)
            {
                anim.enabled = true;
            }

            if (m_Sensor) m_Sensor.enabled = true;
            if (m_tempTarget) Target = m_tempTarget;

            if (gameObject.layer != LayerMask.NameToLayer("Enemies"))
            {
                if (m_Rb) m_Rb.isKinematic = false;
            }               

            else if (m_Rb) m_Rb.constraints = RigidbodyConstraints.None;

            if (agent)
            {
                agent.isStopped = false;
                agent.updateRotation = true;
            }

            ChangeState(EStates.DefaultMovement);
            if (m_HealthBase) m_HealthBase.CanBeOneShotted = false;
        }

        public virtual Vector3 CreateRandomWaypoint(Vector3 lastPos, float minRange, float maxRange)
        {
            Vector3 newWayPointDirection = Vector3.forward * UnityEngine.Random.Range(minRange, maxRange);
            Vector3 newWayPoint = lastPos + newWayPointDirection;
            bool found = false;
            int searchCount = 50;
            while (!found)
            {
                newWayPointDirection = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f) * newWayPointDirection;
                newWayPoint = lastPos + newWayPointDirection;

                var groundDetectStart = new Vector3(newWayPoint.x, newWayPoint.y + 0.2f, newWayPoint.z);
                RaycastHit rh;
                if (Physics.Raycast(groundDetectStart, Vector3.down, out rh, GroundDetectDistance * 2, HNDAI.Settings.GroundLayer))
                {
                    NavMeshHit nmh;
                    NavMesh.SamplePosition(rh.point, out nmh, 1f, NavMesh.AllAreas);
                    if (nmh.hit)
                    {
                        //agent.destination = nmh.position;
                        newWayPoint = nmh.position + Vector3.up * 0.2f;
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

        public virtual bool SetAgentDestination(Vector3 destination)
        {
            if (!agent.isOnNavMesh)
            {
                return false;
            }
            // First make sure that the destination is grounded
            var groundDetectStart = new Vector3(destination.x, destination.y + 1f, destination.z);
            RaycastHit rh;
            if (Physics.Raycast(groundDetectStart, Vector3.down, out rh, GroundDetectDistance * 2, HNDAI.Settings.GroundLayer))
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
    }
}