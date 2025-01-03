﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.HNDLoc
{
    public class HNDLocData
    {
        Dictionary<string, Dictionary<string, string>> m_KeyToValuesLangs;

        Dictionary<string, HNDLocTerm> m_KeyToTerm;

        List<string> m_LangCodes;

        public List<string> LangCodes { get { return m_LangCodes; } }

#if UNITY_EDITOR

        public Dictionary<string, Dictionary<string, string>> KeyToValuesLangs { get { return m_KeyToValuesLangs; } }

        public Dictionary<string, HNDLocTerm> KeyToTermValue { get { return m_KeyToTerm; } }

        public string GetRawStringValue(string lang, string key)
        {
            if (!m_KeyToValuesLangs.ContainsKey(key))
            {
                return null;
            }

            Dictionary<string, string> data = m_KeyToValuesLangs[key];
            if (!data.ContainsKey(lang))
            {
                return null;
            }

            return data[lang];
        }

        public void Wipe()
        {
            ClearAllData();
        }

#endif

        private void ClearAllData()
        {
            m_KeyToValuesLangs = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            m_KeyToTerm = new Dictionary<string, HNDLocTerm>(StringComparer.OrdinalIgnoreCase);
            m_LangCodes = new List<string>();
        }

        public HNDLocData()
        {
            ClearAllData();
        }

        public string GetStringValue(string lang, string key, bool debug)
        {
            if (string.IsNullOrEmpty(key))
            {
                // D.LocWarning("Tried to get string value for NULL key. This is not possible. Returning empty string.");
                return "";
            }

            if (!m_KeyToValuesLangs.ContainsKey(key))
            {
                D.LocWarning("No definitions for key " + key);

                if (debug)
                {
                    key = "#" + key;
                }

                return key;
            }

            Dictionary<string, string> data = m_KeyToValuesLangs[key];
            if (!data.ContainsKey(lang))
            {
                D.LocWarning("No value for key " + key + " for lang code " + lang);

                if (debug)
                {
                    key = "#" + key;
                }

                return key;
            }

            // New lines
            string val = data[lang].Replace("\\n", "\n");

            //If string is empty, return suorce text
            if (val == "")
                return "#" + m_KeyToTerm[key].SourceText;


            if (debug)
            {
                val = "*" + val;
            }

            return val;
        }

        public void AddKey(string key, string source = null)
        {
            if (!m_KeyToValuesLangs.ContainsKey(key))
            {
                HNDLocTerm t = new HNDLocTerm();
                t.Key = key;
                t.SourceText = source;
                t.Description = "";
                t.TermType = HNDLocTermType.TEXT;

                m_KeyToValuesLangs.Add(key, new Dictionary<string, string>());
                m_KeyToTerm.Add(key, t);
            }
        }

        public void RemoveKey(string key)
        {
            if (m_KeyToValuesLangs.ContainsKey(key))
            {
                m_KeyToValuesLangs.Remove(key);
                m_KeyToTerm.Remove(key);
            }
        }

        public bool ContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            return m_KeyToTerm.ContainsKey(key);
        }

        public void AddLang(string langCode)
        {
            langCode = langCode.ToLowerInvariant();
            if (!m_LangCodes.Contains(langCode))
            {
                m_LangCodes.Add(langCode);
            }
        }

        public void RemoveLang(string langCode)
        {
            if (m_LangCodes.Contains(langCode))
            {
                m_LangCodes.Remove(langCode);
            }

            foreach (var kv in m_KeyToValuesLangs)
            {
                var kv2 = kv.Value;
                if (kv2.ContainsKey(langCode))
                {
                    kv2.Remove(langCode);
                }
            }
        }

        public void SetKeyValueForLang(string key, string value, string lang, string source, string desc)
        {
            if (!m_LangCodes.Contains(lang))
            {
                D.LocError("Undefined language " + lang);
                return;
            }

            if (!m_KeyToValuesLangs.ContainsKey(key))
            {
                m_KeyToValuesLangs.Add(key, new Dictionary<string, string>());
                m_KeyToTerm.Add(key, new HNDLocTerm());
            }

            Dictionary<string, string> keyVals = m_KeyToValuesLangs[key];
            keyVals[lang] = value;

            if (source != null)
            {
                HNDLocTerm t = m_KeyToTerm[key];
                t.SourceText = source;
                t.Description = desc;
                t.Key = key;
                t.TermType = HNDLocTermType.TEXT;
            }
        }
    }
}