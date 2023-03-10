using Hedronoid.AI;
using Hedronoid.Enemies;
using Hedronoid.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurretSensor : AIBaseSensor
{
    protected Collider[] m_colliderBuffer = new Collider[10];
    protected List<Transform> m_targetsInRange = new List<Transform>(10);

    protected override void Awake()
    {
        base.Awake();

        HNDEvents.Instance.AddListener<LocatedPlayerEvent>(OnLocatedPlayer);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        HNDEvents.Instance.RemoveListener<LocatedPlayerEvent>(OnLocatedPlayer);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        HNDEvents.Instance.RemoveListener<LocatedPlayerEvent>(OnLocatedPlayer);
    }

    private void OnLocatedPlayer(LocatedPlayerEvent e)
    {
        List<UbhBaseShot> shotList = new List<UbhBaseShot>();

        if (shotList.Count == 0) shotList = GetComponentsInChildren<UbhBaseShot>().ToList();
        if (shotList.Count == 0)
        {
            D.AIError("No BaseShot pattern was found on " + gameObject.name + " !");
            return;
        }
    }

    public virtual Transform GetTargetWithinReach(float distance)
    {
        m_targetsInRange.Clear();
        // First check if we have any players in range
        var players = Physics.OverlapSphereNonAlloc(transform.position, distance, m_colliderBuffer, HNDAI.Settings.PlayerLayer);
        if (players > 0)
        {
            if (players == 1)
            {
                HNDEvents.Instance.Raise(new LocatedPlayerEvent { sender = gameObject, GOID = gameObject.GetInstanceID(), target = m_colliderBuffer[0].transform});

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
