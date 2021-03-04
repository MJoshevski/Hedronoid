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
    }
}
