using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Hedronoid.Particle
{
    public class ParticleSearchWindow : EditorWindow
    {

        public class PrefabInfo
        {
            public UnityEngine.Object Prefab;
            public List<GameObject> GameObjects = new List<GameObject>();
            public bool Foldout;
        }

        public class SceneInfo
        {
            public string SceneName;
            public string ScenePath;
            public List<SceneObject> SceneObjects = new List<SceneObject>();
            public bool Foldout;
        }

        public class SceneObject
        {
            public string Name;
            public int LocalId;
        }

        public class ScriptInfo
        {
            public UnityEngine.Object Script;
            public bool Foldout;
        }


        private string m_ParticleId;
        private bool m_SearchPrefabs = true;
        private bool m_SearchScenes = true;
        private bool m_SearchScripts = true;

        private Vector2 m_MainScroll;

        public List<PrefabInfo> m_PrefabInfoList = new List<PrefabInfo>();
        public List<SceneInfo> m_SceneInfoList = new List<SceneInfo>();
        public List<ScriptInfo> m_ScriptInfoList = new List<ScriptInfo>();


        private int m_ItemIdx;

        public GUIStyle m_ListStyleEven;
        public GUIStyle m_ListStyleOdd;
        public GUIStyle m_ShowButtonStyle;

        public GUIStyle m_CurrentListStyle;

        private bool m_IsInitialized;


        [MenuItem("Hedronoid/Particle Search...")]
        public static EditorWindow GetWindow()
        {
            var window = GetWindow<ParticleSearchWindow>();
            window.titleContent = new GUIContent("Multi Columns");
            window.Focus();
            window.Repaint();
            return window;
        }


        void Initialize()
        {
            titleContent = new GUIContent("Particle Search", EditorGUIUtility.FindTexture("GameManager Icon"));
            m_ListStyleEven = new GUIStyle((GUIStyle)"ObjectPickerResultsEven");
            m_ListStyleOdd = new GUIStyle((GUIStyle)"ObjectPickerResultsOdd");
            m_ShowButtonStyle = new GUIStyle((GUIStyle)"MiniToolbarButton");
            m_ShowButtonStyle.fixedHeight = 18f;
            m_ShowButtonStyle.margin = new RectOffset(m_ShowButtonStyle.margin.left, m_ShowButtonStyle.margin.right, 2, 2);

            m_PrefabInfoList.Clear();
            m_SceneInfoList.Clear();
            m_ScriptInfoList.Clear();

            m_IsInitialized = true;
        }

        void OnGUI()
        {
            if (!m_IsInitialized) Initialize();

            GUILayoutOption[] expand = { GUILayout.ExpandWidth(true) };
            m_ItemIdx = 0;

            EditorGUILayout.BeginHorizontal("Toolbar"); //, GUILayout.ExpandWidth(true));
            {
                if (GUILayout.Button("Search", (GUIStyle)"toolbarbutton", GUILayout.Height(30f)))
                {
                    Debug.Log("Searching for ParticleId: " + m_ParticleId);
                    try
                    {
                        Search();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Exception: " + e);
                        EditorUtility.ClearProgressBar();
                    }
                }

                m_ParticleId = EditorGUILayout.TextField(m_ParticleId, (GUIStyle)"ToolbarSeachTextField", GUILayout.Width(200f));
                if (GUILayout.Button("", (GUIStyle)"ToolbarSeachCancelButton"))
                {
                    // Remove focus if cleared
                    m_ParticleId = "";
                    GUI.FocusControl(null);
                }
                m_SearchPrefabs = GUILayout.Toggle(m_SearchPrefabs, "Search Prefabs", (GUIStyle)"toolbarbutton");
                m_SearchScenes = GUILayout.Toggle(m_SearchScenes, "Search Scenes", (GUIStyle)"toolbarbutton");
                m_SearchScripts = GUILayout.Toggle(m_SearchScripts, "Search Scripts", (GUIStyle)"toolbarbutton");

                GUILayout.Space(800);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(expand);
            {
                m_MainScroll = EditorGUILayout.BeginScrollView(m_MainScroll);
                {
                    if (m_PrefabInfoList.Count > 0)
                    {
                        EditorGUILayout.BeginVertical(expand);
                        {
                            LabelItem("Prefabs: " + m_PrefabInfoList.Count);

                            foreach (var info in m_PrefabInfoList)
                            {
                                PrefabItem(info);
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }

                    if (m_SceneInfoList.Count > 0)
                    {
                        EditorGUILayout.BeginVertical(expand);
                        {
                            LabelItem("Scene Objects: " + m_SceneInfoList.Count);

                            foreach (var info in m_SceneInfoList)
                            {
                                SceneItem(info);
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }

                    if (m_ScriptInfoList.Count > 0)
                    {
                        EditorGUILayout.BeginVertical(expand);
                        {
                            LabelItem("Scripts: " + m_ScriptInfoList.Count);

                            foreach (var info in m_ScriptInfoList)
                            {
                                ScriptItem(info);
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void LabelItem(string label)
        {
            EditorGUILayout.LabelField(label, (GUIStyle)"PreToolbar");
        }


        private void PrefabItem(PrefabInfo info)
        {
            m_CurrentListStyle = m_ItemIdx++ % 2 == 0 ? m_ListStyleEven : m_ListStyleOdd;
            EditorGUILayout.BeginHorizontal(m_CurrentListStyle);
            {
                var nameContent = new GUIContent(info.Prefab.name, EditorGUIUtility.FindTexture("PrefabNormal Icon"));
                info.Foldout = EditorGUILayout.Foldout(info.Foldout, nameContent, true);

                if (GUILayout.Button("Select", m_ShowButtonStyle, GUILayout.Width(100f)))
                {
                    Selection.activeObject = info.Prefab;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (info.Foldout)
            {
                EditorGUILayout.BeginVertical();
                {
                    EditorGUI.indentLevel += 2;
                    foreach (var go in info.GameObjects)
                    {
                        if (go) PrefabChildItem(go);
                    }
                    EditorGUI.indentLevel -= 2;
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void PrefabChildItem(GameObject go)
        {
            m_CurrentListStyle = m_ItemIdx++ % 2 == 0 ? m_ListStyleEven : m_ListStyleOdd;
            EditorGUILayout.BeginHorizontal(m_CurrentListStyle);
            {
                var nameContent = new GUIContent(go.name, EditorGUIUtility.FindTexture("Prefab Icon"));
                EditorGUILayout.LabelField(nameContent);

                if (GUILayout.Button("Select", m_ShowButtonStyle, GUILayout.Width(100f)))
                {
                    Selection.activeGameObject = go;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void SceneItem(SceneInfo info)
        {
            m_CurrentListStyle = m_ItemIdx++ % 2 == 0 ? m_ListStyleEven : m_ListStyleOdd;
            EditorGUILayout.BeginHorizontal(m_CurrentListStyle);
            {
                var nameContent = new GUIContent(info.SceneName, EditorGUIUtility.FindTexture("SceneAsset Icon"));
                info.Foldout = EditorGUILayout.Foldout(info.Foldout, nameContent, true);

                if (GUILayout.Button("Select", m_ShowButtonStyle, GUILayout.Width(100f)))
                {
                    var sceneObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(info.ScenePath);
                    Debug.Log("sceneObj: " + sceneObj + ", info.ScenePath: " + info.ScenePath);
                    Selection.activeObject = sceneObj;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (info.Foldout)
            {
                EditorGUILayout.BeginVertical();
                {
                    EditorGUI.indentLevel += 2;
                    foreach (var go in info.SceneObjects)
                    {
                        SceneObjectItem(go, info.ScenePath);
                    }
                    EditorGUI.indentLevel -= 2;
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void SceneObjectItem(SceneObject sceneObj, string scenePath)
        {
            m_CurrentListStyle = m_ItemIdx++ % 2 == 0 ? m_ListStyleEven : m_ListStyleOdd;
            EditorGUILayout.BeginHorizontal(m_CurrentListStyle);
            {
                var nameContent = new GUIContent(sceneObj.Name, EditorGUIUtility.FindTexture("PrefabNormal Icon"));
                EditorGUILayout.LabelField(nameContent);

                if (GUILayout.Button("Open & Select", m_ShowButtonStyle, GUILayout.Width(100f)))
                {
                    var scene = EditorSceneManager.OpenScene(scenePath);
                    var allGameObjects = GetAllSceneGOs(scene);
                    for (int i = 0; i < allGameObjects.Length; i++)
                    {
                        var go = allGameObjects[i];
                        if (GetSceneObjectLocalId(go) == sceneObj.LocalId)
                        {
                            Selection.activeObject = allGameObjects[i];
                        }
                    }


                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ScriptItem(ScriptInfo info)
        {
            m_CurrentListStyle = m_ItemIdx++ % 2 == 0 ? m_ListStyleEven : m_ListStyleOdd;
            EditorGUILayout.BeginHorizontal(m_CurrentListStyle);
            {
                var nameContent = new GUIContent(info.Script.name, EditorGUIUtility.FindTexture("cs Script Icon"));
                EditorGUILayout.LabelField(nameContent);

                if (GUILayout.Button("Select", m_ShowButtonStyle, GUILayout.Width(100f)))
                {
                    Selection.activeObject = info.Script;
                }
            }
            EditorGUILayout.EndHorizontal();
        }


        private void Search()
        {
            m_PrefabInfoList.Clear();
            m_SceneInfoList.Clear();
            m_ScriptInfoList.Clear();

            //Searching assets (prefabs)
            if (m_SearchPrefabs)
            {
                var objectGuids = AssetDatabase.FindAssets("t:Object");
                var objectPaths = objectGuids.Select(g => AssetDatabase.GUIDToAssetPath(g)).Where(p => p.Contains(".prefab")).ToArray();
                long total = objectPaths.LongLength;

                for (int i = 0; i < total; i++)
                {
                    PrefabInfo info = null;

                    var objectPath = objectPaths[i];
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(objectPath);
                    var gameObjects = AssetDatabase.LoadAllAssetsAtPath(objectPath).OfType<GameObject>().ToArray();

                    for (int j = 0; j < gameObjects.Length; j++)
                    {
                        var go = gameObjects[j];

                        if (IsGameObjectReferringParticle(go))
                        {
                            if (info == null) info = new PrefabInfo { Prefab = prefab };
                            if (go != prefab) info.GameObjects.Add(go);
                        }
                    }

                    if (info != null) m_PrefabInfoList.Add(info);
                    if (EditorUtility.DisplayCancelableProgressBar("Searching prefabs...", objectPath, i / (float)total))
                    {
                        break;
                    }
                }
                EditorUtility.ClearProgressBar();
            }

            //Searching scenes
            if (m_SearchScenes)
            {
                var currentScenePath = EditorSceneManager.GetActiveScene().path;
                var sceneGuids = AssetDatabase.FindAssets("t:Scene");
                var scenePaths = sceneGuids.Select(g => AssetDatabase.GUIDToAssetPath(g)).Where(p => p.Contains(".unity")).ToArray();
                int sceneCount = scenePaths.Length;

                for (int i = 0; i < sceneCount; i++)
                {
                    SceneInfo info = null;

                    var scenePath = scenePaths[i];
                    var scene = EditorSceneManager.OpenScene(scenePath);
                    var allGameObjects = GetAllSceneGOs(scene);

                    for (int j = 0; j < allGameObjects.Length; j++)
                    {
                        var go = allGameObjects[j];
                        if (IsGameObjectReferringParticle(go))
                        {
                            if (info == null) info = new SceneInfo { SceneName = scene.name, ScenePath = scene.path };
                            int localId = GetSceneObjectLocalId(go);
                            info.SceneObjects.Add(new SceneObject { Name = go.name, LocalId = localId });
                        }
                    }

                    if (info != null) m_SceneInfoList.Add(info);

                    if (EditorUtility.DisplayCancelableProgressBar("Searching scenes...", scenePath, i / (float)sceneCount))
                    {
                        break;
                    }
                }
                EditorUtility.ClearProgressBar();
                EditorSceneManager.OpenScene(currentScenePath);
            }

            //Searching assets (scripts)
            if (m_SearchScripts)
            {
                var scriptGuids = AssetDatabase.FindAssets("t:Script");
                var scriptPaths = scriptGuids.Select(g => AssetDatabase.GUIDToAssetPath(g)).Where(p => p.Contains(".cs")).ToArray();
                long total = scriptPaths.LongLength;

                for (int i = 0; i < total; i++)
                {
                    ScriptInfo info = null;

                    var path = scriptPaths[i];
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                    if (IsScriptReferringSound(script))
                    {
                        info = new ScriptInfo { Script = script };
                    }

                    if (info != null) m_ScriptInfoList.Add(info);
                    if (EditorUtility.DisplayCancelableProgressBar("Searching scripts...", path, i / (float)total))
                    {
                        break;
                    }
                }
                EditorUtility.ClearProgressBar();
            }
        }

        private bool IsGameObjectReferringParticle(GameObject go)
        {
            // foreach (var behaviour in go.GetComponents<MonoBehaviour>())
            // {
            //     if (behaviour == null) continue;
            //     var behaviourObj = new SerializedObject(behaviour);
            //     var prop = behaviourObj.GetIterator();

            //     while (prop.NextVisible(true))
            //     {
            //         if (prop.propertyType == SerializedPropertyType.Enum)
            //         {
            //             var targetEnum = GetBaseProperty<Enum>(prop);

            //             if (targetEnum is ParticleList.ParticleSystems)
            //             {
            //                 var soundId = (ParticleList.ParticleSystems)targetEnum;
            //                 if (soundId.ToString().IndexOf(m_ParticleId, StringComparison.OrdinalIgnoreCase) != -1)
            //                 {
            //                     return true;
            //                 }
            //             }
            //         }
            //     }
            // }
            return false;
        }

        private bool IsScriptReferringSound(MonoScript script)
        {
            var scriptText = script.text;
            var matches = Regex.Matches(scriptText, @"ParticleList\.ParticleSystems\.([0-9a-zA-Z_]+)");
            foreach (Match match in matches)
            {
                var capturedSoundId = match.Groups[1].Value;
                // Debug.Log("script: "+ script.name + ", capturedSoundId: " + capturedSoundId);
                if (capturedSoundId.IndexOf(m_ParticleId, StringComparison.OrdinalIgnoreCase) != -1) return true;
            }
            return false;
        }

        private static T GetBaseProperty<T>(SerializedProperty prop)
        {
            // Separate the steps it takes to get to this property
            var separatedPaths = prop.propertyPath.Split('.');

            // Go down to the root of this serialized property
            var reflectionTarget = prop.serializedObject.targetObject as object;
            // Walk down the path to get the target object
            foreach (var path in separatedPaths)
            {
                // Debug.Log("reflectionTarget: "+ reflectionTarget + ", path: "+ path);
                FieldInfo fieldInfo = null;
                Type type = reflectionTarget.GetType();

                // Go through base types too if necessary, as GetField doesn't return private fields for base types.
                while (fieldInfo == null && type != null)
                {
                    fieldInfo = type.GetField(path, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetField);
                    type = type.BaseType;
                }

                if (fieldInfo == null)
                {
                    //Debug.LogError("Field not found for property " + prop.name + " of " + prop.serializedObject.targetObject.name);
                    return default(T);
                }

                reflectionTarget = fieldInfo.GetValue(reflectionTarget);
            }
            return (T)reflectionTarget;
        }

        private GameObject[] GetAllSceneGOs(Scene scene)
        {
            if (!scene.IsValid()) return null;

            return scene.GetRootGameObjects().SelectMany(
                g => g.GetComponentsInChildren<Component>(true)
                .Where(c => c && c is Transform)
                .Select(t => t.gameObject)
            ).ToArray();
        }

        private int GetSceneObjectLocalId(GameObject go)
        {
            var inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
            var serializedObject = new SerializedObject(go);
            inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);

            var localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");   //note the misspelling!

            return localIdProp.intValue;
        }
    }
}