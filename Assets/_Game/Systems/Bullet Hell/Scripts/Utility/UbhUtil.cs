﻿using System.Collections;
using UnityEngine;

/// <summary>
/// Ubh util.
/// </summary>
public static class UbhUtil
{
    public static readonly Vector3 VECTOR3_ZERO = Vector3.zero;
    public static readonly Vector3 VECTOR3_ONE = Vector3.one;
    public static readonly Vector3 VECTOR3_HALF = new Vector3(0.5f, 0.5f, 0.5f);

    public static readonly Vector2 VECTOR2_ZERO = Vector2.zero;
    public static readonly Vector2 VECTOR2_ONE = Vector2.one;
    public static readonly Vector2 VECTOR2_HALF = new Vector2(0.5f, 0.5f);

    public static readonly Quaternion QUATERNION_IDENTITY = Quaternion.identity;

    /// <summary>
    /// Time types.
    /// </summary>
    public enum TIME
    {
        DELTA_TIME,
        UNSCALED_DELTA_TIME,
        FIXED_DELTA_TIME,
    }

    /// <summary>
    /// Determines if is mobile platform.
    /// </summary>
    public static bool IsMobilePlatform()
    {
#if UNITY_IOS || UNITY_ANDROID
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    /// Get Transform from tag name.
    /// </summary>
    //public static Transform GetTransformFromTagName(string tagName, bool randomSelect)
    //{
    //    if (string.IsNullOrEmpty(tagName))
    //    {
    //        return null;
    //    }

    //    GameObject goTarget = null;
    //    if (randomSelect)
    //    {
    //        GameObject[] goTargets = GameObject.FindGameObjectsWithTag(tagName);
    //        if (goTargets != null && goTargets.Length > 0)
    //        {
    //            goTarget = goTargets[Random.Range(0, goTargets.Length)];
    //        }
    //    }
    //    else
    //    {
    //        goTarget = GameObject.FindWithTag(tagName);
    //    }
    //    if (goTarget == null)
    //    {
    //        return null;
    //    }
    //    return goTarget.transform;
    //}

    /// <summary>
    /// Get shifted angle.
    /// </summary>
    public static float GetShiftedAngle(int wayIndex, float baseAngle, float betweenAngle)
    {
        float angle = wayIndex % 2 == 0 ?
                      baseAngle - (betweenAngle * (float)wayIndex / 2f) :
                      baseAngle + (betweenAngle * Mathf.Ceil((float)wayIndex / 2f));
        return angle;
    }

    /// <summary>
    /// Get 0 ~ 360 angle.
    /// </summary>
    public static float GetNormalizedAngle(float angle)
    {
        while (angle < 0f)
        {
            angle += 360f;
        }
        while (360f < angle)
        {
            angle -= 360f;
        }
        return angle;
    }

    /// <summary>
    /// Get Z angle from two transforms position.
    /// </summary>
    public static float GetZangleFromTwoPosition(Transform fromTrans, Transform toTrans)
    {
        if (fromTrans == null || toTrans == null)
        {
            return 0f;
        }
        float xDistance = toTrans.position.x - fromTrans.position.x;
        float yDistance = toTrans.position.y - fromTrans.position.y;
        float angle = (Mathf.Atan2(yDistance, xDistance) * Mathf.Rad2Deg) - 90f;
        angle = GetNormalizedAngle(angle);

        return angle;
    }

    /// <summary>
    /// Get Y angle from two transforms position.
    /// </summary>
    public static float GetYangleFromTwoPosition(Transform fromTrans, Transform toTrans)
    {
        if (fromTrans == null || toTrans == null)
        {
            return 0f;
        }
        float xDistance = toTrans.position.x - fromTrans.position.x;
        float zDistance = toTrans.position.z - fromTrans.position.z;
        float angle = (Mathf.Atan2(zDistance, xDistance) * Mathf.Rad2Deg) - 90f;
        angle = GetNormalizedAngle(angle);

        return angle;
    }

    /// <summary>
    /// Get X angle from two transforms position.
    /// </summary>
    public static float GetXangleFromTwoPosition(Transform fromTrans, Transform toTrans)
    {
        if (fromTrans == null || toTrans == null)
        {
            return 0f;
        }
        float yDistance = toTrans.position.y - fromTrans.position.y;
        float zDistance = toTrans.position.z - fromTrans.position.z;
        float angle = (Mathf.Atan2(zDistance, yDistance) * Mathf.Rad2Deg) - 90f;
        angle = GetNormalizedAngle(angle);

        return angle;
    }
}