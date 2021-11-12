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
    // "Always aim to target."
    [FormerlySerializedAs("_Aiming")]
    public bool m_aiming;

    private int m_nowIndex;
    private float m_delayTimer;
    private SplineComputer m_splineComputer;
    private SplinePoint[] m_splinePoints;
    private float m_originalSqrMagnitude;
    protected override void Awake()
    {
        base.Awake();

        m_splineComputer = GetComponentInChildren<SplineComputer>();
        m_splinePoints = m_splineComputer.GetPoints();

        m_originalSqrMagnitude =
            (m_splinePoints[m_splinePoints.Length - 1].position - m_splinePoints[0].position).sqrMagnitude;
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
    private void AimTarget()
    {
        if (!m_targetTransform) return;

        Vector3 pointDir = (m_splinePoints[m_splinePoints.Length - 1].position - m_splinePoints[0].position).normalized;
        Vector3 bulletOriginDir = (m_splinePoints[m_splinePoints.Length - 1].position - m_bulletOrigin.position).normalized;

        Quaternion rot = Quaternion.FromToRotation(bulletOriginDir, pointDir);
        Vector3 parentPosModified = transform.parent.position;

        for (int i = 1; i < m_splinePoints.Length - 1; i++)
        {
            parentPosModified.y = m_splinePoints[i].position.y;
 
            Vector3 rotatedPos = m_splinePoints[i].position.Rotated(rot, parentPosModified);
            m_splinePoints[i].position = rotatedPos;
        }

        float aimedSqrMagnitude = (m_splinePoints[m_splinePoints.Length - 1].position - m_splinePoints[0].position).sqrMagnitude;
        float difference = aimedSqrMagnitude - m_originalSqrMagnitude;

        //for (int i = 1; i < m_splinePoints.Length - 1; i++)
        //    m_splinePoints[i].position.z += difference / m_splinePoints.Length;

        m_splinePoints[0].position = m_bulletOrigin.position;
        m_splinePoints[m_splinePoints.Length - 1].position = m_targetTransform.position;

        m_splineComputer.SetPoints(m_splinePoints);
    }
    protected virtual void Update()
    {
        if (m_aiming && !m_targetTransform) return;

        if (/*m_shooting &&*/ m_aiming)
        {
            AimTarget();
        }

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