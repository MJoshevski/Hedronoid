using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CohesionFlockingBehavior", menuName = "Flocking/Behaviors/CohesionFlockingBehavior", order = 1)]
public class CohesionFlockingBehavior : FlockingBehavior
{
    public override Vector3 GetResults() 
    {

        if (m_owner.m_flockNeighbors.Length > 0)
        {
             return FlockingManager.Instance.cohesionBehaviorResults[m_owner.agentIdx];

        }
        else
        {
            if (m_owner.m_fallBackNeighbor != null)
            {
                return (m_owner.m_fallBackNeighbor.Position - m_owner.Position).normalized;
            }
        }

        return Vector3.zero;
    }

    public Vector3 ComputeCohesionTarget()
    {
        Vector3 clusterPos = Vector3.zero;

        for (int k = 0; k < m_owner.m_flockNeighbors.Length; k++)
        {
            FlockingAgent neighbor = m_owner.m_flockNeighbors[k];
            clusterPos += neighbor.Position;

        }

        clusterPos *= m_owner.oneOverNeighbourCount;
        return clusterPos;
    }


}
