using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Hedronoid.TriggerSystem
{
    public class RandomGameObjectEnablerAction : HNDAction
    {
        // Used for grouping in inspector
        public static string path { get { return "Basic/"; } }

        [Header("'Random GameObject Enabler' Specific Settings")]
        [SerializeField]
        private GameObject[] m_TargetObjects;
        [SerializeField]
        private bool m_DisableAllFirst;
        [Tooltip("If you want the random enabled object to be each time different than previous time.")]
        [SerializeField]
        private bool m_DifferentObject;

        private List<GameObject> m_ObjectsEnabled = new List<GameObject>();

        private GameObject m_LastEnabled;

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);

            if (m_DisableAllFirst)
            {
                foreach (var targetObj in m_TargetObjects)
                {
                    targetObj.SetActive(false);
                }
            }

            var randomObj = m_TargetObjects[Random.Range(0, m_TargetObjects.Length)];
            if (m_DifferentObject && m_TargetObjects.Length > 1 && randomObj == m_LastEnabled)
            {
                while (randomObj == m_LastEnabled)
                {
                    randomObj = m_TargetObjects[Random.Range(0, m_TargetObjects.Length)];
                }
            }

            randomObj.SetActive(true);
            m_LastEnabled = randomObj;
            m_ObjectsEnabled.Add(randomObj);
        }

        protected override void Revert(GameObject triggeringObject)
        {
            foreach (var objEnabled in m_ObjectsEnabled)
            {
                objEnabled.SetActive(false);
            }
            m_ObjectsEnabled.Clear();

            base.Revert(triggeringObject);
        }
    }
}