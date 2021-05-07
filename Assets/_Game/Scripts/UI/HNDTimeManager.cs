using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hedronoid;
using DG.Tweening;
using Hedronoid.Events;

namespace Hedronoid.HNDTime
{
    public class TimeManagerPausedLevelChanged : HNDBaseEvent { public int PauseLevel; }
    public class TimeManagerPausedStateChanged : HNDBaseEvent { public bool Paused; }
    public class TimeManagerAudioPausedStateChanged : HNDBaseEvent { public bool Paused; }
    public class TimeManagerAudioPausedLevelChanged : HNDBaseEvent { public int PauseLevel; }

    public class HNDTimeManager : HNDMonoSingleton<HNDTimeManager>
    {
        protected HNDTimeManager() { } // guarantee this will be always a singleton only - can't use the constructor!

        private bool m_IsPaused = false;
        public bool IsPaused { get { return m_IsPaused; } }

        private bool m_IsAudioPaused = false;
        public bool IsAudioPaused { get { return m_IsAudioPaused; } }

        private float m_DeltaTime;
        private float m_UnscaledTime;
        private float m_LastTime;

        private List<bool> m_PrePauseStack = new List<bool>();
        private List<float> m_PrePauseTimeScaleStack = new List<float>();

        private List<bool> m_PrePauseAudioStack = new List<bool>();

        private float m_DefaultFixedDeltaTime;

        private Coroutine m_SlowMoRoutine;

        private List<Animator> m_UnscaledAnimators = new List<Animator>();

        private const string PAUSE_TAG = "IgnorePause";

        private HashSet<GameObject> m_ObjectsIgnoringPause = new HashSet<GameObject>();

        protected override void Awake()
        {
            base.Awake();

            m_IsAudioPaused = false;
            m_IsPaused = false;

            DOTween.Init();
            DOTween.defaultTimeScaleIndependent = true;

            m_DefaultFixedDeltaTime = Time.fixedDeltaTime;

            Instance.StartCoroutine(UpdateTime());
        }

        public void IgnorePauseForObject(GameObject o)
        {
            m_ObjectsIgnoringPause.Add(o);
        }

        public int CurrentPauseLevel { get { return m_PrePauseStack.Count; } }

        public void StartSlowMo(float scale, float duration = -1f)
        {
            D.CoreLog("StartSlowMo - scale: " + scale + ", duration: " + duration + ", realtime: " + Time.realtimeSinceStartup);

            if (m_SlowMoRoutine != null)
            {
                D.CoreWarning("SlowMo already in progress, cannot start new!");
                return;
            }

            m_SlowMoRoutine = StartCoroutine(SlowMo(duration, scale));
        }

        public void Reset()
        {
            StopSlowMo();
            m_IsPaused = false;
            Time.timeScale = 1;
            m_PrePauseStack.Clear();
            m_PrePauseTimeScaleStack.Clear();
            m_PrePauseAudioStack.Clear();
            HNDEvents.Instance.Raise(new TimeManagerPausedLevelChanged { PauseLevel = CurrentPauseLevel });
            HNDEvents.Instance.Raise(new TimeManagerAudioPausedLevelChanged { PauseLevel = CurrentPauseLevel });
            HNDEvents.Instance.Raise(new TimeManagerPausedStateChanged { Paused = false });
            HNDEvents.Instance.Raise(new TimeManagerAudioPausedStateChanged { Paused = false });
        }

        public void StopSlowMo()
        {
            D.CoreLog("StopSlowMo - realtime: " + Time.realtimeSinceStartup);

            if (m_SlowMoRoutine == null)
            {
                D.CoreWarning("SlowMo not active, nothing to stop!");

                // HACK : before is everything called from TimeManager
                // return;
            }
            else
            {
                StopCoroutine(m_SlowMoRoutine);
            }

            SlowMoStopped();
        }

