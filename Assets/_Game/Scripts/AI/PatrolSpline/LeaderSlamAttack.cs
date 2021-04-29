using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using Hedronoid.Weapons;
using Hedronoid.Fire;
using Hedronoid.Events;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    /// <summary>
    /// This is used for Blockehead leaders to make an impact attack.
    /// A call to move will trigger the attack.
    /// </summary>
    public class LeaderSlamAttack : AIBaseMotor
    {
        [Header("Slam timing controls")]
        [SerializeField]
        [Tooltip("The blockhead will turn towards you before slamming so you can see what it will do. This is how far it turns.")]
        private float m_turnRate;
        [SerializeField]
        [Tooltip("After turning, The blockhead will wait a bit before slamming.")]
        private float m_windupTime = 1;
        [SerializeField]
        [Tooltip("After slamming, The blockhead will wait a bit before resuming navigation and turn back to where it was.")]
        private float m_cooldownTime = 1;
        [Header("Slam explosion setting")]
        [SerializeField]
        [Tooltip("This is the layers of the objects that are affected by the explosion")]
        private LayerMask m_objectsAffected;
        [SerializeField]
        [Tooltip("This is the force of the slam explosion")]
        private float m_explosiveForce = 1;
        [SerializeField]
        [Tooltip("This is the range of the explosion.")]
        private float m_explosionRadius = 1;
        [SerializeField]
        [Tooltip("This is the upwards modifier of the explosion so the player is proplelled into the air.")]
        private float m_explosionUpwardsModifier = 1;
        [SerializeField]
        [Tooltip("This is the particle effect of the slam explosion.")]
        private GameObject m_slamParticleEffect;

        [Header("Slam Bullets")]
        [SerializeField]
        protected bool m_UseBullets = false;
        [SerializeField]
        protected float m_BulletsRadius = 15;
        [SerializeField]
        protected float m_BulletsTargetDistance = 10;
        [SerializeField]
        protected float m_BulletsShootStrength = 10;
        [SerializeField]
        protected float m_BulletsDamage = 1;

        [Header("Slam to Position")]
        [SerializeField]
        protected float m_AngleToShoot=35;
        [SerializeField]
        protected List<Transform> m_Positions = new List<Transform>();
        [SerializeField]
        protected LayerMask m_ObjectsToSend;


        [Header("Damage against players")]
        [Tooltip("This is the damage done to players.")]
        [SerializeField]
        private float m_Damage = .5f;
        [SerializeField]
        private Animator m_Animator;

        private bool slamInProgress = false;
        private bool slamAnimationDone = false;
        [SerializeField]
        private GameObject m_BulletExplosionEffect;
        [SerializeField]
        private GameObject m_BulletImpactEffect;

        protected override void Start()
        {
            base.Awake();
            if(!m_Animator)
                m_Animator = GetComponent<Animator>();
        }

        public override void Move(Vector3 target)        
        {
            if (!slamInProgress)
            {
                StartCoroutine(Slam(target));
            }
        }

        public void SlamAnimationDone()
        {
            slamAnimationDone = true;
        }

        private IEnumerator Slam(Vector3 target)
        {
            slamInProgress = true;
            slamAnimationDone = false;

            // Turn towards the target direction while winding up, and then go, remove y so we don't look up or down.
            target.y = transform.position.y;
            var lookDir = (target - transform.position);
            var lookRot = Quaternion.LookRotation(lookDir.normalized);
            var remainingTime = m_windupTime;
            while ((remainingTime -= Time.deltaTime) > 0)
            {
                cachedRigidbody.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, m_turnRate * Time.deltaTime);
                yield return null;
            }

            // SoundRouter.CutsceneAudioFilter("Blockhead_Stomp");

            m_Animator.SetTrigger("SlamAttack");
            // Wait for the slam animation
            while (!slamAnimationDone)
            {
                yield return null;
            }
            
            // We have hit - do the explosion
            if (m_slamParticleEffect)
            {
                GameObject particle = Instantiate(m_slamParticleEffect, transform.position, Quaternion.identity);
                particle.transform.position = transform.position + Vector3.down * 8.4f;
                particle.transform.localScale = transform.localScale * 6.2f;
            }
            if (m_UseBullets)
            {
                List<Collider> collisionsTarget = new List<Collider>(Physics.OverlapSphere(transform.position, m_BulletsTargetDistance, LayerMask.GetMask(new string[] { "Players" })));
                if (collisionsTarget.Count > 0)
                {
                    var bullets = Physics.OverlapSphere(transform.position, m_BulletsRadius, LayerMask.GetMask(new string[] { "NPCs" }));
                    foreach (Collider npc in bullets)
                    {
                        int index = 0;
                        Vector3 npcPosCache = npc.transform.position;
                        while (collisionsTarget.Count >= index + 1 &&
                            Physics.Raycast(npcPosCache,
                                collisionsTarget[index].transform.position - npcPosCache,
                                Vector3.Distance(npcPosCache, collisionsTarget[index].transform.position) + 0.1f,
                                LayerMask.GetMask(new string[] { "Default" }))
                            &&
                            collisionsTarget[index].gameObject != gameObject
                            &&
                            collisionsTarget[index].GetComponent<NPC>()
                            &&
                            (collisionsTarget[index].GetComponent<NPC>().NpcType != NPC.NPCType.Zombie &&
                            collisionsTarget[index].GetComponent<NPC>().NpcType != NPC.NPCType.Firembie)
                            )
                        {
                            index++;
                        }
                        if (collisionsTarget.Count >= index + 1)
                        {
                            DirectionalBullet bul = npc.gameObject.AddComponent<DirectionalBullet>();
                            bul.Force = m_BulletsShootStrength;
                            bul.BulletDamage = m_BulletsDamage;
                            bul.UpwardsTime = 1f;
                            bul.UpwardsForce = 6f;
                            bul.SenderLayer = gameObject.layer;
                            bul.LayerToHit = LayerMask.NameToLayer("Players");
                            bul.Target = collisionsTarget[index].transform;
                            bul.TargetUpwardsForce = 2f;

                            OnFire fire = npc.GetComponent<OnFire>();
                            bul.ImpactParticle = fire && fire.Status == FireStatus.OnFire ? m_BulletExplosionEffect : m_BulletImpactEffect;
                            bul.FireMagicBullet();
                        }
                    }
                }

            }

            var targets = Physics.OverlapSphere(transform.position, m_explosionRadius, m_objectsAffected);
            for (int i = 0; i < targets.Length; i++)
            {
                NPC npc = targets[i].gameObject.GetComponent<NPC>();
                if (npc && m_UseBullets && (npc.NpcType == NPC.NPCType.Zombie || npc.NpcType == NPC.NPCType.Firembie))
                    continue;
                var rb = targets[i].GetComponent<Rigidbody>();
                var bullet = targets[i].GetComponent<Bullet>();

                if (rb && !bullet)
                {
                    //rb.AddExplosionForce(m_explosiveForce, transform.position, m_explosionRadius, m_explosionUpwardsModifier, ForceMode.VelocityChange);
                    var direction = (rb.transform.position - transform.position).normalized;
                    direction.y = 0f;
                    direction += Vector3.up * m_explosionUpwardsModifier; //rb.transform.position - (transform.position + Vector3.down * m_explosionUpwardsModifier);
                    rb.AddForce(direction.normalized * m_explosiveForce, ForceMode.VelocityChange);

                    HNDEvents.Instance.Raise(new DoDamagePlayer { Damage = m_Damage, RecieverGOID = rb.gameObject.GetInstanceID(), SenderGOID = gameObject.GetInstanceID() });
                }
            }
            if (m_Positions.Count > 0)
            {
                targets = Physics.OverlapSphere(transform.position, m_explosionRadius, m_ObjectsToSend);
                for (int i = 0; i < targets.Length; i++)
                {
                    var rb = targets[i].GetComponent<Rigidbody>();
                    if (rb)
                    {
                        rb.velocity = BallisiticVel(rb.transform,m_Positions[UnityEngine.Random.Range(0, m_Positions.Count)], m_AngleToShoot) * 100f;
                    }
                }
            }

            yield return new WaitForSeconds(m_cooldownTime);

            slamInProgress = false;
            if (m_Navigation is LeaderNavigation)
            {
                (m_Navigation as LeaderNavigation).SlamDone();
            }
            // if(m_Navigation is FredNavigation)
            // {
            //     (m_Navigation as FredNavigation).SlamDone();
            // }
        }

        Vector3 BallisiticVel(Transform obj, Transform target, float angle)
        {
            var dir = target.position - obj.position;
            float h = dir.y;
            dir.Normalize();
            dir.y = 0;
            float distance = dir.magnitude;
            float a = angle * Mathf.Deg2Rad;
            dir *= distance * Physics.gravity.magnitude / Mathf.Sqrt(distance * Physics.gravity.magnitude * 2f * Mathf.Tan(a));
            dir.y = Mathf.Sqrt(distance * Physics.gravity.magnitude * 2f * Mathf.Tan(a));            

            return dir;
        }
    }
}