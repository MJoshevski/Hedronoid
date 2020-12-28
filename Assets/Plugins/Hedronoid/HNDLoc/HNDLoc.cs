using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Hedronoid;
using Hedronoid.Events;
using UnityEngine;

namespace Hedronoid.HNDLoc
{

    public class LanguageChangedEvent : NNBaseEvent
    {
        public string To;
        public string From;
    }

    [Serializable]
    public class FallbackLanguage
    {
        public string ISOCode;
        public string FallbackISOCode;
    }

    public class HNDLoc : MonoBehaviour
    {

        private static HNDLoc m_HNDLoc;

        private bool m_DebugMode;

        public bool m_DebugLongTextMode;

        HNDLocSettings m_HNDLocSettings;

        public static HNDLoc Instance
        {
            get
            {
                if (m_HNDLoc == null)
                {
                    GameObject go = new GameObject();
                    go.name = "HNDLoc";
                    go.AddComponent<HNDLoc>();
                    DontDestroyOnLoad(go);
                    m_HNDLoc = go.GetComponent<HNDLoc>();
                    m_HNDLoc.InitIfNot();
                }
                return m_HNDLoc;
            }
        }

        public HNDLocSettings Settings { get { return m_HNDLocSettings; } }

        private string m_CurrentLangCode;

        private CultureInfo m_CurrentLangCulture;

        private HNDLocData m_Data;

        private string m_AssetBundlesPath;
        private AssetBundle m_LocalizedImageBundle;


        private HNDLocLanguageConfig m_CurrentLanguageConfig;

        private List<string> m_PreferredLocales;

        public List<string> PreferredLocales { get { return m_PreferredLocales; } }

        public HNDLocLanguageConfig CurrentLanguageConfig { get { return m_CurrentLanguageConfig; } }

        public bool CanChangeLanguageRuntime
        {
            get { return m_HNDLocSettings.SupportsRuntimeLanguageChange; }
        }

        public static HNDLocSettings GetOrCreateSettings()
        {
            HNDLocSettings s = NNScriptableObjectExtensions.LoadOrCreateScriptableObjectInResources<HNDLocSettings>("HNDLocSettings");
            return s;
        }

        public void InitWithSettings(HNDLocSettings settings)
        {
            m_HNDLocSettings = settings;

            // read all HNDLocs
            //TODO: Martin should read only specific lang later
            HNDLocImportExport ie = new HNDLocImportExport();
            m_Data = ie.ImportAll("HNDLoc");

            // Find closest preferred language:
            List<string> available = m_Data.LangCodes;

            List<string> includedInBuild = m_HNDLocSettings.SupportedLanguages;
            if (includedInBuild != null && includedInBuild.Count > 0)
            {
                D.LocLog("Included Languages:");
                for (int i = 0; i < includedInBuild.Count; i++)
                {
                    D.LocLog("#" + i + ": " + includedInBuild[i]);
                }

                // Check if language is really available
                available = includedInBuild.Where(i => available.Contains(i)).ToList();
            }

            D.LocLog("Available Languages:");
            for (int i = 0; i < available.Count; i++)
            {
                D.LocLog("#" + i + ": " + available[i]);
            }

            List<string> preferred = HNDLocLocale.GetPreferredLangCodes();

            D.LocLog("Preferred Languages:");
            for (int i = 0; i < preferred.Count; i++)
            {
                D.LocLog("#" + i + ": " + preferred[i]);
            }

            // Testing specific lang
            if (!string.IsNullOrEmpty(m_HNDLocSettings.DebugForceLanguageCode))
            {
                D.LocWarning("Inserting debug language as preferred!");
                preferred.Insert(0, m_HNDLocSettings.DebugForceLanguageCode);
            }

            m_PreferredLocales = preferred;

            string langCode = null;

            // Try almost exact match (en-gb, pt-br, en -> en-gb)
            if (string.IsNullOrEmpty(langCode))
            {
                foreach (string pref in preferred)
                {
                    // 1. Try exact match
                    if (available.Contains(pref))
                    {
                        D.LocLog("Picking exact match: " + pref);
                        langCode = pref;
                        break;
                    }

                    // Get the lang code part
                    string[] split = pref.Split('-');
                    string prefix = split[0];

                    // Is the prefix in fallback languages? -> switch to fallback
                    prefix = m_HNDLocSettings.FallbackLanguages.Any(l => l.ISOCode == prefix) ? m_HNDLocSettings.FallbackLanguages.First(l => l.ISOCode == prefix).FallbackISOCode : prefix;
                    split[0] = prefix;
                    string fallbackFull = string.Join("-", split);

                    // 2. Try fallback exact match
                    if (available.Contains(fallbackFull))
                    {
                        D.LocLog("Picking fallback exact match: " + fallbackFull);
                        langCode = fallbackFull;
                        break;
                    }

                    // 3. If the first language is not well defined, we'll try to find the preferred as the first similar one
                    if (!pref.Contains("-"))
                    {
                        string closestAvailable = available.FirstOrDefault(l2 => l2.Split('-')[0] == prefix);
                        if (!string.IsNullOrEmpty(closestAvailable))
                        {
                            D.LocLog("Picking closest from general preference: " + closestAvailable);
                            langCode = closestAvailable;
                            break;
                        };
                    }

                    // 4. If the language is defined but not found exact match, try just the language part
                    string closestGeneral = available.FirstOrDefault(l2 => l2.Split('-')[0] == prefix);
                    if (!string.IsNullOrEmpty(closestGeneral))
                    {
                        D.LocLog("Picking closest from general preference: " + closestGeneral);
                        langCode = closestGeneral;
                        break;
                    };
                }
            }

            // Set project default
            if (string.IsNullOrEmpty(langCode))
            {
                langCode = m_HNDLocSettings.GlobalFallbackLanguage;
            }

            m_CurrentLangCode = langCode;
            m_CurrentLanguageConfig = m_HNDLocSettings.GetConfigForLanguage(langCode);
            UpdateCulture(m_CurrentLanguageConfig);

            D.LocLog("Selected language: " + langCode);

            m_DebugMode = m_HNDLocSettings.DebugMode;
            m_DebugLongTextMode = m_HNDLocSettings.LongTextDebugMode;
        }

