using UnityEngine;
using System.Collections;
using Hedronoid;

namespace Hedronoid.TriggerSystem
{
    public class DestroyGameObjectAction : HNDAction
    {

        // Used for grouping in inspector
        public static string path { get { return "Basic/"; } }

        [Header("'Destroy Game Object' Specific Settings")]
        [SerializeField]
        private bool m_DestroyTriggeringObject;
        [SerializeField]
        private GameObject[] m_DestroyGameObjects;

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);

            if (m_DestroyTriggeringObject)
            {
                HNDMonoBehaviour.CleanUpHNDMonoBehaviours(triggeringObject);
                Destroy(triggeringObject);
            }

            for (int i = 0; i < m_DestroyGameObjects.Length; i++)
            {
                if (m_DestroyGameObjects[i] != null)
                {
                    HNDMonoBehaviour.CleanUpHNDMonoBehaviours(m_DestroyGameObjects[i]);
                    Destroy(m_DestroyGameObjects[i]);
                }
            }
        }
    }
}