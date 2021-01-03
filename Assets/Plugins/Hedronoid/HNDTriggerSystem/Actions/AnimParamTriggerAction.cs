using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hedronoid.TriggerSystem
{
    public class AnimParamTriggerAction : HNDAction
    {
        // Used for grouping in inspector
        public static string path { get { return "Animation/"; } }

        [SerializeField]
        private Animator m_Animator;

        [Tooltip("automatically fetch all triggers from the animator")]
        [SerializeField]
        protected bool m_FetchAllTriggers;

        [Tooltip("Trigger only one of parameters randomly.")]
        [SerializeField]
        protected bool m_TriggerOneRandom;

        [SerializeField]
        protected string[] m_AnimationTriggers;

        protected override void Awake()
        {
            base.Awake();

            if (m_Animator == null)
                m_Animator = GetComponent<Animator>();

            if (m_FetchAllTriggers)
                m_AnimationTriggers = new List<AnimatorControllerParameter>(m_Animator.parameters).FindAll(
                    ap => ap.type == AnimatorControllerParameterType.Trigger).Select(ap => ap.name).ToArray();
        }

        protected override void PerformAction(GameObject other)
        {
            if (m_AnimationTriggers.Length == 0)
                D.CoreWarning(name + ": No animation triggers available, not triggering anything.");
            else
            {
                if (m_TriggerOneRandom)
                    m_Animator.SetTrigger(m_AnimationTriggers[Random.Range(0, m_AnimationTriggers.Length)]);
                else
                {
                    foreach(var trigger in m_AnimationTriggers)
                    {
                        m_Animator.SetTrigger(trigger);
                    }
                }
            }
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);
            // do we want to revert the trigger?
        }
    }
}