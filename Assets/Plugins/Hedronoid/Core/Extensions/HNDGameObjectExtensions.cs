using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HNDGameObjectExtensions
{
    public static T CreateInvincibleInstance<T>(string name = null) where T : MonoBehaviour
    {
        GameObject go = new GameObject();
        go.name = string.IsNullOrEmpty(name) ? typeof(T).Name : name;
        go.AddComponent<T>();
        MonoBehaviour.DontDestroyOnLoad(go);
        return go.GetComponent<T>();
    }

    public static T GetComponentInParent<T>(this GameObject gameObject, bool includeInactive) where T : MonoBehaviour
    {
        if (!includeInactive)
        {
            return gameObject.GetComponentInParent<T>();
        }

        T component = null;
        Transform t = gameObject.transform;
        while (t != null)
        {
            component = t.GetComponent<T>();
            if (component != null)
            {
                break;
            }
            t = t.parent;
        }
        return component;
    }
}