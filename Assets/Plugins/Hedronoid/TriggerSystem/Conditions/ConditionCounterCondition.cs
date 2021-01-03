using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class ConditionCounterCondition : CounterCondition
    {
        [SerializeField]
        private HNDCondition m_Condition;

        void Update()
        {
            if (m_Condition.TriggeredCount > m_Count)
            {
                IncreaseCount();
            }
            else if (m_Condition.TriggeredCount < m_Count)
            {
                DecreaseCount();
            }
        }
    }
}