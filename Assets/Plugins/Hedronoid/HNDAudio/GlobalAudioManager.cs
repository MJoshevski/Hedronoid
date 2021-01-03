using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Reflection;
using System.IO;
using System.Linq;
using Hedronoid.Events;
using Hedronoid.ObjectPool;

namespace Hedronoid.Audio
{

    #region Events

    /// <summary>
    /// When audio begins playing (nice for subtitles)
    /// </summary>
    public class OnAudioBeganPlaying : HNDBaseEvent { public string ClipName; }

    /// <summary>
    /// When audio stops playing (nice for subtitles)
    /// </summary>
    public class OnAudioStoppedPlaying : HNDBaseEvent { public string ClipName; }

    /// <summary>
    /// When audio manager finished preloading some prefix
    /// </summary>
    public class OnPreloadedAudio : HNDBaseEvent { public string Prefix; public bool Async = true; }

    /// <summary>
    /// When audio manager finished unloading some prefix
    /// </summary>
    public class OnUnloadedAudio : HNDBaseEvent { public string Prefix; }

    #endregion

    /// <summary>
    /// Global audio manager for the whole project.
    /// Uses AudioClipPlayer for playing individual clips (pooled and using AudioSource + effect)
    /// Accessible through events.
    /// </summary>
    [Serializable]
    public class GlobalAudioManager : HNDGameObject
    {

        #region Enums and classes

        public enum AudioMode
        {
            OCULUS_READY = 0,
            AUDIO_2D,
            AUDIO_3D,
            STEREO_CONTROL
        }

        public enum StereoControl
        {
            LEFT = 0,
            RIGHT
        }

        #endregion

        [SerializeField]
        private AudioMode m_AudioMode = AudioMode.AUDIO_2D;

        [SerializeField]
        private static Dictionary<string, GameObject> m_AudioManagersPrefabs;

        [SerializeField]
        private static Dictionary<string, GlobalAudioManager> m_Instances;

        [SerializeField]
        private AudioManagerData m_AudioData;

        private Dictionary<string, AudioManagerCategory> m_Leafs;

        private Dictionary<int, float> m_ClipsLastPlayedTimes;

        private Dictionary<string, List<string>> m_LeafPrefixes;

        private Dictionary<int, int> m_CurrentPlayingCategoryCounts;

        private Dictionary<string, AudioMixerSnapshot> m_audioMixerSnapShots;

        private AudioCache m_Cache;

        [SerializeField]
        private HNDObjectPool m_AudioClipPlayerPool;

        public AudioManagerData AudioData
        {
            get { return m_AudioData; }
            set { m_AudioData = value; }
        }

        protected AudioMixer m_MainMixer;

        private static GlobalAudioManager m_Instance;
        public static GlobalAudioManager Instance { get { return m_Instance; } }

        private bool m_Paused = false;
        public bool Paused
        {
            get { return m_Paused; }
            set
            {
                if (m_Paused == value) return;

                m_Paused = value;

                if (m_Paused)
                {
                    m_CustomTimePauseStart = DateTime.Now.Ticks;
                }
                else
                {
                    m_CustomTimePausedDuration += DateTime.Now.Ticks - m_CustomTimePauseStart;
                }
                D.AudioLogFormat("Audio Time set to: {0}", value);
            }
        }

        private long m_CustomTimePauseStart = 0;
        private long m_CustomTimePausedDuration = 0;

        public float PausableAudioTime
        {
            get
            {
                return (DateTime.Now.Ticks - m_CustomTimePausedDuration - (m_Paused ? (DateTime.Now.Ticks - m_CustomTimePauseStart) : 0)) * 0.0000001f;
            }
        }

        HNDAudioPreferences m_Preferences;

        public void LoadAudioBundle(string path)
        {
            if (m_Cache == null)
            {
                m_Cache = gameObject.AddComponent<AudioCache>();
            }

            m_Cache.LoadBundle(path);
        }

        public void LoadAdditionalBundle(AssetBundle bundle)
        {
            if (m_Cache == null)
            {
                m_Cache = gameObject.AddComponent<AudioCache>();
            }

            m_Cache.LoadAdditionalBundle(bundle);
        }

