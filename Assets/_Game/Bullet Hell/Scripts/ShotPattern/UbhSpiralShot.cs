using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh spiral shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/Spiral Shot")]
public class UbhSpiralShot : UbhBaseShot
{
    [Header("===== SpiralShot Settings =====")]
    // "Set a number of shot row."
    [FormerlySerializedAs("_RowNum")]
    public int m_rowNum = 5;
    // "Set a center angle of rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_VerticalAngle")]
    public float m_verticalAngle = 180f;
    // "Set a center angle of shot. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_HorizontalAngle")]
    public float m_horizontalAngle = 180f;
    // "Set a shift angle of spiral. (-360 to 360)"
    [Range(-360f, 360f), FormerlySerializedAs("_ShiftAngle")]
    public float m_shiftAngle = 5f;
    // "Set a angle between bullet rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenRowAngle")]
    public float m_betweenRowAngle = 10f;
    // "Set a delay time between bullet and next bullet. (sec)"
    [FormerlySerializedAs("_BetweenDelay")]
    public float m_betweenDelay = 0.2f;

    private int m_nowIndex;
    private float m_delayTimer;

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

        for (int i = 0; i < m_rowNum; i++)
        {
            float baseAngleRow = m_rowNum % 2 == 0 ? m_verticalAngle - (m_betweenRowAngle / 2f) : m_verticalAngle;

            float angleRow = UbhUtil.GetShiftedAngle(i, baseAngleRow, m_betweenRowAngle);

            UbhBullet bullet = GetBullet(m_bulletOrigin.position);

            if (bullet == null)
            {
                FinishedShot();
                return;
            }

            float angle = m_horizontalAngle + (m_shiftAngle * m_nowIndex);

            ShotBullet(bullet, m_bulletSpeed, angle, angleRow);

            FiredShot();

            m_nowIndex++;

            if (m_nowIndex >= m_bulletNum)
            {
                FinishedShot();
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
}