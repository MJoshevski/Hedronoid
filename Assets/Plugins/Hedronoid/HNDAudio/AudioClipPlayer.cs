using UnityEngine;
using System.Collections.Generic;
using Hedronoid;
using Hedronoid.Events;
using System.Collections;
using Hedronoid.ObjectPool;
using System.Reflection;

namespace Hedronoid.Audio
{
    /// <summary>
    /// Clip player prefab spawned by AudioManager for playing one instance of audio clip
    /// AudioClipPlayer is recycled and is using an object pool.
    /// It is never destroyed.
    /// </summary>
    public class AudioClipPlayer : HNDGameObject
    {
        public bool IsEmpty = true;

        public string ClipID;

        public AudioManagerCategory Category;

        private AudioClipHandle m_Handle;

        private AudioCache m_Cache;

        private GlobalAudioManager m_AudioManager;

        private HNDObjectPool m_ClipPool;

        private AudioSource m_Source;

        private AudioFadeInFilterData m_FadeInData;
        private AudioFadeOutFilterData m_FadeOutData;

        private Coroutine m_FadeCR;

        // EAudioFilter but using int to generate less garbage
        private Dictionary<int, Behaviour> m_Filters = new Dictionary<int, Behaviour>();

        private List<WaitUntilAudioClipFinished> m_WaitUntilFinishedInstructions = new List<WaitUntilAudioClipFinished>();

        private List<WaitUntilAudioClipStarted> m_WaitUntilStartedInstructions = new List<WaitUntilAudioClipStarted>();

        OnAudioBeganPlaying m_CachedBeganPlayingEvent;
        OnAudioStoppedPlaying m_CachedStopppedPlayingEvent;

        public AudioSource AudioSource
        {
            get
            {
                if (m_Source == null)
                {
                    m_Source = GetComponent<AudioSource>();
                }

                return m_Source;
            }
        }

        public AudioClipHandle Handle
        {
            get { return m_Handle; }
        }

        public void Setup(AudioCache cache, HNDObjectPool pool, GlobalAudioManager manager)
        {
            m_Cache = cache;
            m_ClipPool = pool;
            m_AudioManager = manager;
            m_CachedBeganPlayingEvent = new OnAudioBeganPlaying();
            m_CachedStopppedPlayingEvent = new OnAudioStoppedPlaying();
        }

        private T EnableFilter<T>(EAudioFilter filterType) where T : Behaviour
        {
            int intType = (int)filterType;
            T filter = null;
            if (!m_Filters.ContainsKey(intType))
            {
                filter = cachedGameObject.AddComponent<T>();
                m_Filters[intType] = filter;
            }
            else
            {
                filter = (T)m_Filters[intType];
            }
            filter.enabled = true;
            return filter;
        }

        public void SetFilters(AudioManagerCategory cat)
        {
            if (cat.UseLowPassFilter)
            {
                AudioLowPassFilter filter = EnableFilter<AudioLowPassFilter>(EAudioFilter.LowPass);
                cat.AudioFilters.LowPass.SetToFilter(filter);
            }

            if (cat.UseHighPassFilter)
            {
                AudioHighPassFilter filter = EnableFilter<AudioHighPassFilter>(EAudioFilter.HighPass);
                cat.AudioFilters.HighPass.SetToFilter(filter);
            }

            if (cat.UseChorusFilter)
            {
                AudioChorusFilter filter = EnableFilter<AudioChorusFilter>(EAudioFilter.Chorus);
                cat.AudioFilters.Chorus.SetToFilter(filter);
            }

            if (cat.UseDistortionFilter)
            {
                AudioDistortionFilter filter = EnableFilter<AudioDistortionFilter>(EAudioFilter.Distortion);
                cat.AudioFilters.Distortion.SetToFilter(filter);
            }

            if (cat.UseEchoFilter)
            {
                AudioEchoFilter filter = EnableFilter<AudioEchoFilter>(EAudioFilter.Echo);
                cat.AudioFilters.Echo.SetToFilter(filter);
            }

            if (cat.UseReverbFilter)
            {
                AudioReverbFilter filter = EnableFilter<AudioReverbFilter>(EAudioFilter.Reverb);
                cat.AudioFilters.Reverb.SetToFilter(filter);
            }

            if (cat.UseFadeIn)
            {
                m_FadeInData = cat.AudioFilters.FadeIn;
            }

            if (cat.UseFadeOut)
            {
                m_FadeOutData = cat.AudioFilters.FadeOut;
            }
        }

        public void SetClip(AudioClip clip, Transform trackObject, float releaseDelayGameTime, float releaseDelayRealTime, string triggerName)
        {
            IsEmpty = false;
            ClipID = clip.name;
#if UNITY_EDITOR
            name = string.Format("{0} - {1}", triggerName, clip.name);
#endif
            m_Handle = new AudioClipHandle(this);

            if (trackObject != null)
            {
                StartCoroutine(TrackObjectCR(trackObject));
            }

            if (releaseDelayGameTime != 0 || releaseDelayRealTime != 0)
            {
                StartCoroutine(DelayedReleaseCR(releaseDelayGameTime, releaseDelayRealTime));
            }
        }

        public void PlayDelayed(float delay)
        {
            if (delay <= 0)
            {
                StartPlaying();
            }
            else
            {
                StartCoroutine(PlayDelayedCR(delay));
            }
        }

