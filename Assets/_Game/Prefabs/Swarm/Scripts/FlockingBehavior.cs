using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FlockingBehavior : ScriptableObject
{
    [HideInInspector]
    public FlockingAgent m_owner;

    public OutputType outputType = OutputType.Direction;

    public enum OutputType
    {
        Direction,
        Displacement 
    }

    public virtual void Initialize() { }
    public virtual void Update() { }
    public virtual Vector3 GetResults(){ return Vector3.zero; }
}

[System.Serializable]
public class FlockingBehaviorInstance
{
    public FlockingBehavior m_behavior;
    public float m_weight = 1.0f;
}
