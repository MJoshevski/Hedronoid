using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh circle shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/Circle Shot")]
public class UbhCircleShot : UbhBaseShot
{
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

        ParticleHelper.PlayParticleSystem(ShootParticles, m_bulletOrigin.position, -m_bulletOrigin.up);

        for (int j = 0; j < m_rowNum; j++)
        {
            float baseAngleRow = m_rowNum % 2 == 0 ? m_centerRowAngle - (m_betweenRowAngle / 2f) : m_centerRowAngle;

            float angleRow = UbhUtil.GetShiftedAngle(j, baseAngleRow, m_betweenRowAngle);

            float shiftAngle = 360f / (float)m_bulletNum;

            for (int i = 0; i < m_bulletNum; i++)
            {
                UbhBullet bullet = GetBullet(m_bulletOrigin.position);

                if (bullet == null)
                {
                    break;
                }

                float angle = shiftAngle * i;

                ShotBullet(bullet, m_bulletSpeed, angle, angleRow);
            }

            FiredShot();

            FinishedShot();
        }
    }
}