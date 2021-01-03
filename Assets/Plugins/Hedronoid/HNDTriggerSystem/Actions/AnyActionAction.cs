using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace Hedronoid.TriggerSystem
{
    public class AnyActionAction : HNDAction
    {
        // Used for grouping in inspector
        public static string path { get { return "Basic/"; } }

        [SerializeField]
        private UnityEvent m_Action;

        [SerializeField]
        private UnityEvent m_RevertAction;

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);

            if (m_Action != null)
                m_Action.Invoke();
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);

            if (m_RevertAction != null)
                m_RevertAction.Invoke();
        }
    }
}