        public void UnloadAdditionalBundles()
        {
            if (m_Cache != null)
            {
                m_Cache.UnloadAdditionalBundles();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            m_Preferences = HNDAudioPreferences.Instance;

            // So audio time starts from zero
            m_CustomTimePausedDuration = DateTime.Now.Ticks;

            AudioData.ReconstructTreeChildren();
            m_Leafs = AudioData.TreeData.GetLeafDictionary();
            m_ClipsLastPlayedTimes = new Dictionary<int, float>(1000);
            m_CurrentPlayingCategoryCounts = new Dictionary<int, int>(100);
            m_audioMixerSnapShots = new Dictionary<string, AudioMixerSnapshot>();

            PrepareLeafPrefixes();

            m_MainMixer = m_AudioData.TreeData.DefaultBus.audioMixer;

            m_Instance = this;
        }

        public void SetAudioGroupsPaused(bool paused, List<AudioMixerGroup> includeGroups, List<AudioMixerGroup> excludeGroups)
        {
            foreach (GameObject go in m_AudioClipPlayerPool.RentedObjects)
            {
                if (go == null) { continue; }

                AudioClipPlayer p = go.GetComponent<AudioClipPlayer>();
                if (p == null) { continue; }

                AudioClipHandle handle = p.Handle;
                if (handle == null) { continue; }

                bool shouldAffect = (excludeGroups == null || !excludeGroups.Contains(handle.MixerGroup));
                shouldAffect &= (includeGroups == null || includeGroups.Contains(handle.MixerGroup));

                if (shouldAffect)
                {
                    if (paused) { handle.Pause(); }
                    else { handle.UnPause(); }
                }
            }
        }

        private void PrepareLeafPrefixes()
        {
            m_LeafPrefixes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (AudioManagerCategory c in m_Leafs.Values)
            {
                List<string> cat;
                string prefix = c.IdPrefix.ToUpperInvariant();
                if (!m_LeafPrefixes.ContainsKey(prefix))
                {
                    m_LeafPrefixes[prefix] = new List<string>();
                }
                cat = m_LeafPrefixes[prefix];

                List<string> audios = c.AudioData;
                if (audios != null)
                {
                    foreach (string origPathAudio in audios)
                    {
                        if (!String.IsNullOrEmpty(origPathAudio))
                        {
                            string path = StripResourcePath(origPathAudio);
                            cat.Add(path);
                        }
                    }
                }
            }
        }

        public void SetMixerParameter(string paramName, float value)
        {
            if (m_MainMixer == null) return;
            m_MainMixer.SetFloat(paramName, value);
        }

        public float GetMixerParameterValue(string paramName)
        {
            if (m_MainMixer == null)
            {
                Debug.LogWarning("GetMixerParameterValue failed as m_MainMixer is null, returning 0f as default");
                return 0f;
            }
            float value;
            m_MainMixer.GetFloat(paramName, out value);
            return value;
        }

        public IEnumerator PreloadAudioAsync(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) yield break;

            prefix = prefix.ToUpperInvariant();
            D.AudioLog("Preloading audio with prefix " + prefix);

            // ordinal ignore case comparison
            if (m_LeafPrefixes.ContainsKey(prefix))
            {
                var files = m_LeafPrefixes[prefix];
                foreach (string clipPath in files)
                {
                    //D.AudioLog("Preloading: " + clipPath);
                    yield return StartCoroutine(m_Cache.RetainClipAsync(clipPath));
                }
            }

            D.AudioLog("Preloaded audio with prefix " + prefix);
            HNDEvents.Instance.Raise(new OnPreloadedAudio { Prefix = prefix, Async = true });
        }

        public void PreloadAudioSync(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return;

            prefix = prefix.ToUpperInvariant();
            D.AudioLog("Preloading audio with prefix " + prefix);

            if (m_LeafPrefixes.ContainsKey(prefix))
            {
                var files = m_LeafPrefixes[prefix];
                foreach (string clipPath in m_LeafPrefixes[prefix])
                {
                    //D.AudioLog("Preloading: " + clipPath);
                    m_Cache.RetainClip(clipPath);
                }
            }

            D.AudioLog("Preloaded audio with prefix " + prefix);
            HNDEvents.Instance.Raise(new OnPreloadedAudio { Prefix = prefix, Async = false });
        }

        public void UnloadAudio(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return;

            prefix = prefix.ToUpperInvariant();
            D.AudioLog("Unloading preloaded audio with prefix " + prefix);

            if (m_LeafPrefixes.ContainsKey(prefix))
            {
                foreach (string clipPath in m_LeafPrefixes[prefix])
                {
                    m_Cache.ReleaseClipNow(clipPath);
                }
            }
            HNDEvents.Instance.Raise(new OnUnloadedAudio { Prefix = prefix });
        }

