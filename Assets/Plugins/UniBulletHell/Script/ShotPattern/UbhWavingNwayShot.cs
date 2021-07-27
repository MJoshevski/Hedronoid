using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh waving nway shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/Waving nWay Shot")]
public class UbhWavingNwayShot : UbhBaseShot
{
    [Header("===== WavingNwayShot Settings =====")]
    // "Set a number of shot way."
    [FormerlySerializedAs("_WayNum")]
    public int m_wayNum = 5;
    // "Set a number of shot row."
    [FormerlySerializedAs("_RowNum")]
    public int m_rowNum = 5;
    // "Set a center angle of rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_VerticalAngle")]
    public float m_verticalAngle = 180f;
    // "Set a center angle of shot. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_HorizontalAngle")]
    public float m_horizontalAngle = 180f;
    // "Set a size of wave range. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_WaveRangeSize")]
    public float m_waveRangeSize = 40f;
    // "Set a speed of wave. (0 to 10)"
    [Range(0f, 10f), FormerlySerializedAs("_WaveSpeed")]
    public float m_waveSpeed = 5f;
    // "Set a angle between bullet and next bullet. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenAngle")]
    public float m_betweenAngle = 5f;
    // "Set a angle between bullet rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenRowAngle")]
    public float m_betweenRowAngle = 10f;
    // "Set a delay time between shot and next line shot. (sec)"
    [FormerlySerializedAs("_NextLineDelay")]
    public float m_nextLineDelay = 0.1f;

    private int m_nowIndex;
    private float m_delayTimer;

    public override void Shot()
    {
        if (m_bulletNum <= 0 || m_bulletSpeed <= 0f || m_wayNum <= 0)
        {
            Debug.LogWarning("Cannot shot because BulletNum or BulletSpeed or WayNum is not set.");
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

        for (int j = 0; j < m_rowNum; j++)
        {
            float baseAngleRow = m_rowNum % 2 == 0 ? m_verticalAngle - (m_betweenRowAngle / 2f) : m_verticalAngle;

            float angleRow = UbhUtil.GetShiftedAngle(j, baseAngleRow, m_betweenRowAngle);

            for (int i = 0; i < m_wayNum; i++)
            {
                UbhBullet bullet = GetBullet(transform.position);

                if (bullet == null)
                {
                    break;
                }

                float centerAngle = m_horizontalAngle + (m_waveRangeSize / 2f * Mathf.Sin(UbhTimer.instance.totalFrameCount * m_waveSpeed / 100f));

                float baseAngle = m_wayNum % 2 == 0 ? centerAngle - (m_betweenAngle / 2f) : centerAngle;

                float angle = UbhUtil.GetShiftedAngle(i, baseAngle, m_betweenAngle);

                ShotBullet(bullet, m_bulletSpeed, angle, angleRow);

                m_nowIndex++;
                if (m_nowIndex >= m_bulletNum)
                {
                    break;
                }
            }

            FiredShot();

            if (m_nowIndex >= m_bulletNum)
            {
                FinishedShot();
            }
            else
            {
                m_delayTimer = m_nextLineDelay;
                if (m_delayTimer <= 0f)
                {
                    Update();
                }
            }
        }
    }
}