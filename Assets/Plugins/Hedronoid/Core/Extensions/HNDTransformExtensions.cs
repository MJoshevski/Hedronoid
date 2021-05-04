using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class HNDTransformExtensions
{
    /// <summary>
    /// Recursively finds first child with given name. Search could be very expensive, so avoid calling it each frame.
    /// </summary>
    /// <returns>Transform or null.</returns>
    /// <param name="transform">Transform.</param>
    /// <param name="name">Name.</param>
    public static Transform FindRecursive(this Transform thisTransform, string name)
    {
        Transform found = thisTransform.Find(name);
        if (found != null)
        {
            return found;
        }

        for (int i = 0; i < thisTransform.childCount; i++)
        {
            found = thisTransform.GetChild(i).FindRecursive(name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns all children in a list
    /// </summary>
    /// <param name="t">Transform</param>
    /// <param name="onlyActive">if only active ones should be returned</param>
    /// <returns></returns>
    public static List<Transform> GetChildren(this Transform t, bool onlyActive = false)
    {
        List<Transform> children = new List<Transform>();
        for(int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
            if ((onlyActive && child.gameObject.activeSelf) || !onlyActive)
            {
                children.Add(t.GetChild(i));
            }
        }
        return children;
    }

    public static void CopyLocalValuesTo(this Transform src, Transform dst)
    {
        dst.localPosition = src.localPosition;
        dst.localRotation = src.localRotation;
        dst.localScale = src.localScale;
    }

    public static HNDTransformValues GetLocalTransformValues(this Transform src)
    {
        HNDTransformValues values = new HNDTransformValues();
        values.Position = src.localPosition;
        values.Rotation = src.localRotation;
        values.LocalScale = src.localScale;
        return values;
    }

    public static HNDTransformValues GetWorldTransformValues(this Transform src)
    {
        HNDTransformValues values = new HNDTransformValues();
        values.Position = src.position;
        values.Rotation = src.rotation;
        values.LocalScale = src.localScale;
        return values;
    }

    public static void SetFromLocalTransformValues(this Transform dst, HNDTransformValues values)
    {
        dst.localPosition = values.Position;
        dst.localRotation = values.Rotation;
        dst.localScale = values.LocalScale;
    }

    public static void SetFromWorldTransformValuesExceptScale(this Transform dst, HNDTransformValues values)
    {
        dst.position = values.Position;
        dst.rotation = values.Rotation;
        // Ignoring scale
    }
}