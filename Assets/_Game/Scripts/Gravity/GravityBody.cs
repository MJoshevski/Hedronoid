using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MDKShooter.Gravity
{
    public class GravityBody : MonoBehaviour
    {
        public GravityAttractor m_GravityAttractor;

        private Rigidbody m_RB;

        private void Awake()
        {
            m_RB = GetComponent<Rigidbody>();

            if (!m_RB) m_RB = GetComponentInChildren<Rigidbody>();
            if (!m_RB) m_RB = GetComponentInParent<Rigidbody>();
            if (!m_RB) Debug.LogError("No Rigidbody found on object!");
        }

        private void Start()
        {
            m_RB.useGravity = false;
        }

        private void Update()
        {
            m_GravityAttractor.Attract(m_RB);
        }
    }
}
