using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class RigidbodyKinematicAction : HNDAction
    {
        // Used for grouping in inspector
        public static string path { get { return "Rigidbody/"; } }

        [Header("'Rigidbody Kinematic' Specific Settings")]
        [SerializeField]
        private bool m_SetTrue = true;
        [SerializeField]
        private Rigidbody[] m_Rigidbodies;

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);
            for (int i = 0; i < m_Rigidbodies.Length; i++)
            {
                m_Rigidbodies[i].isKinematic = m_SetTrue;
                if (!m_SetTrue) m_Rigidbodies[i].WakeUp();
            }
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);
            for (int i = 0; i < m_Rigidbodies.Length; i++)
            {
                m_Rigidbodies[i].isKinematic = !m_SetTrue;
                if (m_SetTrue) m_Rigidbodies[i].WakeUp();
            }
        }
    }
}