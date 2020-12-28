using System.Collections;
using System.Collections.Generic;
using Hedronoid.Events;
using UnityEngine;
using UnityEngine.Audio;

namespace Hedronoid.Audio
{
    /// <summary>
    /// Only thing exposed externally when playing a sound.
    /// As audio sources (in AudioClipPlayers) are recycled, people can break things easily.
    /// To fix that, only Handle is exposed without the ability to touch the real AudioSource or AudioClip
    /// </summary>
    public class AudioClipHandle
    {
        private AudioClipPlayer m_Player;

        public void Clear()
        {
            m_Player = null;
        }

        public AudioClipHandle(AudioClipPlayer player)
        {
            m_Player = player;
        }

        public AudioMixerGroup MixerGroup
        {
            get
            {
                if (m_Player != null && m_Player.AudioSource != null)
                {
                    return m_Player.AudioSource.outputAudioMixerGroup;
                }
                else
                {
                    return null;
                }
            }
        }

        public void Pause()
        {
            if (m_Player != null)
            {
                D.AudioLog("Pausing " + m_Player.gameObject.name);
                m_Player.AudioSource.Pause();
            }
        }

        public void UnPause()
        {
            if (m_Player != null)
            {
                D.AudioLog("UnPausing " + m_Player.gameObject.name);
                m_Player.AudioSource.UnPause();
            }
        }

        public void Stop()
        {
            if (m_Player != null)
            {
                m_Player.AudioSource.Stop();
            }
        }

        public void Play()
        {
            if (m_Player != null)
            {
                m_Player.AudioSource.Play();
            }
        }

        public void PlayScheduled(double dspTime)
        {
            if (m_Player != null)
            {
                m_Player.AudioSource.PlayScheduled(dspTime);
            }
        }

        public void StopAndRelease()
        {
            if (m_Player != null)
            {
                m_Player.ReleaseClip();
                m_Player = null;
            }
        }

        public WaitUntilAudioClipFinished WaitForAudioClipFinished()
        {
            WaitUntilAudioClipFinished w = new WaitUntilAudioClipFinished(m_Player);
            if (m_Player != null)
            {
                m_Player.AddWaitUntilFinishedInstruction(w);
            }
            return w;
        }

        public WaitUntilAudioClipStarted WaitForAudioClipStarted()
        {
            WaitUntilAudioClipStarted w = new WaitUntilAudioClipStarted(m_Player);
            if (m_Player != null)
            {
                m_Player.AddWaitUntilStartedInstruction(w);
            }
            return w;
        }

        public float Pitch
        {
            set
            {
                if (m_Player != null)
                {
                    m_Player.AudioSource.pitch = value;
                }
            }
        }

        public float Volume
        {
            set
            {
                if (m_Player != null)
                {
                    m_Player.AudioSource.volume = value;
                }
            }
        }

        public float Time
        {
            set
            {
                if (m_Player != null)
                {
                    m_Player.AudioSource.time = value;
                }
            }
        }

        public bool Loop
        {
            set
            {
                if (m_Player != null)
                {
                    m_Player.AudioSource.loop = value;
                }
            }
        }

        public float ClipLength
        {
            get
            {
                if (m_Player != null && m_Player.AudioSource.clip != null)
                {
                    return m_Player.AudioSource.clip.length;
                }

                D.AudioWarning("No clip found!");
                return 0;
            }
        }

        public float ClipDurationToEnd
        {
            get
            {
                if (m_Player != null && m_Player.AudioSource.clip != null)
                {
                    return m_Player.AudioSource.clip.length - m_Player.AudioSource.time;
                }

                D.AudioWarning("No clip found!");
                return 0;
            }
        }

        /// <summary>
        /// Don't use this! Use only for plugin compatibility issues
        /// </summary>    
        public AudioSource UnsafeAudioSource
        {
            get
            {
                if (m_Player == null) { return null; }
                return m_Player.AudioSource;
            }
        }

        public bool IsPlaying
        {
            get { return m_Player != null ? m_Player.AudioSource.isPlaying : false; }
        }

        public static AudioClipHandle ReleaseHandle(AudioClipHandle handle)
        {
            if (handle != null)
            {
                handle.StopAndRelease();
            }
            return null;
        }

        public void ChangePitch(AnimationCurve curve, float duration = 0f, bool destroyAfterDone = false)
        {
            if (m_Player == null || curve == null) return;
            m_Player.ChangePitch(curve, duration, destroyAfterDone);
        }

        public void ChangeVolume(AnimationCurve curve, float duration = 0f, bool destroyAfterDone = false)
        {
            if (m_Player == null || curve == null) return;
            m_Player.ChangeVolume(curve, duration, destroyAfterDone);
        }
    }
}