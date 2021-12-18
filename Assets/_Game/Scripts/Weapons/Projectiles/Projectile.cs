using Hedronoid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : HNDMonoBehaviour
{
    [Header("Collisions and overlaps")]
    [Tooltip("Which layers are allowed to collide with this gravity source?")]
    public LayerMask triggerLayers;
    public List<ParticleList.ParticleSystems> CollisionParticles = new List<ParticleList.ParticleSystems>();

    private Rigidbody m_rigidBody;
    private List<GameObject> m_collidedObjects = new List<GameObject>();
    protected override void Start()
    {
        base.Start();

        if (!m_rigidBody) TryGetComponent(out m_rigidBody);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsInLayerMask(collision)) return;

        if (!m_collidedObjects.Contains(collision.gameObject))
        {
            m_collidedObjects.Add(collision.gameObject);

            for (int i = 0; i < CollisionParticles.Count; i++)
            {
                ParticleHelper.PlayParticleSystem(CollisionParticles[i], transform.position, -collision.contacts[0].normal);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!IsInLayerMask(collision)) return;

        if (m_collidedObjects.Contains(collision.gameObject))
        {
            m_collidedObjects.Remove(collision.gameObject);
        }

    }
    void FixedUpdate()
    {
        if (m_rigidBody.velocity.normalized != Vector3.zero)
            m_rigidBody.transform.forward = m_rigidBody.velocity.normalized;
    }

    protected bool IsInLayerMask(Collision other)
    {
        return ((triggerLayers.value & (1 << other.gameObject.layer)) > 0);
    }
}
