using UnityEngine;
using System.Collections.Generic;
using Hedronoid;

/// <summary>
/// 
/// </summary>
public class NPCExpelTrigger : HNDGameObject
{
    [SerializeField]
    private float m_ExpelForce = 3f;
    [SerializeField]
    private ForceMode m_ExpelForceMode = ForceMode.VelocityChange;
    Collider CachedCollider;

    protected override void Awake()
    {
        base.Awake();
        CachedCollider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        /*Vector3 nextPosition = CachedCollider.ClosestPointOnBounds(other.transform.position);
        nextPosition = other.transform.position + (other.transform.position - transform.position).normalized * ((SphereCollider)CachedCollider).radius * transform.localScale.magnitude;
        other.transform.position = nextPosition;*/
        var dir = (other.transform.position - transform.position);
        dir.y = 0f;
        if (other.attachedRigidbody)
            other.attachedRigidbody.AddForce((dir).normalized * m_ExpelForce, m_ExpelForceMode);
    }

    void OnTriggerStay(Collider other)
    {
        /*Vector3 nextPosition = CachedCollider.ClosestPointOnBounds(other.transform.position);
        nextPosition = other.transform.position + (other.transform.position - transform.position).normalized * ((SphereCollider)CachedCollider).radius * transform.localScale.magnitude;
        other.transform.position = nextPosition;*/
        var dir = (other.transform.position - transform.position);
        dir.y = 0f;
        if (other.attachedRigidbody)
            other.attachedRigidbody.AddForce((dir).normalized * m_ExpelForce/2f, m_ExpelForceMode);
    }
}
