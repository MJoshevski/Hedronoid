using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HNDMonoBehaviourExtensions
{

    public static Coroutine StopCoroutineSafely(this MonoBehaviour mb, Coroutine c)
    {
        if (c != null)
        {
            mb.StopCoroutine(c);
        }
        return null;
    }
}
