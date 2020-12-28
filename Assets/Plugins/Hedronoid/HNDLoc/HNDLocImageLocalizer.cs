using System.Collections;
using System.Collections.Generic;
using Hedronoid.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Hedronoid.HNDLoc
{
    [RequireComponent(typeof(Image))]
    public class HNDLocImageLocalizer : HNDGameObject
    {

        Image m_Image;

        Sprite m_DefaultSprite;

        [SerializeField]
        string m_ImageNameInBundle;

        protected override void Awake()
        {
            base.Awake();

            m_Image = GetComponent<Image>();
            m_DefaultSprite = m_Image.sprite;

            if (string.IsNullOrEmpty(m_ImageNameInBundle) && m_DefaultSprite != null)
            {
                m_ImageNameInBundle = m_DefaultSprite.name;
            }

            UpdateImage();

            if (HNDLoc.Instance.CanChangeLanguageRuntime)
            {
                NNEvents.Instance.AddListener<LanguageChangedEvent>(OnLanguageChanged);
            }
        }

        private void OnLanguageChanged(LanguageChangedEvent e)
        {
            UpdateImage();
        }

        private void UpdateImage()
        {
            if (string.IsNullOrEmpty(m_ImageNameInBundle)) return;

            Sprite s = HNDLoc.Instance.GetLocalizedSprite(m_ImageNameInBundle);
            if (s == null)
            {
                D.LocWarningFormat("Can't find localized image named '{0}'", m_ImageNameInBundle);
                return;
            }

            m_Image.sprite = s;
        }
    }
}