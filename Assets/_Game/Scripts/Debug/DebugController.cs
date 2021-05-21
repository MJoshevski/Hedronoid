using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid.HNDTime;
using System;
using static InControl.InControlInputModule;
using static System.Net.Mime.MediaTypeNames;
using DG.Tweening;
using Hedronoid.Core;
using UnityEngine.SceneManagement;
using Hedronoid.Events;

namespace Hedronoid
{
    public class DebugController : HNDMonoSingleton<DebugController>
    {
        protected DebugController() { } // guarantee this will be always a singleton only - can't use the constructor!

        [Tooltip("Restarts current level.")]
        public KeyCode m_restartLevelKey = KeyCode.R;
        [Tooltip("Pauses current game.")]
        public KeyCode m_almostPauseKey = KeyCode.P;
        [Tooltip("Pauses current game.")]
        public KeyCode m_pauseGameKey = KeyCode.Pause;
        [Tooltip("Pauses game and brings up debug menu.")]
        public KeyCode m_debugMenuKey = KeyCode.Escape;
        [Tooltip("Speeds up the game.")]
        public KeyCode m_increaseTimeScaleKey = KeyCode.Equals;
        [Tooltip("Slows down the game.")]
        public KeyCode m_decreaseTimeScaleKey = KeyCode.Minus;
        [Tooltip("When paused, moves the game by one frame forward.")]
        public KeyCode m_nextFrameKey = KeyCode.N;
        [Tooltip("Toggles the visible Debug HUD On or Off.")]
        public KeyCode m_toggleHUDKey = KeyCode.RightBracket;
        [Tooltip("Toggles the audio On or Off.")]
        public KeyCode m_toggleAudioKey = KeyCode.M;

        public enum EShortcuts
        {
            PAUSE,
            ALMOST_PAUSE,
            DEBUG_MENU,
            NEXT_FRAME,
            TIMESCALE_INCREASE,
            TIMESCALE_DECREASE,
            RESET_SCENE,
            TOGGLE_DEBUG_HUD,
            TOGGLE_AUDIO
        }

        public struct EShortcutsComparer : IEqualityComparer<EShortcuts>
        {
            public bool Equals(EShortcuts x, EShortcuts y)
            {
                return x == y;
            }

            public int GetHashCode(EShortcuts obj)
            {
                // you need to do some thinking here,
                return (int)obj;
            }
        }

        [SerializeField]
        private bool ShortcutsEnabled = true;

        [SerializeField]
        private Camera m_DebugUICamera;

        public bool timeScaleControl = true;

        [SerializeField]
        private GameObject[] m_HUDObjects;

        private bool m_EnabledHUD = true;
        private bool EnabledHUD
        {
            set
            {
                m_EnabledHUD = value;
                foreach (var item in m_HUDObjects)
                {
                    item.SetActive(m_EnabledHUD);
                }
                m_DebugUICamera.enabled = m_EnabledHUD;
            }
        }

        // PUBLIC FIELDS
        public GameObject DebugPanel;
        public TMPro.TextMeshProUGUI TimeScaleText;
        public float GameTimePassed = 0;

        // PRIVATE FIELDS
        private Dictionary<EShortcuts, bool> m_ShortcutPressedDict = new Dictionary<EShortcuts, bool>(new EShortcutsComparer());
        private HNDTimeManager m_TimeManager;
        private bool m_skippedFrame = false;

        protected override void Awake()
        {
            base.Awake();

#if !(DEVELOPMENT_BUILD || UNITY_STANDALONE || UNITY_EDITOR)
            m_EnabledHUD = false;
#endif
            m_TimeManager = HNDTimeManager.Instance;
            EnabledHUD = m_EnabledHUD;
        }

