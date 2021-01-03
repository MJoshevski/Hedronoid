using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class EnableMonoBehaviourAction : HNDAction
    {

        // Used for grouping in inspector
        public static string path { get { return "Basic/"; } }

        [Header("'Enable Mono Behaviour' Specific Settings")]
        [SerializeField]
        private MonoBehaviour[] m_TargetBehaviours;
        [SerializeField]
        private bool m_ForceRevertToDisabled;

        private bool[] m_StatesWhenActivated;

        protected override void Awake()
        {
            base.Awake();

            m_StatesWhenActivated = new bool[m_TargetBehaviours.Length];
            for (int i = 0; i < m_TargetBehaviours.Length; i++)
            {
                if (m_TargetBehaviours[i] == null)
                    continue;
                m_StatesWhenActivated[i] = m_TargetBehaviours[i].enabled;
            }
        }

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);

            m_StatesWhenActivated = new bool[m_TargetBehaviours.Length];
            for (int i = 0; i < m_TargetBehaviours.Length; i++)
            {
                if (m_TargetBehaviours[i] == null)
                    continue;
                m_StatesWhenActivated[i] = m_TargetBehaviours[i].enabled;
                m_TargetBehaviours[i].enabled = true;
            }
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);
            for (int i = 0; i < m_TargetBehaviours.Length; i++)
            {
                if (m_TargetBehaviours[i] == null)
                    continue;
                if (m_StatesWhenActivated.Length <= i || m_ForceRevertToDisabled)
                    m_TargetBehaviours[i].enabled = false;
                else
                    m_TargetBehaviours[i].enabled = m_StatesWhenActivated[i];
            }
        }
    }
}