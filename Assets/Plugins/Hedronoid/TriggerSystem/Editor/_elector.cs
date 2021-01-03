using UnityEngine;
using System.Collections;

public class Selector : MonoBehaviour
{

#if UNITY_EDITOR
    static GameObject CreateTrigger(UnityEditor.MenuCommand menuCommand)
    {
        GameObject go = new GameObject("Trigger");
        if (UnityEditor.SceneView.lastActiveSceneView != null && UnityEditor.SceneView.lastActiveSceneView.camera != null)
            go.transform.position = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position + UnityEditor.SceneView.lastActiveSceneView.camera.transform.forward * 5f;
        go.AddComponent<Selector>();
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        UnityEditor.GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        UnityEditor.Selection.activeGameObject = go;

        return go;
    }

    [UnityEditor.MenuItem("GameObject/Triggers/Basic Trigger", false, 10)]
    static void CreateBasicTrigger(UnityEditor.MenuCommand menuCommand)
    {
        CreateTrigger(menuCommand);
    }

    [UnityEditor.MenuItem("GameObject/Triggers/Box Collider Trigger", false, 10)]
    static void CreateBoxColliderTrigger(UnityEditor.MenuCommand menuCommand)
    {
        GameObject go = CreateTrigger(menuCommand);
        go.AddComponent<BoxCollider>().isTrigger = true;
    }

    [UnityEditor.MenuItem("GameObject/Triggers/Sphere Collider Trigger", false, 10)]
    static void CreateSphereColliderTrigger(UnityEditor.MenuCommand menuCommand)
    {
        GameObject go = CreateTrigger(menuCommand);
        go.AddComponent<SphereCollider>().isTrigger = true;
    }

    [UnityEditor.MenuItem("GameObject/Triggers/Capsule Collider Trigger", false, 10)]
    static void CreateCapsuleColliderTrigger(UnityEditor.MenuCommand menuCommand)
    {
        GameObject go = CreateTrigger(menuCommand);
        go.AddComponent<CapsuleCollider>().isTrigger = true;
    }
#endif
}