using UnityEngine;
using System.Collections.Generic;
using Hedronoid;

/// <summary>
/// 
/// </summary>
public class Arrow : HNDGameObject
{
    [SerializeField]
    private AnimationCurve m_Trajectory;
    [SerializeField]
    private float m_Speed = 1f;
    [SerializeField]
    private float m_Damage = 10f;
    [SerializeField]
    private GameObject m_ImpactParticle;
    [SerializeField]
    private GameObject m_FlyingParticle;

    private Vector3 m_Target;
    private Vector3 m_InitialPosition;
    private Vector3 m_MovingPos;
    private float m_InitialDistance;

    private bool m_Shooting = false;
    private float m_TimeOfShooting;
    private Vector3 vec;
    private float LastAvalDist = 2f;
    private float MovingAfter = 1f;

    public void ShootAt(Transform target)
    {
        transform.LookAt(target);
        m_Target = target.position;
        m_InitialPosition = transform.position;
        m_MovingPos = m_InitialPosition;
        m_InitialDistance = Vector3.Distance(m_MovingPos, m_Target);
        m_Shooting = true;
        m_TimeOfShooting = Time.time;
        vec = m_Target - m_MovingPos;
    }
	
	void Update ()
    {
        if (m_Shooting)
        {
            m_MovingPos = m_MovingPos + vec.normalized * m_Speed;
            var newPos = m_MovingPos;
            var avalDist = Vector3.Distance(m_MovingPos, m_Target) / m_InitialDistance;
            if (avalDist <= LastAvalDist)
                newPos.y += m_Trajectory.Evaluate(1f - (avalDist));
            else
            {
                MovingAfter += 0.05f;
                newPos.y += m_Trajectory.Evaluate(MovingAfter);
            }
            LastAvalDist = avalDist;
            transform.LookAt(newPos);
            transform.position = newPos;
        }
        if (m_TimeOfShooting + 5f < Time.time)
            Destroy(gameObject);
	}


    void OnCollisionEnter(Collision collision)
    {
        if (!m_Shooting) return;
        m_Shooting = false;
        if (m_ImpactParticle)
        {
            var particle = Instantiate(m_ImpactParticle);
            particle.transform.position = transform.position;
            Destroy(particle, 10f);
        }

        // @TODO
        // if (collision.gameObject.CompareTag("Player"))
        // {
        //     CharacterBase player = collision.gameObject.GetComponent<CharacterBase>();
        //     if (player)
        //         player.LoseMagicFromDamage(m_Damage);
        // }

        Destroy(gameObject);
        
    }
}
