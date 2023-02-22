using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AlignmentFlockingBehavior", menuName = "Flocking/Behaviors/AlignmentFlockingBehavior", order = 1)]
public class AlignmentFlockingBehavior : FlockingBehavior
{


    public override Vector3 GetResults()
    {
        float pn = ( Mathf.PerlinNoise(0.3f * m_owner.Position.x, 0.3f * m_owner.Position.z));

        //Debug.DrawRay(m_owner.Position, Vector3.forward * pn * 6.0f);

        //return ComputeAlignmentTarget() + 10.0f * Vector3.forward * (pn - 0.5f);
        return FlockingManager.Instance.alignmentBehaviorResults[m_owner.agentIdx] + 10.0f * Vector3.forward * (pn - 0.5f);

    }

    public Vector3 ComputeAlignmentTarget()
    {
        Vector3 finalAlignment = Vector3.zero;

        for (int k = 0; k < m_owner.m_flockNeighbors.Length; k++)
        {
            FlockingAgent neighbor = m_owner.m_flockNeighbors[k];

            finalAlignment += neighbor.Forward;
        }




        //finalAlignment /= (float)m_owner.m_flockNeighbors.Count;
        return finalAlignment.normalized;
    }


}
