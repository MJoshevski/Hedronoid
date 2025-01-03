﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hedronoid;
using Hedronoid.Events;
using Hedronoid.HNDLoc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hedronoid.TMProExtension
{
    /// <summary>
    /// Manager for switching fonts for different languages
    /// </summary>
    public class FontManager : HNDGameObject
    {
        [Serializable]
        public class FontConfig
        {
            public string OriginalFontName;
            public List<string> MaterialVariations;
            public List<LanguageFontConfig> LangConfigs;
        }

        [Serializable]
        public class LanguageFontConfig
        {
            public string ForLangCodes;

            public string ReplacementFontName;

            [HideInInspector]
            public TMP_FontAsset FontAsset;

            [HideInInspector]
            public Dictionary<string, Material> FontMaterials = new Dictionary<string, Material>();
        }

        private static FontManager m_Instance;
        public static FontManager Instance { get { return m_Instance; } }

        [SerializeField]
        private List<FontConfig> m_FontConfigs;

        [SerializeField]
        private string m_FontBundlePath;

        AssetBundle m_FontBundle;

        protected override void Awake()
        {
            base.Awake();

            if (m_Instance != null)
            {
                Destroy(this);
                return;
            }

            m_Instance = this;

            PreloadFontAssets(Hedronoid.HNDLoc.HNDLoc.Instance.CurrentLangCode);
        }

        public void PreloadFontAssets(string langCode)
        {
            LoadBundle(m_FontBundlePath);

            if (m_FontBundle == null) { return; }

            foreach (var config in m_FontConfigs)
            {
                // Should be preload this one?
                LanguageFontConfig lfc = config.LangConfigs.FirstOrDefault(c => c.ForLangCodes.Contains(langCode));
                if (lfc != null)
                {
                    D.LocLog("Preloading font " + lfc.ReplacementFontName + " for lang " + langCode);
                    lfc.FontAsset = m_FontBundle.LoadAsset(lfc.ReplacementFontName) as TMP_FontAsset;
                    Material defaultMat = lfc.FontAsset.material;
                    foreach (string matVariant in config.MaterialVariations)
                    {
                        string matName = lfc.ReplacementFontName + " " + matVariant;
                        Material m = null;
                        if (matName == defaultMat.name)
                        {
                            m = defaultMat;
                        }
                        else
                        {
                            m = m_FontBundle.LoadAsset(matName) as Material;
                        }

                        if (m == null)
                        {
                            D.LocError("Material '" + matVariant + "' not found for font " + lfc.ReplacementFontName);
                            continue;
                        }

                        lfc.FontMaterials[matName] = m;
                        D.LocLog("Loaded material " + matName);
                    }
                }
            }
        }

        private void LoadBundle(string bundlePath)
        {
            if (!string.IsNullOrEmpty(bundlePath))
            {
                string path = Path.Combine(Application.streamingAssetsPath, m_FontBundlePath);
                m_FontBundle = AssetBundle.LoadFromFile(path);
                if (m_FontBundle != null)
                {
                    D.LocLog("Font Atlas bundle loaded successfully.");
                }
                else
                {
                    D.LocError("Asset bundle load failed from path: " + path);
                }
            }
        }

        public bool GetFontAndMaterial(string fontName, string materialName, out TMP_FontAsset fontAsset, out Material materialAsset)
        {
            // D.LocLog("GetFont - fontMatName: "+ fontMatName + ", HNDLoc.Instance.CurrentLangCode: "+ HNDLoc.Instance.CurrentLangCode);
            fontAsset = null;
            materialAsset = null;

            if (m_FontBundle == null)
            {
                D.LocWarning("Fonts bundle not loaded!");
                return false;
            }

            FontConfig fontConfig = m_FontConfigs.FirstOrDefault(fc => fc.OriginalFontName == fontName);
            if (fontConfig == null)
            {
                D.LocError("Font config not found for font: " + fontName);
                return false;
            }


            var languageFontConfig = fontConfig.LangConfigs.FirstOrDefault(c => c.ForLangCodes.Contains(Hedronoid.HNDLoc.HNDLoc.Instance.CurrentLangCode));
            if (languageFontConfig == null)
            {
                D.LocError("Font config not found for font: " + fontName + " and lang " + Hedronoid.HNDLoc.HNDLoc.Instance.CurrentLangCode);
                return false;
            }

            string replacementFontMatName = materialName.Replace(fontConfig.OriginalFontName, languageFontConfig.ReplacementFontName);
            if (!languageFontConfig.FontMaterials.ContainsKey(replacementFontMatName))
            {
                D.LocError("Font material not found for font: " + fontName + " and lang " + Hedronoid.HNDLoc.HNDLoc.Instance.CurrentLangCode + " and material " + replacementFontMatName);
                D.LocError("Available materials: " + string.Join(", ", languageFontConfig.FontMaterials.Keys.ToArray()));

                if (Hedronoid.HNDLoc.HNDLoc.Instance.Settings.AllowAnyFontMaterial && languageFontConfig.FontMaterials.Keys.Count > 0)
                {
                    replacementFontMatName = languageFontConfig.FontMaterials.Keys.FirstOrDefault();
                }
                else
                {
                    return false;
                }
            }

            fontAsset = languageFontConfig.FontAsset;
            materialAsset = languageFontConfig.FontMaterials[replacementFontMatName];

            return true;
        }
    }
}
