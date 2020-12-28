using System;
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
    [RequireComponent(typeof(TMP_InputField))]
    public class HNDLocInputFieldLocalizer : HNDGameObject
    {
        [SerializeField]
        private bool m_ShouldFormatText = false;

        private TMP_InputField m_InputField;
        public TMP_Text TMPText { get { return m_InputField.textComponent; } }

        private string m_OrigFontName;
        private string m_OrigFontMatName;

        private float m_DefaultCharacterSpacing = 0; //We need to store char spacing, since this will get set to 0 if playing in arabic 

        protected override void Awake()
        {
            base.Awake();

            m_InputField = GetComponent<TMP_InputField>();

            m_InputField.onValueChanged.AddListener(OnValueChanged);

            // TMPro.TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
            m_DefaultCharacterSpacing = TMPText.characterSpacing;

            UpdateText(TMPText.text, true);

            if (HNDLoc.Instance.CanChangeLanguageRuntime)
            {
                NNEvents.Instance.AddListener<LanguageChangedEvent>(OnLanguageChanged);
            }
        }

        private void OnValueChanged(string newValue)
        {
            // D.CoreLog("OnTextChanged!!!! - newValue: " + newValue + ", Text: "+ TMPText, this.gameObject);
            if (isActiveAndEnabled)
            {
                StartCoroutine(UpdateTextEndOfFrame());
            }
        }

        private void OnLanguageChanged(LanguageChangedEvent e)
        {
            UpdateText(TMPText.text, true);
        }

        private void UpdateFont()
        {
            if (string.IsNullOrEmpty(m_OrigFontMatName))
            {
                m_OrigFontMatName = TMPText.fontSharedMaterial.name;
                m_OrigFontName = TMPText.font.name;
            }

            if (FontManager.Instance)
            {
                TMP_FontAsset font = null;
                Material material = null;
                bool found = FontManager.Instance.GetFontAndMaterial(m_OrigFontName, m_OrigFontMatName, out font, out material);
                if (found)
                {
                    TMPText.font = font;
                    TMPText.fontSharedMaterial = material;

                    if (font != m_InputField.fontAsset)
                    {
                        m_InputField.fontAsset = font;
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
            if (forceFontChange)
            {
                UpdateFont();
            }

            if (m_ShouldFormatText)
            {
                //Handle arabic RTL conversion + letter merging
                var langConfig = HNDLoc.Instance.CurrentLanguageConfig;
                bool isRTL = langConfig.IsRightToLeft;
                if (langConfig.IsArabic){
                    TMPText.text = ArabicSupportExtension.FixWithRichSupport(TMPText.text, HNDLoc.Instance.Settings.ArabicUseTashkil, false,
                        TMPText.richText, HNDLoc.Instance.Settings.ArabicCombineTashkil);
                }

                if (TMPText.isRightToLeftText != isRTL) TMPText.isRightToLeftText = isRTL;
            }

            float charSpacing = HNDLoc.Instance.CurrentLanguageConfig.PreferredLetterSpacing(m_DefaultCharacterSpacing);
            if (TMPText.characterSpacing != charSpacing) TMPText.characterSpacing = charSpacing;
        }

        private IEnumerator UpdateTextEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            UpdateText(TMPText.text, false);
        }
    }
}