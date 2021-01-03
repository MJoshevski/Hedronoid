using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class CounterCondition : HNDCondition
    {
        [SerializeField]
        private int m_TargetCount;
        [SerializeField]
        private int m_InitialCount;
        [SerializeField]
        private ComparisonMethod m_Comparison;

        protected int m_Count;

        protected override void Awake()
        {
            base.Awake();

            m_Count = m_InitialCount;
        }

        public void IncreaseCount()
        {
            m_Count++;
            CheckCount();
        }

        public void DecreaseCount()
        {
            m_Count--;
            CheckCount();
        }

        void CheckCount()
        {
            if (m_Comparison == ComparisonMethod.EQUAL)
            {
                if (m_Count == m_TargetCount)
                {
                    if (!Fulfilled)
                        SetConditionFulfilled(true, null);
                }
                else
                {
                    if (Fulfilled)
                        SetConditionUnfulfilled(null);
                }
            }
            else if (m_Comparison == ComparisonMethod.GREATER_THAN)
            {
                if (m_Count > m_TargetCount)
                {
                    if (!Fulfilled)
                        SetConditionFulfilled(true, null);
                }
                else
                {
                    if (Fulfilled)
                        SetConditionUnfulfilled(null);
                }
            }
            else if (m_Comparison == ComparisonMethod.LESS_THAN)
            {
                if (m_Count < m_TargetCount)
                {
                    if (!Fulfilled)
                        SetConditionFulfilled(true, null);
                }
                else
                {
                    if (Fulfilled)
                        SetConditionUnfulfilled(null);
                }
            }
        }
    }
}