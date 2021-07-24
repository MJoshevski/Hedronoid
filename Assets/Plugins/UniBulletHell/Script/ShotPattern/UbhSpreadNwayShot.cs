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
    // "Set a center angle of shot. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_CenterAngle")]
    public float m_centerAngle = 180f;
    // "Set a angle between bullet and next bullet. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenAngle")]
    public float m_betweenAngle = 10f;
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

                UbhBullet bullet = GetBullet(pos);
                bullet.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));

                if (bullet == null)
                {
                    break;
                }

                float baseAngle = m_wayNum % 2 == 0 ? m_centerAngle - (m_betweenAngle / 2f) : m_centerAngle;

                float angle = UbhUtil.GetShiftedAngle(wayIndex, baseAngle, m_betweenAngle);

                //ShotBullet(bullet, bulletSpeed, null, angle);

                wayIndex++;
            }

            FiredShot();

            FinishedShot();
        }
    }
}