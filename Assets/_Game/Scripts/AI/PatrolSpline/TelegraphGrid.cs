using UnityEngine;
using System.Collections.Generic;
using Hedronoid;

/// <summary>
/// 
/// </summary>
public class TelegraphGrid : HNDGameObject
{
    private Dictionary<int, TelegraphPosition> m_Positions = new Dictionary<int, TelegraphPosition>();

    public Dictionary<int, TelegraphPosition> Positions
    {
        get
        {
            return m_Positions;
        }

        set
        {
            m_Positions = value;
        }
    }

    protected override void Start ()
    {
        base.Start();
        TelegraphPosition[] positions = GetComponentsInChildren<TelegraphPosition>();
        foreach (TelegraphPosition tp in positions)
        {
            Positions.Add(tp.PosID, tp);
        }
    }
}
