﻿using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Hedronoid
{
    public static class QuaternionExtensions
    {

        public static bool IsNaN(Quaternion q)
        {
            return float.IsNaN(q.x * q.y * q.z * q.w);
        }

        #region Utils

        public static Quaternion Normalize(Quaternion q)
        {
            var mag = System.Math.Sqrt(q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z);
            q.w = (float)((double)q.w / mag);
            q.x = (float)((double)q.x / mag);
            q.y = (float)((double)q.y / mag);
            q.z = (float)((double)q.z / mag);
            return q;
        }

        /// <summary>
        /// A cleaner version of FromToRotation, Quaternion.FromToRotation for some reason can only handle down to #.## precision.
        /// This will result in true 7 digits of precision down to depths of 0.00000# (depth tested so far).
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Quaternion FromToRotation(Vector3 v1, Vector3 v2)
        {
            var a = Vector3.Cross(v1, v2);
            double w = System.Math.Sqrt(v1.sqrMagnitude * v2.sqrMagnitude) + Vector3.Dot(v1, v2);
            if (a.sqrMagnitude < 0.0001f)
            {
                //the vectors are parallel, check w to find direction
                //if w is 0 then values are opposite, and we should rotate 180 degrees around some axis
                //otherwise the vectors in the same direction and no rotation should occur
                return (System.Math.Abs(w) < 0.0001d) ? new Quaternion(0f, 1f, 0f, 0f) : Quaternion.identity;
            }
            else
            {
                return new Quaternion(a.x, a.y, a.z, (float)w).normalized;
            }
        }

        public static Quaternion FromToRotation(Vector3 v1, Vector3 v2, Vector3 defaultAxis)
        {
            var a = Vector3.Cross(v1, v2);
            double w = System.Math.Sqrt(v1.sqrMagnitude * v2.sqrMagnitude) + Vector3.Dot(v1, v2);
            if (a.sqrMagnitude < 0.0001f)
            {
                //the vectors are parallel, check w to find direction
                //if w is 0 then values are opposite, and we should rotate 180 degrees around the supplied axis
                //otherwise the vectors in the same direction and no rotation should occur
                return (System.Math.Abs(w) < 0.0001d) ? new Quaternion(defaultAxis.x, defaultAxis.y, defaultAxis.z, 0f).normalized : Quaternion.identity;
            }
            else
            {
                return new Quaternion(a.x, a.y, a.z, (float)w).normalized;
            }
        }

        /// <summary>
        /// Get the rotation that would be applied to 'start' to end up at 'end'.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static Quaternion FromToRotation(Quaternion start, Quaternion end)
        {
            return Quaternion.Inverse(start) * end;
        }

        public static Quaternion SpeedSlerp(Quaternion from, Quaternion to, float angularSpeed, float dt, bool bUseRadians = false)
        {
            if (bUseRadians) angularSpeed *= Mathf.Rad2Deg;
            var da = angularSpeed * dt;
            return Quaternion.RotateTowards(from, to, da);
        }

        public static Vector3 Normalize(Vector3 euler)
        {
            euler.x = MathUtil.NormalizeAngle(euler.x, false);
            euler.y = MathUtil.NormalizeAngle(euler.y, false);
            euler.z = MathUtil.NormalizeAngle(euler.z, false);
            return euler;
        }

        #endregion

        #region Transform

        /// <summary>
        /// Create a LookRotation for a non-standard 'forward' axis.
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="forwardAxis"></param>
        /// <returns></returns>
        public static Quaternion AltForwardLookRotation(Vector3 dir, Vector3 forwardAxis, Vector3 upAxis)
        {
            //return Quaternion.LookRotation(dir, upAxis) * Quaternion.FromToRotation(forwardAxis, Vector3.forward);
            return Quaternion.LookRotation(dir) * Quaternion.Inverse(Quaternion.LookRotation(forwardAxis, upAxis));
        }
        //public static Quaternion AltForwardLookRotation(Vector3 dir, Vector3 forwardAxis)
        //{
        //    return Quaternion.LookRotation(dir) * Quaternion.FromToRotation(forwardAxis, Vector3.forward);
        //}

        /// <summary>
        /// Get the rotated forward axis based on some base forward.
        /// </summary>
        /// <param name="rot">The rotation</param>
        /// <param name="baseForward">Forward with no rotation</param>
        /// <returns></returns>
        public static Vector3 GetAltForward(Quaternion rot, Vector3 baseForward)
        {
            return rot * baseForward;
        }

        /// <summary>
        /// Returns a rotation of up attempting to face in the general direction of forward.
        /// </summary>
        /// <param name="up"></param>
        /// <param name="targForward"></param>
        /// <returns></returns>
        public static Quaternion FaceRotation(Vector3 forward, Vector3 up)
        {
            forward = VectorExtensions.GetForwardTangent(forward, up);
            return Quaternion.LookRotation(forward, up);
        }

        public static void GetAngleAxis(this Quaternion q, out Vector3 axis, out float angle)
        {
            if (q.w > 1) q = QuaternionExtensions.Normalize(q);

            //get as doubles for precision
            var qw = (double)q.w;
            var qx = (double)q.x;
            var qy = (double)q.y;
            var qz = (double)q.z;
            var ratio = System.Math.Sqrt(1.0d - qw * qw);

            angle = (float)(2.0d * System.Math.Acos(qw)) * Mathf.Rad2Deg;
            if (ratio < 0.001d)
            {
                axis = new Vector3(1f, 0f, 0f);
            }
            else
            {
                axis = new Vector3(
                    (float)(qx / ratio),
                    (float)(qy / ratio),
                    (float)(qz / ratio));
                axis.Normalize();
            }
        }

        public static void GetShortestAngleAxisBetween(Quaternion a, Quaternion b, out Vector3 axis, out float angle)
        {
            var dq = Quaternion.Inverse(a) * b;
            if (dq.w > 1) dq = QuaternionExtensions.Normalize(dq);

            //get as doubles for precision
            var qw = (double)dq.w;
            var qx = (double)dq.x;
            var qy = (double)dq.y;
            var qz = (double)dq.z;
            var ratio = System.Math.Sqrt(1.0d - qw * qw);

            angle = (float)(2.0d * System.Math.Acos(qw)) * Mathf.Rad2Deg;
            if (ratio < 0.001d)
            {
                axis = new Vector3(1f, 0f, 0f);
            }
            else
            {
                axis = new Vector3(
                    (float)(qx / ratio),
                    (float)(qy / ratio),
                    (float)(qz / ratio));
                axis.Normalize();
            }
        }

        #endregion

        #region String

        public static string Stringify(Quaternion q)
        {
            return q.x.ToString() + "," + q.y.ToString() + "," + q.z.ToString() + "," + q.w.ToString();
        }

        public static string ToDetailedString(this Quaternion v)
        {
            return System.String.Format("<{0}, {1}, {2}, {3}>", v.x, v.y, v.z, v.w);
        }

        #endregion

        public static float ComputeXAngle(this quaternion q)
        {
            float sinr_cosp = 2 * (q.value.w * q.value.x + q.value.y * q.value.z);
            float cosr_cosp = 1 - 2 * (q.value.x * q.value.x + q.value.y * q.value.y);
            return math.atan2(sinr_cosp, cosr_cosp);
        }

        public static float ComputeYAngle(this quaternion q)
        {
            float sinp = 2 * (q.value.w * q.value.y - q.value.z * q.value.x);
            if (math.abs(sinp) >= 1)
                return math.PI / 2 * math.sign(sinp); // use 90 degrees if out of range
            else
                return math.asin(sinp);
        }

        public static float ComputeZAngle(this quaternion q)
        {
            float siny_cosp = 2 * (q.value.w * q.value.z + q.value.x * q.value.y);
            float cosy_cosp = 1 - 2 * (q.value.y * q.value.y + q.value.z * q.value.z);
            return math.atan2(siny_cosp, cosy_cosp);
        }

        public static float3 EulerAngles(this quaternion q)
        {
            return new float3(ComputeXAngle(q), ComputeYAngle(q), ComputeZAngle(q));
        }

        public static quaternion ToQuaternion(float3 angles)
        {

            float cy = math.cos(angles.z * 0.5f);
            float sy = math.sin(angles.z * 0.5f);
            float cp = math.cos(angles.y * 0.5f);
            float sp = math.sin(angles.y * 0.5f);
            float cr = math.cos(angles.x * 0.5f);
            float sr = math.sin(angles.x * 0.5f);

            float4 q;
            q.w = cr * cp * cy + sr * sp * sy;
            q.x = sr * cp * cy - cr * sp * sy;
            q.y = cr * sp * cy + sr * cp * sy;
            q.z = cr * cp * sy - sr * sp * cy;

            return q;

        }
    }
}