using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hedronoid.HNDBuild
{
    [CustomEditor(typeof(HNDBuildPipeline), true)]
    public class HNDBuildPipelineEditor : Editor
    {
        HNDBuildPipeline thisTarget;

        void OnEnable()
        {
            thisTarget = (HNDBuildPipeline)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Separator();

            if (GUILayout.Button("Build!"))
            {
                thisTarget.Build();
            }
        }
    }
}