using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Hedronoid.TriggerSystem
{
    public class AnimParamFloatAction : HNDAction
    {
        public enum eFloatParamAnimMode
        {
            OneValue,
            ChangeOverTimeWithCurve
        }

        // Used for grouping in inspector
        public static string path { get { return "Animation/"; } }

        [SerializeField]
        private Animator m_Animator;

        [Tooltip("List of float parameters that will be set to value.")]
        [SerializeField]
        protected List<string> m_FloatParams;

        [Tooltip("Mode of changing float parameter. Either changes to value or runs a change over a curve (useful for e.g. value connected to BlendTree)")]
        [SerializeField]
        protected eFloatParamAnimMode m_FloatAnimParamMode;
        public eFloatParamAnimMode FloatAnimParamMode
        {
            get { return m_FloatAnimParamMode; }
            set { m_FloatAnimParamMode = value; }
        }

        [Tooltip("Value that will be set for all float parameters")]
        [SerializeField]
        protected float m_SetValue;

        [Tooltip("Normalized curve for changing the value of float parameter. Will get run over time with span of values.")]
        [SerializeField]
        protected AnimationCurve m_ValueCurve;

        [Tooltip("Span for changing the values with the Value Curve.")]
        [SerializeField]
        protected Vector2 m_CurveValueSpan;

        [Tooltip("Duration of curve changing.")]
        [SerializeField]
        protected float m_CurveDuration;

        [SerializeField]
        [Tooltip("When reverting an action, when the curve is running, should it be stopped?")]
        protected bool m_StopCurveOnRevert;

        private List<CoroutineHolder> m_RunningCurves = new List<CoroutineHolder>();
        private class CoroutineHolder
        {
            public Coroutine coroutine;
        }

        private Dictionary<string, float> m_originalParamValues = new Dictionary<string, float>();

        protected override void Awake()
        {
            base.Awake();

            if (m_Animator == null)
                m_Animator = GetComponent<Animator>();
        }

        protected override void PerformAction(GameObject other)
        {
            if (m_FloatParams.Count == 0)
                D.CoreWarning(name + ": No bool params available, not setting anything.");
            else
            {
                switch(m_FloatAnimParamMode)
                {
                    case eFloatParamAnimMode.OneValue:
                        foreach (var floatParam in m_FloatParams)
                        {
                            if (!m_originalParamValues.ContainsKey(floatParam))
                            {
                                m_originalParamValues.Add(floatParam, m_Animator.GetFloat(floatParam));
                            }
                            else
                            {
                                m_originalParamValues[floatParam] = m_Animator.GetFloat(floatParam);
                            }
                            m_Animator.SetFloat(floatParam, m_SetValue);
                        }
                        break;

                    case eFloatParamAnimMode.ChangeOverTimeWithCurve:
                        foreach (var floatParam in m_FloatParams)
                        {
                            if (!m_originalParamValues.ContainsKey(floatParam))
                            {
                                m_originalParamValues.Add(floatParam, m_Animator.GetFloat(floatParam));
                            }
                            else
                            {
                                m_originalParamValues[floatParam] = m_Animator.GetFloat(floatParam);
                            }

                            var corHolder = new CoroutineHolder();
                            var cor = StartCoroutine(ChangeFloatParamUsingCurve(corHolder));
                            m_RunningCurves.Add(corHolder);
                            corHolder.coroutine = cor;
                        }
                        break;
                }
            }
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);

            foreach(var curve in m_RunningCurves)
            {
                StopCoroutine(curve.coroutine);
            }

            foreach(var paramOrigValuePair in m_originalParamValues)
            {
                m_Animator.SetFloat(paramOrigValuePair.Key, paramOrigValuePair.Value);
            }
        }

        private IEnumerator ChangeFloatParamUsingCurve(CoroutineHolder holder)
        {
            var timePassed = 0f;
            var progress = 0f;

            while (progress < 1f)
            {
                timePassed += Time.deltaTime;
                progress = timePassed / m_CurveDuration;

                var val = m_ValueCurve.Evaluate(progress) * (m_CurveValueSpan.y - m_CurveValueSpan.x) + m_CurveValueSpan.x;
                foreach (var floatParam in m_FloatParams)
                {
                    m_Animator.SetFloat(floatParam, val);
                }

                yield return null;
            }

            m_RunningCurves.Remove(holder);
        }

    }
}