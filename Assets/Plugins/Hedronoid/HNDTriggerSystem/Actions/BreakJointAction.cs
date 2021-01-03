using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class BreakJointAction : HNDAction
    {

        // Used for grouping in inspector
        public static string path { get { return "Physics/"; } }

        [Header("'Break Joint' Specific Settings")]
        [SerializeField]
        private Joint m_Joint;

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);
            Destroy(m_Joint);
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);
        }
    }
}