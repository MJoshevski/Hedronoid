using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh hole circle shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/Hole Circle Shot")]
public class UbhHoleCircleShot : UbhBaseShot
{
    [Header("===== HoleCircleShot Settings =====")]
    // "Set a center angle of hole. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_HoleCenterAngle")]
    public float m_holeCenterAngle = 180f;
    // "Set a size of hole. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_HoleSize")]
    public float m_holeSize = 20f;
    // "Set a number of shot row."
    [FormerlySerializedAs("_RowNum")]
    public int m_rowNum = 5;
    // "Set a angle between bullet rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenRowAngle")]
    public float m_betweenRowAngle = 10f;
    // "Set distance between bullet rows. (0 to 360)"
    [FormerlySerializedAs("_RowDistance")]
    public float m_rowDistance = 0f;
    // "Set a center angle for the row alignment. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_StartAngle")]
    public float m_centerRowAngle = 180f;

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
            float baseAngleRow = m_rowNum % 2 == 0 ? m_centerRowAngle - (m_betweenRowAngle / 2f) : m_centerRowAngle;

            float angleRow = UbhUtil.GetShiftedAngle(j, baseAngleRow, m_betweenRowAngle);

            Vector3 pos = new Vector3(
                transform.position.x,
                transform.position.y + m_rowDistance,
                transform.position.z);

            Vector3 rot = new Vector3(
                transform.rotation.eulerAngles.x + angleRow,
                transform.rotation.eulerAngles.y,
                transform.rotation.eulerAngles.z);

            m_holeCenterAngle = UbhUtil.GetNormalizedAngle(m_holeCenterAngle);
            float startAngle = m_holeCenterAngle - (m_holeSize / 2f);
            float endAngle = m_holeCenterAngle + (m_holeSize / 2f);

            float shiftAngle = 360f / (float)m_bulletNum;

            for (int i = 0; i < m_bulletNum; i++)
            {
                float angle = shiftAngle * i;
                if (startAngle <= angle && angle <= endAngle)
                {
                    continue;
                }

                UbhBullet bullet = GetBullet(pos);
                bullet.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));

                if (bullet == null)
                {
                    break;
                }

                //ShotBullet(bullet, m_bulletSpeed, null, angle);
            }

            FiredShot();

            FinishedShot();
        }
    }
}