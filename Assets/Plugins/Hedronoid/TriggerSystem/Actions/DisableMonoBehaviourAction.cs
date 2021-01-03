using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class DisableMonoBehaviourAction : HNDAction
    {

        // Used for grouping in inspector
        public static string path { get { return "Basic/"; } }

        [Header("'Disable Mono Behaviour' Specific Settings")]
        [SerializeField]
        private MonoBehaviour[] m_TargetBehaviours;
        [SerializeField]
        private bool m_ForceRevertToEnabled;

        private bool[] m_StatesWhenDeactivated;

        protected override void Awake()
        {
            base.Awake();

            m_StatesWhenDeactivated = new bool[m_TargetBehaviours.Length];
            for (int i = 0; i < m_TargetBehaviours.Length; i++)
            {
                if (m_TargetBehaviours[i] == null)
                    continue;
                m_StatesWhenDeactivated[i] = m_TargetBehaviours[i].enabled;
            }
        }

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);

            m_StatesWhenDeactivated = new bool[m_TargetBehaviours.Length];
            for (int i = 0; i < m_TargetBehaviours.Length; i++)
            {
                if (m_TargetBehaviours[i] == null)
                    continue;
                m_StatesWhenDeactivated[i] = m_TargetBehaviours[i].enabled;
                m_TargetBehaviours[i].enabled = false;
            }
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);
            for (int i = 0; i < m_TargetBehaviours.Length; i++)
            {
                if (m_TargetBehaviours[i] == null)
                    continue;
                if (m_StatesWhenDeactivated.Length <= i || m_ForceRevertToEnabled)
                    m_TargetBehaviours[i].enabled = true;
                else
                    m_TargetBehaviours[i].enabled = m_StatesWhenDeactivated[i];
            }
        }
    }
}