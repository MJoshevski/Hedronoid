using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;

public abstract class HNDMonoSingleton<T> : HNDMonoBehaviour where T : class
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

    protected override void Awake()
    {
        _instance = this as T;
    }

    protected override void OnDestroy()
    {
        _instance = default(T);
    }
}