        public static AudioSource Add3DData(AudioSource sound, AudioSource3DData data)
        {
            sound.dopplerLevel = data.dopplerLevel;
            sound.minDistance = data.minDistance;
            sound.maxDistance = data.maxDistance;
            sound.panStereo = data.panStereo;
            sound.rolloffMode = data.rolloffMode;
            sound.spread = data.spread;
            sound.spatialize = data.spatialize;
            sound.spatialBlend = data.spatialBlend;
            sound.spatializePostEffects = data.spatializePostEffects;
            return sound;
        }

        public static AudioSource AddStereoControl(AudioSource sound, StereoControl stereoControl = StereoControl.LEFT)
        {
            if (sound == null) return null;
            sound.panStereo = stereoControl == StereoControl.LEFT ? -1f : 1f;
            return sound;
        }

        public static AudioSource AddStereoControl(AudioSource sound, float panStereo)
        {
            if (sound == null) return null;
            sound.panStereo = panStereo;
            return sound;
        }

        string StripResourcePath(string fullPath)
        {
            return Path.GetFileNameWithoutExtension(fullPath);
        }

        AudioClip GetClip(string soundId)
        {
            AudioClip clipToPlay = null;

            if (m_Leafs.ContainsKey(soundId))
            {
                List<string> audios = m_Leafs[soundId].AudioData;
                if (audios != null && audios.Count > 0)
                {
                    string origPath = "";
                    if (m_Leafs[soundId].SoundRandomization)
                    {
                        origPath = m_Leafs[soundId].RandomWeightedAudioData();
                    }
                    else
                    {
                        origPath = m_Leafs[soundId].GetNextOrderedAudioData();
                    }
                    if (!String.IsNullOrEmpty(origPath))
                    {
                        string path = StripResourcePath(origPath);
                        clipToPlay = m_Cache.RetainClip(path);
                    }
                }
            }

            return clipToPlay;
        }

        void PlayMissingSound(string originalTrigger)
        {
            if (originalTrigger == m_Preferences.MissingSoundTriggerId) return;

            if (!m_Leafs.ContainsKey(originalTrigger))
            {
                D.AudioWarning("Missing trigger: " + originalTrigger);
            }
            else
            {
                D.AudioWarning("Missing clip for trigger: " + originalTrigger);
            }

            if (m_Preferences.PlayMissingSound && !string.IsNullOrEmpty(m_Preferences.MissingSoundTriggerId))
            {
                PlayAudioInternal(new PlayConfig { SoundID = m_Preferences.MissingSoundTriggerId });
            }
        }

