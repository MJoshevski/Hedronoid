using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.SceneManagement;
using Hedronoid.TriggerSystem;

[CustomEditor(typeof(RigidbodyForceAction))]
public class RigidbodyForceActionEditor : Editor
{
    RigidbodyForceAction thisRFA;

    void OnEnable()
    {
        thisRFA = (RigidbodyForceAction)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        thisRFA.ForceDirectionType = (RigidbodyForceAction.eRigidBodyForceActionDirectionType) EditorGUILayout.EnumPopup(new GUIContent("Direction Type", "Choose in which direction will force be applied."), thisRFA.ForceDirectionType);
        switch(thisRFA.ForceDirectionType)
        {
            case RigidbodyForceAction.eRigidBodyForceActionDirectionType.StaticDirection:
                thisRFA.Force = EditorGUILayout.Vector3Field(new GUIContent("Force", "The force applied to rigidbodies."), thisRFA.Force);
                thisRFA.InLocalCoords = EditorGUILayout.Toggle(new GUIContent("Force In Local Space", "Is the force in local coordinates?"), thisRFA.InLocalCoords);
                break;

            case RigidbodyForceAction.eRigidBodyForceActionDirectionType.InDirectionOfObject:
                thisRFA.DirectionObject = EditorGUILayout.ObjectField(new GUIContent("Direction Object", "If this is true, the rigidbody will have force applied in direction of another object"), thisRFA.DirectionObject, typeof(GameObject), true) as GameObject;
                thisRFA.ProportionalToDistance = EditorGUILayout.Toggle(new GUIContent("Proportional To Distance", "If this is true, the rigidbody will have force applied in direction of another object"), thisRFA.ProportionalToDistance);
                if (thisRFA.ProportionalToDistance)
                {
                    thisRFA.MaxProportionalDistance = EditorGUILayout.FloatField(new GUIContent("Max Proportional Distance", "If force is applied in direction of object the applied force can be scaled proportionally to the distance. Distance == 0 means max force, max proportional distance means 0 force."), thisRFA.MaxProportionalDistance);
                }

                break;
            case RigidbodyForceAction.eRigidBodyForceActionDirectionType.InHitDirection:
                break;
            case RigidbodyForceAction.eRigidBodyForceActionDirectionType.OppositeVelocity:
                break;
        }

        thisRFA.MinRandomMultipliers = EditorGUILayout.Vector3Field(new GUIContent("Min Random Multipliers", "Minimum random multiplier applied to the force applied to rigidbody."), thisRFA.MinRandomMultipliers);
        thisRFA.MaxRandomMultipliers = EditorGUILayout.Vector3Field(new GUIContent("Max Random Multipliers", "Maximum random multiplier applied to the force applied to rigidbody."), thisRFA.MaxRandomMultipliers);

        thisRFA.ForceMultiplier = EditorGUILayout.FloatField(new GUIContent("Force Multiplier", "The multiplication factor of the force applied to the rigidbody"), thisRFA.ForceMultiplier);
        thisRFA.NormalizeForceVector = EditorGUILayout.Toggle(new GUIContent("Normalize Force Vector", "Will normalize the force vector to get only direction"), thisRFA.NormalizeForceVector);
        // this essentially is a hack, proper way of editing fields is using serialized object and properties and then serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(thisRFA);
        if(!Application.isPlaying) // another shameful hack
            EditorSceneManager.MarkSceneDirty(thisRFA.gameObject.scene);
    }
}
