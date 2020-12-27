using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    /// <summary>
    /// Extension method to check if a layer is in a layermask
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool Contains(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }

    public static void Raise(this object o, Action action)
    {
        if (action != null)
        {
            action();
        }
    }

    public static void Raise<T>(this object o, Action<T> action, T p1)
    {
        if (action != null)
        {
            action(p1);
        }
    }

    public static void Raise<T1, T2>(this object o, Action<T1, T2> action, T1 p1, T2 p2)
    {
        if (action != null)
        {
            action(p1, p2);
        }
    }

}
