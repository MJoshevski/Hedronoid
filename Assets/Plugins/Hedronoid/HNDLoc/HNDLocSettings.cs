using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hedronoid.HNDLoc
{
    [CreateAssetMenu]
    public class HNDLocSettings : ScriptableObject
    {
        public bool IntegrateWithLAMS = false;

        [Tooltip("HNDLoc will be automatically initialized upon Instance call. Otherwise InitWithSettings() method must be called before touching HNDLoc.")]
        public bool AutoInitHNDLoc = true;

        [Tooltip("Language can be changed runtime and all should still work properly, this cannot be switched on during runtime!")]
        public bool SupportsRuntimeLanguageChange = true;

        [Tooltip("If no available language is found, this will be a fallback language")]
        public string GlobalFallbackLanguage = "en-gb";

        public List<FallbackLanguage> FallbackLanguages = new List<FallbackLanguage>();

        [Tooltip("All supported languages")]
        public List<string> SupportedLanguages = new List<string>();

        public List<HNDLocLanguageConfig> LanguageConfigs = new List<HNDLocLanguageConfig>();

        // Some codes are different than the ones we're using in LAMS
        // http://www.csharp-examples.net/culture-names/
        [Tooltip("This is culture codes for C#. So essentially replacing LAMS/Device code to C#.")]
        public List<HNDLocReplacementCultureCode> ReplacementCultureCodes = new List<HNDLocReplacementCultureCode> {
            new HNDLocReplacementCultureCode { OriginalCode = "no-no", ReplacementCode = "nb-no"},
            new HNDLocReplacementCultureCode { OriginalCode = "ar-ar", ReplacementCode = "ar-sa"},
        };

        [Tooltip("Can be used for loading alternative bundles if the desired ones are not present")]
        public List<HNDLocReplacementCultureCode> FallbackAudioLangCodes = new List<HNDLocReplacementCultureCode> {
            new HNDLocReplacementCultureCode { OriginalCode = "es-mx", ReplacementCode = "es-es"},
            new HNDLocReplacementCultureCode { OriginalCode = "pt-br", ReplacementCode = "pt-pt"},
            new HNDLocReplacementCultureCode { OriginalCode = "en-us", ReplacementCode = "en-gb"},
        };

        [Tooltip("Can be used for loading alternative bundles if the deisred ones are not present")]
        public List<HNDLocReplacementCultureCode> FallbackSubtitlesLangCodes = new List<HNDLocReplacementCultureCode> {};

        [Tooltip("Use tashkil dialect (kids and learning materials)")]
        public bool ArabicUseTashkil = false;

        [Tooltip("Combine tashkil signs")]
        public bool ArabicCombineTashkil = false;

        [Header("Debug settings")]

        [Tooltip("If the correct font material can't be found at runtime, use any material for the font")]
        public bool AllowAnyFontMaterial = false;

        [Tooltip("Showing * or # depending if key was found")]
        public bool DebugMode = false;

        [Tooltip("Tripling all the strings to test autosizing and bounds of text fields")]
        public bool LongTextDebugMode = false;

        [Tooltip("This will set the default culture based on a selected language")]
        public bool DebugSetCultureByLang = false;

        [Tooltip("This will be a first preferred language if filled")]
        public string DebugForceLanguageCode = null;

        public HNDLocLanguageConfig GetConfigForLanguage(string code)
        {
            HNDLocLanguageConfig config = LanguageConfigs.FirstOrDefault(c => c.Code == code);

            if (config == null)
            {
                // return default
                config = new HNDLocLanguageConfig();
                config.Code = code;
            }

            HNDLocReplacementCultureCode cultureCode = ReplacementCultureCodes.FirstOrDefault(c => c.OriginalCode == code);
            if (cultureCode != null)
            {
                config.ReplacementCultureCode = cultureCode.ReplacementCode;
            }

            return config;
        }
    }

    [Serializable]
    public class HNDLocLanguageConfig
    {
        public string Code;
        public bool IsRightToLeft = false;
        public float OverrideLetterSpacing = -1;
        public bool IsArabic = false;
        public bool TMPInvertGeometrySorting = false;
        public string ReplacementCultureCode { get; set; }

        public string CultureCode { get { return string.IsNullOrEmpty(ReplacementCultureCode) ? Code : ReplacementCultureCode; } }

        public float PreferredLetterSpacing(float originalSpacing)
        {
            return OverrideLetterSpacing >= 0 ? OverrideLetterSpacing : originalSpacing;
        }
    }

    [Serializable]
    public class HNDLocReplacementCultureCode
    {
        public string OriginalCode;
        public string ReplacementCode;
    }
}
