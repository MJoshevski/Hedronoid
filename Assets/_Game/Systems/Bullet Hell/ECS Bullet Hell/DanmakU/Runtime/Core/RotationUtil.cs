using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// A set of lower-precision functions for assisting in working with Danmaku rotations.
/// </summary>
public static class RotationUtiliity
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToUnitVector(float rotation)
    {
        return new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
    }

    public static Vector3 ToUnitVector(float rotationYaw, float rotationPitch)
    {
        return new Vector3(
            Mathf.Cos(rotationYaw) * Mathf.Cos(rotationPitch),
            Mathf.Cos(rotationYaw) * Mathf.Sin(rotationPitch),
            Mathf.Sin(rotationPitch));
    }

}
