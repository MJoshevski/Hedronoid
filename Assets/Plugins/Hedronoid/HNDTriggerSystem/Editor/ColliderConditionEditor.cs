using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.SceneManagement;

namespace Hedronoid.TriggerSystem
{
    [CustomEditor(typeof(ColliderCondition))]
    public class ColliderConditionEditor : Editor
    {

        ColliderCondition thisCC;

        void OnEnable()
        {
            thisCC = (ColliderCondition)target;
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(thisCC), typeof(ColliderCondition), false);
            GUI.enabled = true;

            //EditorGUIUtility.LookLikeInspector();
            EditorGUILayout.Toggle("Fulfilled", thisCC.Fulfilled);
            thisCC.ColliderFilterType = (ColliderCondition.ColliderFilter)EditorGUILayout.EnumPopup("Collider filter", thisCC.ColliderFilterType);
            if (thisCC.ColliderFilterType == ColliderCondition.ColliderFilter.LAYER)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ColliderLayer"), new GUIContent("Layer Mask"));
            }
            if (thisCC.ColliderFilterType == ColliderCondition.ColliderFilter.TAG)
                thisCC.ColliderTag = EditorGUILayout.TextField("Tag", thisCC.ColliderTag);
            if (thisCC.ColliderFilterType == ColliderCondition.ColliderFilter.SPECIFIC)
            {
                thisCC.ColliderSpecific = (Collider)EditorGUILayout.ObjectField("Collider", thisCC.ColliderSpecific, typeof(Collider), true);
            }
            thisCC.Condition = (ColliderCondition.ConditionType)EditorGUILayout.EnumPopup("Condition type", thisCC.Condition);
            thisCC.CheckSpeed = EditorGUILayout.Toggle("Check speed", thisCC.CheckSpeed);
            if (thisCC.CheckSpeed)
            {
                thisCC.SpeedThreshold = EditorGUILayout.FloatField("Speed threshold", thisCC.SpeedThreshold);
                thisCC.ShouldBeAboveThreshold = EditorGUILayout.Toggle("Should Be Above Threshold", thisCC.ShouldBeAboveThreshold);
            }

            EditorUtility.SetDirty(thisCC);

            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(thisCC.gameObject.scene);
        }
    }
}