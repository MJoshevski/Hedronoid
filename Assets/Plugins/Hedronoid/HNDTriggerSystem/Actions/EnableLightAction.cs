using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class EnableLightAction : HNDAction
    {
        [Header("'Enable Light' Specific Settings")]
        [SerializeField]
        private Light m_TargetLight;

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);
            m_TargetLight.enabled = true;
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);
            m_TargetLight.enabled = false;
        }
    }
}