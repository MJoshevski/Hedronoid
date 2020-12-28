using UnityEngine;

/// <summary>
/// For saving or loading Transform values
/// </summary>
public struct NNTransformValues
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 LocalScale;

    public void SetFromLocalTransformValues(Transform dst)
    {
        dst.SetFromLocalTransformValues(this);
    }

    public void SetFromWorldTransformValuesExceptScale(Transform dst)
    {
        dst.SetFromWorldTransformValuesExceptScale(this);
    }
}