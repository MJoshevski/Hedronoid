using UnityEngine;
using System.Collections;
using Hedronoid;
using System.Collections.Generic;

namespace Hedronoid.TriggerSystem
{
    public enum ComparisonMethod
    {
        LESS_THAN,
        EQUAL,
        GREATER_THAN
    }

    /// <summary>
    /// Base action for all other trigger actions to derive from. Actions needs conditions to work (See HNDCondition class)
    /// </summary>
    public class HNDAction : HNDGameObject
    {
        public enum ConditionCombination
        {
            AND,
            OR
        }

        [Header("Basic Action Settings")]
        [Tooltip("If true, this trigger will automatically use all conditions on this game object.")]
        [SerializeField]
        private bool m_UseAllConditions = true;
        [Tooltip("If 'Use All Conditions' is false, you can specify a specific list of conditions that this trigger should use.")]
        [SerializeField]
        private HNDCondition[] m_Conditions;
        [Tooltip("If 'AND' is chosen, all selected conditions need to be true before the action is triggered. If 'OR' is chosen, only one condition need to be true.")]
        [SerializeField]
        private ConditionCombination m_ConditionCombination;
        [Tooltip("If this is true, this action will check id conditions are fulfilled as soon as it is activated in the scene.")]
        [SerializeField]
        private bool m_CheckConditionsOnEnable;
        [Tooltip("If this is true, the trigger action will be executed every frame as long as the conditions are met.")]
        [SerializeField]
        private bool m_TriggerContinously;
        [Tooltip("If this is true, the trigger action will be executed every frame as long as the conditions are met.")]
        [SerializeField]
        private bool m_TriggerContinouslyWithPhysics;
        [Tooltip("If this is true, the trigger action will revert whatever it did, when the conditions are no longer met.")]
        [SerializeField]
        private bool m_RevertOnUnfulfilled;
        [Tooltip("If this is true, the trigger action will only be triggered once in its lifetime.")]
        [SerializeField]
        private bool m_TriggerOnlyOnce;
        [Tooltip("Delay in seconds before the action is actually performed.")]
        [SerializeField]
        private float m_Delay;
        [Tooltip("Delay in seconds until the action can be triggered again.")]
        [SerializeField]
        private float m_ReactivateTime = 0;
        [Tooltip("Is this is true, this action will only be performed at the end of the current frame, meaning it will trigger after all actions that does not have this flag enabled.")]
        [SerializeField]
        private bool m_RerformLast;

        protected bool m_Toggle;

        private float m_DisabledCountdownCounter = 0; // Leftover time until the action becomes trigerrable again (reduced in Update() ).
        private bool m_HasBeenTriggered;
        private List<GameObject> m_ContinousTriggerObjects = new List<GameObject>();
        private IEnumerator m_PerformActionDelayedCR;

        public int TriggeredCount { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (m_UseAllConditions)
                m_Conditions = cachedGameObject.GetComponents<HNDCondition>();
        }

        protected override void Start()
        {
            base.Start();

            if (m_Conditions.Length == 0)
            {
                TriggerOnStart();
            }
        }

        private void TriggerIfConditionsAreFulfilled()
        {
            if (CheckConditions())
            {
                m_DisabledCountdownCounter = m_ReactivateTime;
                m_HasBeenTriggered = true;
                DoTrigger(cachedGameObject);
            }
        }

        private void DoTrigger(GameObject triggeringObject)
        {
            if (m_Delay == 0)
            {
                if (m_RerformLast)
                {
                    StartPerformActionDelayedCoroutine(triggeringObject);
                }
                else
                    PerformAction(cachedGameObject);
            }
            else
            {
                StartPerformActionDelayedCoroutine(triggeringObject);
            }
        }

        private void StartPerformActionDelayedCoroutine(GameObject triggeringObject)
        {
            if (m_PerformActionDelayedCR != null)
            {
                StopCoroutine(m_PerformActionDelayedCR);
            }
            m_PerformActionDelayedCR = PerformActionDelayed(triggeringObject);
            StartCoroutine(m_PerformActionDelayedCR);
        }