        private bool CheckCooldown(AudioManagerCategory c, float currentTime)
        {
            // Traverse all parents
            while (c != null)
            {
                if (c.NextAllowedAudioDelay >= 0 && m_ClipsLastPlayedTimes.ContainsKey(c.UniqueID)
                    && (m_ClipsLastPlayedTimes[c.UniqueID] + c.NextAllowedAudioDelay) >= currentTime)
                {
                    return false;
                }
                c = c.Parent;


                // only parent nodes (categories)
                if (c != null)
                {
                    for (int i = 0; i < c.BlockedByCategoryIds.Length; i++)
                    {
                        int blockedCat = c.BlockedByCategoryIds[i];
                        if (m_CurrentPlayingCategoryCounts.ContainsKey(blockedCat) && m_CurrentPlayingCategoryCounts[blockedCat] > 0)
                        {
                            //D.AudioError("Audio blocked by a different audio playing!");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void UpdateCooldown(AudioManagerCategory c, float currentTime)
        {
            // Traverse all parents
            while (c != null)
            {
                m_ClipsLastPlayedTimes[c.UniqueID] = currentTime;
                c = c.Parent;

                // only parent nodes (categories)
                if (c != null)
                {
                    if (!m_CurrentPlayingCategoryCounts.ContainsKey(c.UniqueID))
                    {
                        m_CurrentPlayingCategoryCounts[c.UniqueID] = 0;
                    }
                    m_CurrentPlayingCategoryCounts[c.UniqueID]++;
                }
            }
        }

        private void FreeCooldown(AudioManagerCategory c)
        {
            // Traverse all parents
            while (c != null)
            {
                c = c.Parent;

                // only parent nodes (categories)
                if (c != null)
                {
                    if (!m_CurrentPlayingCategoryCounts.ContainsKey(c.UniqueID))
                    {
                        m_CurrentPlayingCategoryCounts[c.UniqueID] = 0;
                    }
                    m_CurrentPlayingCategoryCounts[c.UniqueID]--;
                }
            }
        }

        public AudioClipHandle PlayAudio(PlayConfig c)
        {
            if (c == null) return null;
            AudioClipPlayer p = PlayAudioInternal(c);
            if (p == null) return null;
            return p.Handle;
        }

        private AudioClipPlayer PlayAudioInternal(PlayConfig c)
        {
            if (c == null || string.IsNullOrEmpty(c.SoundID))
            {
                D.AudioWarning("Null config or sound ID: " + (c != null ? c.SoundID : ""));
                return null;
            }

            if (!m_Leafs.ContainsKey(c.SoundID))
            {
                PlayMissingSound(c.SoundID);
                return null;
            }

            AudioClip clipToPlay = GetClip(c.SoundID);

            if (clipToPlay == null)
            {
                D.AudioWarning("Null clipToPlay: " + (c != null ? c.SoundID : ""));
                PlayMissingSound(c.SoundID);
                return null;
            }


            AudioManagerCategory cat = m_Leafs[c.SoundID];
            var closestOutbus = cat.GetClosestBus();
            //D.AudioLog("Playing sound: " + c.SoundID + " through mixer: " + closestOutbus.name);
            bool startPaused = c.StartPaused.HasValue ? c.StartPaused.Value : false;

            // Skip processing if only allowing for one sound instance.
            float currentTime = Time.time;
            if (!startPaused)
            {
                if (CheckCooldown(cat, currentTime) == false)
                {
                    D.AudioWarning("Skipping sound '" + c.SoundID + "' as it would be played too soon or because it is blocked by some playing category");
                    return null;
                }
            }

            Vector3? pos = null;
            if (c.Position)
                pos = c.Position.position;

            if (c.PositionV3.HasValue)
                pos = c.PositionV3.Value;
                
            AudioClipPlayer player = PrepareClip(clipToPlay,
                cat,
                c.Loop.HasValue ? c.Loop.Value : cat.Loop,
                startPaused,
                closestOutbus,
                c.Volume.HasValue ? c.Volume.Value : cat.Volume,
                c.Delay.HasValue ? c.Delay.Value : 0,
                c.In3D.HasValue ? c.In3D.Value : false,
                pos,
                c.MinDistance.HasValue ? c.MinDistance.Value : 1f,
                c.MaxDistance.HasValue ? c.MaxDistance.Value : 500f,
                c.VolumeRolloffMode.HasValue ? c.VolumeRolloffMode.Value : AudioRolloffMode.Logarithmic,
                c.PitchRandomisation.HasValue ? c.PitchRandomisation.Value : cat.PitchRandomizationValue,
                c.Target,
                c.ReferenceAudioSource,
                c.Parent);
            
            //D.AudioLogFormat("Playing sound {0} with delay {1}", c.SoundID, (c.Delay.HasValue ? c.Delay.Value : 0));

            player.SetFilters(cat);

            if (!startPaused)
            {
                UpdateCooldown(cat, currentTime);
                player.PlayDelayed(c.Delay.HasValue ? c.Delay.Value : 0);
            }

#if NN_OCULUS
            if (m_AudioMode == AudioMode.OCULUS_READY)
            {
					ONSPAudioSource tempOculusSource = audio.gameObject.AddComponent<ONSPAudioSource>();
					tempOculusSource.EnableSpatialization = true;
					tempOculusSource.EnableRfl = true;
					tempOculusSource.Gain = 10;
					tempOculusSource.Near = 0.25f;
					tempOculusSource.Far = 20000f;
            }
#endif

            if (m_AudioMode == AudioMode.STEREO_CONTROL)
            {
                AddStereoControl(player.AudioSource, cat.StereoPan);
            }

            if ((m_AudioMode == AudioMode.AUDIO_3D || m_AudioMode == AudioMode.OCULUS_READY) && !c.ReferenceAudioSource)
            {
                Add3DData(player.AudioSource, m_Preferences.Default3DSettings);
            }

            return player;
        }


        AudioClipPlayer PrepareClip(AudioClip clip, AudioManagerCategory trigger, bool loop = false, bool startPaused = false, AudioMixerGroup outBus = null, float volume = 1.0f, float delay = 0.0f, bool in3D = false, Vector3? position = null, float minDistance = 1f, float maxDistance = 500f, AudioRolloffMode volumeRolloffMode = AudioRolloffMode.Logarithmic, float pitchRandomisation = 0.0f, Transform target = null, AudioSource referenceAudioSource = null, Transform parent = null)
        {
            AudioClipPlayer go = m_AudioClipPlayerPool.RentObject().GetComponent<AudioClipPlayer>();
            go.Setup(m_Cache, m_AudioClipPlayerPool, this);
            go.Category = trigger;

            if (go.IsEmpty == false)
            {
                D.LogError("Rented AudioClipPlayer is not empty! Used for " + go.ClipID);
            }

            go.transform.parent = parent != null ? parent : transform;
            go.transform.localPosition = Vector3.zero;

            var audioSource = go.AudioSource;

            if (outBus != null)
            {
                audioSource.outputAudioMixerGroup = outBus;
            }

            audioSource.pitch = referenceAudioSource ? referenceAudioSource.pitch : 1;
            audioSource.loop = loop;
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.spatialBlend = referenceAudioSource ? referenceAudioSource.spatialBlend : (in3D) ? 1.0f : 0.0f;
            if (in3D)
            {
                audioSource.minDistance = referenceAudioSource ? referenceAudioSource.minDistance : minDistance;
                audioSource.maxDistance = referenceAudioSource ? referenceAudioSource.maxDistance : maxDistance;
                audioSource.rolloffMode = referenceAudioSource ? referenceAudioSource.rolloffMode : volumeRolloffMode;
                if (position != null)
                {
                    go.transform.position = position.Value;
                }

                if(audioSource.rolloffMode == AudioRolloffMode.Custom && referenceAudioSource)
                {
                    foreach(AudioSourceCurveType audioCurve in (AudioSourceCurveType[]) Enum.GetValues(typeof(AudioSourceCurveType)))
                    {
                        audioSource.SetCustomCurve(audioCurve, referenceAudioSource.GetCustomCurve(audioCurve));
                    }
                }
                audioSource.dopplerLevel = referenceAudioSource ? referenceAudioSource.dopplerLevel : audioSource.dopplerLevel;
                audioSource.spread = referenceAudioSource ? referenceAudioSource.spread : audioSource.spread;
                audioSource.spatialize = referenceAudioSource ? referenceAudioSource.spatialize : audioSource.spatialize;
                audioSource.spatializePostEffects = referenceAudioSource ? referenceAudioSource.spatializePostEffects : audioSource.spatializePostEffects;

            }

            if (pitchRandomisation != 0.0f)
            {
                pitchRandomisation = Mathf.Abs(pitchRandomisation);
                float defaultPitch = referenceAudioSource ? referenceAudioSource.pitch : 1.0f;
                audioSource.pitch = defaultPitch + UnityEngine.Random.Range(-pitchRandomisation, pitchRandomisation);
            }

            float releaseDelayGameTime = 0;
            float releaseDelayRealTime = 0;

            if (!loop && !startPaused)
            {
                // Return to pool after the clip is played
                releaseDelayGameTime = delay;
                releaseDelayRealTime = clip.length + 0.1f;
            }

            go.SetClip(clip, target, releaseDelayGameTime, releaseDelayRealTime, trigger.ID);

            return go;
        }

        public void ClipPlayerFinishedPlaying(AudioClipPlayer p)
        {
            FreeCooldown(p.Category);
        }

        public AudioMixerSnapshot GetSnapShot(string snapShotID)
        {
            if (m_audioMixerSnapShots.ContainsKey(snapShotID))
                return m_audioMixerSnapShots[snapShotID];
            else
            {
                var snapShot = m_MainMixer.FindSnapshot(snapShotID);
                if (snapShotID != null)
                    m_audioMixerSnapShots.Add(snapShotID, snapShot);
                return snapShot;
            }
        }

        public void TransitionToSnapShots(AudioMixerSnapshot[] snapShots, float[] weights, float transitionDuration)
        {
            m_MainMixer.TransitionToSnapshots(snapShots, weights, transitionDuration);
        }
    }
}