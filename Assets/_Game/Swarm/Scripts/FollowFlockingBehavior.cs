using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "FollowFlockingBehavior", menuName = "Flocking/Behaviors/FollowFlockingBehavior", order = 1)]
public class FollowFlockingBehavior : FlockingBehavior
{
    public Transform followTarget;
    public float engagementDistance = 100.0f;

    public override Vector3 GetResults()
    {
        return FlockingManager.Instance.followBehaviorResults[m_owner.agentIdx];
        //followTarget = FlockingManager.Instance.tempFollowTarget;
        //Vector3 vectToTarget = followTarget.position - m_owner.Position;
        //float magnSq = vectToTarget.sqrMagnitude;

        //if (magnSq < 0.001f) return Vector3.zero;

        //if (magnSq > engagementDistance * engagementDistance) return Vector3.zero;


        //return vectToTarget.normalized;
    }

}
