using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Hedronoid.Weapons;
using System.Collections;
using UnityEngine.AI;
using Hedronoid.AI;
using System;
using Hedronoid;
using Hedronoid.Events;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.Health
{
    [RequireComponent(typeof(DamageReactionController), typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class DamageHandler : HNDGameObject
    {
        [SerializeField]
        private string m_handlerID;
        public string HandlerID
        {
            get {
                return m_handlerID;
            }
        }
        [SerializeField]
        private Renderer m_BlinkRenderer;
        private List<Texture> m_NormalBlinkTexture = new List<Texture>();
        private List<Color> m_normalBlinkColor = new List<Color>();
        [SerializeField]
        private Texture m_DamageBlinkTexture;

        [SerializeField]
        private List<EDamageType> m_DamagesItCanRecieve = new List<EDamageType>();
        [SerializeField]
        private List<DamageMultiplier> m_DamageTypeMultipliers = new List<DamageMultiplier>();
        private EDamageType m_LastDamageType = EDamageType.Any;

        [SerializeField]
        private GameObject m_LODGroup;
        private GameObject m_LastDamage;
        [SerializeField]
        private bool m_Static = false;
        [SerializeField]
        private bool m_Invulnernable = false;

        [Tooltip("Wait time before damage can be dealt again")]
        [SerializeField]
        private float m_InvulnerableTime = 0.4f;
        [Tooltip("Force that pushes the hit object")]
        [SerializeField]
        private float m_damagePushForce = 0f;
        [SerializeField]
        [Tooltip("When the magnitude of the velocity of the rigidbody is equal or lower than this value, the NavMeshAgent will reactivate (If applicable)")]
        private float m_damageRecoverSpeed = 1.0f;
        private float m_LastDamageTimeStamp;

        [SerializeField]
        private List<GameObject> m_OnDeathSpawnObjects = new List<GameObject>();

        private Vector3 cachedScale;
        private Vector3 cachedRealScale;
        private Vector3 m_InitialScale;

        [SerializeField]
        private HealthBase m_Health;
        private DamageReactionController m_damageReactionController;
        private Collider m_Collider;
        private NavMeshAgent m_agent;

        public delegate void DamagedEvent(DamageHandler damagedHandler, DamageInfo damagedInfo, HealthBase health, HealthInfo healthInfo);
        public DamagedEvent damagedEvent;
        
        //public HitReaction hitReaction;

        protected override void Awake()
        {
            base.Awake();
            if (m_DamagesItCanRecieve.Count == 0)
            {
                m_DamagesItCanRecieve.Add(EDamageType.Any);
            }
            m_LastDamageTimeStamp = Time.time - 2f;
            cachedScale = transform.localScale;
            cachedRealScale = transform.localScale;
            if(m_Health == null)
                m_Health = GetComponent<HealthBase>();
            if(m_Health == null)
                m_Health = GetComponentInParent<HealthBase>();
            m_damageReactionController = GetComponent<DamageReactionController>();
            m_Collider = GetComponent<Collider>();
            m_agent = GetComponent<NavMeshAgent>();
            //if(!hitReaction) hitReaction = GetComponent<HitReaction>();
            //if(!hitReaction) hitReaction = GetComponentInParent<HitReaction>();
        }

        protected override void Start()
        {
            base.Start();
            m_InitialScale = transform.localScale;
            if (m_LODGroup)
            {
                foreach (Renderer renderer in m_LODGroup.GetComponentsInChildren<Renderer>())
                {
                    m_NormalBlinkTexture.Add(renderer.material.mainTexture);
                    m_normalBlinkColor.Add(renderer.material.color);
                }
            }
            else if (m_BlinkRenderer)
            {
                m_NormalBlinkTexture.Add(m_BlinkRenderer.material.mainTexture);
                m_normalBlinkColor.Add(m_BlinkRenderer.material.color);
            }
        }

        public void AddStunSource(IStunSource src)
        {
            m_Health.AddStunSource(src);
        }

        public void AddRootSource(IRootSource src)
        {
            m_Health.AddRootSource(src);
        }

        public void DoDamage(HNDDamage damage)
        {
            DamageInfo di = new DamageInfo();
            di.Damage = damage.damageValue;
            di.DamageDirection = damage.sender.forward.normalized;
            di.sender = damage.sender.gameObject;
            di.Type = Hedronoid.Weapons.EDamageType.Push;
            DoDamage (di);
        }

        public void DoDamage(DamageInfo e)
        {
            if (!gameObject.activeInHierarchy) { return; }
            if (!m_DamagesItCanRecieve.Contains(EDamageType.Any) && !m_DamagesItCanRecieve.Contains(e.Type)) return;
            if (m_LastDamageTimeStamp + m_InvulnerableTime > Time.time) return;
            if (IsInvulnerable) return;
            if (!gameObject.activeInHierarchy) return;
            m_LastDamageType = e.Type;
            m_LastDamageTimeStamp = Time.time;
            if ((e.Type == EDamageType.Impact_Above || e.Type == EDamageType.Impact_Side))
            {
                if (e.Type == EDamageType.Impact_Side && e.Col != null && !Static)
                {
                    Vector3 contactpoint = Vector3.zero;
                    foreach (var contact in e.Col.contacts)
                    {
                        contactpoint += contact.point;
                    }
                    if (m_BlinkRenderer)
                    {
                        StartCoroutine(Wobbly(contactpoint));
                    }
                }
            }
            m_LastDamage = e.sender;
            if (cachedRigidbody && e.DamageDirection != Vector3.zero && !Static && m_damagePushForce > 0f)
            {
                StartCoroutine(DelayedForce(e.DamageDirection));
            }
            var healthInfo = new HealthInfo { amount = -DamageAfterResistance(e) };
            
            m_Health.HealthInfoReciever(healthInfo);
            if (damagedEvent != null)
                damagedEvent(this, e, m_Health, healthInfo);

            if (m_damageReactionController)
                m_damageReactionController.React(e.Type);
            if (!m_IsBlinking && e.Damage != 0 && (m_BlinkRenderer || m_LODGroup) && m_DamageBlinkTexture)
            {
                StartCoroutine(Blink());
            }
        }

        public void MakeInvunerable(float time)
        {
            StartCoroutine(InvulnerableBlink(time));
        }

        IEnumerator InvulnerableBlink(float time)
        {
            m_Invulnernable = false;
            var timestamp = Time.time;
            bool rendererstate = true;
            while (timestamp + time > Time.time)
            {
                yield return new WaitForSeconds(0.1f);
                rendererstate = !rendererstate;
                if (m_LODGroup)
                {
                    m_LODGroup.SetActive(rendererstate);
                }
                else
                {
                    if (m_BlinkRenderer)
                        m_BlinkRenderer.enabled = rendererstate;
                }
            }
            if (m_LODGroup)
            {
                m_LODGroup.SetActive(true);
            }
            else
            {
                if (m_BlinkRenderer)
                    m_BlinkRenderer.enabled = true;
            }
            m_Invulnernable = false;
        }

        IEnumerator Wobbly(Vector3 point)
        {
            Vector4 hitLocal = transform.worldToLocalMatrix.MultiplyPoint(point);
            float timeStamp = Time.time;
            Renderer renderer = null;
            if (m_LODGroup)
            {
                Transform lodTransform = m_LODGroup.transform;
                foreach (Renderer childRenderer in lodTransform.GetComponentsInChildren<Renderer>())
                {
                    if (childRenderer.isVisible)
                    {
                        renderer = childRenderer;
                    }
                }
            }
            else
            {
                renderer = m_BlinkRenderer;
            }
            if (!renderer) yield break;
            renderer.material.SetFloat("_CurrentTime", Time.time);
            renderer.material.SetVector("_hitPosition", hitLocal);
            renderer.material.SetFloat("_hitTime", timeStamp);
            renderer.material.SetFloat("_hitDuration", 4.0f);
            renderer.material.SetFloat("_hitRadius", 2.5f);
            renderer.material.SetFloat("_hitForce", 2.5f);
            while (timeStamp + 4.4f > Time.time)
            {
                yield return new WaitForFixedUpdate();
                if (!renderer)
                {
                    break;
                }
                renderer.material.SetFloat("_CurrentTime", Time.time);
            }
            yield return null;
        }

        public void StopWobbly()
        {
            Renderer renderer = null;
            if (m_LODGroup)
            {
                Transform lodTransform = m_LODGroup.transform;
                foreach (Renderer childRenderer in lodTransform.GetComponentsInChildren<Renderer>())
                {
                    if (childRenderer.isVisible)
                    {
                        renderer = childRenderer;
                    }
                }
            }
            else
            {
                renderer = m_BlinkRenderer;
            }
            if (!renderer) return;
            renderer.material.SetVector("_hitPosition", new Vector3(99f, 99f, 99f));
            renderer.material.SetFloat("_hitTime", Time.time - 60f);
            renderer.material.SetFloat("_hitDuration", 1.0f);
            renderer.material.SetFloat("_hitRadius", 0.1f);
            renderer.material.SetFloat("_hitForce", 0.1f);
        }

        IEnumerator DelayedForce(Vector3 direction)
        {
            yield return new WaitForFixedUpdate();

            if(m_agent != null)
                m_agent.enabled = false;

            bool wasKinematic = false;
            if(cachedRigidbody.isKinematic) {
                cachedRigidbody.isKinematic = false;
                wasKinematic = true;
            }

            bool wasGravitational = cachedRigidbody.useGravity;
            cachedRigidbody.useGravity = true;

            cachedRigidbody.AddForce(direction * m_damagePushForce, ForceMode.Impulse);

            //Wait so that the rigidbody has a velocity we can compare to
            yield return new WaitForFixedUpdate();

            while(cachedRigidbody.velocity.magnitude > m_damageRecoverSpeed)
            {
                yield return new WaitForFixedUpdate();
            }

            if(wasKinematic) cachedRigidbody.isKinematic = true;
            if(!wasGravitational) cachedRigidbody.useGravity = false;

            if(m_agent != null)
                m_agent.enabled = true;
        }

        public bool IsInvulnerable
        {
            set
            {
                m_Invulnernable = value;

            }
            get { return m_Invulnernable; }
        }

        public bool Static
        {
            get
            {
                return m_Static;
            }

            set
            {
                m_Static = value;
            }
        }

        bool m_IsBlinking = false;

        private IEnumerator Blink()
        {
            if (!m_BlinkRenderer && !m_LODGroup) yield break;
            m_IsBlinking = true;

            if (m_LODGroup)
            {
                foreach (Renderer renderer in m_LODGroup.GetComponentsInChildren<Renderer>())
                {
                    renderer.material.mainTexture = m_DamageBlinkTexture;
                    renderer.material.SetColor("_Color", Color.red);
                }
            }
            else
            {
                m_BlinkRenderer.material.mainTexture = m_DamageBlinkTexture;
                m_BlinkRenderer.material.SetColor("_Color", Color.red);
            }
            yield return new WaitForSeconds(0.25f);
            if (m_LODGroup)
            {
                int i = 0;
                foreach (Renderer renderer in m_LODGroup.GetComponentsInChildren<Renderer>())
                {
                    renderer.material.mainTexture = m_NormalBlinkTexture[i];
                    renderer.material.SetColor("_Color", m_normalBlinkColor[i]);
                    i++;
                }
            }
            else
            {
                m_BlinkRenderer.material.mainTexture = m_NormalBlinkTexture[0];
                m_BlinkRenderer.material.SetColor("_Color", m_normalBlinkColor[0]);
            }
            m_IsBlinking = false;

        }
        
        private float DamageAfterResistance(DamageInfo e)
        {
            float amount = e.Damage;
            foreach(var dm in m_DamageTypeMultipliers)
            {
                if(dm.damageType == e.Type)
                    amount *= dm.damageMultiplier;
            }
            return amount;
        }

        private IEnumerator Freeze()
        {
            if (!m_BlinkRenderer && !m_LODGroup) yield break;

            if (m_LODGroup)
            {
                foreach (Renderer renderer in m_LODGroup.GetComponentsInChildren<Renderer>())
                {
                    renderer.material.mainTexture = m_DamageBlinkTexture;
                    renderer.material.SetColor("_Tint", Color.blue);

                }
            }
            else
            {
                m_BlinkRenderer.material.mainTexture = m_DamageBlinkTexture;
                m_BlinkRenderer.material.SetColor("_Tint", Color.blue);
            }
            yield return new WaitForSeconds(3f);
            if (m_LODGroup)
            {
                int i = 0;
                foreach (Renderer renderer in m_LODGroup.GetComponentsInChildren<Renderer>())
                {
                    renderer.material.mainTexture = m_NormalBlinkTexture[i];
                    renderer.material.SetColor("_Tint", Color.white);
                    i++;
                }
            }
            else
            {
                m_BlinkRenderer.material.mainTexture = m_NormalBlinkTexture[0];
                m_BlinkRenderer.material.SetColor("_Tint", Color.white);
            }
        }

        public void Die()
        {
            StopWobbly();

            if (gameObject.activeInHierarchy)
            {
                StopAllCoroutines();
            }
            else
            {
                gameObject.SetActive(false);
            }

            // Instantiate object on die (explosion, etc...)
            if (m_OnDeathSpawnObjects.Count > 0)
            {
                for (int i = 0; i < m_OnDeathSpawnObjects.Count; i++)
                {
                    GameObject.Instantiate(m_OnDeathSpawnObjects[i], cachedTransform.position, Quaternion.identity);
                }

                // HACK : to destory object on explosion, etc..
                Destroy(cachedGameObject, 0.15f);
            }
        }
    }

    public class DamageInfo
    {
        public float Damage;
        public Vector3 DamageDirection;
        public GameObject sender;
        public EDamageType Type;
        public Collision Col;
    }

    [Serializable]
    public class DamageMultiplier
    {
        public EDamageType damageType;
        public float damageMultiplier;
    }

    public interface IStunSource
    {
        bool Expired();
    }

    public interface IRootSource
    {
        bool Expired();
    }

    public static class HNDDamageHelper
    {
        /// <summary>
        /// Apply damage to gameObject if <see cref="CanReceiveDamage(GameObject)"/>
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="damage"></param>
        public static void ApplyDamage(this GameObject receiver, Hedronoid.HNDDamage damage)
        {
            var receivers = receiver.GetComponents<DamageHandler>();
            if (receivers != null)
            {
                for (int i = 0; i < receivers.Length; i++)
                {
                    receivers[i].DoDamage(damage);
                }
            }
        }

    }
}