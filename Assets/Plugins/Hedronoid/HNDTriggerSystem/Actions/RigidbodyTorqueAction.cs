using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class RigidbodyTorqueAction : HNDAction
    {

        // Used for grouping in inspector
        public static string path { get { return "Rigidbody/"; } }

        [Header("'Rigidbody Torque' Specific Settings")]
        [SerializeField]
        private bool m_ApplyToCollidingObject;
        [SerializeField]
        private Rigidbody[] m_TargetRigidbodies;
        [SerializeField]
        private bool m_StopBeforeApplying;
        [SerializeField]
        private Vector3 m_Forward = Vector3.forward;
        [SerializeField]
        private float m_MinTorque;
        [SerializeField]
        private float m_MaxTorque;
        [SerializeField]
        private ForceMode m_ForceMode;

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);

            if (m_ApplyToCollidingObject)
            {
                Rigidbody rb = triggeringObject.GetComponent<Rigidbody>();
                if (rb != null)
                    ApplyTorque(rb);
            }

            for (int i = 0; i < m_TargetRigidbodies.Length; i++)
            {
                if (m_TargetRigidbodies[i] != null)
                    ApplyTorque(m_TargetRigidbodies[i]);
            }
        }

        void ApplyTorque(Rigidbody rb)
        {
            if (m_StopBeforeApplying)
                rb.angularVelocity = Vector3.zero;

            rb.AddTorque(m_Forward * Random.Range(m_MinTorque, m_MaxTorque), m_ForceMode);
        }
    }
}