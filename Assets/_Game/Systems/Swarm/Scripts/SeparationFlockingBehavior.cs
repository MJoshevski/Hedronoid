using static Unity.Mathematics.math;


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;

[CreateAssetMenu(fileName = "SeparationFlockingBehavior", menuName = "Flocking/Behaviors/SeparationFlockingBehavior", order = 1)]
public class SeparationFlockingBehavior : FlockingBehavior
{
    public float separationDistance = 6.0f;

    static Vector3 zeroVector = Vector3.zero;

    float sepDistSq;
    float oneOverSepDistance;

    SeparationTargetJob separationTargetJob;
    SeparationAccumulateJob separationAccumulateJob;




    [BurstCompile]
    public struct SeparationTargetJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeSlice<Vector3> neighborPositions;

        [WriteOnly]
        public NativeSlice<Vector3> separationVectors;

        [ReadOnly]
        public float3 ownerPos;

        [ReadOnly]
        public float separationDistSq;

        [ReadOnly]
        public float oneOverSeparationDistance;

        public void Execute(int i)
        {

            float3 dist = ownerPos -
                float3(neighborPositions[i].x, neighborPositions[i].y, neighborPositions[i].z);
            float distSq = lengthsq(dist);
            if (distSq < separationDistSq)
            {

                float magn = Mathf.Sqrt(distSq);
                float sep = 1.0f - (magn * oneOverSeparationDistance);

                separationVectors[i] = (Vector3)(sep * dist / magn);
            }



        }
    }

    [BurstCompile]
    public struct SeparationAccumulateJob : IJob
    {
        [ReadOnly]
        public NativeSlice<Vector3> separationVectors;

        public NativeArray<Vector3> result;

        public void Execute()
        {
            for (int i = 0; i < separationVectors.Length; i++)
            {
                result[0] = result[0] + separationVectors[i];
            }
        }

    }

    JobHandle separationJobHandle;
    JobHandle accumulationJobHandle;

    public override void Update()
    { 
       // if (m_owner.m_numNeighbors > 0) ComputeSeparationTarget();
    }

    public override Vector3 GetResults()
    {
        if (m_owner.m_numNeighbors < 1) return Vector3.zero;

        // accumulationJobHandle.Complete();

        //return m_owner.sepResults[0];
        return FlockingManager.Instance.separationBehaviorResults[m_owner.agentIdx];

    }

    public override void Initialize()
    {
        base.Initialize();

       sepDistSq = separationDistance * separationDistance;
       oneOverSepDistance = 1.0f / separationDistance;
    }



    public void ComputeSeparationTarget()
    {
        NativeSlice<Vector3> sepSlice = m_owner.separationVects.Slice<Vector3>(0, m_owner.m_numNeighbors);

        separationTargetJob = new SeparationTargetJob()
        {
            oneOverSeparationDistance = oneOverSepDistance,
            separationDistSq = sepDistSq,
            ownerPos = m_owner.Position,

            separationVectors = sepSlice,
            neighborPositions = FlockingManager.Instance.m_neighborPositionsGlobalNativeArray.Slice<Vector3>(m_owner.startInGlobalIdx, m_owner.m_numNeighbors),

        };

        separationAccumulateJob = new SeparationAccumulateJob()
        {
            separationVectors = sepSlice,
            result = m_owner.sepResults
        };

        m_owner.sepResults[0] = Vector3.zero;

        separationJobHandle = separationTargetJob.Schedule(m_owner.m_numNeighbors, 64, FlockingManager.Instance.extractNeighborPositionsJobHandle);
        accumulationJobHandle = separationAccumulateJob.Schedule<SeparationAccumulateJob>(separationJobHandle);

    }


}