        private void InitIfNot()
        {
            if (m_Data != null)
            {
                // already initialized
                return;
            }

            m_HNDLocSettings = GetOrCreateSettings();
            if (m_HNDLocSettings.AutoInitHNDLoc == false)
            {
                D.LocLog("Not initializing HNDLoc yet as AutoInitHNDLoc is false");
                return;
            }

            InitWithSettings(m_HNDLocSettings);
        }

        void Awake()
        {
            InitIfNot();
        }

        private void UpdateCulture(HNDLocLanguageConfig config)
        {
            m_CurrentLangCulture = null;

            try
            {
                string code = config.CultureCode;
                m_CurrentLangCulture = new CultureInfo(code);

                if (m_HNDLocSettings.DebugSetCultureByLang)
                {
                    Thread.CurrentThread.CurrentCulture = m_CurrentLangCulture;
                    Thread.CurrentThread.CurrentUICulture = m_CurrentLangCulture;
                }
            }
            catch (Exception e)
            {
                D.LocError("Cannot instantiate culture with code: '" + config.CultureCode + "': " + e);
            }
        }

        public void LoadAssetBundles(string path)
        {
            m_AssetBundlesPath = path;
            TryLoadLocalizedImageBundle(m_CurrentLangCode);
        }

        private void TryLoadLocalizedImageBundle(string code)
        {
            if (m_LocalizedImageBundle != null)
            {
                m_LocalizedImageBundle.Unload(false);
            }
            m_LocalizedImageBundle = null;

            bool loaded = LoadLocalizedImageAssetBundles(code);

            if (!loaded && !string.IsNullOrEmpty(m_HNDLocSettings.GlobalFallbackLanguage))
            {
                LoadLocalizedImageAssetBundles(m_HNDLocSettings.GlobalFallbackLanguage);
            }
        }

        private bool LoadLocalizedImageAssetBundles(string languageCode)
        {
            if (string.IsNullOrEmpty(m_AssetBundlesPath))
            {
                D.LocError("Trying to load localized image asset bundles, but the asset bundle path is undefined.");
                return false;
            }

            string path = m_AssetBundlesPath;
            path = path + "_" + languageCode;
            AssetBundle b = null;
            b = AssetBundle.LoadFromFile(path);
            if (b != null)
            {
                m_LocalizedImageBundle = b;
                D.LocLog("Asset bundle loaded: " + path);
            }
            else
            {
                D.LocError("Asset bundle load failed: " + path);
            }
            return m_LocalizedImageBundle != null;
        }

        public string CurrentLangCode
        {
            get { return m_CurrentLangCode; }
            set
            {
                if (!m_HNDLocSettings.SupportsRuntimeLanguageChange)
                {
                    D.LocError("Runtime language change is disabled in HNDLoc settings!");
                    return;
                }

                if (m_CurrentLangCode == value)
                {
                    return;
                }

                TryLoadLocalizedImageBundle(value);
                LanguageChangedEvent lc = new LanguageChangedEvent { From = m_CurrentLangCode, To = value };
                m_CurrentLangCode = value;
                m_CurrentLanguageConfig = m_HNDLocSettings.GetConfigForLanguage(value);
                UpdateCulture(m_CurrentLanguageConfig);
                D.LocLog("Language changed to " + value);
                NNEvents.Instance.Raise(lc);
            }
        }

        public string GetLocalizedString(string key)
        {
            string text = m_Data.GetStringValue(m_CurrentLangCode, key, m_DebugMode);
            return m_DebugLongTextMode ? (text + " " + text + " " + text) : text;
        }

        public string GetLocalizedUpperString(string key)
        {
            string text = m_Data.GetStringValue(m_CurrentLangCode, key, m_DebugMode);

            if (m_CurrentLangCulture != null)
            {
                text = text.ToUpper(m_CurrentLangCulture);
            }
            else
            {
                text = text.ToUpper();
            }

            return m_DebugLongTextMode ? (text + " " + text + " " + text) : text;
        }

        public string GetLocalizedStringFormat(string key, params object[] list)
        {
            string text = string.Format(m_Data.GetStringValue(m_CurrentLangCode, key, m_DebugMode), list);
            return m_DebugLongTextMode ? (text + " " + text + " " + text) : text;
        }

        public Texture GetLocalizedTexture(string key)
        {
            if (m_LocalizedImageBundle == null)
            {
                D.CoreWarning("No asset bundle loaded for localized images.");
                return null;
            }

            Texture tex = m_LocalizedImageBundle.LoadAsset(key) as Texture;
            D.LocLog("Localized texture for " + key + " = " + (tex != null ? tex.name : "NULL"));
            return tex;
        }

        public Sprite GetLocalizedSprite(string key)
        {
            if (m_LocalizedImageBundle == null)
            {
                D.CoreWarning("No asset bundle loaded for localized images.");
                return null;
            }

            Texture tex = GetLocalizedTexture(key);
            if (tex != null)
            {
                Sprite sprite = Sprite.Create(tex as Texture2D, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                return sprite;
            }

            return null;
        }
    }
}