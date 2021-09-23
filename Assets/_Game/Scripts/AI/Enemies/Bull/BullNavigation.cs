using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;
using Hedronoid.Health;
using Hedronoid.Events;
using static Hedronoid.Health.HealthBase;
using Hedronoid.Weapons;
using Pathfinding.RVO;
using Pathfinding;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    [RequireComponent(typeof(BullDash))]
    [RequireComponent(typeof(BullSensor))]
    public class BullNavigation : AIBaseNavigation
    {
        public enum EBullStates
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

        protected float remainingSensorTime;
        protected float m_targetEvaluationDistance = 3f;

        public float repathRate = 1;

        private float nextRepath = 0;

        private bool canSearchAgain = true;

        private Vector3 m_TargetV3;

        public float maxSpeed = 10;

        Path path = null;

        List<Vector3> vectorPath;
        int wp;

        public float moveNextDist = 1;
        public float slowdownDistance = 1;
        public LayerMask groundMask;
        protected override void Awake()
        {
            base.Awake();

            m_BullSensor = (BullSensor) m_Sensor;
            if (!m_BullDash) TryGetComponent(out m_BullDash);

            CreateState(EBullStates.DashToTarget, OnDashUpdate, null, null);

            enemyEmojis = GetComponent<EnemyEmojis>();
            HNDEvents.Instance.AddListener<KillEvent>(OnKilled);
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

        protected override void OnDisable()
        {
            base.OnDisable();

            HNDEvents.Instance.RemoveListener<KillEvent>(OnKilled);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            HNDEvents.Instance.RemoveListener<KillEvent>(OnKilled);
        }

        public override void SetAgentDestination(Vector3 target)
        {
            m_TargetV3 = target;
            RecalculatePath();
        }

        public void OnPathComplete(Path _p)
        {
            ABPath p = _p as ABPath;

            canSearchAgain = true;

            if (path != null) path.Release(this);
            path = p;
            p.Claim(this);

            if (p.error)
            {
                wp = 0;
                vectorPath = null;
                return;
            }


            Vector3 p1 = p.originalStartPoint;
            Vector3 p2 = transform.position;
            p1.y = p2.y;
            float d = (p2 - p1).magnitude;
            wp = 0;

            vectorPath = p.vectorPath;
            Vector3 waypoint;

            if (moveNextDist > 0)
            {
                for (float t = 0; t <= d; t += moveNextDist * 0.6f)
                {
                    wp--;
                    Vector3 pos = p1 + (p2 - p1) * t;

                    do
                    {
                        wp++;
                        waypoint = vectorPath[wp];
                    } while (m_RVOController.To2D(pos - waypoint).sqrMagnitude < moveNextDist * moveNextDist && wp != vectorPath.Count - 1);
                }
            }
        }

        public void RecalculatePath()
        {
            canSearchAgain = false;
            nextRepath = Time.time + repathRate * (Random.value + 0.5f);
            m_Seeker.StartPath(transform.position, m_TargetV3, OnPathComplete);
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

            if (IsInState(EBullStates.DashToTarget))
            {
                if (m_Motor is BullDash)
                {
                    (m_Motor as BullDash).DoDash(m_Target);
                }
            }
        }
        public override void OnDefaultMovementUpdate()
        {
            // Decrease sensor time and check the sensor if nessecary
            // Note that we will not check for this is the agent is not on the NavMesh - i.e. in the air or somewhere else.
            if ((remainingSensorTime -= Time.deltaTime) <= 0)
            {
                ChangeTarget();
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
        public void OnDashUpdate()
        {
            // We are not doing anything here. It got started when we changed state.
        }
        public override void OnFleeFromTargetUpdate()
        {
        }
        public override void OnGoToTargetUpdate()
        {
            if (m_Target)
            {
                ai.destination = m_Target.position;

                var distanceToTarget = Vector3.Distance(transform.position, m_Target.position);

                // If we are within dash distance, change to the dash state
                if (distanceToTarget <= m_dashDistance)
                {
                    if (!dashed)
                    {
                        ChangeState(EBullStates.DashToTarget);
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

        //protected override void Update()
        //{
        //    base.Update();

        //    if (Time.time >= nextRepath && canSearchAgain)
        //    {
        //        RecalculatePath();
        //    }

        //    Vector3 pos = transform.position;

        //    if (vectorPath != null && vectorPath.Count != 0)
        //    {
        //        while ((m_RVOController.To2D(pos - vectorPath[wp]).sqrMagnitude < moveNextDist * moveNextDist && wp != vectorPath.Count - 1) || wp == 0)
        //        {
        //            wp++;
        //        }

        //        // Current path segment goes from vectorPath[wp-1] to vectorPath[wp]
        //        // We want to find the point on that segment that is 'moveNextDist' from our current position.
        //        // This can be visualized as finding the intersection of a circle with radius 'moveNextDist'
        //        // centered at our current position with that segment.
        //        var p1 = vectorPath[wp - 1];
        //        var p2 = vectorPath[wp];

        //        // Calculate the intersection with the circle. This involves some math.
        //        var t = VectorMath.LineCircleIntersectionFactor(m_RVOController.To2D(transform.position), m_RVOController.To2D(p1), m_RVOController.To2D(p2), moveNextDist);
        //        // Clamp to a point on the segment
        //        t = Mathf.Clamp01(t);
        //        Vector3 waypoint = Vector3.Lerp(p1, p2, t);

        //        // Calculate distance to the end of the path
        //        float remainingDistance = m_RVOController.To2D(waypoint - pos).magnitude + m_RVOController.To2D(waypoint - p2).magnitude;
        //        for (int i = wp; i < vectorPath.Count - 1; i++) remainingDistance += m_RVOController.To2D(vectorPath[i + 1] - vectorPath[i]).magnitude;

        //        // Set the target to a point in the direction of the current waypoint at a distance
        //        // equal to the remaining distance along the path. Since the rvo agent assumes that
        //        // it should stop when it reaches the target point, this will produce good avoidance
        //        // behavior near the end of the path. When not close to the end point it will act just
        //        // as being commanded to move in a particular direction, not toward a particular point
        //        var rvoTarget = (waypoint - pos).normalized * remainingDistance + pos;
        //        // When within [slowdownDistance] units from the target, use a progressively lower speed
        //        var desiredSpeed = Mathf.Clamp01(remainingDistance / slowdownDistance) * maxSpeed;
        //        Debug.DrawLine(transform.position, waypoint, Color.red);
        //        m_RVOController.SetTarget(rvoTarget, desiredSpeed, maxSpeed);
        //    }
        //    else
        //    {
        //        // Stand still
        //        m_RVOController.SetTarget(pos, maxSpeed, maxSpeed);
        //    }

        //    // Get a processed movement delta from the rvo controller and move the character.
        //    // This is based on information from earlier frames.
        //    var movementDelta = m_RVOController.CalculateMovementDelta(Time.deltaTime);
        //    pos += movementDelta;

        //    // Rotate the character if the velocity is not extremely small
        //    if (Time.deltaTime > 0 && movementDelta.magnitude / Time.deltaTime > 0.01f)
        //    {
        //        var rot = transform.rotation;
        //        var targetRot = Quaternion.LookRotation(movementDelta, m_RVOController.To3D(Vector2.zero, 1));
        //        const float RotationSpeed = 5;
        //        if (m_RVOController.movementPlaneMode == MovementPlane.XY)
        //        {
        //            targetRot = targetRot * Quaternion.Euler(-90, 180, 0);
        //        }
        //        transform.rotation = Quaternion.Slerp(rot, targetRot, Time.deltaTime * RotationSpeed);
        //    }

        //    if (m_RVOController.movementPlaneMode == MovementPlane.XZ)
        //    {
        //        RaycastHit hit;
        //        if (Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, 2, groundMask))
        //        {
        //            pos.y = hit.point.y;
        //        }
        //    }

        //    transform.position = pos;
        //}
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            newTarget = m_BullSensor.GetTargetInsideCone(m_Rb.transform.forward);

            if (!newTarget)
                newTarget = m_BullSensor.GetTargetWithinReach(m_BullSensor.SensorRange);

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

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // if (!HNDAI.Settings.DrawGizmos)
            // {
            //     return;
            // }

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