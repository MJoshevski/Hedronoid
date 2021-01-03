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

namespace Hedronoid.EditorSupport
{
	//TODO: Improve functionality:
	// find objects in scenes
	// make gui
	//drop-down for choosing Layer
	//search for tags
	public class LayerSearchWindow : EditorWindow 
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


		// private string m_LayerName;
		private int m_LayerIdx;
		private string m_TagName;
		private bool m_SearchPrefabs = true;
		private bool m_SearchScenes = true;

		private Vector2 m_MainScroll;

		public List<PrefabInfo> m_PrefabInfoList = new List<PrefabInfo>();
		public List<SceneInfo> m_SceneInfoList = new List<SceneInfo>();

		private static LayerSearchWindow Instance;

		private int m_ItemIdx;
		//private Color m_GrayColor = Color.gray;
		//private Color m_WhiteColor = Color.white;

		public GUIStyle m_ListStyleEven;
		public GUIStyle m_ListStyleOdd;
		public GUIStyle m_ShowButtonStyle;

		public GUIStyle m_CurrentListStyle;

		private bool m_UseLayerSearch = true;
		private bool m_UseTagSearch = false;

		private bool m_IsInitialized;


        [MenuItem("Hedronoid/Layers and Tags Search")]
        public static EditorWindow GetWindow()
        {
            // (LayerSearchWindow) EditorWindow.GetWindow(typeof(LayerSearchWindow));
			var window = GetWindow<LayerSearchWindow>();
			window.titleContent = new GUIContent("Multi Columns");
			window.Focus();
			window.Repaint();
			return window;
        }


		void Initialize()
		{
			titleContent = new GUIContent("LayerSearch", EditorGUIUtility.FindTexture("GameManager Icon"));
			m_ListStyleEven = new GUIStyle(GUI.skin.FindStyle("ObjectPickerResultsEven"));
			m_ListStyleOdd = new GUIStyle(GUI.skin.FindStyle("ObjectPickerResultsOdd"));
			m_ShowButtonStyle = new GUIStyle(GUI.skin.FindStyle("MiniToolbarButton"));
			m_ShowButtonStyle.fixedHeight = 18f;
			m_ShowButtonStyle.margin = new RectOffset(m_ShowButtonStyle.margin.left, m_ShowButtonStyle.margin.right, 2, 2);

			m_PrefabInfoList.Clear();
			m_SceneInfoList.Clear();

			m_IsInitialized = true;
		}
	
        void OnGUI()
        {
			if (!m_IsInitialized) Initialize();

			GUILayoutOption[] expand = { GUILayout.ExpandWidth(true) };
			m_ItemIdx = 0; 

			GUILayout.BeginHorizontal();
	
			if( GUILayout.Toggle( m_UseLayerSearch, "Layers", EditorStyles.toolbarButton ) != m_UseLayerSearch )
			{
				m_UseLayerSearch = !m_UseLayerSearch;
				m_UseTagSearch = !m_UseLayerSearch;
			}
	
			if( GUILayout.Toggle( m_UseTagSearch, "Tags", EditorStyles.toolbarButton ) != m_UseTagSearch )
			{
				m_UseTagSearch = !m_UseTagSearch;
				m_UseLayerSearch = !m_UseTagSearch;
			}
	
			GUILayout.EndHorizontal();


			EditorGUILayout.BeginHorizontal("Toolbar"); //, GUILayout.ExpandWidth(true));
			{
				if (GUILayout.Button("Search", GUI.skin.FindStyle("toolbarbutton"), GUILayout.Height(30f)))
				{
					Debug.Log("Searching for layer: " + LayerMask.LayerToName(m_LayerIdx));
					Search();
				}

				if (m_UseLayerSearch) m_LayerIdx = EditorGUILayout.LayerField(m_LayerIdx, GUI.skin.FindStyle("ToolbarDropDown"), GUILayout.Width(120f));	
				else if (m_UseTagSearch) m_TagName = EditorGUILayout.TagField(m_TagName, GUI.skin.FindStyle("ToolbarDropDown"), GUILayout.Width(120f));	
				
				m_SearchPrefabs = GUILayout.Toggle(m_SearchPrefabs, "Search Prefabs", GUI.skin.FindStyle("toolbarbutton"));
				m_SearchScenes = GUILayout.Toggle(m_SearchScenes, "Search Scenes", GUI.skin.FindStyle("toolbarbutton"));

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
							LabelItem("Prefabs: "+ m_PrefabInfoList.Count);

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
							LabelItem("Scene Objects: "+ m_SceneInfoList.Count);

							foreach (var info in m_SceneInfoList)
							{
								SceneItem(info);
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
			EditorGUILayout.LabelField(label, GUI.skin.FindStyle("PreToolbar"));
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


		private void Search()
		{			
			m_PrefabInfoList.Clear();
			m_SceneInfoList.Clear();

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

						if ((m_UseLayerSearch && go.layer == m_LayerIdx) ||
							m_UseTagSearch && go.tag == m_TagName)
						{
							if (info == null) info = new PrefabInfo(){Prefab = prefab};
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
						if ((m_UseLayerSearch && go.layer == m_LayerIdx) ||
							m_UseTagSearch && go.tag == m_TagName)
						{
							if (info == null) info = new SceneInfo{SceneName = scene.name, ScenePath = scene.path};
							int localId = GetSceneObjectLocalId(go);
							info.SceneObjects.Add(new SceneObject{Name = go.name, LocalId = localId});
						}
					}

					if (info != null) m_SceneInfoList.Add(info);
						
					if (EditorUtility.DisplayCancelableProgressBar("Searching scenes...", scenePath, i / (float) sceneCount))
					{
						break;
					}
				}
				EditorUtility.ClearProgressBar();	
				EditorSceneManager.OpenScene(currentScenePath);
			}
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