using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using Hedronoid.AI;
using Hedronoid;

/// <summary>
/// 
/// </summary>
/// 
public struct TelegraphConfig
{
    public float delay;
    public int id;
}

public class TelegraphShoot : HNDGameObject
{
    [SerializeField]
    private float m_DelayAfterShoot = 3f;
    [SerializeField]
    private TelegraphGrid m_Grid;
    [SerializeField]
    private char m_PositionsDelimiter='|';
    [SerializeField]
    private char m_ConfigsDelimiter = ';';
    [Tooltip("Wave settings strings to be parsed. Use it like 'id;delay|id;delay|id;delay' if using default delimiters")]
    [SerializeField]
    private List<string> m_WavesPatterns = new List<string>();
    [Tooltip("Order of waves patterns to call based on the settings from Waves Patterns. Can use repeated patterns.")]
    [Header("Waves")]
    [SerializeField]
    private List<int> m_WavesOrder = new List<int>();
    [SerializeField]
    private List<int> m_WavesOrderStage2 = new List<int>();
    [SerializeField]
    private List<int> m_WavesOrderStage3 = new List<int>();
    [SerializeField]
    private List<int> m_WavesOrderStage4 = new List<int>();
    private List<List<int>> m_Waves = new List<List<int>>();
    private List<List<TelegraphConfig>> m_WavesConfigs = new List<List<TelegraphConfig>>();
    private int m_CurrentWave = 0;
    private AIBaseNavigation m_Navigation;

    private bool m_Shooting = false;
    private int m_CurrentStage = 0;

    public int CurrentWave
    {
        get { return m_CurrentWave; }
        set { m_CurrentWave = value; }
    }

    public int CurrentStage
    {
        get { return m_CurrentStage; }
        set { m_CurrentStage = value; }
    }

    protected override void Start ()
    {
        base.Start();
        ParseWaves();
        m_Navigation = GetComponent<AIBaseNavigation>();
        m_Waves.Add(m_WavesOrder);
        m_Waves.Add(m_WavesOrderStage2);
        m_Waves.Add(m_WavesOrderStage3);
        m_Waves.Add(m_WavesOrderStage4);
    }

    private void ParseWaves()
    {
        for(int i = 0; i < m_WavesPatterns.Count; i++)
        {
            m_WavesConfigs.Add(new List<TelegraphConfig>());
            string [] configs = m_WavesPatterns[i].Split(m_PositionsDelimiter);

            for(int j = 0; j < configs.Length; j++)
            {
                string[] parameters = configs[j].Split(m_ConfigsDelimiter);
                TelegraphConfig aux = new TelegraphConfig
                {
                    id = int.Parse(parameters[0]),
                    delay = float.Parse(parameters[1])
                };
                m_WavesConfigs[i].Add(aux);
            }
        }
    }
	
    public bool StartShooting()
    {
        if (m_Shooting) return false;
        m_Shooting = true;
        StartCoroutine(Shoot());
        return true;
    }

    IEnumerator Shoot()
    {

        List<TelegraphConfig> Configs = m_WavesConfigs[m_Waves[CurrentStage][CurrentWave]];

        float maxDelay = 0f;
        float minDelay = Mathf.Infinity;
        foreach (TelegraphConfig tc in Configs)
        {
            if (maxDelay < tc.delay)
                maxDelay = tc.delay;
            if (minDelay > tc.delay)
                minDelay = tc.delay;
        }
        foreach (TelegraphConfig tc in Configs)
        {
            m_Grid.Positions[tc.id].Fire(tc.delay, minDelay);
        }

        yield return new WaitForSeconds(maxDelay);

        CurrentWave++;
        CurrentWave %= m_Waves[CurrentStage].Count;
        yield return new WaitForSeconds(m_DelayAfterShoot);
        m_Shooting = false;

        // if (m_Navigation is FredNavigation)
        // {
        //     (m_Navigation as FredNavigation).ShotDone();
        // }

        if (m_Navigation is LeaderNavigation)
        {
            (m_Navigation as LeaderNavigation).ShotDone();
        }


        yield return null;
    }
}
