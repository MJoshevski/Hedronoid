﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hedronoid.HNDLoc
{
    public class HNDLocImportExport
    {
        private const string NEWLINE_REPLACEMENT = "\\n";

        public HNDLocData ImportAll(string rootPath)
        {
            HNDLocData d = new HNDLocData();
            UnityEngine.Object[] assets = Resources.LoadAll(rootPath);
            foreach (UnityEngine.Object o in assets)
            {
                if (o is TextAsset)
                {
                    TextAsset ta = (TextAsset)o;
                    d.AddLang(ta.name);

                    ImportLang(ta, d, ta.name);
                }
            }

            return d;
        }

        private List<string> ParseAllValues(string line)
        {
            List<string> values = new List<string>();
            int start = 1;
            int end = start;
            while (start < line.Length)
            {
                end = FindTerm(line, start);
                string key = line.Substring(start, end - start - 1);
                key = key.Replace("\"\"", "\"");
                start = end + 2;
                values.Add(key);
            }
            return values;
        }

        public void ImportLang(TextAsset asset, HNDLocData data, string langCode)
        {
            D.LocLog("Importing: " + asset.name);

            StreamReader reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(asset.text)));
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                List<string> parsed = ParseAllValues(line);
                if (parsed == null || parsed.Count < 5)
                {
                    D.LocError("Parse failed on line: " + line);
                }
                //Martin: not yet used: HNDLocTermType termType = (HNDLocTermType)(Int32.Parse(parsed[0]));
                string key = parsed[1];
                string source = parsed[2];
                string value = parsed[3];
                string desc = parsed[4];

                //D.LocLog(key + " : " + value);

                if (string.IsNullOrEmpty(key))
                {
                    D.LocWarning("Key is empty: " + key + " : " + value);
                    continue;
                }

                // if(string.IsNullOrEmpty(value)){
                //     value = source;
                // }
                // source = value;

                data.SetKeyValueForLang(key, value, langCode, source, desc);
            }
            reader.Close();
        }

        private string Sanitize(string s)
        {
            if (s == null)
            {
                s = string.Empty;
            }

            s = s.Replace("\"", "\"\"");
            s = s.Replace("\r\n", NEWLINE_REPLACEMENT).Replace("\n", NEWLINE_REPLACEMENT).Replace("\r", NEWLINE_REPLACEMENT);
            s = "\"" + s + "\"";
            return s;
        }

#if UNITY_EDITOR
        public void ExportAll(string rootPath, HNDLocData data)
        {
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            foreach (string langCode in data.LangCodes)
            {
                string path = Path.Combine(rootPath, langCode + ".txt");
                ExportLang(path, data, langCode);
            }
        }

        public void ExportLang(string filePath, HNDLocData data, string langCode)
        {
            StreamWriter writer = new StreamWriter(filePath);

            List<string> allKeys = data.KeyToValuesLangs.Keys.OrderBy(k => k).ToList();
            Dictionary<string, HNDLocTerm> terms = data.KeyToTermValue;
            foreach (string key in allKeys)
            {
                string value = data.GetRawStringValue(langCode, key);
                HNDLocTerm term = terms[key];
                writer.WriteLine(Sanitize(((int)(term.TermType)).ToString()) + ";" + Sanitize(key) + ";" + Sanitize(term.SourceText) + ";" + Sanitize(value) + ";" + Sanitize(term.Description) + ";");
            }

            writer.Close();
        }
#endif

        private int FindTerm(string line, int start)
        {
            bool nextOrEnd = false;
            int i = start;
            for (; i < line.Length; i++)
            {
                char c = line[i];
                if (nextOrEnd)
                {
                    if (c == '"')
                    {
                        nextOrEnd = false;
                        continue;
                    }

                    if (c == ';')
                    {
                        break;
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        nextOrEnd = true;
                    }
                }
            }

            return i;
        }
    }
}