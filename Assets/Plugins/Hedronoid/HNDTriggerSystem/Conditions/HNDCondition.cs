using UnityEngine;
using System.Collections;
using Hedronoid;
using System.Collections.Generic;

namespace Hedronoid.TriggerSystem
{
    /// <summary>
    /// Base condition for all other conditions to inherit from. Conditions are used for trigger actions to know when to trigger
    /// </summary>
    public class HNDCondition : HNDGameObject
    {

        public delegate void OnConditionFulfilled(HNDCondition condition, GameObject triggeringObject);
        public OnConditionFulfilled HandleConditionFulfilled;
        public delegate void OnConditionUnfulfilled(HNDCondition condition, GameObject triggeringObject);
        public OnConditionUnfulfilled HandleConditionUnfulfilled;

        [SerializeField]
        private bool m_ForcedFulfilled = false;
        public bool Fulfilled {
            get { return (m_fulfilledList.Count > 0 || m_ForcedFulfilled) ; }
        }

        public int TriggeredCount { get; private set; }

        private List<GameObject> m_fulfilledList = new List<GameObject>();

        private Collider m_cachedCollider;
        public Collider cachedCollider
        {
            get
            {
                if (m_cachedCollider == null)
                    m_cachedCollider = GetComponent<Collider>();
                return m_cachedCollider;
            }
        }

        protected virtual void SetConditionFulfilled(bool remainFulfilled, GameObject triggeringObject, bool forcedFulfill = false)
        {
            if(forcedFulfill)
                m_ForcedFulfilled = true;
            if (!m_fulfilledList.Contains(triggeringObject))
                m_fulfilledList.Add(triggeringObject);
            if (HandleConditionFulfilled != null)
                HandleConditionFulfilled(this, triggeringObject);
            if (!remainFulfilled && m_fulfilledList.Contains(triggeringObject))
                m_fulfilledList.Remove(triggeringObject);
            TriggeredCount++;
        }

        protected virtual void SetConditionUnfulfilled(GameObject triggeringObject, bool forcedFulfill = false)
        {
            if (forcedFulfill)
                m_ForcedFulfilled = false;
            if (m_fulfilledList.Contains(triggeringObject))
                m_fulfilledList.Remove(triggeringObject);
            if (HandleConditionUnfulfilled != null)
                HandleConditionUnfulfilled(this, triggeringObject);
        }

        /// <summary>
        /// Force fulfil. Use this only in editor! It's not intended to be used during actual gameplay
        /// </summary>
        /// <param name='remainFulfilled'>
        /// Remain fulfilled.
        /// </param>
        public void ForceFulfil(bool remainFulfilled)
        {
            SetConditionFulfilled(remainFulfilled, cachedGameObject);
        }

        /// <summary>
        /// Force unfulfil. Use this only in editor! It's not intended to be used during actual gameplay
        /// </summary>
        public void ForceUnfulfil()
        {
            SetConditionUnfulfilled(cachedGameObject);
        }
    }
}