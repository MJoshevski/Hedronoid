using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

namespace Hedronoid.Gravity
{
    [CustomEditor(typeof(GravityManager))]
    class GravityManagerEditor : Editor
    {
        GravityManager thisGM;
        void OnEnable()
        {
            thisGM = (GravityManager) target;

            if (thisGM.gravitySourcesInScene.Count == 0)
                thisGM.ScanForGravitySources();
        }
        public override bool RequiresConstantRepaint()
        {
            thisGM.ScanForGravitySources();
            return true;
        }
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(thisGM), typeof(GravityManager), false);
            GUI.enabled = true;

            List<GravitySource> list = thisGM.gravitySourcesInScene;

            if (list.Count == 0 || list == null) return;

            // Populate list of sources
            int newCount = Mathf.Max(0, EditorGUILayout.DelayedIntField("Size", list.Count));

            while (newCount < list.Count)
                list.RemoveAt(list.Count - 1);
            while (newCount > list.Count)
                list.Add(null);

            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical();
                    {
                        GUIStyle style = EditorStyles.foldout;
                        style.fontSize = 15;
                        style.fontStyle = FontStyle.Bold;

                        list[i] = (GravitySource)EditorGUILayout.ObjectField(
                            thisGM.gravityParentNamesInScene[i],
                            list[i], typeof(GravitySource), true);

                        EditorGUI.indentLevel++;
                        EditorGUILayout.IntField("Current Priority Weight", list[i].CurrentPriorityWeight);
                        EditorGUILayout.Toggle("Is Player In Gravity", list[i].IsPlayerInGravity);
                    }

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginVertical();
                    {
                        List<GravitySource> overlapList = list[i].OverlappingSources;
                        for (int j = 0; j < overlapList.Count; j++)
                        {
                            overlapList[j] = (GravitySource)EditorGUILayout.ObjectField(
                                string.Format("Overlap #{0}", j + 1), overlapList[j], typeof(GravitySource), true);
                        }             
                    }

                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
            if (GUILayout.Button("Scan for Gravity Sources"))
                thisGM.ScanForGravitySources();
        }
    }
}
