using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class ChangeLightColorAction : HNDAction
    {
        [Header("'Change Light Color' Specific Settings")]
        [SerializeField]
        private Light m_TargetObject;
        [SerializeField]
        private Color m_ChangeToColor;

        private Color m_OriginalColor;

        protected override void Awake()
        {
            base.Awake();

            m_OriginalColor = m_TargetObject.GetComponent<Light>().color;
        }

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);

            m_TargetObject.GetComponent<Light>().color = m_ChangeToColor;
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);

            m_TargetObject.GetComponent<Light>().color = m_OriginalColor;
        }
    }
}