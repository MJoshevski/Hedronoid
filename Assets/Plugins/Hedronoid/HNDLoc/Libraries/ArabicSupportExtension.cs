﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hedronoid.HNDLoc
{
    public static class ArabicSupportExtension
    {

        // Cache those so we don't allocate new memory every time
        static Dictionary<string, string> s_hookToRichText = new Dictionary<string, string>();
        static StringBuilder s_builder = new StringBuilder(1000);

        public static string FixWithRichSupport(string str, bool showTashkeel, bool useHinduNumbers, bool richTextEnabled, bool combineTashkil = true)
        {
            if (string.IsNullOrEmpty(str)) return null;
            string s = str;

            // Only if rich text is enabled and there is actually some rich text
            if (richTextEnabled && s.IndexOf('<') >= 0)
            {
                s_hookToRichText.Clear();
                s_builder.Remove(0, s_builder.Length);
                s_builder.Append(str);

                // Fix rich text

                // replace rich text with hooks
                int startBracketIndex = -1;
                int hookIndex = 0;
                s = s_builder.ToString();
                while ((startBracketIndex = s.IndexOf('<')) >= 0)
                {
                    int endBrackedIndex = s.IndexOf('>', startBracketIndex);
                    if (endBrackedIndex < 0) break;

                    string richContent = s_builder.ToString(startBracketIndex, endBrackedIndex - startBracketIndex + 1);
                    s_builder.Remove(startBracketIndex, endBrackedIndex - startBracketIndex + 1);

                    string hookText = "AAA" + hookIndex + "AAA";
                    s_builder.Insert(startBracketIndex, hookText);
                    s_hookToRichText[hookText] = richContent;
                    s = s_builder.ToString();
                    hookIndex++;
                }

                //D.LocLog("Before fix: " + s);
            }

            string fix = ArabicSupport.ArabicFixer.Fix(s, showTashkeel, combineTashkil, false);

            if (richTextEnabled)
            {
                foreach (var kv in s_hookToRichText)
                {
                    fix = fix.Replace(kv.Key, ArabicSupportExtension.ReverseText(kv.Value));
                }

                //D.LocLog("After fix: " + fix);
            }

            fix = ArabicSupportExtension.ReverseText(fix);
            return fix;
        }


        public static string ReverseText(string source)
        {
            char[] output = new char[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                output[(output.Length - 1) - i] = source[i];
            }
            return new string(output);
        }
    }
}