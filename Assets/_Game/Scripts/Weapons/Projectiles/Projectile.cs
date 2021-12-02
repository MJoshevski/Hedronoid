using Hedronoid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : HNDMonoBehaviour
{
    private Rigidbody m_rigidBody;
    public List<ParticleList.ParticleSystems> CollisionParticles = new List<ParticleList.ParticleSystems>();
    private List<GameObject> m_collidedObjects = new List<GameObject>();
    protected override void Start()
    {
        base.Start();

        if (!m_rigidBody) TryGetComponent(out m_rigidBody);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == this.gameObject || collision.gameObject.layer == this.gameObject.layer) return;
        
        if (!m_collidedObjects.Contains(collision.gameObject))
        {
            Debug.LogError("COLLIDED: " + gameObject.name);
            m_collidedObjects.Add(collision.gameObject);

            for (int i = 0; i < CollisionParticles.Count; i++)
            {
                ParticleHelper.PlayParticleSystem(CollisionParticles[i], collision.transform.position, collision.contacts[0].normal);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == this.gameObject || collision.gameObject.layer == this.gameObject.layer) return;

        if (m_collidedObjects.Contains(collision.gameObject))
        {
            m_collidedObjects.Remove(collision.gameObject);
        }

    }
    void FixedUpdate()
    {
        m_rigidBody.transform.forward = m_rigidBody.velocity.normalized;
    }
}
