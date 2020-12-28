using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hedronoid.HNDBuild
{
    [CustomEditor(typeof(HNDBuildStep), true)]
    public class HNDBuildStepEditor : Editor
    {
        HNDBuildStep thisTarget;

        void OnEnable()
        {
            thisTarget = (HNDBuildStep)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            List<string> reqValues = thisTarget.GetRequiredEnvValues();

            if (reqValues == null || reqValues.Count == 0) return;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Used environment variables:");
            EditorGUILayout.TextArea(string.Join("\n", reqValues.ToArray()));
        }
    }
}