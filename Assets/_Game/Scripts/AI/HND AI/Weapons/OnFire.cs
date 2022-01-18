using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Hedronoid.Weapons;
using Hedronoid;
using Hedronoid.Events;

/// <summary>
/// 
/// </summary>

namespace Hedronoid.Fire
{
    public enum FireStatus
    {
        NotOnFire = 0,
        OnFire
    }
    
    public class OnFire : HNDGameObject
    {
        [SerializeField]
        private FireStatus m_Status;
        [SerializeField]
        protected GameObject m_FlamePrefab;
        [SerializeField]
        protected GameObject m_FlameDamagePrefab;
        [SerializeField]
        protected Vector3 m_FlameOffset;
        [SerializeField]
        protected bool m_Spread = true;
        [SerializeField]
        protected bool m_SpreadTransfer = true;
        [SerializeField]
        protected bool m_DamageOverTime = false;
        [SerializeField]
        protected float m_DamagePerSecond = 1f;
        [SerializeField]
        protected float m_PutOutTime = 10f;
        protected float m_PutOutTimer = 0f;
        [SerializeField]
        protected bool m_OnlyMagicFire = false;
        [SerializeField]
        protected string m_FireStart = "Fire_Start";
        [SerializeField]
        protected string m_FireAfterburn = "Fire_Afterburn";

        protected GameObject m_InstantiatedFlame;

        public FireStatus Status
        {
            get { return m_Status; }
            set { m_Status = value; }
        }

        protected override void Start()
        {
            base.Start();
        }

        public void SetOnFire(bool magicFire = false)
        {
            if (m_OnlyMagicFire && !magicFire) return;
            if (Status == FireStatus.OnFire)
            {
                m_PutOutTimer = 0f;
                return;
            }

            Status = FireStatus.OnFire;
            if (m_InstantiatedFlame)
            {
                m_InstantiatedFlame.GetComponent<ParticleSystem>().Stop();
                Destroy(m_InstantiatedFlame, 3f);
            }
            if (m_FlamePrefab)
            {
                m_InstantiatedFlame = Instantiate(m_FlamePrefab);
                m_InstantiatedFlame.transform.position = transform.position + m_FlameOffset;
                AutoFollow af = m_InstantiatedFlame.AddComponent<AutoFollow>();
                af.Target = transform;
                af.Offset = m_FlameOffset;
            }
            StartCoroutine(FireDamage());

            HNDEvents.Instance.Raise(new OnCatchFire { GOID = gameObject.GetInstanceID() });
        }

        public void PutOut()
        {
            if (Status != FireStatus.OnFire) return;
            Status = FireStatus.NotOnFire;
            if (m_InstantiatedFlame)
            {
                m_InstantiatedFlame.GetComponent<ParticleSystem>().Stop();
                Destroy(m_InstantiatedFlame, 3f);
            }
            Destroy(m_InstantiatedFlame, 3f);
            HNDEvents.Instance.Raise(new OnPutOutFire { GOID = gameObject.GetInstanceID() });
        }

        public void Update()
        {
            if(Status == FireStatus.OnFire)
            {
                m_PutOutTimer += Time.deltaTime;
                if (m_PutOutTimer >= m_PutOutTime)
                    PutOut();
            }
        }
        
        IEnumerator FireDamage()
        {
            while (Status == FireStatus.OnFire)
            {
                yield return new WaitForSeconds(m_FireDamageInterval);
                if (m_DamageOverTime)
                {
                    HNDEvents.Instance.Raise(new DoDamage
                    {
                        GOID = gameObject.GetInstanceID(),
                        Damage = m_DamagePerSecond * m_FireDamageInterval,
                        Skill = "",
                        Particle = m_FlameDamagePrefab,
                        sender = gameObject
                    });
                }
            }
        }
        private float m_FireDamageInterval = 3f;

        void OnCollisionEnter(Collision collision)
        {
            if (m_Spread && Status == FireStatus.OnFire && m_PutOutTimer > 0.5f)
            {
                OnFire fire = collision.gameObject.GetComponent<OnFire>();
                if (fire)
                {
                    fire.SetOnFire();
                    if (m_SpreadTransfer)
                        PutOut();
                }
            }
        }
    }
}