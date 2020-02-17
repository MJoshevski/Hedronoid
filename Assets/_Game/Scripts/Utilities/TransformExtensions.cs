using UnityEngine;

namespace Hedronoid
{
    /// <summary>
    /// Describe an axis in cartesian coordinates, Useful for components that need to serialize which axis to use in some fashion.
    /// </summary>
    public enum CartesianAxis
    {
        Zneg = -3,
        Yneg = -2,
        Xneg = -1,
        X = 0,
        Y = 1,
        Z = 2
    }

    public static class TransformExtensions
    {
        public static Transform relativeTransform(this Transform transform)
        {
            IGravityService service = GravityService.Instance;
            if (service == null)
                Debug.LogError("No gravity service initialized.");

            Matrix4x4 matrix = transform.localToWorldMatrix;
            Quaternion rotation = MatrixExtensions.ExtractRotation(ref matrix);

            Transform newTransform = transform;
            return newTransform;
        }

        /// <summary>
        /// Set transform component from TRS matrix.
        /// </summary>
        /// <param name="transform">Transform component.</param>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        public static void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
        {
            transform.localPosition = MatrixExtensions.ExtractTranslation(ref matrix);
            transform.localRotation = MatrixExtensions.ExtractRotation(ref matrix);
            transform.localScale = MatrixExtensions.ExtractScale(ref matrix);
        }
    }
}
