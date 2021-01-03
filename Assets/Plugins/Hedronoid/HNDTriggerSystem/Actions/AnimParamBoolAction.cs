using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class AnimParamBoolAction : Hedronoid.TriggerSystem.HNDAction
    {
        // Used for grouping in inspector
        public static string path { get { return "Animation/"; } }

        [SerializeField]
        private Animator m_Animator;

        [Tooltip("List of bool parameters that will be set to value.")]
        [SerializeField]
        protected string[] m_BoolParams;

        [Tooltip("Value that will be set for all bool parameters")]
        [SerializeField]
        protected bool m_SetValue;

        protected override void Awake()
        {
            base.Awake();

            if (m_Animator == null)
                m_Animator = GetComponent<Animator>();
        }

        protected override void PerformAction(GameObject triggeringObject)
        {
            if (m_BoolParams.Length == 0)
                D.CoreWarning(name + ": No bool params available, not setting anything.");
            else
            {
                foreach(var boolParam in m_BoolParams)
                {
                    m_Animator.SetBool(boolParam, m_SetValue);
                }
            }
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);

            foreach (var boolParam in m_BoolParams)
            {
                m_Animator.SetBool(boolParam, !m_SetValue);
            }
        }
    }
}