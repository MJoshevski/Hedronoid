using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh spiral multi shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/Spiral Multi Shot")]
public class UbhSpiralMultiShot : UbhBaseShot
{
    [Header("===== SpiralMultiShot Settings =====")]
    // "Set a number of shot spiral way."
    [FormerlySerializedAs("_SpiralWayNum")]
    public int m_spiralWayNum = 4;
    // "Set a number of shot row."
    [FormerlySerializedAs("_RowNum")]
    public int m_rowNum = 5;
    // "Set a angle between bullet rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenRowAngle")]
    public float m_betweenRowAngle = 10f;
    // "Set distance between bullet rows. (0 to 360)"
    [FormerlySerializedAs("_RowDistance")]
    public float m_rowDistance = 0f;
    // "Set a starting angle of shot. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_StartAngle")]
    public float m_startAngle = 180f;
    // "Set a center angle for the row alignment. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_StartAngle")]
    public float m_centerRowAngle = 180f;
    // "Set a shift angle of spiral. (-360 to 360)"
    [Range(-360f, 360f), FormerlySerializedAs("_ShiftAngle")]
    public float m_shiftAngle = 5f;
    // "Set a delay time between bullet and next bullet. (sec)"
    [FormerlySerializedAs("_BetweenDelay")]
    public float m_betweenDelay = 0.2f;

    private int m_nowIndex;
    private float m_delayTimer;

    public override void Shot()
    {
        if (m_bulletNum <= 0 || m_bulletSpeed <= 0f || m_spiralWayNum <= 0)
        {
            Debug.LogWarning("Cannot shot because BulletNum or BulletSpeed or SpiralWayNum is not set.");
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
            float baseAngleRow = m_rowNum % 2 == 0 ? m_centerRowAngle - (m_betweenRowAngle / 2f) : m_centerRowAngle;

            float angleRow = UbhUtil.GetShiftedAngle(j, baseAngleRow, 10);

            Vector3 pos = new Vector3(
                transform.position.x,
                transform.position.y + m_rowDistance,
                transform.position.z);

            Vector3 rot = new Vector3(
                transform.rotation.eulerAngles.x + angleRow,
                transform.rotation.eulerAngles.y,
                transform.rotation.eulerAngles.z);

            float spiralWayShiftAngle = 360f / m_spiralWayNum;

            for (int i = 0; i < m_spiralWayNum; i++)
            {
                UbhBullet bullet = GetBullet(pos);
                bullet.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));

                if (bullet == null)
                {
                    break;
                }

                float angle = m_startAngle + (spiralWayShiftAngle * i) + (m_shiftAngle * Mathf.Floor(m_nowIndex / m_spiralWayNum));

                ShotBullet(bullet, m_bulletSpeed, null, angle);

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
                m_delayTimer = m_betweenDelay;
                if (m_delayTimer <= 0f)
                {
                    Update();
                }
            }
        }
    }
}