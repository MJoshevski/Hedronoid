using UnityEngine;
using System.Collections.Generic;

public static class NNVector2Extensions
{
    public static Vector2 NormalizeIfLongerThanOne(this Vector2 vector)
    {
        return vector.sqrMagnitude > 1f ? vector.normalized : vector;
    }

    public static Vector3 ToVector3XZ(this Vector2 vect2)
    {
        return new Vector3(vect2.x, 0f, vect2.y);
    }

    /// <summary>
    /// Threat vector as position and make it as if it would lie on or inside of circle with radius of 1 
    /// </summary>
    public static Vector2 Circulize(this Vector2 vect)
    {
        var larger = Mathf.Max(Mathf.Abs(vect.x), Mathf.Abs(vect.y));
        var outputOnCircle = vect / larger;
        outputOnCircle = vect / outputOnCircle.magnitude;
        return outputOnCircle;
    }   
}