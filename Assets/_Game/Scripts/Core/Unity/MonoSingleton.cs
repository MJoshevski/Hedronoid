using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : class
{
    static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("Trying to access not awoken instance");
                return default(T);
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        _instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        _instance = default(T);
    }
}
