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
    // "Set a center angle of shot. (0 to 360) (center of first line)"
    [Range(0f, 360f), FormerlySerializedAs("_PaintCenterAngle")]
    public float m_paintCenterAngle = 180f;
    // "Set a center angle of shot. (0 to 360) (center of first line)"
    [Range(0f, 360f), FormerlySerializedAs("_PaintCenterAngle")]
    public float m_rowCenterAngle = 180f;
    // "Set a angle between bullet and next bullet. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenAngle")]
    public float m_betweenAngle = 3f;
    // "Set a angle between bullet rows. (0 to 360)"
    [Range(0f, 360f), FormerlySerializedAs("_BetweenRowAngle")]
    public float m_betweenRowAngle = 3f;
    // "Set a delay time between shot and next line shot. (sec)"
    [FormerlySerializedAs("_NextLineDelay")]
    public float m_nextLineDelay = 0.1f;
    // "Set distance between bullet rows. (0 to 360)"
    [FormerlySerializedAs("_RowDistance")]
    public float m_rowDistance = 0f;

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

        m_paintStartAngle = m_paintCenterAngle - (m_paintData[0].Count % 2 == 0 ?
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
            float baseAngleRow = m_paintData.Count % 2 == 0 ? m_rowCenterAngle - (m_betweenRowAngle / 2f) : m_rowCenterAngle;

            float angleRow = UbhUtil.GetShiftedAngle(j, baseAngleRow, m_betweenRowAngle);

            Vector3 pos = new Vector3(
                transform.position.x,
                transform.position.y + m_rowDistance,
                transform.position.z);

            Vector3 rot = new Vector3(
                transform.rotation.eulerAngles.x + angleRow,
                transform.rotation.eulerAngles.y,
                transform.rotation.eulerAngles.z);

            if (up) index = index + m_nowIndex;
            else index = index - m_nowIndex;

            List<int> lineData = m_paintData[index];

            for (int i = 0; i < lineData.Count; i++)
            {
                if (lineData[i] == 1)
                {
                    UbhBullet bullet = GetBullet(pos);
                    bullet.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));

                    if (bullet == null)
                    {
                        break;
                    }

                    float angle = m_paintStartAngle + (m_betweenAngle * i);

                    //ShotBullet(bullet, m_bulletSpeed, null, angle);
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