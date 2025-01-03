﻿using System;
using System.Collections;
using System.Collections.Generic;
using Hedronoid;
using Hedronoid.Events;
using Hedronoid.TMProExtension;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Hedronoid.HNDLoc
{
    [RequireComponent(typeof(TMP_Text))]
    public class HNDLocTextLocalizer : HNDGameObject
    {
        private TMP_Text m_Text;

        private string m_OrigFontName;
        private string m_OrigFontMatName;

        [SerializeField]
        private bool m_ShouldLocalizeText = true;

        [SerializeField]
        private bool m_ShouldFormatText = true;

        private float m_MaxSizeAtStart = -1;

        private VertexSortingOrder m_GeometrySortingAtStart;

        private float m_DefaultCharacterSpacing = 0; //We need to store char spacing, since this will get set to 0 if playing in Arabic 

        private string m_LastText = "";

        [Serializable]
        public class OverrideSizeForLang
        {
            public string Lang;
            public float Size;
        }

        [SerializeField]
        private List<OverrideSizeForLang> m_OverrideSizes = null;


        public bool ShouldLocalizeText
        {
            get { return m_ShouldLocalizeText; }
            set { m_ShouldLocalizeText = value; }
        }

        public TMP_Text TMPText
        {
            get
            {
                if (m_Text == null)
                {
                    m_Text = GetComponent<TMP_Text>();
                }
                return m_Text;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            m_Text = GetComponent<TMP_Text>();

            if (string.IsNullOrEmpty(m_LastText))
                m_LastText = m_Text.text;
            else // Andrej : .Text setter has been called before awake.
                m_Text.text = m_LastText;

            m_MaxSizeAtStart = m_Text.fontSizeMax;
            m_DefaultCharacterSpacing = m_Text.characterSpacing;
            m_GeometrySortingAtStart = m_Text.geometrySortingOrder;

            UpdateText(m_LastText, true);

            if (HNDLoc.Instance.CanChangeLanguageRuntime)
            {
                HNDEvents.Instance.AddListener<LanguageChangedEvent>(OnLanguageChanged);
            }
        }

        private void OnLanguageChanged(LanguageChangedEvent e)
        {
            UpdateText(m_LastText, true);
        }

        public string Text
        {
            set
            {
                if (m_Text != null && value != m_LastText)
                    UpdateText(value, false);

                m_LastText = value;
            }
            get
            {
                return m_LastText;
            }
        }

        private void UpdateFont()
        {
            if (string.IsNullOrEmpty(m_OrigFontMatName))
            {
                m_OrigFontMatName = m_Text.fontSharedMaterial.name;
                m_OrigFontName = m_Text.font.name;
            }

            if (FontManager.Instance)
            {
                TMP_FontAsset font = null;
                Material material = null;
                bool found = FontManager.Instance.GetFontAndMaterial(m_OrigFontName, m_OrigFontMatName, out font, out material);
                if (found)
                {
                    m_Text.font = font;
                    m_Text.fontSharedMaterial = material;
                    m_Text.fontSizeMax = m_MaxSizeAtStart;
                }

                if (m_OverrideSizes != null && m_OverrideSizes.Count > 0)
                {
                    OverrideSizeForLang o = m_OverrideSizes.Find(s => s.Lang == HNDLoc.Instance.CurrentLangCode);
                    if (o != null)
                    {
                        m_Text.fontSize = o.Size;
                        m_Text.fontSizeMax = o.Size;
                    }
                }
            }
            else
            {
                D.LocWarning("FontManager.Instance does not exists - cannot change font for: " + this.gameObject);
            }
        }

        private void UpdateText(string text, bool forceFontChange)
        {
            // D.LocLog("UpdateText - this: "+ this.gameObject + ", m_ShouldLocalizeText: "+ m_ShouldLocalizeText + ", isArabic: " + (HNDLoc.Instance.CurrentLangCode.ToLowerInvariant() == "ar-ar"), this.gameObject);

            if (forceFontChange)
            {
                UpdateFont();
            }

            if (m_ShouldLocalizeText)
            {
                m_Text.text = text.HNDLocalized();
            }
            else
            {
                m_Text.text = text;
            }

            if (m_ShouldFormatText)
            {
                //Handle arabic RTL conversion + letter merging + dialects
                var langConfig = HNDLoc.Instance.CurrentLanguageConfig;
                if (langConfig.IsArabic)
                {
                    m_Text.text = ArabicSupportExtension.FixWithRichSupport(m_Text.text, HNDLoc.Instance.Settings.ArabicUseTashkil, false, m_Text.richText,
                        HNDLoc.Instance.Settings.ArabicCombineTashkil);
                }

                if (m_Text.isRightToLeftText != langConfig.IsRightToLeft) m_Text.isRightToLeftText = langConfig.IsRightToLeft;

                // Update character spacing
                float charSpacing = langConfig.PreferredLetterSpacing(m_DefaultCharacterSpacing);
                if (m_Text.characterSpacing != charSpacing) m_Text.characterSpacing = charSpacing;

                // Invert geometry sorting order (useful for shadows in RTL languages)
                if (langConfig.TMPInvertGeometrySorting)
                {
                    m_Text.geometrySortingOrder = m_GeometrySortingAtStart == VertexSortingOrder.Normal ? VertexSortingOrder.Reverse : VertexSortingOrder.Normal;
                }
            }
        }


        [ContextMenu("Set Max Font Size")]
        public void SetMaxFontSize()
        {
            m_Text.fontSizeMax = m_MaxSizeAtStart;
        }
    }
}
