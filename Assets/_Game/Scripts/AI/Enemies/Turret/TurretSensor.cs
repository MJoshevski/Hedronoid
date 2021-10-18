using Hedronoid.AI;
using Hedronoid.Enemies;
using Hedronoid.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSensor : AIBaseSensor
{
    protected Collider[] m_colliderBuffer = new Collider[10];
    protected List<Transform> m_targetsInRange = new List<Transform>(10);

    [SerializeField]
    [Tooltip("This is how far away we will detect the player or NPCs")]
    protected float m_sensorRange = 3f;
    public float SensorRange
    {
        get { return m_sensorRange; }
    }

    [SerializeField]
    [Tooltip("This is how far the cutoff will be once the player has already entered the sensor range")]
    protected float m_sensorCutoffRange = 20f;
    public float SensorCutoffRange
    {
        get { return m_sensorCutoffRange; }
    }

    [SerializeField]
    [Tooltip("This is how often we will check for new targets around us if we are patrolling.")]
    protected float m_sensorTimestep = 0.25f;
    public float SensorTimeStep
    {
        get { return m_sensorTimestep; }
    }

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
        UbhBaseShot shot = null;
        TryGetComponent(out shot);

        if (!shot) shot = GetComponentInChildren<UbhBaseShot>();
        if (!shot)
        {
            D.AIError("No BaseShot pattern was found on " + gameObject.name + " !");
            return;
        }

        shot.m_targetTransform = e.target;
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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, SensorRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, SensorCutoffRange);
    }
#endif
}
