using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class RandomTimeCondition : HNDCondition
    {
        [SerializeField]
        protected Vector2 m_RandomTimeSpan;

        protected float m_TimePassed;
        protected float m_NextTriggerTime;

        protected override void Awake()
        {
            base.Awake();

            m_NextTriggerTime = Random.Range(m_RandomTimeSpan.x, m_RandomTimeSpan.y);
        }

        protected void Update()
        {
            m_TimePassed += Time.deltaTime;

            if (m_TimePassed > m_NextTriggerTime)
            {
                SetConditionFulfilled(true, null);

                // set up next generation time
                m_NextTriggerTime = m_TimePassed + Random.Range(m_RandomTimeSpan.x, m_RandomTimeSpan.y);
            }
        }
    }
}