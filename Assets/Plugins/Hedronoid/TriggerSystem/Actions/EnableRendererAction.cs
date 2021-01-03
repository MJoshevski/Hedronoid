using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class EnableRendererAction : HNDAction
    {
        // Used for grouping in inspector
        public static string path { get { return "Basic/"; } }

        [Header("'Enable Renderer' Specific Settings")]
        [SerializeField]
        private Renderer m_TargetRenderer;

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);
            m_TargetRenderer.enabled = true;
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);
            m_TargetRenderer.enabled = false;
        }
    }
}