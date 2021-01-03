using UnityEngine;
using System.Collections.Generic;
using Hedronoid;

/// <summary>
/// 
/// </summary>
public class AutoFollow : HNDGameObject
{
    private Transform m_Target;
    private Vector3 m_Offset = Vector3.zero;

    public Transform Target
    {
        get
        {
            return m_Target;
        }

        set
        {
            m_Target = value;
        }
    }

    public Vector3 Offset
    {
        get
        {
            return m_Offset;
        }

        set
        {
            m_Offset = value;
        }
    }

    protected override void Start ()
    {
        base.Start();
	}
	
	void Update ()
    {
        if (m_Target && m_Target.gameObject.activeSelf)
            transform.position = m_Target.position + m_Offset;
        else
            Destroy(gameObject);
	}
}
