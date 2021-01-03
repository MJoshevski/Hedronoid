using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class DisableGameObjectAction : HNDAction
    {

        // Used for grouping in inspector
        public static string path { get { return "Basic/"; } }

        [Header("'Disable Game Object' Specific Settings")]
        [SerializeField]
        private GameObject[] m_TargetObjects;
        [Tooltip("Disable whatever object that was sent via condition.")]
        [SerializeField]
        private bool m_DisableCollided;
        [SerializeField]
        private bool m_OnlyExecuteOnChanged = false;

        private bool[] m_StatesWhenDeactivated;

        protected override void Awake()
        {
            base.Awake();

            m_StatesWhenDeactivated = new bool[m_TargetObjects.Length];
            for (int i = 0; i < m_TargetObjects.Length; i++)
            {
                if (m_TargetObjects[i] == null)
                    continue;
                m_StatesWhenDeactivated[i] = m_TargetObjects[i].activeSelf;
            }
        }

        protected override void PerformAction(GameObject triggeringObject)
        {
            if (!m_Toggle || !m_OnlyExecuteOnChanged)
            {
                base.PerformAction(triggeringObject);
                m_StatesWhenDeactivated = new bool[m_TargetObjects.Length];
                for (int i = 0; i < m_TargetObjects.Length; i++)
                {
                    if (m_TargetObjects[i] == null)
                        continue;
                    m_StatesWhenDeactivated[i] = m_TargetObjects[i].activeSelf;
                    m_TargetObjects[i].SetActive(false);
                }
            }

            if (m_DisableCollided)
            {
                triggeringObject.SetActive(false);
            }
        }

        protected override void Revert(GameObject triggeringObject)
        {
            if (m_Toggle || !m_OnlyExecuteOnChanged)
            {
                base.Revert(triggeringObject);
                for (int i = 0; i < m_TargetObjects.Length; i++)
                {
                    if (m_TargetObjects[i] == null)
                        continue;
                    if (m_StatesWhenDeactivated.Length <= i)
                        m_TargetObjects[i].SetActive(true);
                    else
                        m_TargetObjects[i].SetActive(m_StatesWhenDeactivated[i]);
                }
            }

            if (m_DisableCollided)
            {
                triggeringObject.SetActive(true);
            }
        }
    }
}