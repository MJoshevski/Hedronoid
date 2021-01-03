using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class ToggleAction : HNDAction
    {
        [Header("'Toggle' Specific Settings")]
        [SerializeField]
        private ToggleCondition m_ToggleCondition;

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);
            m_ToggleCondition.Toggle();
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);
            m_ToggleCondition.Toggle();
        }
    }
}