        void Update()
        {
            m_ShortcutPressedDict[EShortcuts.RESET_SCENE] = Input.GetKeyDown(m_restartLevelKey);
            m_ShortcutPressedDict[EShortcuts.PAUSE] = Input.GetKeyDown(m_pauseGameKey);
            m_ShortcutPressedDict[EShortcuts.ALMOST_PAUSE] = Input.GetKeyDown(m_almostPauseKey);
            m_ShortcutPressedDict[EShortcuts.TIMESCALE_DECREASE] = Input.GetKey(m_decreaseTimeScaleKey);
            m_ShortcutPressedDict[EShortcuts.TIMESCALE_INCREASE] = Input.GetKey(m_increaseTimeScaleKey);
            m_ShortcutPressedDict[EShortcuts.NEXT_FRAME] = Input.GetKeyDown(m_nextFrameKey);
            m_ShortcutPressedDict[EShortcuts.DEBUG_MENU] = Input.GetKeyDown(m_debugMenuKey);
            m_ShortcutPressedDict[EShortcuts.TOGGLE_DEBUG_HUD] = Input.GetKeyDown(m_toggleHUDKey);
            m_ShortcutPressedDict[EShortcuts.TOGGLE_AUDIO] = Input.GetKeyDown(m_toggleAudioKey);

            if (!m_EnabledHUD)
            {
                // We need to hide the console with every update, otherwise it will appear on next update when error is raised
                Debug.developerConsoleVisible = false;
            }

            if (!ShortcutsEnabled)
            {
                return;
            }

            if (m_skippedFrame)
            {
                m_TimeManager.SetPaused(true, false);
                m_skippedFrame = false;
            }

            if (IsShortcutPressed(EShortcuts.TOGGLE_DEBUG_HUD))
            {
                EnabledHUD = !m_EnabledHUD;
            }

            TimeScaleText.text = Time.timeScale.ToString();

            if (IsShortcutPressed(EShortcuts.DEBUG_MENU))
            {
                if (!m_TimeManager.IsPaused)
                {
                    m_TimeManager.SetPaused(true, false);
                    DebugPanel.SetActive(true);
                    HNDEvents.Instance.Raise(new DebugMenuOpened());
                }
                else
                {
                    DebugPanel.SetActive(false);
                    m_TimeManager.SetPaused(false, false);
                    HNDEvents.Instance.Raise(new DebugMenuClosed());
                }
            }

            if (timeScaleControl && IsShortcutPressed(EShortcuts.PAUSE))
            {
                if (!m_TimeManager.IsPaused)
                {
                    m_TimeManager.SetPaused(true, true);
                }
                else
                {
                    m_TimeManager.SetPaused(false, true);
                }
            }

            if (timeScaleControl && IsShortcutPressed(EShortcuts.ALMOST_PAUSE))
            {
                if (!m_TimeManager.IsPaused)
                {
                    m_TimeManager.SetPaused(true, true, true);
                }
                else
                {
                    m_TimeManager.SetPaused(false, true);
                }
            }

            if (timeScaleControl && IsShortcutPressed(EShortcuts.TIMESCALE_INCREASE))
            {
                Time.timeScale = Mathf.Approximately(Time.timeScale, 0f) ? 
                    0.05f : Mathf.Lerp(Time.timeScale, 10f, Time.deltaTime);

                DOTween.timeScale = Time.timeScale;
            }

            if (timeScaleControl && IsShortcutPressed(EShortcuts.TIMESCALE_DECREASE))
            {
                Time.timeScale = Mathf.Lerp(Time.timeScale, 0f, Time.deltaTime);
                DOTween.timeScale = Time.timeScale;
            }

            if (timeScaleControl && IsShortcutPressed(EShortcuts.NEXT_FRAME) && m_TimeManager.IsPaused && !m_skippedFrame)
            {
                m_TimeManager.SetPaused(false, true);
                m_skippedFrame = true;
            }

            if (IsShortcutPressed(EShortcuts.RESET_SCENE))
            {
                GameController.Instance.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            // Mute/unmute sounds
            if (IsShortcutPressed(EShortcuts.TOGGLE_AUDIO))
            {
                string masterBusString = "Bus:/";
                FMOD.Studio.Bus masterBus;

                masterBus = FMODUnity.RuntimeManager.GetBus(masterBusString);
                float volume;
                masterBus.getVolume(out volume);

                if (volume > 0.5f)
                    masterBus.setVolume(0);
                else masterBus.setVolume(1);
            }
        }

        float _timeScaleBeforePause;

        public bool IsShortcutPressed(EShortcuts shortcut)
        {
            return m_ShortcutPressedDict[shortcut];
        }
    }
}