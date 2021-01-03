using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class DebugAction : HNDAction
    {

        [Header("'Debug' Specific Settings")]
        [SerializeField]
        public string m_LogMessage;

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);

            Debug.Log(m_LogMessage);
        }
    }
}