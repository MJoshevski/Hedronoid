using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class ActionCounterCondition : CounterCondition
    {
        [SerializeField]
        private HNDAction m_Action;

        void Update()
        {
            if (m_Action.TriggeredCount > m_Count)
            {
                IncreaseCount();
            }
            else if (m_Action.TriggeredCount < m_Count)
            {
                DecreaseCount();
            }
        }
    }
}