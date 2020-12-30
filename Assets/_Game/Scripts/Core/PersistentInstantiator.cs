using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Hedronoid;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Hedronoid.Core
{
    //	Script to ensure this object, and all its children persist on load
    //	To support the common prefab being in every scene, then we also
    //	make sure that there are no objects sharing the same name as this
    //	gameObject - presuming that the incoming scene also was trying to 
    //	instaniate such common functionality
    public class PersistentInstantiator : MonoBehaviour
    {
        static readonly string PERSISTENT_TAG = "Persistent";
        
        [SerializeField]
        GameObject[] m_Spawns;

        void Awake()
        {
            float bootStart = Time.realtimeSinceStartup;
            Debug.Log("Perf-Persistent-start " + bootStart);
            GameObject PersistentRoot = GameObject.FindGameObjectWithTag(PERSISTENT_TAG);
            if(PersistentRoot == null)
            {
                PersistentRoot = new GameObject("PersistentRoot");
                PersistentRoot.tag = PERSISTENT_TAG;
                GameObject.DontDestroyOnLoad(PersistentRoot);
            }

            List<Transform> testObjects = PersistentRoot.transform.GetChildren();

            // Create Persistent instances for new prefabs
            foreach(GameObject spawn in m_Spawns)
            {
                // If not already instantiated
                if(spawn != null && !testObjects.Any(o => o.gameObject.name == spawn.name))
                {
                    GameObject go = (GameObject)Instantiate(spawn, Vector3.zero, Quaternion.identity);
                    go.name = spawn.name;
                    go.tag = PERSISTENT_TAG;
                    go.transform.SetParent(PersistentRoot.transform, true);
                    D.CoreLog("# Persisting " + go.name + " in " + gameObject.name);
                }
            }
            Debug.LogFormat("Perf-Persistent-end: {0} seconds", Time.realtimeSinceStartup - bootStart);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PersistentInstantiator))]
    public class PersistentInstantiatorEditor : Editor 
    { 
        private ReorderableList list;

        private bool m_ShowAsReorderableList;

        private void OnEnable() 
        {
            list = new ReorderableList(serializedObject, 
                    serializedObject.FindProperty("m_Spawns"), 
                    true, true, true, true);

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => 
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI() 
        {
            serializedObject.Update();
            if (m_ShowAsReorderableList)
            {
                list.DoLayoutList();
            }else
            {
                DrawDefaultInspector();
            }
            
            if (GUILayout.Button("Toggle view"))
            {
                m_ShowAsReorderableList = !m_ShowAsReorderableList;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}