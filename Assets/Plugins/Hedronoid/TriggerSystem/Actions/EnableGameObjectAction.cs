using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class EnableGameObjectAction : HNDAction
    {

        // Used for grouping in inspector
        public static string path { get { return "Basic/"; } }

        [Header("'Enable Game Object' Specific Settings")]
        [SerializeField]
        private GameObject[] m_TargetObjects;
        [SerializeField]
        private bool m_OnlyExecuteOnChanged = false;

        private bool[] m_StatesWhenActivated;

        
        protected override void Awake()
        {
            base.Awake();

            m_StatesWhenActivated = new bool[m_TargetObjects.Length];
            for (int i = 0; i < m_TargetObjects.Length; i++)
            {
                if (m_TargetObjects[i] == null)
                    continue;
                m_StatesWhenActivated[i] = m_TargetObjects[i].activeSelf;
            }
        }

        protected override void PerformAction(GameObject triggeringObject)
        {
            if (!m_Toggle || !m_OnlyExecuteOnChanged)
            {
                base.PerformAction(triggeringObject);

                m_StatesWhenActivated = new bool[m_TargetObjects.Length];
                for (int i = 0; i < m_TargetObjects.Length; i++)
                {
                    if (m_TargetObjects[i] == null)
                        continue;
                    m_StatesWhenActivated[i] = m_TargetObjects[i].activeSelf;
                    m_TargetObjects[i].SetActive(true);
                }
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
                    if (m_StatesWhenActivated.Length <= i)
                        m_TargetObjects[i].SetActive(false);
                    else
                        m_TargetObjects[i].SetActive(m_StatesWhenActivated[i]);
                }
            }
        }
    }
}