using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Hedronoid;
using Hedronoid.Events;
using Hedronoid.Weapons;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.Health
{
    public class HealthBase : HNDGameObject
    {
        [SerializeField]
        protected bool m_HasGlobalUI = false;
        [SerializeField]
        protected float m_MaxHealth = 10f;
        public float MaxHealth { get { return m_MaxHealth; } }
        [SerializeField]
        protected float m_CurrentHealth = 10f;
        protected float m_LastDamageTimestamp = 0f;
        public Action stunStarted;
        public Action stunEnded;
        protected List<IStunSource> m_stunSources;
        public Action rootStarted;
        public Action rootEnded;
        protected List<IRootSource> m_rootSources;
        public List<IRootSource> RootSources
        {
            get 
            {
                return m_rootSources;
            }
        }

        protected bool m_RecieveFriendlyFire = false;
        public bool RecieveFriendlyFire { get { return m_RecieveFriendlyFire; } }
        protected bool m_CanBeOneShotted = false;
        public delegate void HealthChanged(float m_CurrentHealth, float m_MaxHealth);
        public HealthChanged healthChangedEvent;
        public bool CanBeOneShotted
        {
            get { return m_CanBeOneShotted; }
            set { m_CanBeOneShotted = value; }
        }
        [SerializeField]
        protected bool m_shouldRagdollOnDeath;

        protected DamageHandler[] m_damageHandlers;

        [Header("Debug")]
        [SerializeField]
        protected GameObject HealthBarPrefab;
        [SerializeField]
        protected Transform m_rootTransform;
        [SerializeField]
        protected Vector3 textOffset = Vector3.zero;
        protected Slider m_healthBarSlider;
        protected RectTransform m_healthBar;

        protected override void Awake()
        {
            base.Awake();

            m_damageHandlers = GetComponents<DamageHandler>();
            if(m_damageHandlers != null || m_damageHandlers.Length == 0)
                m_damageHandlers = GetComponentsInChildren<DamageHandler>();

            if (m_HasGlobalUI)
            {
                m_healthBarSlider = GameObject.FindWithTag("HealthBar").GetComponent<Slider>();
                m_healthBar = m_healthBarSlider.GetComponent<RectTransform>();
                m_healthBarSlider.maxValue = MaxHealth;
            }

            m_stunSources = new List<IStunSource>();
            m_rootSources = new List<IRootSource>();
            InitHealthBar();
        }

        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {
            //UpdateRooted();
            //UpdateStunned();
            //UpdateHealthBarOrientation();
        }

        public void HealthInfoReciever(HealthInfo e)
        {
            if (!IsAlive()) return;
            m_CurrentHealth += e.amount;
            m_CurrentHealth = Mathf.Clamp(m_CurrentHealth, 0, e.canGoAboveMax ? m_MaxHealth + 1 : m_MaxHealth);
            if (m_CurrentHealth <= 0f)
            {
                foreach(DamageHandler dh in m_damageHandlers)
                {
                    dh.Die();
                }
                OnDeath();
            }
            m_LastDamageTimestamp = Time.time;
            HealthChangedUpdate();
        }

        private void OnDeath()
        {
            HNDEvents.Instance.Raise(new KillEvent { sender = gameObject, GOID = gameObject.GetInstanceID() });
            if(m_shouldRagdollOnDeath)
                SetFullRagdoll();
            if(m_healthBar) Destroy(m_healthBar.gameObject);
        }

        private void HealthChangedUpdate()
        {
            if(healthChangedEvent != null)
            {
                healthChangedEvent(m_CurrentHealth, m_MaxHealth);
            }
            UpdateHealthBar();
        }

        public void Revive()
        {
            m_CurrentHealth = m_MaxHealth;
            ReviveEvent e = new ReviveEvent {
                GOID = gameObject.GetInstanceID(),
                sender = gameObject
            };
            HNDEvents.Instance.Raise(e);
            HealthChangedUpdate();
        }

        public void InstaKill()
        {
            if (m_CurrentHealth <= 0f) return;
            m_CurrentHealth = 0f;
            m_LastDamageTimestamp = Time.time;
            OnDeath();
            HealthChangedUpdate();

            foreach (DamageHandler dh in m_damageHandlers)
            {
                dh.Die();
            }
        }

        public float CurrentHealth
        {
            get { return m_CurrentHealth; }
        }

        public float CurrentHealthFraction
        {
            get { return m_CurrentHealth / m_MaxHealth; }
        }

        public float TimeSinceLastDamage()
        {
            return Time.time - m_LastDamageTimestamp;
        }

        private void UpdateStunned()
        {
            if (m_stunSources.Count == 0)
                return;
            for (int i = 0; i < m_stunSources.Count; i++)
            {
                if (m_stunSources[i].Expired())
                    m_stunSources.RemoveAt(i);
            }
            if (m_stunSources.Count == 0 && stunEnded != null)
                stunEnded();
        }

        /// <summary>
        /// Exposed to bolt
        /// </summary>
        /// <returns>If there is a non-expired stun applied</returns>
        public bool IsStunned()
        {
            return m_stunSources.Count > 0;
        }

        private void UpdateRooted()
        {
            if (m_rootSources.Count == 0)
                return;
            for (int i = 0; i < m_rootSources.Count; i++)
            {
                if (m_rootSources[i].Expired())
                    m_rootSources.RemoveAt(i);
            }
            if (m_rootSources.Count == 0 && rootEnded != null)
                rootEnded();
        }

        /// <summary>
        /// Exposed to bolt
        /// </summary>
        /// <returns>If there is a non-expired root applied</returns>
        public bool IsRooted()
        {
            return m_rootSources.Count > 0f;
        }

        public bool IsAlive()
        {
            return m_CurrentHealth > 0;
        }

        public void AddStunSource(IStunSource source)
        {
            if(m_stunSources.Count == 0 && stunStarted != null)
                stunStarted();
            m_stunSources.Add(source);
        }

        public void AddRootSource(IRootSource source)
        {
            m_rootSources.Add(source);
            if(rootStarted!=null)
                rootStarted();
        }


        private void SetFullRagdoll()
        {
            Rigidbody[] rbodies = GetComponentsInChildren<Rigidbody>();
            foreach(Rigidbody rb in rbodies)
            {
                rb.isKinematic = false;
            }

            Animator animator = GetComponentInChildren<Animator>();
            animator.enabled = false;
        }

        private void UpdateHealthBar()
        {
            if(!m_healthBar) return;
            m_healthBarSlider.value = CurrentHealth;
        }

        
        private void InitHealthBar()
        {
            if (!m_HasGlobalUI)
            {
                if (!HealthBarPrefab) return;
                m_healthBar = Instantiate(HealthBarPrefab).GetComponent<RectTransform>();
                m_healthBar.transform.SetParent(transform);
                if (!m_rootTransform) m_rootTransform = transform;
                m_healthBarSlider = m_healthBar.GetComponentInChildren<Slider>();
                m_healthBarSlider.minValue = 0f;
                m_healthBarSlider.maxValue = MaxHealth;
            }

            UpdateHealthBar();
        }

        protected virtual void UpdateHealthBarOrientation()
        {
            if(!m_healthBar) return;
            m_healthBar.position = m_rootTransform.position + (Vector3.up * textOffset.y) + (Vector3.right * textOffset.x);
            m_healthBar.position = Vector3.MoveTowards(m_healthBar.transform.position, Camera.main.transform.position, textOffset.z);
            m_healthBar.LookAt(2*m_healthBar.position - Camera.main.transform.position);
        }

        //public void Reset(bool showPopupMsg = false)
        //{
        //    cachedRigidbody.velocity = Vector3.zero;
        //    cachedRigidbody.angularVelocity = Vector3.zero;
        //    m_CurrentHealth = m_MaxHealth;
        //    HNDEvents.Instance.Raise(new RecieveHealthEvent { GOID = gameObject.GetInstanceID() });
        //    HNDEvents.Instance.Raise(new RespawnEvent { GOID = gameObject.GetInstanceID() });
        //}
    }

    public class HealthInfo
    {
        public float amount;
        public bool canGoAboveMax; 
    }
}
