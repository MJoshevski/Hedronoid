using UnityEngine;
using UnityEditor;
using System.Collections;
using Hedronoid.TriggerSystem;

[CustomEditor(typeof(InstantiateGameObjectAction))]
public class InstantiateGameObjectActionEditor : Editor
{
    InstantiateGameObjectAction thisIGOA;

    void OnEnable()
    {
        thisIGOA = (InstantiateGameObjectAction)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        thisIGOA.CloneTriggeringObject = EditorGUILayout.Toggle(new GUIContent("Clone Triggering Object", "If this is true, the object that caused the action to trigger will be duplicated"), thisIGOA.CloneTriggeringObject);
        GUI.enabled = !thisIGOA.CloneTriggeringObject;
        thisIGOA.InstantiateObject = EditorGUILayout.ObjectField(new GUIContent("Instantiate Object", "The object to instantiate, if we're not cloning the incoming object"), thisIGOA.InstantiateObject, typeof(GameObject), false) as GameObject;
        GUI.enabled = true;

        thisIGOA.UseTriggeringObjectParent = EditorGUILayout.Toggle(new GUIContent("Use Triggering Object Parent", "If this is true, the newly created object will get the same parent as the triggering object"), thisIGOA.UseTriggeringObjectParent);
        GUI.enabled = !thisIGOA.UseTriggeringObjectParent;
        thisIGOA.NewParent = EditorGUILayout.ObjectField(new GUIContent("New Parent", "The parent for the newly created object, if not set to use triggering object's parent"), thisIGOA.NewParent, typeof(Transform), true) as Transform;
        GUI.enabled = true;

        ////////////////////////////////////////
        // POSITION                           //
        ////////////////////////////////////////
        thisIGOA.ObjectPositioning = (InstantiateGameObjectAction.EObjectPositioning)EditorGUILayout.EnumPopup(new GUIContent("Object Positioning", "The method of choosing a position for the newly created object"), thisIGOA.ObjectPositioning);

        if (thisIGOA.ObjectPositioning == InstantiateGameObjectAction.EObjectPositioning.OBJECT_POSITION)
        {
            EditorGUI.indentLevel++;
            thisIGOA.ObjectPos = EditorGUILayout.ObjectField(new GUIContent("Object Pos", "Reference to the object at which position the newly created object should be spawned"), thisIGOA.ObjectPos, typeof(Transform), true) as Transform;
            EditorGUI.indentLevel--;
        }

        if (thisIGOA.ObjectPositioning == InstantiateGameObjectAction.EObjectPositioning.WORLD_POSITION)
        {
            EditorGUI.indentLevel++;
            thisIGOA.WorldPos = EditorGUILayout.Vector3Field(new GUIContent("World Pos", "The world position at which the newly created object should be spawned"), thisIGOA.WorldPos);
            EditorGUI.indentLevel--;
        }

        if (thisIGOA.ObjectPositioning == InstantiateGameObjectAction.EObjectPositioning.LOCAL_POSITION)
        {
            EditorGUI.indentLevel++;
            thisIGOA.LocalPos = EditorGUILayout.Vector3Field(new GUIContent("Local Pos", "The local position at which the newly created object should be spawned"), thisIGOA.LocalPos);
            EditorGUI.indentLevel--;
        }

        if (thisIGOA.ObjectPositioning == InstantiateGameObjectAction.EObjectPositioning.TRIGGER_POSITION)
        {
            EditorGUI.indentLevel++;
            thisIGOA.TriggerPosOffset = EditorGUILayout.Vector3Field(new GUIContent("Trigger Pos Offset", "The newly created object will be spawned with this offset, relative to the trigger position"), thisIGOA.TriggerPosOffset);
            EditorGUI.indentLevel--;
        }
        
        if (thisIGOA.ObjectPositioning == InstantiateGameObjectAction.EObjectPositioning.TRIGGERING_OBJECT_POSITION)
        {
            EditorGUI.indentLevel++;
            thisIGOA.TriggeringObjectPosOffset = EditorGUILayout.Vector3Field(new GUIContent("Triggering Object Pos Offset", "The newly created object will be spawned with this offset, relative to the triggering object's position"), thisIGOA.TriggeringObjectPosOffset);
            EditorGUI.indentLevel--;
        }

        ////////////////////////////////////////
        // ROTATION                           //
        ////////////////////////////////////////
        thisIGOA.ObjectOrientation = (InstantiateGameObjectAction.EObjectOrientation)EditorGUILayout.EnumPopup(new GUIContent("Object Orientation", "The method of choosing an orientation for the newly created object"), thisIGOA.ObjectOrientation);

        if (thisIGOA.ObjectOrientation == InstantiateGameObjectAction.EObjectOrientation.WORLD_ROTATION)
        {
            EditorGUI.indentLevel++;
            thisIGOA.WorldRotation = EditorGUILayout.Vector3Field(new GUIContent("World Rotation", "The newly created object will be spawned with this world rotation"), thisIGOA.WorldRotation);
            EditorGUI.indentLevel--;
        }

        if (thisIGOA.ObjectOrientation == InstantiateGameObjectAction.EObjectOrientation.LOCAL_ROTATION)
        {
            EditorGUI.indentLevel++;
            thisIGOA.LocalRotation = EditorGUILayout.Vector3Field(new GUIContent("Local Rotation", "The newly created object will be spawned with this local rotation"), thisIGOA.LocalRotation);
            EditorGUI.indentLevel--;
        }
    }
}
