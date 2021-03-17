using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid.AI;
using Hedronoid;
using UnityEngine.AI;

namespace Hedronoid.AI
{
    public class BullNavigation : GruntNavigation
    {
        private Vector3 upAxis, targetDirection;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            //targetDirection = (m_Target.position - transform.position).normalized;

            upAxis = GravityService.GetUpAxis(m_Rb.transform.position);

            //Quaternion targetRotation = Quaternion.Slerp(
            //    transform.rotation,
            //    Quaternion.LookRotation(targetDirection, upAxis),
            //    Time.fixedDeltaTime);

            //transform.rotation = targetRotation;
        }

        public override bool SetAgentDestination(Vector3 destination)
        {
            if (!agent.isOnNavMesh)
            {
                return false;
            }
            // First make sure that the destination is grounded
            var groundDetectStart = new Vector3(destination.x, destination.y + 1f, destination.z);
            RaycastHit rh;
            if (Physics.Raycast(groundDetectStart, -upAxis, out rh, GroundDetectDistance * 2, HNDAI.Settings.GroundLayer))
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

                if (distanceToTarget > m_sensorRange)
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

        public override void ChangeTarget()
        {
            Transform newTarget;

            newTarget = (m_Sensor as BullSensor).GetTargetInsideCone(10f, m_Rb.transform.forward, 35f, 15f);

            if (!newTarget)
                newTarget = (m_Sensor as BullSensor).GetTargetWithinReach(m_sensorRange);

            if (newTarget)
            {
                m_Target = newTarget.transform;
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
    }
}
