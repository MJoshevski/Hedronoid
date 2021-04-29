using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hedronoid;

namespace Hedronoid.UI
{
    public class LoadScreenManager : HNDMonoSingleton<LoadScreenManager>
    {
        protected LoadScreenManager() { } // guarantee this will be always a singleton only - can't use the constructor!

        [SerializeField]
        private Image m_BlackImage;
        [SerializeField]
        private GameObject m_LoadGraphics;
        [SerializeField]
        private float m_FadeDuration;

        private IEnumerator m_FadeToBlackCR;
        private Action m_ActionWhenFadedToBlack;
        private IEnumerator m_FadeFromBlackCR;
        private Action m_ActionWhenFadedFromBlack;

        public bool FadeToBlack(Action action = null)
        {
            if (m_FadeToBlackCR != null || m_FadeFromBlackCR != null)
            {
                return false;
            }

            m_ActionWhenFadedToBlack = action;
            m_FadeToBlackCR = FadeToBlackCR();
            StartCoroutine(m_FadeToBlackCR);

            return true;
        }

        public bool FadeFromBlack(Action action = null)
        {
            if (m_FadeToBlackCR != null || m_FadeFromBlackCR != null)
            {
                return false;
            }

            m_ActionWhenFadedFromBlack = action;
            m_FadeFromBlackCR = FadeFromBlackCR();
            StartCoroutine(m_FadeFromBlackCR);

            return true;
        }

        private IEnumerator FadeToBlackCR()
        {
            Color c = new Color(0f, 0f, 0f, 0f);
            m_BlackImage.color = c;
            float time = 0f;
            while (time <= m_FadeDuration)
            {
                WaitForEndOfFrame waitYield = new WaitForEndOfFrame();
                if (m_FadeDuration == 0)
                {
                    c.a = 1f;
                }
                else
                {
                    c.a = Mathf.Clamp01(time / m_FadeDuration);
                }
                m_BlackImage.color = c;
                time += Time.deltaTime;
                yield return waitYield;
            }

            c.a = 1f;
            m_BlackImage.color = c;
            if (m_LoadGraphics != null)
            {
                m_LoadGraphics.SetActive(true);
            }

            if (m_ActionWhenFadedToBlack != null)
            {
                m_ActionWhenFadedToBlack.Invoke();
            }
            m_FadeToBlackCR = null;
        }

        private IEnumerator FadeFromBlackCR()
        {
            if (m_LoadGraphics != null)
            {
                m_LoadGraphics.SetActive(false);
            }

            Color c = new Color(0f, 0f, 0f, 1f);
            m_BlackImage.color = c;
            float time = 0f;
            while (time <= m_FadeDuration)
            {
                WaitForEndOfFrame waitYield = new WaitForEndOfFrame();
                if (m_FadeDuration == 0)
                {
                    c.a = 0f;
                }
                else
                {
                    c.a = 1f - Mathf.Clamp01(time / m_FadeDuration);
                }
                m_BlackImage.color = c;
                time += Time.deltaTime;
                yield return waitYield;
            }

            c.a = 0f;
            m_BlackImage.color = c;

            if (m_ActionWhenFadedFromBlack != null)
            {
                m_ActionWhenFadedFromBlack.Invoke();
            }
            m_FadeFromBlackCR = null;
        }
    }
}