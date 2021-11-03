using Dreamteck.Splines;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh linear shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/Spline Shot")]
public class UbhSplineShot : UbhBaseShot
{
    [Header("===== Spline Shot Settings =====")]
    // "Set a delay time between bullet and next bullet. (sec)"
    [FormerlySerializedAs("_BetweenDelay")]
    public float m_betweenDelay = 0.1f;

    private int m_nowIndex;
    private float m_delayTimer;
    private SplineComputer m_splineComputer;
    protected override void Awake()
    {
        base.Awake();

        m_splineComputer = GetComponentInChildren<SplineComputer>();
    }

    public override void Shot()
    {
        if (m_bulletNum <= 0 || m_bulletSpeed <= 0f)
        {
            Debug.LogWarning("Cannot shot because BulletNum or BulletSpeed is not set.");
            return;
        }

        if (m_shooting)
        {
            return;
        }

        m_shooting = true;
        m_nowIndex = 0;
        m_delayTimer = 0f;
    }

    public virtual void ShootSplineProjectile(GameObject bullet)
    {
        Rigidbody rb;
        bullet.TryGetComponent(out rb);

        if (!rb)
        {
            D.AIError("No rigidbody found on this spline shot projectile!");
            return;
        }

        SplineTracer tracer;
        bullet.TryGetComponent(out tracer);

        if (!tracer)
        {
            D.AIError("No spline tracer was found on this spline shot projectile!");
            return;
        }

        if (!m_splineComputer)
        {
            D.AIError("No spline computer has been attached to this spline shot projectile!");
            return;
        }

        tracer.spline = m_splineComputer;

        Vector3 forceDirection = (m_splineComputer.GetPoint(1).position -
            m_splineComputer.GetPoint(0).position).normalized;

        rb.AddForce(forceDirection * m_bulletSpeed, ForceMode.Impulse);
    }

    protected virtual void Update()
    {
        if (m_shooting == false)
        {
            return;
        }

        if (m_delayTimer >= 0f)
        {
            m_delayTimer -= UbhTimer.instance.deltaTime;
            if (m_delayTimer >= 0f)
            {
                return;
            }
        }

        GameObject bullet = GetBulletGO(m_bulletOrigin.position);
        if (bullet == null)
        {
            m_shooting = false;
            //FinishedShot();
            return;
        }

        ShootSplineProjectile(bullet);

        m_nowIndex++;

        if (m_nowIndex >= m_bulletNum)
        {
            m_shooting = false;
            //FinishedShot();
        }
        else
        {
            m_delayTimer = m_betweenDelay;
            if (m_delayTimer <= 0f)
            {
                Update();
            }
        }
    }

}