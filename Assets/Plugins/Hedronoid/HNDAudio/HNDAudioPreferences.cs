using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Hedronoid.Audio
{
    public class NapAudioPreferences : ScriptableObject
    {
        private static NapAudioPreferences m_LoadedInstance;

        public static NapAudioPreferences Instance
        {
            get
            {
                if (m_LoadedInstance == null)
                {
                    m_LoadedInstance = NNScriptableObjectExtensions.LoadOrCreateScriptableObjectInResources<NapAudioPreferences>("NapAudioPreferences");
                }
                return m_LoadedInstance;
            }
        }

        public string AudioManagerDataPath = "Assets/Global/Audio/AudioManagerData.prefab";

        public string AudioEnumPath = "Assets/Global/Audio/Scripts/AudioList.cs";

        public string MissingSoundTriggerId = "GEN_Missing_Sound";

        public bool PlayMissingSound = false;

        public AudioSource3DData Default3DSettings;

        [Header("LAMS")]
        public string ProjectVOCategoryName = "VO";
        public string LAMSVOCategoryName = "VO";
        
        void Reset()
        {
            Default3DSettings = new AudioSource3DData(1f, 100f, 20000f, 1f, AudioRolloffMode.Logarithmic, 0f, 1f, true, true);
        }
    }
}