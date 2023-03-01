using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh paint shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/Paint Shot")]
public class UbhPaintShot : UbhBaseShot
{
    private static readonly string[] SPLIT_VAL = { "\n", "\r", "\r\n" };

    [Header("===== PaintShot Settings =====")]
    // "Set a paint data text file. (ex.[UniBulletHell] > [Example] > [PaintShotData] in Project view)"
    // "BulletNum is ignored."
    [FormerlySerializedAs("_PaintDataText")]
    public TextAsset m_paintDataText;
    // "Set a center angle of rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_VerticalAngle")]
    public float m_verticalAngle = 180f;
    // "Set a center angle of shot. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_HorizontalAngle")]
    public float m_horizontalAngle = 180f;
    // "Set a angle between bullet and next bullet. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenAngle")]
    public float m_betweenAngle = 3f;
    // "Set a angle between bullet rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenRowAngle")]
    public float m_betweenRowAngle = 3f;
    // "Set a delay time between shot and next line shot. (sec)"
    [FormerlySerializedAs("_NextLineDelay")]
    public float m_nextLineDelay = 0.1f;

    private int m_nowIndex;
    private float m_delayTimer;

    private List<List<int>> m_paintData;
    private float m_paintStartAngle;

    public override void Shot()
    {
        if (m_bulletSpeed <= 0f || m_paintDataText == null || string.IsNullOrEmpty(m_paintDataText.text))
        {
            Debug.LogWarning("Cannot shot because BulletSpeed or PaintDataText is not set.");
            return;
        }

        if (m_shooting)
        {
            return;
        }

        if (m_paintData != null)
        {
            for (int i = 0; i < m_paintData.Count; i++)
            {
                m_paintData[i].Clear();
                m_paintData[i] = null;
            }
            m_paintData.Clear();
            m_paintData = null;
        }

        m_paintData = LoadPaintData();
        if (m_paintData == null || m_paintData.Count <= 0)
        {
            Debug.LogWarning("Cannot shot because PaintDataText load error.");
            return;
        }

        m_paintStartAngle = m_horizontalAngle - (m_paintData[0].Count % 2 == 0 ?
                                                  (m_betweenRowAngle * m_paintData[0].Count / 2f) + (m_betweenRowAngle / 2f) :
                                                  m_betweenRowAngle * Mathf.Floor(m_paintData[0].Count / 2f));

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

        bool up = true;
        int index = m_paintData.Count % 2 == 0 ? m_paintData.Count / 2 : (int)Mathf.Floor(m_paintData.Count / 2f);

        for (int j = 0; j < m_paintData.Count; j++)
        {
            float baseAngleRow = m_paintData.Count % 2 == 0 ? m_verticalAngle - (m_betweenRowAngle / 2f) : m_verticalAngle;

            float angleRow = UbhUtil.GetShiftedAngle(j, baseAngleRow, m_betweenRowAngle);

            if (up) index = index + m_nowIndex;
            else index = index - m_nowIndex;

            List<int> lineData = m_paintData[index];

            for (int i = 0; i < lineData.Count; i++)
            {
                if (lineData[i] == 1)
                {
                    UbhBullet bullet = GetBullet(m_bulletOrigin.position);

                    if (bullet == null)
                    {
                        break;
                    }

                    float angle = m_paintStartAngle + (m_betweenAngle * i);

                    ShotBullet(bullet, m_bulletSpeed, angle, angleRow);
                }
            }

            FiredShot();

            m_nowIndex++;
            up = !up;

            if (m_nowIndex >= m_paintData.Count)
            {
                FinishedShot();
            }
        }
    }

    private List<List<int>> LoadPaintData()
    {
        if (m_paintDataText == null || string.IsNullOrEmpty(m_paintDataText.text))
        {
            Debug.LogWarning("Cannot load paint data because PaintDataText file is null or empty.");
            return null;
        }

        string[] lines = m_paintDataText.text.Split(SPLIT_VAL, System.StringSplitOptions.RemoveEmptyEntries);

        var paintData = new List<List<int>>(lines.Length);

        for (int i = 0; i < lines.Length; i++)
        {
            // lines beginning with "#" are ignored as comments.
            if (lines[i].StartsWith("#"))
            {
                continue;
            }

            // add line
            paintData.Add(new List<int>(lines[i].Length));

            for (int j = 0; j < lines[i].Length; j++)
            {
                // bullet is fired into position of "*".
                paintData[paintData.Count - 1].Add(lines[i][j] == '*' ? 1 : 0);
            }
        }

        return paintData;
    }
}