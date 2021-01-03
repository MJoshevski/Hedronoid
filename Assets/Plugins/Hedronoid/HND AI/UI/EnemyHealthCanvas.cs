using UnityEngine;
using System.Collections.Generic;
using Hedronoid.Health;
using UnityEngine.UI;
using Hedronoid;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.UI
{
    public class EnemyHealthCanvas : HNDGameObject
    {
        [SerializeField]
        private HealthBase m_EnemyHealthBase;

        [SerializeField]
        private Image m_HealthBar;
        [SerializeField]
        private Image m_HealthBarBackground;

        [SerializeField]
        private float m_FOVFactor = 18f;
        [SerializeField]
        private float m_DistanceFactor = 10f;

        private Camera m_Camera;

        private Vector3 m_InitialScale;

        private float m_LastUpdate;
        private float m_Percent = -1;

        protected override void Awake()
        {
            base.Awake();
            if (!m_EnemyHealthBase)
            {
                m_EnemyHealthBase = transform.parent.GetComponent<HealthBase>();
            }
            m_InitialScale = m_HealthBar.rectTransform.localScale;
            UpdateHealth();

            m_Camera = Camera.main;
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateHealth();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UpdateHealth();
        }


        protected void UpdateHealth()
        {
            if (m_EnemyHealthBase.CurrentHealthFraction == m_Percent) return;
            m_Percent = m_EnemyHealthBase.CurrentHealthFraction;
            if (m_Percent > 0.99f || m_Percent < 0.01f)
            {
                m_HealthBar.enabled = false;
                m_HealthBarBackground.enabled = false;

            }
            else
            {
                m_HealthBar.enabled = true;
                m_HealthBarBackground.enabled = true;
                m_HealthBar.fillAmount = m_Percent;
                if (m_HealthBar.fillAmount <= 0f) m_HealthBar.fillAmount = 0.000001f;
            }
            m_LastUpdate = Time.time;
        }

        protected override void Start()
        {
            base.Start();
            m_LastUpdate = Time.time + 0.5f;
            Invoke("UpdateHealth", 0.5f);
        }

        void Update()
        {
            if (m_Camera)
            {
                transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
                    m_Camera.transform.rotation * Vector3.up);
                m_HealthBar.rectTransform.localScale = m_InitialScale * (1f / m_FOVFactor * m_Camera.fieldOfView + Vector3.Distance(transform.position, m_Camera.transform.position) * 1f / m_DistanceFactor);
                m_HealthBarBackground.rectTransform.localScale = m_HealthBar.rectTransform.localScale;
            }

            if (m_LastUpdate + 0.1f < Time.time)
            {
                UpdateHealth();
            }
        }

    }

}
