using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hedronoid.DependencyChecker
{
    public class DependencyCheckerWindow : EditorWindow
    {
        class UnusedInfo
        {
            public string Path;
            public System.DateTime LastModified;
        }

        private List<UnusedInfo> m_Unused = new List<UnusedInfo>();
        private Vector2 m_ScrollPos;

        private bool m_IgnoreScripts = true;

        private bool m_IgnoreTextFiles = true;

        private bool m_IgnoreAudio = true;

        private bool m_IgnorePersonal = false;

        private bool m_IgnoreShaders = true;

        private bool m_IgnoreDefaultFolders = false;

        [MenuItem("Hedronoid/Dependency Checker")]
        private static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(DependencyCheckerWindow));
        }

        void OnGUI()
        {
            GUI.changed = false;

            GUILayoutOption[] expand = { GUILayout.ExpandWidth(true) };

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"), expand);
                {
                    if (GUILayout.Button("Search for unused assets", GUI.skin.FindStyle("toolbarbutton"), GUILayout.Width(150)))
                    {
                        List<string> extensionsToSkip = DependencyChecker.AssetExtensionsWithoutReference.ToList();
                        if (m_IgnoreScripts)
                        {
                            extensionsToSkip.AddRange(DependencyChecker.ScriptAssetExtensions);
                        }
                        if (m_IgnoreTextFiles)
                        {
                            extensionsToSkip.AddRange(DependencyChecker.TextAssetExtensions);
                        }
                        if (m_IgnoreAudio)
                        {
                            extensionsToSkip.AddRange(DependencyChecker.AudioAssetExtensions);
                        }
                        if (m_IgnorePersonal)
                        {
                            extensionsToSkip.AddRange(new string[] { "Assets/Personal/" });
                        }
                        if (m_IgnoreShaders)
                        {
                            extensionsToSkip.AddRange(DependencyChecker.ShaderAssetExtensions);
                        }
                        if (m_IgnoreDefaultFolders)
                        {
                            extensionsToSkip.AddRange(new string[] { "Assets/Plugins/", "Assets/StreamingAssets/", "/Resources/" });
                        }

                        var referenceMap = DependencyChecker.BuildReferenceMap(extensionsToSkip.ToArray());
                        m_Unused = DependencyChecker.GetUnusedAssets(referenceMap).Select(p =>
                        {
                            UnusedInfo i = new UnusedInfo();
                            i.Path = p;
                            i.LastModified = File.GetLastWriteTime(p);
                            return i;
                        }).ToList();
                        m_ScrollPos = Vector3.zero;
                    }

                    m_IgnoreScripts = GUILayout.Toggle(m_IgnoreScripts, "Ignore scripts");
                    m_IgnoreTextFiles = GUILayout.Toggle(m_IgnoreTextFiles, "Ignore text files");
                    m_IgnoreAudio = GUILayout.Toggle(m_IgnoreAudio, "Ignore audio");
                    m_IgnorePersonal = GUILayout.Toggle(m_IgnorePersonal, "Ignore personal");
                    m_IgnoreShaders = GUILayout.Toggle(m_IgnoreShaders, "Ignore shaders");
                    m_IgnoreDefaultFolders = GUILayout.Toggle(m_IgnoreDefaultFolders, "Ignore default folders");
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("box", expand);
                {
                    m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

                    EditorGUILayout.BeginVertical("box", expand);
                    for (int i = 0; i < m_Unused.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal("box", expand);
                        {
                            if (GUILayout.Button(">", GUI.skin.FindStyle("toolbarbutton"), GUILayout.Width(24)))
                            {
                                SelectAsset(i);
                            }
                            GUILayout.Label(m_Unused[i].Path);
                            GUILayout.FlexibleSpace();
                            GUILayout.Label(m_Unused[i].LastModified.ToString());
                            if (GUILayout.Button("Delete", GUI.skin.FindStyle("toolbarbutton"), GUILayout.Width(80)))
                            {
                                DeleteAsset(i);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("box");
                {
                    GUILayout.Label("Unused assets: " + m_Unused.Count);
                }
                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.EndVertical();


            if (GUI.changed)
            {

            }
        }

        // private List<string> FilterResults(List<string> results)
        // {
        // 	List<string> r = results;
        //     if(m_IgnoreScripts){
        // 		r = r.Where(p => DependencyChecker.ScriptAssetExtensions.Any(t => path.Contains(t)) p.Contains( DependencyChecker.ScriptAssetExtensions)
        // 	}
        // 	return results.Where()
        // }

        private void SelectAsset(int index)
        {
            string path = m_Unused[index].Path;
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        private void DeleteAsset(int index)
        {
            string path = m_Unused[index].Path;
            m_Unused.RemoveAt(index);
            AssetDatabase.DeleteAsset(path);
        }
    }
}