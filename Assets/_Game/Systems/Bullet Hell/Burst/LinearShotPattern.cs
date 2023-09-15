using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LinearShotPattern", menuName = "Hedronoid/ShotPatterns/Burst/LinearShotPattern", order = 1)]

public class LinearShotPattern : BaseShotPattern
{
    [Header("===== LinearShot Settings =====")]
    // "Set a delay time between bullet and next bullet. (sec)"
    public float m_betweenDelay = 0.1f;
    // "Set a center angle of rows. (0 to 360)"
    [Range(0f, 360f)]
    public float m_verticalAngle = 180f;
    // "Set a center angle of shot. (0 to 360)"
    [Range(0f, 360f)]
    public float m_horizontalAngle = 180f;

    private int m_nowIndex;
    private float m_delayTimer;

    public override void Initialize()
    {
        if (m_bulletNum <= 0 || m_speed <= 0f)
        {
            Debug.LogWarning("Cannot shot because BulletNum or BulletSpeed is not set.");
            return;
        }

        if (m_isShooting)
        {
            return;
        }

        m_isShooting = true;
        m_nowIndex = 0;
        m_delayTimer = 0f;
    }

    public override void UpdateStatus()
    {
        if (m_isShooting == false)
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

        // Should be firing
        m_isShooting = true;

        m_nowIndex++;

        if (m_nowIndex >= m_bulletNum)
        {
            m_isShooting = false;
        }
        else
        {
            m_delayTimer = m_betweenDelay;
            if (m_delayTimer <= 0f)
            {
                UpdateStatus();
            }
        }
    }

    public override float GetHorizontalAngle()
    {
        return m_horizontalAngle;
    }

    public override float GetVerticalAngle()
    {
        return m_verticalAngle;
    }
}