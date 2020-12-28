using UnityEngine;
using Hedronoid;

namespace Hedronoid
{
    /// <summary>
    /// Every object that exists in the scene should inherit from this.
    /// </summary>
    public class HNDGameObject : HNDMonoBehaviour
    {

        private GameObject m_cachedGameObject;
        public GameObject cachedGameObject
        {
            get
            {
                if (m_cachedGameObject == null)
                    m_cachedGameObject = gameObject;
                return m_cachedGameObject;
            }
        }

        private Transform m_cachedTransform;
        public Transform cachedTransform
        {
            get
            {
                if (m_cachedTransform == null)
                    m_cachedTransform = transform;
                return m_cachedTransform;
            }
        }

        private Rigidbody m_cachedRigidbody;
        public Rigidbody cachedRigidbody
        {
            get
            {
                if (m_cachedRigidbody == null)
                    m_cachedRigidbody = GetComponent<Rigidbody>();
                return m_cachedRigidbody;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            m_cachedGameObject = gameObject;
            m_cachedTransform = transform;
            m_cachedRigidbody = GetComponent<Rigidbody>();
        }
    }
}