        protected void TriggerOnStart()
        {
            DoTrigger(cachedGameObject);

            AddContinousTriggeringObject(cachedGameObject);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            for (int i = 0; i < m_Conditions.Length; i++)
            {
                if (m_Conditions[i] != null)
                {
                    m_Conditions[i].HandleConditionFulfilled += OnConditionFulfilled;
                    m_Conditions[i].HandleConditionUnfulfilled += OnConditionUnfulfilled;
                }
            }

            if (m_CheckConditionsOnEnable)
            {
                if ((m_HasBeenTriggered && m_TriggerOnlyOnce) || m_DisabledCountdownCounter != 0)
                    return;
                TriggerIfConditionsAreFulfilled();
                if (m_TriggerContinously || m_TriggerContinouslyWithPhysics)
                    m_ContinousTriggerObjects = new List<GameObject>();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            for (int i = 0; i < m_Conditions.Length; i++)
            {
                if (m_Conditions[i] != null)
                {
                    m_Conditions[i].HandleConditionFulfilled -= OnConditionFulfilled;
                    m_Conditions[i].HandleConditionUnfulfilled -= OnConditionUnfulfilled;
                }
            }
        }

        protected virtual void Update()
        {
            m_DisabledCountdownCounter = Mathf.Max(m_DisabledCountdownCounter - Time.deltaTime, 0);

            if ((m_HasBeenTriggered && m_TriggerOnlyOnce) || m_DisabledCountdownCounter != 0)
                return;

            if (m_TriggerContinously)
            {
                if (CheckConditions())
                {
                    for (int i = 0; i < m_ContinousTriggerObjects.Count; i++)
                    {
                        PerformAction(m_ContinousTriggerObjects[i]);
                    }
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            if ((m_HasBeenTriggered && m_TriggerOnlyOnce) || m_DisabledCountdownCounter != 0)
                return;

            if (m_TriggerContinouslyWithPhysics)
            {
                if (CheckConditions())
                {
                    for (int i = 0; i < m_ContinousTriggerObjects.Count; i++)
                    {
                        PerformAction(m_ContinousTriggerObjects[i]);
                    }
                }
            }
        }

        protected virtual void PerformAction(GameObject triggeringObject)
        {
            TriggeredCount++;
            m_Toggle = true;
        }

        protected virtual void Revert(GameObject triggeringObject)
        {
            m_Toggle = false;
        }

        protected virtual void ResetCounter()
        {
            TriggeredCount = 0;
        }

        bool CheckConditions()
        {
            if (m_Conditions.Length == 0)
                return true;

            bool trigger = false;
            if (m_ConditionCombination == ConditionCombination.AND)
                trigger = true;

            for (int i = 0; i < m_Conditions.Length; i++)
            {
                if (m_Conditions[i].Fulfilled && m_ConditionCombination == ConditionCombination.OR)
                    trigger = true;
                if (!m_Conditions[i].Fulfilled && m_ConditionCombination == ConditionCombination.AND)
                    trigger = false;
            }

            return trigger;
        }

        protected virtual void OnConditionFulfilled(HNDCondition condition, GameObject triggeringObject)
        {
            if ((m_HasBeenTriggered && m_TriggerOnlyOnce) || m_DisabledCountdownCounter != 0)
                return;
            TriggerIfConditionsAreFulfilled();
            AddContinousTriggeringObject(triggeringObject);
        }

        IEnumerator PerformActionDelayed(GameObject triggeringObject)
        {
            m_HasBeenTriggered = true;
            if (m_Delay > 0)
                yield return new WaitForSeconds(m_Delay);
            if (m_RerformLast)
                yield return new WaitForEndOfFrame();
            PerformAction(triggeringObject);
        }

        protected virtual void OnConditionUnfulfilled(HNDCondition condition, GameObject triggeringObject)
        {
            if (!CheckConditions() && m_RevertOnUnfulfilled)
                Revert(triggeringObject);
            if (m_ContinousTriggerObjects.Contains(triggeringObject))
                m_ContinousTriggerObjects.Remove(triggeringObject);
        }

        private void AddContinousTriggeringObject(GameObject triggeringObject)
        {
            if (m_ContinousTriggerObjects.Contains(triggeringObject))
                return;

            if (m_TriggerContinously || m_TriggerContinouslyWithPhysics)
                m_ContinousTriggerObjects.Add(triggeringObject);
        }

        /// <summary>
        /// Force Perform Action. Use this only in editor! It's not intended to be used during actual gameplay
        /// </summary>
        /// <param name='remainFulfilled'>
        /// Remain fulfilled.
        /// </param>
        public void ForcePerformAction()
        {
            StopAllCoroutines();
            PerformAction(cachedGameObject);
        }
    }
}