        public void SetPaused(bool pause, bool affectsAudio, bool almostPause = false)
        {
            if(!almostPause)
                D.CoreLog("SetPaused - pause: " + pause + ", prepause stack: " + m_PrePauseStack.Count);
            else
                D.CoreLog("SetAlmostPaused - almostPause: " + almostPause + ", prepause stack: " + m_PrePauseStack.Count);

            bool wasPaused = m_IsPaused;

            if (pause)
            {
                m_PrePauseStack.Add(m_IsPaused);
                m_PrePauseTimeScaleStack.Add(Time.timeScale);

                if (!almostPause)
                    SetScale(0f);
                else
                    SetScale(0.00001f);

                m_IsPaused = pause;

                DOTween.PauseAll();

                foreach (var anim in FindObjectsOfType<Animator>()) //TODO: Get Animators in some more efficient way!!
                {
                    if (anim.GetComponent<Button>()) continue; //HACK: Prevent setting updatemode on buttons
                    if (m_ObjectsIgnoringPause.Contains(anim.gameObject)) continue;
                    if (anim.updateMode == AnimatorUpdateMode.UnscaledTime && !anim.CompareTag(PAUSE_TAG))
                    {
                        anim.updateMode = AnimatorUpdateMode.Normal;
                        m_UnscaledAnimators.Add(anim);
                    }
                }
            }
            else
            {
                m_IsPaused = m_PrePauseStack.Count == 0 ? false : m_PrePauseStack.PopLast();

                //HACK: If when unpausing, we arent actually paused (something went wrong), we dont use the prepaused time scale
                if (Time.timeScale == 0)
                {
                    SetScale(m_PrePauseTimeScaleStack.Count == 0 ? 1.0f : m_PrePauseTimeScaleStack.PopLast());
                }
                else
                {   //we still pop to make sure everything still works as intended
                    float prev = m_PrePauseTimeScaleStack.Count == 0 ? 1.0f : m_PrePauseTimeScaleStack.PopLast();
                    D.CoreWarning("TimeManager: Unpausing while time scale was not 0, setting scale to 1. PrePauseScale = " + prev);
                    SetScale(1.0f);
                }

                DOTween.PlayAll();

                foreach (var anim in m_UnscaledAnimators)
                {
                    if (anim) anim.updateMode = AnimatorUpdateMode.UnscaledTime;
                }
                m_UnscaledAnimators.Clear();
            }

            HNDEvents.Instance.Raise(new TimeManagerPausedLevelChanged { PauseLevel = CurrentPauseLevel });

            // D.CoreWarning("Is Game Paused : " + m_IsPaused);

            if (m_IsPaused != wasPaused)
            {
                D.CoreLogFormat("DEBUG : Toggle pause to {0}", m_IsPaused);
                HNDEvents.Instance.Raise(new TimeManagerPausedStateChanged { Paused = m_IsPaused });
            }

            if (affectsAudio)
            {
                SetAudioPaused(pause);
            }
        }

        private void SetAudioPaused(bool pause)
        {
            bool wasPaused = m_IsAudioPaused;

            if (pause)
            {
                m_PrePauseAudioStack.Add(m_IsAudioPaused);
                m_IsAudioPaused = pause;
            }
            else
            {
                m_IsAudioPaused = m_PrePauseAudioStack.Count == 0 ? false : m_PrePauseAudioStack.PopLast();
            }

            // D.CoreWarning("Is Audio Paused : " + m_IsPaused);
            HNDEvents.Instance.Raise(new TimeManagerAudioPausedLevelChanged { PauseLevel = m_PrePauseAudioStack.Count });

            if (m_IsAudioPaused != wasPaused)
            {
                HNDEvents.Instance.Raise(new TimeManagerAudioPausedStateChanged { Paused = m_IsAudioPaused });
            }
        }

        private void SetScale(float newScale)
        {
            D.CoreLog("SetScale - Time.timeScale: " + Time.timeScale + ", newScale: " + newScale + ", realtime: " + Time.realtimeSinceStartup);

            Time.timeScale = newScale;
        }

        private void SlowMoStopped()
        {
            m_SlowMoRoutine = null;
            SetScale(1f);
        }

        private IEnumerator SlowMo(float duration, float scale)
        {
            //HACK: Hack for waiting until we're not paused anymore
            while (HNDTimeManager.Instance.IsPaused)
            {
                yield return null;
            }
            yield return null;
            SetScale(scale);

            float timePassed = 0f;

            // TODO
            // AudioHelper.PlayAudio(AudioList.Sounds.GEN_SLOWMO);

            while (duration == -1 || timePassed < duration)
            {
                timePassed += m_DeltaTime;
                yield return null;
            }

            //HACK: Hack for waiting until we're not paused anymore
            while (HNDTimeManager.Instance.IsPaused)
            {
                yield return null;
            }
            yield return null;

            SlowMoStopped();
        }

        /// <summary>
        /// Updating our internal time
        /// </summary>
        private IEnumerator UpdateTime()
        {
            while (true)
            {
                if (m_IsPaused)
                {   //HACK: If when unpausing, we arent actually paused (something went no proper scale), we set scale
                    if (Time.timeScale != 0)
                    {
                        float prev = m_PrePauseTimeScaleStack.Count == 0 ? 1.0f : m_PrePauseTimeScaleStack.PopLast();
                        D.CoreWarning("TimeManager: Is paused while time scale was not 0, setting scale to 1. PrePauseScale = " + prev);
                        SetScale(0);
                    }
                    m_DeltaTime = 0f;
                }
                else
                {
                    m_DeltaTime = Time.realtimeSinceStartup - m_LastTime;
                    m_UnscaledTime += m_DeltaTime;
                }
                m_LastTime = Time.realtimeSinceStartup;

                yield return null;
            }
        }

        #region DEBUGGING
        [ContextMenu("Start SlowMo 5 seconds")]
        public void DBG_Start5SecondSlowMo()
        {
            StartSlowMo(0.1f, 5f); //Debug
        }

        [ContextMenu("Start SlowMo")]
        public void DBG_StartSlowMo()
        {
            StartSlowMo(0.1f); //Debug
        }

        [ContextMenu("Stop SlowMo")]
        public void DBG_StopSlowMo()
        {
            StopSlowMo();
        }

        [ContextMenu("Pause")]
        public void DBG_SetPause()
        {
            SetPaused(true, true);
        }

        [ContextMenu("Unpause")]
        public void DBG_SetUnpause()
        {
            SetPaused(false, true);
        }
        #endregion DEBUGGING

    }
}