        private void StartPlaying()
        {
            if (m_FadeInData != null)
            {
                float val = m_FadeInData.Fade.Evaluate(0);
                if (m_FadeInData.Type == AudioFadeInFilterData.EFadeType.VOLUME)
                {
                    AudioSource.volume = val;
                }
                else if (m_FadeInData.Type == AudioFadeInFilterData.EFadeType.PITCH)
                {
                    AudioSource.pitch = val;
                }
            }

            AudioSource.Play();

            m_CachedBeganPlayingEvent.ClipName = ClipID;
            HNDEvents.Instance.Raise(m_CachedBeganPlayingEvent);

            for (int i = 0; i < m_WaitUntilStartedInstructions.Count; i++)
            {
                m_WaitUntilStartedInstructions[i].StopWaiting();
            }
            m_WaitUntilStartedInstructions.Clear();

            if (m_FadeInData != null)
            {
                float duration = m_FadeInData.Fade.keys[m_FadeInData.Fade.length - 1].time;

                StartCoroutine(StartFadeCR(m_FadeInData.Fade, 0, duration, false, m_FadeInData.Type == AudioFadeInFilterData.EFadeType.PITCH));
            }
            if (m_FadeOutData != null)
            {
                float duration = m_FadeOutData.Fade.keys[m_FadeOutData.Fade.length - 1].time;
                float clipDuration = AudioSource.clip.length;

                StartCoroutine(StartFadeCR(m_FadeOutData.Fade, clipDuration - duration, duration, false, m_FadeOutData.Type == AudioFadeOutFilterData.EFadeType.PITCH));
            }
        }

        private IEnumerator PlayDelayedCR(float delay)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            StartPlaying();
        }

        private IEnumerator TrackObjectCR(Transform o)
        {
            while (o != null)
            {
                cachedTransform.position = o.position;
                
                yield return null;
            }
        }

        public void ChangePitch(AnimationCurve curve, float duration = 0f, bool destroy = false)
        {
            if (m_Source == null) return;
            if (duration <= 0f) { if (destroy) ReleaseClip(); return; }
            StartCoroutine(StartFadeCR(curve, 0, duration, destroy, true));
        }

        public void ChangeVolume(AnimationCurve curve, float duration = 0f, bool destroy = false)
        {
            if (m_Source == null) return;
            if (duration <= 0f) { if (destroy) ReleaseClip(); return; }
            StartCoroutine(StartFadeCR(curve, 0, duration, destroy, false));
        }

        IEnumerator StartFadeCR(AnimationCurve curve, float waitDuration, float duration, bool destroy, bool pitch)
        {
            if (waitDuration > 0)
            {
                yield return new WaitForPausedAudioRealtime(waitDuration);
            }

            m_FadeCR = this.StopCoroutineSafely(m_FadeCR);
            m_FadeCR = StartCoroutine(FadeCR(curve, duration, destroy, pitch));
        }

        IEnumerator FadeCR(AnimationCurve curve, float duration, bool destroy, bool pitch)
        {
            float startTime = m_AudioManager.PausableAudioTime;
            float endTime = startTime + duration;
            while (m_AudioManager.PausableAudioTime <= endTime)
            {
                float val = (m_AudioManager.PausableAudioTime - startTime) / duration;
                if (pitch)
                {
                    m_Source.pitch = curve.Evaluate(val);
                }
                else
                {
                    m_Source.volume = curve.Evaluate(val);
                }
                yield return null;
            }

            if (destroy) ReleaseClip();
        }

        private IEnumerator DelayedReleaseCR(float gameTimeDelay, float realtimeDelay)
        {
            yield return new WaitForSeconds(gameTimeDelay);
            yield return new WaitForPausedAudioRealtime(realtimeDelay);

            ReleaseClip();
        }

        public void AddWaitUntilFinishedInstruction(WaitUntilAudioClipFinished i)
        {
            if (!m_WaitUntilFinishedInstructions.Contains(i))
            {
                m_WaitUntilFinishedInstructions.Add(i);
            }
        }

        public void AddWaitUntilStartedInstruction(WaitUntilAudioClipStarted i)
        {
            if (!m_WaitUntilStartedInstructions.Contains(i))
            {
                if (AudioSource != null && AudioSource.isPlaying)
                {
                    // We're already playing!
                    i.StopWaiting();
                }
                else
                {
                    m_WaitUntilStartedInstructions.Add(i);
                }
            }
        }

        public void ReleaseClip()
        {
            if (!IsEmpty)
            {
                if (AudioSource.clip.name != ClipID)
                {
                    D.AudioError("Clearing wrong clip! " + ClipID + " expected! Got: " + AudioSource.clip.name);
                }
                AudioSource.Stop();

                for (int i = 0; i < m_WaitUntilFinishedInstructions.Count; i++)
                {
                    m_WaitUntilFinishedInstructions[i].StopWaiting();
                }
                m_WaitUntilFinishedInstructions.Clear();

                for (int i = 0; i < m_WaitUntilStartedInstructions.Count; i++)
                {
                    m_WaitUntilStartedInstructions[i].StopWaiting();
                }
                m_WaitUntilStartedInstructions.Clear();

                m_CachedStopppedPlayingEvent.ClipName = AudioSource.clip.name;
                HNDEvents.Instance.Raise(m_CachedStopppedPlayingEvent);

                m_Cache.ReleaseClipNow(ClipID);
                m_AudioManager.ClipPlayerFinishedPlaying(this);
                PrepareForReuse();
                m_ClipPool.ReturnObject(cachedGameObject);
            }
        }

        private void PrepareForReuse()
        {
            StopAllCoroutines();

            m_FadeInData = null;
            m_FadeOutData = null;

            cachedGameObject.name = "AudioClipPlayer";

            ClipID = null;
            Category = null;

            m_Source.clip = null;
            m_Source.outputAudioMixerGroup = null;
            if (m_Handle != null)
            {
                m_Handle.Clear();
            }
            m_Handle = null;

            foreach (var kv in m_Filters)
            {
                kv.Value.enabled = false;
            }

            cachedTransform.position = Vector3.zero;

            IsEmpty = true;
        }
    }
}