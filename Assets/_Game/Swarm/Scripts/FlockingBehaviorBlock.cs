using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "FlockingBehaviorBlock", menuName = "Flocking/FlockingBehaviorBlock", order = 1)]
public class FlockingBehaviorBlock : ScriptableObject
{
    private FlockingAgent m_owner;

    public Vector3 steering = Vector3.zero;
    public Vector3 displacement = Vector3.zero;

    public FlockingAgent Owner
    {
        get { return m_owner; }
        set { m_owner = value; UpdateBehaviorOwners(); }
    }

    public List<FlockingBehaviorInstance> flockingBehaviorInstances = new List<FlockingBehaviorInstance>();

    public void Update()
    {
        foreach (FlockingBehaviorInstance b in flockingBehaviorInstances)
        {
            b.m_behavior.Update();

    
        }
    }

    public void GetResults()
    {
        Vector3 finalFlockingVector = Vector3.zero;

        steering = Vector3.zero;
        displacement = Vector3.zero;

        if (flockingBehaviorInstances.Count < 1)
        {
            return;
        }

        foreach (FlockingBehaviorInstance b in flockingBehaviorInstances)
        {
            if (b.m_behavior.outputType == FlockingBehavior.OutputType.Direction)
            {
                steering += b.m_behavior.GetResults() * b.m_weight;
            }
            else
            {
                displacement += b.m_behavior.GetResults() * b.m_weight;
            }

        }

        steering.Normalize();


    }

    void UpdateBehaviorOwners()
    {
        foreach (FlockingBehaviorInstance b in flockingBehaviorInstances)
        {
            b.m_behavior = Object.Instantiate(b.m_behavior);
            b.m_behavior.Initialize();
            b.m_behavior.m_owner = m_owner;
        }
    }
}

