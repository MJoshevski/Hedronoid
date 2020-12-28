using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class NNListExtensions
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static T PopLast<T>(this IList<T> list)
    {
        int itemCount = list.Count;
        if (itemCount == 0)
        {
            throw new InvalidOperationException("List is empty!");
        }

        T item = list[itemCount - 1];
        list.RemoveAt(itemCount - 1);
        return item;
    }

    public static T PopFirst<T>(this IList<T> list)
    {
        int itemCount = list.Count;
        if (itemCount == 0)
        {
            throw new InvalidOperationException("List is empty!");
        }

        T item = list[0];
        list.RemoveAt(0);
        return item;
    }
}