using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh random shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/Random Shot")]
public class UbhRandomShot : UbhBaseShot
{
    [Header("===== RandomShot Settings =====")]
    // "Set a number of shot row."
    [FormerlySerializedAs("_RowNum")]
    public int m_rowNum = 5;
    // "Center angle of random range."
    [Range(0f, 360f), FormerlySerializedAs("_RandomCenterAngle")]
    public float m_verticalAngle = 180f;
    // "Set a center angle for the row alignment. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_StartAngle")]
    public float m_horizontalAngle = 180f;
    // "Set a angle between bullet rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenRowAngle")]
    public float m_betweenRowAngle = 10f;
    // "Set a angle between bullet rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenRowAngle")]
    public float m_betweenAngle = 10f;
    // "Set a angle size of random range. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_RandomRangeSize")]
    public float m_randomRangeSize = 360f;
    // "Set a minimum bullet speed of shot."
    // "BulletSpeed is ignored."
    [FormerlySerializedAs("_RandomSpeedMin")]
    public float m_randomSpeedMin = 1f;
    // "Set a maximum bullet speed of shot."
    // "BulletSpeed is ignored."
    [FormerlySerializedAs("_RandomSpeedMax")]
    public float m_randomSpeedMax = 3f;
    // "Set a minimum delay time between bullet and next bullet. (sec)"
    [FormerlySerializedAs("_RandomDelayMin")]
    public float m_randomDelayMin = 0.01f;
    // "Set a maximum delay time between bullet and next bullet. (sec)"
    [FormerlySerializedAs("_RandomDelayMax")]
    public float m_randomDelayMax = 0.1f;
    // "Evenly distribute of all bullet angle."
    [FormerlySerializedAs("_EvenlyDistribute")]
    public bool m_evenlyDistribute = true;

    private float m_delayTimer;

    private List<int> m_numList;

    public override void Shot()
    {
        if (m_bulletNum <= 0 || m_randomSpeedMin <= 0f || m_randomSpeedMax <= 0)
        {
            Debug.LogWarning("Cannot shot because BulletNum or RandomSpeedMin or RandomSpeedMax is not set.");
            return;
        }

        if (m_shooting)
        {
            return;
        }

        m_shooting = true;
        m_delayTimer = 0f;

        if (m_numList != null)
        {
            m_numList.Clear();
            m_numList = null;
        }

        m_numList = new List<int>(m_bulletNum);
        for (int i = 0; i < m_bulletNum; i++)
        {
            m_numList.Add(i);
        }
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

            int index = Random.Range(0, m_numList.Count);

            UbhBullet bullet = GetBullet(m_bulletOrigin.position);

            if (bullet == null)
            {
                return;
            }

            float bulletSpeed = Random.Range(m_randomSpeedMin, m_randomSpeedMax);

            float minAngle = m_horizontalAngle - (m_randomRangeSize / 2f);
            float maxAngle = m_horizontalAngle + (m_randomRangeSize / 2f);
            float angle = 0f;

            if (m_evenlyDistribute)
            {
                float oneDirectionNum = Mathf.Floor((float)m_bulletNum / 4f);
                float quarterIndex = Mathf.Floor((float)m_numList[index] / oneDirectionNum);
                float quarterAngle = Mathf.Abs(maxAngle - minAngle) / 4f;
                angle = Random.Range(minAngle + (quarterAngle * quarterIndex), minAngle + (quarterAngle * (quarterIndex + 1f)));
            }
            else
            {
                angle = Random.Range(minAngle, maxAngle);
            }

            ShotBullet(bullet, bulletSpeed, angle, angleRow);

            FiredShot();

            m_numList.RemoveAt(index);

            if (m_numList.Count <= 0)
            {
                FinishedShot();
            }
            else
            {
                m_delayTimer = Random.Range(m_randomDelayMin, m_randomDelayMax);
                if (m_delayTimer <= 0f)
                {
                    Update();
                }
            }
        }
    }
}