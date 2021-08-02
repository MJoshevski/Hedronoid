using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor.AnimatedValues;
using Hedronoid;
using System.IO;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Hedronoid.Particle
{
    public class ParticleManagerEditor : EditorWindow
    {

        private ParticleManagerData m_ParticleManagerData;
        private Vector2 m_ScrollViewPos;
        private string m_NewName;
        private GameObject m_NewPrefab;
        private string m_SearchString = "";
        private bool m_ItemWasRenamed = false;
        private GUIStyle m_StyleLight;

        private HNDParticleSystemData m_CurrentPRTData;

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Hedronoid/Particle Manager/Manager")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(ParticleManagerEditor));
        }

        private void OnEnable()
        {
            m_ItemWasRenamed = false;
            //m_ParticleManagerData = Resources.Load<ParticleManagerData>("Data/ParticleManagerData");

            m_ParticleManagerData = AssetDatabase.LoadAssetAtPath<ParticleManagerData>("Assets/_Game/ParticleManager/ParticleManagerData.asset");

            if (m_ParticleManagerData == null)
            {
                m_ParticleManagerData = ScriptableObject.CreateInstance<ParticleManagerData>();
                AssetDatabase.CreateAsset(m_ParticleManagerData, "Assets/_Game/ParticleManager/ParticleManagerData.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            m_StyleLight = new GUIStyle();
            m_StyleLight.normal.background = MakeTex(2, 2, new Color(1f, 1f, 1f, 0.1f));

        }

        Texture2D MakeTex(int width, int height, Color color)
        {
            var pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = color;
            }

            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }


        private void OnDisable()
        {
            if (m_ItemWasRenamed)
                GenerateParticleList();
            EditorUtility.SetDirty(m_ParticleManagerData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
            {
                if (GUILayout.Button("Export enum", EditorStyles.toolbarButton, GUILayout.Width(120)))
                {
                    GenerateParticleList();
                }
                if (GUILayout.Button("Open gallery scene", EditorStyles.toolbarButton, GUILayout.Width(120)))
                {
                    ShowGallery();
                }

                m_SearchString = GUILayout.TextField(m_SearchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {
                    // Remove focus if cleared
                    m_SearchString = "";
                    GUI.FocusControl(null);
                }

            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Particle Systems:");
            EditorGUILayout.Space();

            m_ScrollViewPos = EditorGUILayout.BeginScrollView(m_ScrollViewPos);
            int displayCount = 0;
            if (m_ParticleManagerData.ParticleSystems != null)
            {
                for (int i = 0; i < m_ParticleManagerData.ParticleSystems.Count; i++)
                {
                    var prtSystem = m_ParticleManagerData.ParticleSystems[i];
                    if (!string.IsNullOrEmpty(m_SearchString))
                    {
                        if (!prtSystem.Name.Contains(m_SearchString))
                            continue;
                    }
                    EditorGUILayout.BeginHorizontal(displayCount % 2 == 0 ? m_StyleLight : GUIStyle.none);
                    displayCount++;
                    string prevName = prtSystem.Name;
                    prtSystem.Name = EditorGUILayout.TextField(prtSystem.Name);
                    if (prtSystem.Name != prevName)
                    {
                        // If any entry in the list gets renamed, we need to recreate the PaticleList. We don't want to do that every time a name is changed though, so we remember the change and re-create the list when window is closed.
                        m_ItemWasRenamed = true;
                    }

                    string prtType = "UNKNOWN";

                    if (prtSystem.ParticleSystemPrefab == null)
                    {
                        prtType = "EMPTY !!";
                    }
                    else if (prtSystem.ParticleSystemPrefab.GetComponentInChildren<ParticleSystem>() != null)
                    {
                        prtType = "ParticleSystem";
                        var system = prtSystem.ParticleSystemPrefab.GetComponentInChildren<ParticleSystem>();
                        prtType += " (looping=" + system.main.loop;
                        prtType += ", scaling=" + system.main.scalingMode + ")";
                    }
                    else if (prtSystem.ParticleSystemPrefab.GetComponentInChildren<LineRenderer>() != null)
                    {
                        prtType = "LineRenderer";
                    }
                    else if (prtSystem.ParticleSystemPrefab.GetComponentInChildren<MeshRenderer>() != null)
                    {
                        prtType = "MeshRenderer";
                    }
                    EditorGUILayout.LabelField(prtType);

                    prtSystem.ParticleSystemPrefab = EditorGUILayout.ObjectField(prtSystem.ParticleSystemPrefab, typeof(HNDParticleSystem), false) as HNDParticleSystem;
                    if (GUILayout.Button("Show in scene", EditorStyles.miniButton, GUILayout.Width(80)))
                    {
                        ShowGallery(prtSystem);
                    }
                    if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(25)))
                    {
                        m_ParticleManagerData.ParticleSystems.RemoveAt(i);
                        GenerateParticleList();
                        return;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Add new:");
            EditorGUILayout.BeginHorizontal();
            m_NewName = EditorGUILayout.TextField(m_NewName);
            GameObject prevPrefab = m_NewPrefab;
            m_NewPrefab = EditorGUILayout.ObjectField(m_NewPrefab, typeof(GameObject), false) as GameObject;
            if (m_NewPrefab != prevPrefab && m_NewPrefab != null)
            {
                m_NewName = m_NewPrefab.name;
            }
            GUI.enabled = !string.IsNullOrEmpty(m_NewName) && m_NewPrefab != null;
            if (GUILayout.Button("Add"))
            {
                if (IsNameAvailable(m_NewName))
                {
                    int guid = GetNewUniqueID();
                    if (guid > 0)
                    {
                        HNDParticleSystem eps = m_NewPrefab.GetComponent<HNDParticleSystem>();
                        if (eps == null)
                        {
                            if (EditorUtility.DisplayDialog("Missing component", "The selected prefab does not contain a Frantics Particle System component, which is required for the prefab to be used in the particle manager. A Frantics Particle System component will be added automatically, if you continue.", "OK", "Cancel"))
                            {
                                eps = m_NewPrefab.AddComponent<HNDParticleSystem>();
                            }
                            else
                            {
                                return;
                            }
                        }
                        HNDParticleSystemData newData = new HNDParticleSystemData();
                        newData.Name = m_NewName;
                        newData.GUID = guid;
                        newData.ParticleSystemPrefab = eps;
                        m_ParticleManagerData.ParticleSystems.Add(newData);
                        GenerateParticleList();
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Name unavailable", "There is already a particle system with the name '" + m_NewName + "' in the list. Please choose another name.", "OK");
                }
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        bool IsNameAvailable(string name)
        {
            // If we don't find any objects with this name in the list, the name is available
            return m_ParticleManagerData.ParticleSystems.Find(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) == null;
        }

        int GetNewUniqueID()
        {
            int newId = UnityEngine.Random.Range(1, int.MaxValue);
            int firstVal = newId;
            while (m_ParticleManagerData.ParticleSystems.Find(p => p.GUID == newId) != null)
            {
                if (newId >= int.MaxValue)
                    newId = 1;
                else
                    newId++;

                if (firstVal == newId)
                {
                    Debug.LogError("No available IDs left!");
                    return -1;
                }
            }

            return newId;
        }

        void GenerateParticleList()
        {
            string beginMark = "/// PARTICLELIST_START";
            string endMark = "/// PARTICLELIST_END";

            string guid = AssetDatabase.FindAssets("ParticleList").First();
            string filePath = AssetDatabase.GUIDToAssetPath(guid);
            if (File.Exists(filePath))
            {
                string contents = File.ReadAllText(filePath);

                int start = contents.IndexOf(beginMark) + beginMark.Length;
                int end = contents.IndexOf(endMark);

                // Remove old data
                contents = contents.Remove(start, end - start);

                // Write enum
                StringWriter sw = new StringWriter();
                sw.WriteLine(System.Environment.NewLine);
                sw.WriteLine("\tpublic enum ParticleSystems {");
                sw.WriteLine("\t\tNONE" + " = " + 0 + ",");
                for (int i = 0; i < m_ParticleManagerData.ParticleSystems.Count; i++)
                {
                    string newLine = "\t\t" + m_ParticleManagerData.ParticleSystems[i].Name.ToUpper() + " = " + m_ParticleManagerData.ParticleSystems[i].GUID + ",";
                    sw.WriteLine(newLine);
                }
                sw.WriteLine("\t}" + System.Environment.NewLine);

                // Write enum to string
                sw.WriteLine("\tpublic static string Get(ParticleSystems ps){");
                sw.WriteLine("\tstring particleId = null;" + System.Environment.NewLine + "\tswitch(ps){");

                for (int i = 0; i < m_ParticleManagerData.ParticleSystems.Count; i++)
                {
                    string enumString = m_ParticleManagerData.ParticleSystems[i].Name.ToUpper();
                    sw.WriteLine("\t\tcase ParticleSystems." + enumString + ":");
                    sw.WriteLine("\t\t\tparticleId = \"" + m_ParticleManagerData.ParticleSystems[i].Name + "\";");
                    sw.WriteLine("\t\t\tbreak;");
                }

                sw.WriteLine("\t}" + System.Environment.NewLine + "\treturn particleId;");
                sw.WriteLine("\t}");

                contents = contents.Insert(start, sw.ToString());

                File.WriteAllText(filePath, contents);

                EditorUtility.SetDirty(m_ParticleManagerData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        void ShowGallery(HNDParticleSystemData prtData = null)
        {
            // Debug.Log("ShowInScene - timeSinceStartup: " + EditorApplication.timeSinceStartup);

            m_CurrentPRTData = prtData;

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            // EditorSceneManager.sceneOpened += (scene, mode) => GallerySceneOpened(scene, mode, prtData);
            EditorSceneManager.sceneOpened -= GallerySceneOpened;
            EditorSceneManager.sceneOpened += GallerySceneOpened;
            EditorSceneManager.OpenScene("Assets/Global/ParticleManager/Scenes/ParticleManagerGallery.unity");
        }

        private void GallerySceneOpened(Scene scene, OpenSceneMode mode)
        {
            // Debug.Log("SceneOpened - scene: " + scene + ", mode: " + mode + ", timeSinceStartup: " + EditorApplication.timeSinceStartup);

            var galleryCtrl = FindObjectOfType<ParticleGalleryController>();
            if (galleryCtrl)
            {
                galleryCtrl.Init(m_ParticleManagerData, m_CurrentPRTData);
            }


        }
    }

    [CustomPropertyDrawer(typeof(ParticleList.ParticleSystems))]
    public class ParticleListDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Get the current selected index, then get the names, sort them and find new index
            int selectedIndex = property.enumValueIndex;
            List<string> names = new List<string>(Enum.GetNames(typeof(ParticleList.ParticleSystems)));
            names.Sort();
            if (selectedIndex < 0 || selectedIndex >= property.enumNames.Length)
            {
                selectedIndex = 0;
            }
            selectedIndex = names.IndexOf(property.enumNames[selectedIndex]);
            selectedIndex = EditorGUI.Popup(position, selectedIndex, names.ToArray());
            ParticleList.ParticleSystems ps = (ParticleList.ParticleSystems)Enum.Parse(typeof(ParticleList.ParticleSystems), names[selectedIndex], true);
            selectedIndex = Array.IndexOf(Enum.GetValues(typeof(ParticleList.ParticleSystems)), ps);
            property.enumValueIndex = selectedIndex;

            EditorGUI.EndProperty();
        }
    }
}