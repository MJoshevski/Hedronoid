using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hedronoid;

namespace Hedronoid.TriggerSystem
{
    public class ToggleCondition : HNDCondition
    {
        [SerializeField]
        private bool m_Toggled = false;

        protected override void Start()
        {
            base.Start();
            if (m_Toggled)
                SetConditionFulfilled(true, cachedGameObject);
        }

        [ContextMenu("Toggle")]
        public void Toggle()
        {
            m_Toggled = !m_Toggled;
            if (m_Toggled)
                SetConditionFulfilled(true, cachedGameObject);
            else
                SetConditionUnfulfilled(cachedGameObject);
        }
    }
}