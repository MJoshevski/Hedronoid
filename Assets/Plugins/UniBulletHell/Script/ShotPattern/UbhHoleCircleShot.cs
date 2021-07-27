using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh hole circle shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/Hole Circle Shot")]
public class UbhHoleCircleShot : UbhBaseShot
{
    [Header("===== HoleCircleShot Settings =====")]
    // "Set a number of shot row."
    [FormerlySerializedAs("_RowNum")]
    public int m_rowNum = 5;
    // "Set a size of hole. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_HoleSize")]
    public float m_holeSize = 20f;
    // "Set a center angle of rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_VerticalAngle")]
    public float m_verticalAngle = 180f;
    // "Set a center angle of shot. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_HorizontalAngle")]
    public float m_horizontalAngle = 180f;
    // "Set a angle between bullet rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenRowAngle")]
    public float m_betweenRowAngle = 10f;

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

            m_horizontalAngle = UbhUtil.GetNormalizedAngle(m_horizontalAngle);
            float startAngle = m_horizontalAngle - (m_holeSize / 2f);
            float endAngle = m_horizontalAngle + (m_holeSize / 2f);

            float shiftAngle = 360f / (float)m_bulletNum;

            for (int i = 0; i < m_bulletNum; i++)
            {
                float angle = shiftAngle * i;
                if (startAngle <= angle && angle <= endAngle)
                {
                    continue;
                }

                UbhBullet bullet = GetBullet(transform.position);

                if (bullet == null)
                {
                    break;
                }

                ShotBullet(bullet, m_bulletSpeed, angle, angleRow);
            }

            FiredShot();

            FinishedShot();
        }
    }
}