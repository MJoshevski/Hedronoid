using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh spread nway shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/Spread nWay Shot")]
public class UbhSpreadNwayShot : UbhBaseShot
{
    [Header("===== SpreadNwayShot Settings =====")]
    // "Set a number of shot way."
    [FormerlySerializedAs("_WayNum")]
    public int m_wayNum = 8;
    // "Set a number of shot row."
    [FormerlySerializedAs("_RowNum")]
    public int m_rowNum = 5;
    // "Set a center angle of rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_VerticalAngle")]
    public float m_verticalAngle = 180f;
    // "Set a center angle of shot. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_HorizontalAngle")]
    public float m_horizontalAngle = 180f;
    // "Set a angle between bullet and next bullet. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenAngle")]
    public float m_betweenAngle = 10f;
    // "Set a angle between bullet rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenRowAngle")]
    public float m_betweenRowAngle = 10f;
    // "Set a difference speed between shot and next line shot."
    [FormerlySerializedAs("_DiffSpeed")]
    public float m_diffSpeed = 0.5f;

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
    }

    private void Update()
    {
        if (m_shooting == false)
        {
            return;
        }

        for (int j = 0; j < m_rowNum; j++)
        {
            float baseAngleRow = m_rowNum % 2 == 0 ? m_verticalAngle - (m_betweenRowAngle / 2f) : m_verticalAngle;

            float angleRow = UbhUtil.GetShiftedAngle(j, baseAngleRow, m_betweenRowAngle);

            int wayIndex = 0;

            float bulletSpeed = m_bulletSpeed;

            for (int i = 0; i < m_bulletNum; i++)
            {
                if (m_wayNum <= wayIndex)
                {
                    wayIndex = 0;

                    bulletSpeed -= m_diffSpeed;
                    while (bulletSpeed <= 0)
                    {
                        bulletSpeed += Mathf.Abs(m_diffSpeed);
                    }
                }

                UbhBullet bullet = GetBullet(transform.position);

                if (bullet == null)
                {
                    break;
                }

                float baseAngle = m_wayNum % 2 == 0 ? m_horizontalAngle - (m_betweenAngle / 2f) : m_horizontalAngle;

                float angle = UbhUtil.GetShiftedAngle(wayIndex, baseAngle, m_betweenAngle);

                ShotBullet(bullet, bulletSpeed, angle, angleRow);

                wayIndex++;
            }

            FiredShot();

            FinishedShot();
        }
    }
}