using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Hedronoid;

/// <summary>
/// 
/// </summary>
public class TelegraphPosition : HNDGameObject
{
    [SerializeField]
    protected int m_PosID;

    public int PosID { get { return m_PosID; } }

    [SerializeField]
    protected GameObject m_TelegraphIndicator;
    [SerializeField]
    protected GameObject m_DamagePrefab;
    [SerializeField]
    protected LayerMask m_GroundLayers;
    [SerializeField]
    protected float m_GizmoSize = 2.4f;
    protected bool m_IsFiring = false;

    protected override void Start()
    {
        base.Start();
    }

    public void Fire(float delay, float minDelay)
    {
        StartCoroutine(DelayedFire(delay, minDelay));
    }

    IEnumerator DelayedFire(float delay, float minDelay)
    {
        RaycastHit hit;
        bool hasHit = false;
        m_IsFiring = true;
        yield return new WaitForSeconds(delay - minDelay);
        GameObject indicator = Instantiate(m_TelegraphIndicator);
        if (Physics.SphereCast(transform.position + Vector3.up * 2f, 0.5f, Vector3.down, out hit, 20f, m_GroundLayers, QueryTriggerInteraction.Ignore))
        {
            indicator.transform.position = hit.point + Vector3.up * 0.15f;
            hasHit = true;
        }
        else
        {
            D.AILog("RayCast Failed for: " + PosID);
            indicator.transform.position = transform.position;
        }
        yield return new WaitForSeconds(delay);
        if (indicator.GetComponent<TelegraphIndicator>())
        {
            indicator.GetComponent<TelegraphIndicator>().FadeAndDestroy(0.2f);
        }
        else
        {
            Destroy(indicator, 0.2f);
        }
        GameObject damage = Instantiate(m_DamagePrefab);
        if (hasHit)
        {
            damage.transform.position = hit.point;
        }
        else
        {
            damage.transform.position = transform.position;
        }
        yield return new WaitForSeconds(0.2f);
        m_IsFiring = false;
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_GizmoSize);
    }
#endif

}
