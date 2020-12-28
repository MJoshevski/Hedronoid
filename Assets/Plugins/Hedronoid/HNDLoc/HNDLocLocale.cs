using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_PS4
using UnityEngine.PS4;
#endif

public static class HNDLocLocale
{

#if UNITY_IOS
	[DllImport ("__Internal")]
	static extern string GetPreferredLanguages();
#endif

    public static List<string> GetPreferredLangCodes()
    {
        List<string> preferred = new List<string>();

#if UNITY_PS4 && !UNITY_EDITOR

        var sysLangId = UnityEngine.PS4.Utility.systemLanguage;
        string localeVal = null;
        switch (sysLangId)
        {
            case UnityEngine.PS4.Utility.SystemLanguage.JAPANESE:
                localeVal = "ja-JP";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.ENGLISH_US:
                localeVal = "en-US";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.FRENCH:
                localeVal = "fr-FR";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.SPANISH:
                localeVal = "es-ES";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.GERMAN:
                localeVal = "de-DE";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.ITALIAN:
                localeVal = "it-IT";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.DUTCH:
                localeVal = "nl-NL";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.PORTUGUESE_PT:
                localeVal = "pt-PT";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.RUSSIAN:
                localeVal = "ru-RU";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.KOREAN:
                localeVal = "ko-KR";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.CHINESE_T:
                localeVal = "zh-CHT";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.CHINESE_S:
                localeVal = "zh-CHS";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.FINNISH:
                localeVal = "fi-FI";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.SWEDISH:
                localeVal = "sv-SE";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.DANISH:
                localeVal = "da-DK";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.NORWEGIAN:
                localeVal = "no-NO";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.POLISH:
                localeVal = "pl-PL";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.PORTUGUESE_BR:
                localeVal = "pt-BR";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.ENGLISH_GB:
                localeVal = "en-GB";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.TURKISH:
                localeVal = "tr-TR";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.SPANISH_LA:
                localeVal = "es-MX";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.ARABIC:
                localeVal = "ar-AR";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.FRENCH_CA:
                localeVal = "fr-CA";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.CZECH:
                localeVal = "cs-CZ";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.HUNGARIAN:
                localeVal = "hu-HU";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.GREEK:
                localeVal = "el-GR";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.ROMANIAN:
                localeVal = "ro-RO";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.THAI:
                localeVal = "th-TH";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.VIETNAMESE:
                localeVal = "vi-VN";
                break;
            case UnityEngine.PS4.Utility.SystemLanguage.INDONESIAN:
                localeVal = "id-ID";
                break;
            default:
                break;
        }

        D.LocLog("Locale INT: " + sysLangId);

        if (!string.IsNullOrEmpty(localeVal))
        {
            preferred.Add(localeVal);
        }

#elif UNITY_IOS && !UNITY_EDITOR

        string concat = GetPreferredLanguages();
        D.LocLog("Langs: " + concat);
        if (!string.IsNullOrEmpty(concat))
        {
            var codes = concat.Split(';').ToList();
            if (codes.Count > 0)
            {
                foreach (string code in codes)
                {
                    string c = code;
                    if (!string.IsNullOrEmpty(c))
                    {
                        string[] parts = c.Split('-');
                        if(parts.Length > 1 && parts[0] != null && parts[0].Length > 2){
                            parts[0] = parts[0].Substring(0, 2);
                        }

                        string modifiedValue = string.Join("-", parts);
                        preferred.Add(modifiedValue);
                    }
                }
            }
        }

#elif UNITY_ANDROID && !UNITY_EDITOR

        string localeVal = null;
        string localeLang = null;
        string localeCountry = null;
        D.LocLog("locale A ");
        //Resources.getSystem().getConfiguration().locale

        AndroidJavaClass resourcesCls = new AndroidJavaClass("android.content.res.Resources");
        if (resourcesCls != null)
        {
            D.LocLog("locale B ");
            AndroidJavaObject resourcesSystemObj = resourcesCls.CallStatic<AndroidJavaObject>("getSystem");
            if (resourcesSystemObj != null)
            {
                D.LocLog("locale C ");
                AndroidJavaObject configObj = resourcesSystemObj.Call<AndroidJavaObject>("getConfiguration");
                if (configObj != null)
                {
                    D.LocLog("locale D ");
                    AndroidJavaObject localeObj = configObj.Get<AndroidJavaObject>("locale");
                    if (localeObj != null)
                    {
                        D.LocLog("locale A ");
                        localeLang = localeObj.Call<string>("getLanguage");
                        localeCountry = localeObj.Call<string>("getCountry");

                        // Trim to 2 chars for the right ISO
                        if (!string.IsNullOrEmpty(localeLang))
                        {
                            if (localeLang.Length > 2)
                            {
                                localeLang = localeLang.Substring(0, 2);
                            }
                        }

                        localeVal = localeLang + "-" + localeCountry;
                        D.LocLog("locale val " + localeVal);
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(localeVal))
        {
            preferred.Add(localeVal);
        }

#else


        string localeVal = null;

        switch (Application.systemLanguage)
        {
            case SystemLanguage.English:
                localeVal = "en-GB";
                break;
            case SystemLanguage.Danish:
                localeVal = "da-DK";
                break;
            default:
                break;
        }


        if (!string.IsNullOrEmpty(localeVal))
        {
            preferred.Add(localeVal);
        }

#endif

        // Always add en-gb as a fallback
        if (preferred.Count == 0)
        {
            preferred.Add("en-GB");
        }

        preferred = preferred.Where(l => !string.IsNullOrEmpty(l)).Select(l => l.ToLowerInvariant()).ToList();
        D.LocLog("System languages: " + string.Join(";", preferred.ToArray()));
        return preferred;
    }
}
