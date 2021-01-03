using UnityEngine;
using System.Collections;
using UnityEditor;
using Hedronoid.TriggerSystem;

[CustomEditor(typeof(AnimParamFloatAction))]
public class AnimParamFloatActionEditor : Editor
{
    private AnimParamFloatAction m_thisAnimParamFloatAction;

    void OnEnable()
    {
        m_thisAnimParamFloatAction = target as AnimParamFloatAction;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var anim = serializedObject.FindProperty("m_Animator");
        EditorGUILayout.PropertyField(anim);
        var floatParams = serializedObject.FindProperty("m_FloatParams");
        EditorGUILayout.PropertyField(floatParams, true);
        var floatAnimParamMode = serializedObject.FindProperty("m_FloatAnimParamMode");
        EditorGUILayout.PropertyField(floatAnimParamMode);

        if (m_thisAnimParamFloatAction.FloatAnimParamMode == AnimParamFloatAction.eFloatParamAnimMode.OneValue)
        {
            var setVal = serializedObject.FindProperty("m_SetValue");
            EditorGUILayout.PropertyField(setVal);
        }
        else if (m_thisAnimParamFloatAction.FloatAnimParamMode == AnimParamFloatAction.eFloatParamAnimMode.ChangeOverTimeWithCurve)
        {
            var valueCurve = serializedObject.FindProperty("m_ValueCurve");
            EditorGUILayout.PropertyField(valueCurve);

            var curveValueSpan = serializedObject.FindProperty("m_CurveValueSpan");
            EditorGUILayout.PropertyField(curveValueSpan);

            var curveDuration = serializedObject.FindProperty("m_CurveDuration");
            EditorGUILayout.PropertyField(curveDuration);

            var stopCurveOnRevert = serializedObject.FindProperty("m_StopCurveOnRevert");
            EditorGUILayout.PropertyField(stopCurveOnRevert);
        }

        AnimParamFloatAction mp = (AnimParamFloatAction)target;

        serializedObject.ApplyModifiedProperties();
    }
}
