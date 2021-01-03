using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hedronoid.TriggerSystem
{
    [CustomEditor(typeof(Selector))]
    public class SelectorEditor : Editor
    {

        Selector thisSelector;

        public class TriggerNames
        {
            public string[] TypeNames;
            public string[] DisplayNames;
        }

        static Dictionary<string, Type> conditionDict;
        TriggerNames conditionNames;
        int newConditionId;

        static Dictionary<string, Type> actionDict;
        TriggerNames actionNames;
        int newActionId;

        void OnEnable()
        {
            thisSelector = (Selector)target;

            // Find all possible condition types
            conditionDict = new Dictionary<string, Type>();
            AddTypesToDictionary<HNDCondition>(ref conditionDict);
            conditionNames = FindTriggerTypeNames(conditionDict);

            // Find all possible action types
            actionDict = new Dictionary<string, Type>();
            AddTypesToDictionary<HNDAction>(ref actionDict);
            actionNames = FindTriggerTypeNames(actionDict);
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(thisSelector), typeof(Selector), false);
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            newConditionId = EditorGUILayout.Popup(newConditionId, conditionNames.DisplayNames);
            if (GUILayout.Button("Add condition", GUILayout.Width(100)))
            {
                string[] split = conditionNames.TypeNames[newConditionId].Split('/');
                //UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(thisSelector.gameObject, "Assets/Editor/Trigger/SelectorEditor.cs (90,4)", split[split.Length-1]);
                thisSelector.gameObject.AddComponent(GetType(split[split.Length - 1]));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            newActionId = EditorGUILayout.Popup(newActionId, actionNames.DisplayNames);
            if (GUILayout.Button("Add action", GUILayout.Width(100)))
            {
                string[] split = actionNames.TypeNames[newActionId].Split('/');
                //UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(thisSelector.gameObject, "Assets/Editor/Trigger/SelectorEditor.cs (90,4)", split[split.Length-1]);
                thisSelector.gameObject.AddComponent(GetType(split[split.Length - 1]));
            }
            EditorGUILayout.EndHorizontal();
        }

        public static Type GetType(string typeName)
        {
            typeName = typeName.Trim();
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        private void AddTypesToDictionary<T>(ref Dictionary<string, Type> nameTypeDictionary) where T : MonoBehaviour
        {
            Assembly objectAssembly = Assembly.GetAssembly(typeof(T));
            Type[] objectTypes = objectAssembly.GetTypes();
            foreach (Type type in objectTypes)
            {
                if (type.IsSubclassOf(typeof(T)))
                {
                    nameTypeDictionary.Add(type.ToString(), type);
                }
            }
        }

        private TriggerNames FindTriggerTypeNames(Dictionary<string, Type> nameTypeDictionary)
        {
            TriggerNames tn = new TriggerNames();
            tn.TypeNames = new string[nameTypeDictionary.Keys.Count];
            tn.DisplayNames = new string[nameTypeDictionary.Keys.Count];
            List<string> groupedObjectNames = new List<string>();
            List<string> ungroupedObjectNames = new List<string>();
            foreach (KeyValuePair<string, Type> pair in nameTypeDictionary)
            {
                PropertyInfo pi = pair.Value.GetProperty("path");
                if (pi != null)
                    groupedObjectNames.Add(((string)pi.GetValue(pair.Value, null)) + pair.Key);
                else
                    ungroupedObjectNames.Add(pair.Key);
            }
            groupedObjectNames = groupedObjectNames.OrderBy(s => s.Substring(s.LastIndexOf("."))).ToList();
            ungroupedObjectNames = ungroupedObjectNames.OrderBy(s => s.Substring(s.LastIndexOf("."))).ToList();
            for (int i = 0; i < groupedObjectNames.Count; i++)
            {
                //Debug.Log("Grouped Object Names = " + groupedObjectNames[i]);
                tn.TypeNames[i] = groupedObjectNames[i];
                // Seperate the name by '/' to find the actual name
                string[] split = tn.TypeNames[i].Split('/');
                // Remove everything before last '.' to avoid namespace included in display name
                int lastIndexOfDot = split[split.Length - 1].LastIndexOf(".");
                split[split.Length - 1] = split[split.Length - 1].Substring(lastIndexOfDot + 1);
                // Join the string, including '/' again, to get proper path
                tn.DisplayNames[i] = string.Join("/", split);
            }

            for (int i = 0; i < ungroupedObjectNames.Count; i++)
            {
                tn.TypeNames[i + groupedObjectNames.Count] = ungroupedObjectNames[i];
                int lastIndexOfDot = tn.TypeNames[i + groupedObjectNames.Count].LastIndexOf(".");
                tn.DisplayNames[i + groupedObjectNames.Count] = tn.TypeNames[i + groupedObjectNames.Count].Substring(lastIndexOfDot + 1);
            }

            return tn;
        }
    }
}