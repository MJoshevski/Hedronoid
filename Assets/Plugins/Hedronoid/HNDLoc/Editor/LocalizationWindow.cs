using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace Hedronoid.HNDLoc
{
    public class HNDLocWindow : EditorWindow
    {
        private HNDLocData m_Data;

        [MenuItem("Hedronoid/HNDLoc")]
        private static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(HNDLocWindow));
        }

        private Vector2 m_ScrollPos;

        private string m_LangTextFieldText = "";

        private string m_TermTextFieldText = "";

        private List<string> m_SortedKeys;

        private HNDLocImportExport m_ImportExport;

        private string m_RootPath = "Assets/Resources/HNDLoc";

        private bool m_IsChanged = false;

        private Dictionary<string, bool> m_EnabledLangs = new Dictionary<string, bool>();

        private HNDLocSettings m_HNDLocSettings;

        void OnGUI()
        {
            if (m_HNDLocSettings == null)
            {
                m_HNDLocSettings = HNDLoc.GetOrCreateSettings();
            }

            if (m_Data == null)
            {
                Load();
            }

            GUI.changed = false;

            GUILayoutOption[] expand = { GUILayout.ExpandWidth(true) };


            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"), new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(40) });
                {
                    GUILayout.Label("Show hide languages:", GUILayout.Width(140));

                    // Lang on/off
                    for (int k = 0; k < m_Data.LangCodes.Count; k++)
                    {
                        string code = m_Data.LangCodes[k];
                        bool enabled = GUILayout.Toggle(IsLangEnabled(code), code);
                        if (IsLangEnabled(code) != enabled)
                        {
                            m_EnabledLangs[code] = enabled;
                            RefreshCache();
                        }
                    }

                    if (!m_HNDLocSettings.IntegrateWithLAMS)
                    {
                        // Add/remove new language
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("New lang:");
                        m_LangTextFieldText = EditorGUILayout.TextField(m_LangTextFieldText, GUILayout.Width(50));

                        if (GUILayout.Button("+", GUI.skin.FindStyle("toolbarbutton"), GUILayout.Width(30)))
                        {
                            m_Data.AddLang(m_LangTextFieldText);
                            RefreshCache();
                        }
                        if (GUILayout.Button("-", GUI.skin.FindStyle("toolbarbutton"), GUILayout.Width(30)))
                        {
                            m_Data.RemoveLang(m_LangTextFieldText);
                            RefreshCache();
                        }
                    }

                    if (m_HNDLocSettings.IntegrateWithLAMS)
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Import from LAMS", GUI.skin.FindStyle("toolbarbutton"), GUILayout.Width(100)))
                        {
                            ImportFromLAMS();
                        }
                        if (GUILayout.Button("Export to LAMS", GUI.skin.FindStyle("toolbarbutton"), GUILayout.Width(100)))
                        {
                            ExportToLAMS();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"), expand);
                {
                    GUILayout.Label("Term search / add:", GUILayout.Width(140));

                    // Enter term
                    m_TermTextFieldText = EditorGUILayout.TextField(m_TermTextFieldText, expand);

                    // Add term
                    if (!string.IsNullOrEmpty(m_TermTextFieldText) && !m_Data.ContainsKey(m_TermTextFieldText))
                    {
                        if (GUILayout.Button("+", GUI.skin.FindStyle("toolbarbutton"), GUILayout.Width(30)))
                        {
                            m_Data.AddKey(m_TermTextFieldText);
                            RefreshCache();
                        }
                    }

                    // Delete term
                    if (!string.IsNullOrEmpty(m_TermTextFieldText) && m_Data.ContainsKey(m_TermTextFieldText))
                    {
                        if (GUILayout.Button("-", GUI.skin.FindStyle("toolbarbutton"), GUILayout.Width(30)))
                        {
                            m_Data.RemoveKey(m_TermTextFieldText);
                            RefreshCache();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(expand);
                {
                    m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

                    EditorGUILayout.BeginVertical(expand);
                    string searchText = m_TermTextFieldText.ToLower();

                    int defaultWidth = 250;

                    // Header
                    EditorGUILayout.BeginHorizontal(expand);
                    {
                        GUILayout.Space(25);
                        GUILayout.Label("Key", GUILayout.Width(defaultWidth));
                        GUILayout.Label("Source text", GUILayout.Width(defaultWidth));
                        GUILayout.Label("Description", GUILayout.Width(defaultWidth));

                        // Enabled lang textfields
                        for (int k = 0; k < m_Data.LangCodes.Count; k++)
                        {
                            string langCode = m_Data.LangCodes[k];
                            if (!IsLangEnabled(langCode))
                            {
                                continue;
                            }

                            GUILayout.Label(langCode, GUILayout.Width(defaultWidth));
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    // term row
                    for (int i = 0; i < m_SortedKeys.Count; i++)
                    {
                        //var bgStyle = i % 2 == 0 ? GUI.skin.FindStyle("OL EntryBackEven") : GUI.skin.FindStyle("OL EntryBackOdd");
                        string key = m_SortedKeys[i];
                        string desc = m_Data.KeyToTermValue[key].SourceText;

                        if (!string.IsNullOrEmpty(m_TermTextFieldText) && !key.ToLower().Contains(searchText) && (desc != null && !desc.ToLower().Contains(searchText)))
                        {
                            continue;
                        }

                        HNDLocTerm term = m_Data.KeyToTermValue[key];

                        EditorGUILayout.BeginHorizontal(expand);
                        {
                            // Copy key
                            if (GUILayout.Button("C", GUI.skin.FindStyle("toolbarbutton"), GUILayout.Width(20)))
                            {
                                EditorGUIUtility.systemCopyBuffer = key;
                            }

                            // Key name                            
                            GUILayout.Label(key, GUILayout.Width(defaultWidth));

                            // Term source text
                            term.SourceText = EditorGUILayout.TextField(term.SourceText != null ? term.SourceText : "", GUILayout.Width(defaultWidth));

                            // Term descriptions
                            term.Description = EditorGUILayout.TextField(term.Description != null ? term.Description : "", GUILayout.Width(defaultWidth));

                            // Enabled lang textfields
                            for (int k = 0; k < m_Data.LangCodes.Count; k++)
                            {
                                string langCode = m_Data.LangCodes[k];
                                if (!IsLangEnabled(langCode))
                                {
                                    continue;
                                }

                                string value = m_Data.GetRawStringValue(langCode, key);
                                if (value == null)
                                {
                                    value = "";
                                }

                                // Copy key
                                if (GUILayout.Button("C", GUI.skin.FindStyle("toolbarbutton"), GUILayout.Width(20)))
                                {
                                    EditorGUIUtility.systemCopyBuffer = value;
                                }

                                if (m_HNDLocSettings.IntegrateWithLAMS)
                                {
                                    EditorGUILayout.LabelField(value, GUILayout.Width(defaultWidth));
                                }
                                else
                                {
                                    string newValue = EditorGUILayout.TextField(value, GUILayout.Width(defaultWidth));
                                    if (value != newValue)
                                    {
                                        m_Data.SetKeyValueForLang(key, newValue, langCode, term.SourceText, term.Description);
                                    }
                                }
                            }

                            GUILayout.FlexibleSpace();
                            //GUILayout.Label(m_Unused[i].LastModified.ToString());
                            if (GUILayout.Button("-", GUI.skin.FindStyle("toolbarbutton"), GUILayout.Width(30)))
                            {
                                m_Data.RemoveKey(key);
                                RefreshCache();
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndHorizontal();

                // Stats
                EditorGUILayout.BeginHorizontal("box");
                {
                    GUILayout.Label("Terms: " + m_SortedKeys.Count);
                }
                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.EndVertical();

            //CheckCopyPaste();

            if (GUI.changed)
            {
                m_IsChanged = true;
            }
        }

        private void ImportFromLAMS()
        {
            string filename = EditorUtility.OpenFilePanel("Select 'Export for Project' XML file generated by LAMS", "", "xml");
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);

            int amountLanguagesAdded = 0;
            int amountKeysAdded = 0;
            int amountTranslatedTextsChanged = 0;
            int amountSourceTextsChanged = 0;
            int errorCode = 0;

            // Ensure that all languages from LAMS are also in HNDLoc
            Dictionary<string, string> languageIdToCultureCode = new Dictionary<string, string>();
            XmlNodeList allLanguages = xmlDocument.DocumentElement.SelectNodes("/script/properties/search_params/languages/language");
            List<string> allLanguagesInLAMS = new List<string>();

            m_Data.Wipe();

            foreach (XmlNode language in allLanguages)
            {
                string languageId = language.Attributes.GetNamedItem("id").Value;
                string cultureCode = language.Attributes.GetNamedItem("culture_code").Value.ToLowerInvariant();
                if (m_Data.LangCodes.Contains(cultureCode) == false)
                {
                    m_Data.AddLang(cultureCode);
                    amountLanguagesAdded++;
                }
                languageIdToCultureCode.Add(languageId, cultureCode);
                allLanguagesInLAMS.Add(cultureCode);
            }
            // If we have more languages local than in LAMS then give a warning about which language codes that are not in sync with LAMS
            if (allLanguages.Count < m_Data.LangCodes.Count)
            {
                string languagesNotInLAMS = "";
                foreach (string languageCode in m_Data.LangCodes)
                {
                    if (allLanguagesInLAMS.Contains(languageCode) == false) languagesNotInLAMS += languageCode + " ";
                }
                EditorUtility.DisplayDialog("LAMS import warning", "Your local HNDLoc configuration contain more supported languages than the LAMS configuration.\n\nEither add new supported languages in LAMS or delete unsupported local languages.\n\nLanguage codes : " + languagesNotInLAMS, "OK");
            }

            XmlNodeList allSubsectionNodes = xmlDocument.DocumentElement.SelectNodes("/script/body/section/subsection[@type='text']");
            if (allSubsectionNodes.Count == 0) errorCode = 1; // If NO subsections was found in the XML file, it's probably because user selected an incorrect XML file to import. Give a warning in that case
            int amountOnscreenTextsFoundInLAMS = 0;
            foreach (XmlNode subsectionNode in allSubsectionNodes)
            {
                // Each subsection in LAMS corresponds to one type of prefix for our HNDLoc keys
                XmlNodeList onscreenTexts = subsectionNode.SelectNodes("onscreen_text");
                amountOnscreenTextsFoundInLAMS += onscreenTexts.Count;
                foreach (XmlNode onscreenText in onscreenTexts)
                {
                    //              <onscreen_text>
                    // 					<source id="43" unique_name="BIKE_Banana_Pickup_Header" is_remote_fed="false" seq="0" should_be_translated="True" is_redundant="false">
                    // 						<text modified="2017-09-13 11:39:32" xml:space="preserve">Banana Pickup</text>
                    // 						<flags>
                    // 							<flag edit="false" name="Ready to Translate" type="select" icon="ready_to_translate.png" dirty="False" active="true" user="vladimir" date="2017-09-19 11:29:05" xml:space="preserve">Yes</flag>
                    // 							<flag edit="false" name="Source Comment" type="text" icon="comment.png" dirty="False" active="true" user="martin" date="2017-10-06 15:59:45" xml:space="preserve">Banana test</flag>
                    // 						</flags>
                    // 					</source>
                    // 					<links />
                    // 					<translations>
                    // 						<translation translation_id="43" id="43" lang="" langId="1" variantId="1" variantName="Default" variantCode="" translation_ok="True">
                    // 							<flags />
                    // 							<text modified="2017-09-13 11:39:33" textChanged="2017-09-13 11:39:33" updated="2017-09-13 11:39:33" touched="2017-10-06 15:59:45" out_of_date="False" xml:space="preserve">Banana Pickup</text>
                    // 							<subtitles />
                    // 						</translation>
                    // 					</translations>
                    // 					<data />
                    // 				</onscreen_text>

                    string itemKey = onscreenText.SelectSingleNode("source").Attributes.GetNamedItem("unique_name").Value;
                    string itemSourceText = onscreenText.SelectSingleNode("source/text").InnerText;
                    XmlNode descNode = onscreenText.SelectSingleNode("source/flags/flag[@name='Source Comment']");
                    string itemDescription = descNode != null ? DecodeFromXmlDoc(descNode.InnerText.Trim()) : null;
                    //					Debug.Log("unique_name : " + uniqueName + " " + itemDescription);
                    XmlNodeList translations = onscreenText.SelectNodes("translations/translation");
                    foreach (XmlNode translation in translations)
                    {
                        string decodedTranslatedText = DecodeFromXmlDoc(translation.SelectSingleNode("text").InnerText);
                        string languageId = translation.Attributes.GetNamedItem("langId").Value;
                        string languageShortCode = languageIdToCultureCode[languageId];
                        if (!string.IsNullOrEmpty(languageShortCode) && m_Data.LangCodes.Contains(languageShortCode))
                        {
                            if (!m_Data.ContainsKey(itemKey))
                            {
                                m_Data.AddKey(itemKey);
                                amountKeysAdded++;
                            }
                            string oldText = m_Data.GetRawStringValue(languageShortCode, itemKey);
                            if (oldText == null || oldText.Equals(decodedTranslatedText) == false)
                            {
                                m_Data.SetKeyValueForLang(itemKey, decodedTranslatedText, languageShortCode, null, null);
                                if (oldText != null || !string.IsNullOrEmpty(decodedTranslatedText)) amountTranslatedTextsChanged++;
                            }
                            //							Debug.Log("langId : " + HNDLocLanguageKey + " " + decodedTranslatedText);
                        }
                        else
                        {
                            // A translation in the XML file have appeared with a language ID that's not specified further up in the XML file
                            errorCode = 2;
                        }
                    }

                    // Set source text + description
                    HNDLocTerm term = m_Data.KeyToTermValue[itemKey];
                    if (term != null)
                    {
                        if (term.SourceText == null || term.SourceText.Equals(itemSourceText) == false)
                        {
                            if (term.SourceText != null || !string.IsNullOrEmpty(itemSourceText)) amountSourceTextsChanged++;
                            term.SourceText = itemSourceText;
                        }

                        if (!string.IsNullOrEmpty(itemDescription))
                        {
                            term.Description = itemDescription;
                        }
                    }
                    else
                    {
                        // A HNDLocTerm could not be found in our own HNDLoc framework, even though it should be there
                        errorCode = 3;
                    }

                }
            }
            if (errorCode == 0 && amountOnscreenTextsFoundInLAMS < m_SortedKeys.Count)
            {
                EditorUtility.DisplayDialog("LAMS import warning", "Your local HNDLoc configuration contain more keys than the LAMS configuration.\n\nRemember to 'Export to LAMS' to get new keys and source texts exported into LAMS.", "OK");
            }

            RefreshCache();
            if (errorCode > 0)
            {
                EditorUtility.DisplayDialog("LAMS import error", "errorCode = " + errorCode, "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("LAMS import done", "Amount languages added : " + amountLanguagesAdded + "\nAmount keys added : " + amountKeysAdded + "\nAmount translations changed : " + amountTranslatedTextsChanged + "\nAmount source texts changed : " + amountSourceTextsChanged, "OK");
            }
        }

        private void ExportToLAMS()
        {
            string filename = EditorUtility.OpenFilePanel("Select 'Download Source' XML file generated by LAMS", "", "xml");
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);

            int errorCode = 0;
            int amountNewSubsectionsAdded = 0;
            int amountNewKeysAdded = 0;
            int amountKeysRemoved = 0;
            int amountSourceTextsChanged = 0;
            int amountSourceDescriptionsChanged = 0;

            // All subsections must be found under the section 'Games', so find that section first. If not present, go to LAMS and add the 'Games' section manually
            // But if the 'Games' section isn't found then it's VERY LIKELY due to user selecting an incorrect XML file
            XmlNode gamesSection = xmlDocument.DocumentElement.SelectSingleNode("/script/body/section[@name='Games']"); ;
            if (gamesSection == null)
            {
                errorCode = 1;
            }
            else
            {
                XmlNodeList gamesSubsections = gamesSection.SelectNodes("subsection[@type='text']");
                // Since we're processing the keys in sorted order we just keep track of the current subsection node in the XML
                string currentSubsectionName = "";
                XmlNode currentSubsectionNode = null;
                for (int i = 0; i < m_SortedKeys.Count; i++)
                {
                    string key = m_SortedKeys[i];
                    if (IsValidKey(key))
                    {
                        string keySectionName = key.Substring(0, key.IndexOf("_")).Trim().ToUpperInvariant();
                        if (keySectionName.Equals(currentSubsectionName) == false)
                        {
                            // We've reached another subsection in the keys, look up the XML node or create a new one
                            currentSubsectionName = keySectionName;
                            currentSubsectionNode = null;

                            foreach (XmlNode gameSubsection in gamesSubsections)
                            {
                                if (gameSubsection.Attributes.GetNamedItem("name").Value.Equals(currentSubsectionName))
                                {
                                    currentSubsectionNode = gameSubsection;
                                    break;
                                }
                            }

                            // Subsection wasn't found in the XML, create a new node for it
                            if (currentSubsectionNode == null)
                            {
                                string tempXml = "<subsection id=\"0\" section_id=\"0\" name=\"" + keySectionName + "\" seq=\"0\" type=\"text\" is_remote_fed=\"false\"><character_stems /><flags /></subsection>";
                                gamesSection.InnerXml += tempXml;
                                amountNewSubsectionsAdded++;
                                //								Debug.Log(tempXml);
                                // Find the new XML node after we've added the XML for it
                                gamesSubsections = gamesSection.SelectNodes("subsection[@type='text']");
                                foreach (XmlNode gameSubsection in gamesSubsections)
                                {
                                    if (gameSubsection.Attributes.GetNamedItem("name").Value.Equals(currentSubsectionName))
                                    {
                                        currentSubsectionNode = gameSubsection;
                                        break;
                                    }
                                }
                            }
                        }

                        if (currentSubsectionNode != null)
                        {
                            //              <onscreen_text>
                            // 					<source id="43" unique_name="BIKE_Banana_Pickup_Header" is_remote_fed="false" seq="0" should_be_translated="True" is_redundant="false">
                            // 						<text modified="2017-09-13 11:39:32" xml:space="preserve">Banana Pickup</text>
                            // 						<flags>
                            // 							<flag edit="false" name="Ready to Translate" type="select" icon="ready_to_translate.png" dirty="False" active="true" user="vladimir" date="2017-09-19 11:29:05" xml:space="preserve">Yes</flag>
                            // 							<flag edit="false" name="Source Comment" type="text" icon="comment.png" dirty="False" active="true" user="martin" date="2017-10-06 15:59:45" xml:space="preserve">Banana test</flag>
                            // 						</flags>
                            // 					</source>
                            // 					<links />
                            // 					<translations>
                            // 						<translation translation_id="43" id="43" lang="" langId="1" variantId="1" variantName="Default" variantCode="" translation_ok="True">
                            // 							<flags />
                            // 							<text modified="2017-09-13 11:39:33" textChanged="2017-09-13 11:39:33" updated="2017-09-13 11:39:33" touched="2017-10-06 15:59:45" out_of_date="False" xml:space="preserve">Banana Pickup</text>
                            // 							<subtitles />
                            // 						</translation>
                            // 					</translations>
                            // 					<data />
                            // 				</onscreen_text>

                            string encodedSourceText = m_Data.KeyToTermValue[key].SourceText;
                            XmlNode foundSourceNode = currentSubsectionNode.SelectSingleNode("onscreen_text/source[@unique_name='" + key + "']");

                            // Add new key
                            if (foundSourceNode == null)
                            {
                                foundSourceNode = xmlDocument.CreateElement("onscreen_text");
                                foundSourceNode.InnerXml = "<source id = \"0\" unique_name = \"" + key + "\" is_remote_fed = \"false\" seq = \"0\" should_be_translated = \"True\" is_redundant = \"false\"><text xml:space = \"preserve\">" + encodedSourceText + "</text ><flags /></source ><links /><translations /><data />";
                                currentSubsectionNode.AppendChild(foundSourceNode);
                                amountNewKeysAdded++;
                            }
                            else
                            {
                                foundSourceNode = foundSourceNode.ParentNode;
                            }

                            // Check if text changed
                            //Debug.Log(key + " " + foundSourceNode.OuterXml);
                            XmlNode textNode = foundSourceNode.SelectSingleNode("source//text");
                            //Debug.Log(key + " " + textNode.InnerText);
                            if (textNode.InnerText.Equals(encodedSourceText) == false)
                            {
                                //										Debug.Log(key + " " + textNode.InnerText.Trim() + " " + encodedDescription);
                                textNode.InnerText = encodedSourceText;
                                amountSourceTextsChanged++;
                            }

                            // Update description
                            string description = m_Data.KeyToTermValue[key].Description;
                            if (!string.IsNullOrEmpty(description))
                            {
                                XmlNode dSourceNode = foundSourceNode.SelectSingleNode("source");
                                if (dSourceNode == null)
                                {
                                    D.LocError("No source for term: " + key + " node: " + currentSubsectionNode.InnerXml);
                                }
                                XmlNode dFlags = dSourceNode.SelectSingleNode("flags");
                                if (dFlags == null)
                                {
                                    dFlags = xmlDocument.CreateElement("flags");
                                    dSourceNode.AppendChild(dFlags);
                                }

                                XmlNode dCommentNode = dFlags.SelectSingleNode("flag[@name='Source Comment']");
                                if (dCommentNode == null)
                                {
                                    //<flag edit="false" name="Source Comment" type="text" icon="comment.png" dirty="False" active="true" user="martin" date="2017-10-06 15:59:45" xml:space="preserve">Banana test</flag>
                                    dCommentNode = xmlDocument.CreateElement("flag");
                                    ((XmlElement)dCommentNode).SetAttribute("name", "Source Comment");
                                    ((XmlElement)dCommentNode).SetAttribute("edit", "false");
                                    ((XmlElement)dCommentNode).SetAttribute("type", "text");
                                    ((XmlElement)dCommentNode).SetAttribute("icon", "comment.png");
                                    ((XmlElement)dCommentNode).SetAttribute("dirty", "True");
                                    ((XmlElement)dCommentNode).SetAttribute("active", "true");
                                    ((XmlElement)dCommentNode).SetAttribute("user", "martin");
                                    ((XmlElement)dCommentNode).SetAttribute("date", "2017-10-06 15:59:45");
                                    ((XmlElement)dCommentNode).SetAttribute("xml:space", "preserve");
                                    ((XmlElement)dCommentNode).SetAttribute("active", "true");
                                    dFlags.AppendChild(dCommentNode);
                                }

                                string encoded = description.Trim();
                                if (!encoded.Equals(dCommentNode.InnerText))
                                {
                                    amountSourceDescriptionsChanged++;
                                    ((XmlElement)dCommentNode).SetAttribute("dirty", "True");
                                    dCommentNode.InnerText = encoded;
                                }
                            }
                        }
                        else
                        {
                            // For some reason we failed to add subsection to the XML document
                            errorCode = 2;
                        }
                    }
                }

                // Remove stuff from LAMS that have been removed from HNDLoc
                XmlNodeList allOnscreenTexts = gamesSection.SelectNodes("subsection/onscreen_text");
                //				foreach (XmlNode onscreenText in allOnscreenTexts)
                for (int t = allOnscreenTexts.Count - 1; t >= 0; t--)
                {
                    XmlNode onscreenText = allOnscreenTexts[t];
                    XmlNode sourceNode = onscreenText.SelectSingleNode("source");
                    string nodeKey = sourceNode.Attributes.GetNamedItem("unique_name").Value;
                    if (m_Data.ContainsKey(nodeKey) == false)
                    {
                        onscreenText.ParentNode.RemoveChild(onscreenText);
                        amountKeysRemoved++;
                    }
                }

            }

            if (errorCode > 0)
            {
                EditorUtility.DisplayDialog("LAMS export error", "errorCode = " + errorCode, "OK");
            }
            else
            {
                xmlDocument.Save(filename);
                EditorUtility.DisplayDialog("LAMS export done", "Amount new subsections added : " + amountNewSubsectionsAdded +
                "\nAmount new keys added : " + amountNewKeysAdded +
                "\nAmount keys removed : " + amountKeysRemoved +
                "\nAmount source texts changed : " + amountSourceTextsChanged +
                "\nAmount descriptions changed : " + amountSourceDescriptionsChanged,
                "OK");
            }
        }

        private bool IsValidKey(string input)
        {
            int tempIndex = input.IndexOf("_");
            if (tempIndex < 1) return false;
            string tempPrefix = input.Substring(0, tempIndex).Trim();
            return tempPrefix.Length > 0;
        }

        private string DecodeFromXmlDoc(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            input = input.Replace("&amp;", "&");
            input = input.Replace("&lt;", "<");
            input = input.Replace("&gt;", ">");
            return input;
        }

        private bool IsLangEnabled(string code)
        {
            if (!m_EnabledLangs.ContainsKey(code))
            {
                return false;
            }
            return m_EnabledLangs[code];
        }

        private void OnEnable()
        {
            if (m_Data == null)
            {
                Load();
            }
        }

        private void Load()
        {
            D.LocLog("Loading data from " + m_RootPath);
            m_ImportExport = new HNDLocImportExport();
            AssetDatabase.Refresh();
            m_Data = m_ImportExport.ImportAll("HNDLoc");
            RefreshCache();
        }

        private void OnDisable()
        {
            if (m_IsChanged)
            {
                Save();
            }
        }

        private void Save()
        {
            m_ImportExport.ExportAll(m_RootPath, m_Data);
            //AssetDatabase.Refresh();
        }

        private void RefreshCache()
        {
            m_SortedKeys = m_Data.KeyToValuesLangs.Keys.OrderBy(s => s).ToList();
        }
    }
}