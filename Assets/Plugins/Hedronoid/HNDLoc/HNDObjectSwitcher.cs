using System;
using System.Collections;
using System.Collections.Generic;
using Hedronoid;
using Hedronoid.Events;
using UnityEngine;

namespace Hedronoid.HNDLoc
{
    public class NapObjectSwitcher : HNDGameObject
    {
        [Serializable]
        public class LangToObject
        {
            public string CultureCode;
            public GameObject Object;
        }

        [SerializeField]
        private GameObject m_FallbackObject;

        [SerializeField]
        List<LangToObject> m_LangsToObjects = new List<LangToObject>();

        protected override void Awake()
        {
            base.Awake();

            SwitchObjects();

            if (HNDLoc.Instance.CanChangeLanguageRuntime)
            {
                NNEvents.Instance.AddListener<LanguageChangedEvent>(OnLanguageChanged);
            }
        }

        private void OnLanguageChanged(LanguageChangedEvent e)
        {
            SwitchObjects();
        }

        private void SwitchObjects()
        {
            string currentLang = HNDLoc.Instance.CurrentLangCode;

            bool objectFound = false;
            foreach (var lo in m_LangsToObjects)
            {
                bool isCurrentLangObject = currentLang == lo.CultureCode;
                lo.Object.SetActive(isCurrentLangObject);

                objectFound |= isCurrentLangObject;
            }

            m_FallbackObject.SetActive(!objectFound);
        }
    }
}