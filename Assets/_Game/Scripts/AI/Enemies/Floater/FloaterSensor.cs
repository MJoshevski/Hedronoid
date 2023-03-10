using Hedronoid.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloaterSensor : AIBaseSensor
{
    protected Collider[] m_colliderBuffer = new Collider[10];
    protected List<Transform> m_targetsInRange = new List<Transform>(10);

    public virtual Transform GetTargetWithinReach(float distance)
    {
        m_targetsInRange.Clear();
        // First check if we have any players in range
        var players = Physics.OverlapSphereNonAlloc(transform.position, distance, m_colliderBuffer, HNDAI.Settings.PlayerLayer);
        if (players > 0)
        {
            if (players == 1)
            {
                return m_colliderBuffer[0].transform;
            }
        }
        return null;
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, SensorRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, SensorCutoffRange);
    }
#endif
}