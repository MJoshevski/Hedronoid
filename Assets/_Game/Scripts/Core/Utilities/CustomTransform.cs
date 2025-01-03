﻿using UnityEngine;

namespace Hedronoid
{

    [System.Serializable()]
    public struct CustomTransform
    {

        [SerializeField()]
        public Vector3 Position;
        [SerializeField()]
        public Quaternion Rotation;
        [SerializeField()]
        public Vector3 Scale;

        public Matrix4x4 Matrix
        {
            get
            {
                return Matrix4x4.TRS(Position, Rotation, Scale);
            }
            set
            {
                Position = CustomTransformExtensions.GetTranslation(value);
                Rotation = CustomTransformExtensions.GetRotation(value);
                Scale = CustomTransformExtensions.GetScale(value);
            }
        }

        #region CONSTRUCTORS

        public CustomTransform(Vector3 pos, Quaternion rot)
        {
            this.Position = pos;
            this.Rotation = rot;
            this.Scale = Vector3.one;
        }

        public CustomTransform(Vector3 pos, Quaternion rot, Vector3 sc)
        {
            this.Position = pos;
            this.Rotation = rot;
            this.Scale = sc;
        }

        public static CustomTransform Identity
        {
            get
            {
                return new CustomTransform(Vector3.zero, Quaternion.identity, Vector3.one);
            }
        }

        public static CustomTransform NaN
        {
            get
            {
                return new CustomTransform(new Vector3(float.NaN, float.NaN, float.NaN),
                                 new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN),
                                 new Vector3(float.NaN, float.NaN, float.NaN));
            }
        }

        public static CustomTransform Translation(Vector3 pos)
        {
            return new CustomTransform(pos, Quaternion.identity, Vector3.one);
        }

        public static CustomTransform Translation(float x, float y, float z)
        {
            return new CustomTransform(new Vector3(x, y, z), Quaternion.identity, Vector3.one);
        }

        public static CustomTransform Rotated(Quaternion q)
        {
            return new CustomTransform(Vector3.zero, q, Vector3.one);
        }

        public static CustomTransform Rotated(Vector3 eulerAngles)
        {
            return new CustomTransform(Vector3.zero, Quaternion.Euler(eulerAngles), Vector3.one);
        }

        public static CustomTransform Rotated(float x, float y, float z)
        {
            return new CustomTransform(Vector3.zero, Quaternion.Euler(x, y, z), Vector3.one);
        }

        public static CustomTransform Scaled(Vector3 sc)
        {
            return new CustomTransform(Vector3.zero, Quaternion.identity, sc);
        }

        public static CustomTransform Scaled(float x, float y, float z)
        {
            return new CustomTransform(Vector3.zero, Quaternion.identity, new Vector3(x, y, z));
        }

        public static CustomTransform Transform(Matrix4x4 mat)
        {
            var t = new CustomTransform();
            t.Matrix = mat;
            return t;
        }

        public static CustomTransform Transform(Vector3 pos, Quaternion rot)
        {
            return new CustomTransform(pos, rot);
        }

        public static CustomTransform Transform(Vector3 pos, Quaternion rot, Vector3 sc)
        {
            return new CustomTransform(pos, rot, sc);
        }

        #endregion

        #region Properties

        public Vector3 Forward
        {
            get { return this.Rotation * Vector3.forward; }
        }

        public Vector3 Up
        {
            get { return this.Rotation * Vector3.up; }
        }

        public Vector3 Right
        {
            get { return this.Rotation * Vector3.right; }
        }

        #endregion

        #region Methods

        public void Translate(Vector3 v)
        {
            Position += v;
        }

        public void Rotate(Quaternion q)
        {
            Rotation *= q;
        }

        public void Rotate(float x, float y, float z)
        {
            Rotation *= Quaternion.Euler(x, y, z);
        }

        public void Rotate(Vector3 eulerRot)
        {
            Rotation *= Quaternion.Euler(eulerRot);
        }

        public void RotateAround(Vector3 point, float angle, Vector3 axis)
        {
            var v = this.Position - point;
            var q = Quaternion.AngleAxis(angle, axis);
            v = q * v;
            this.Position = point + v;
            this.Rotation *= q;
        }

        public void LookAt(Vector3 point, Vector3 up)
        {
            this.Rotation = Quaternion.LookRotation(point - this.Position, up);
        }

        /// <summary>
        /// Transforms point from local space to global space.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector3 TransformPoint(Vector3 v)
        {
            return this.Matrix.MultiplyPoint(v);
        }

        /// <summary>
        /// Trasnforms direction vector from local space to global space.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector3 TransformDirection(Vector3 v)
        {
            return this.Matrix.MultiplyVector(v);
        }

        public Vector3 InverseTransformPoint(Vector3 v)
        {
            return this.Matrix.inverse.MultiplyPoint(v);
        }

        public Vector3 InverseTransformDirection(Vector3 v)
        {
            return this.Matrix.inverse.MultiplyVector(v);
        }

        /// <summary>
        /// Transforms current state from global space to the local space of the passed in 'parent'. 
        /// </summary>
        /// <param name="parent">The parent to set local to. If null, stays global.</param>
        public void TransformTo(Transform parent)
        {
            if (parent == null) return;

            this.Position = parent.InverseTransformPoint(this.Position);
            this.Rotation = parent.InverseTransformRotation(this.Rotation);
            this.Scale = parent.InverseTransformVector(this.Scale);
        }

        /// <summary>
        /// Transforms current state from local space of passed in 'parent' to the global space.
        /// </summary>
        /// <param name="parent">The parent to set global from. If null, stays same.</param>
        public void TransformFrom(Transform parent)
        {
            if (parent == null) return;

            this.Position = parent.TransformPoint(this.Position);
            this.Rotation = parent.TransformRotation(this.Rotation);
            this.Scale = parent.TransformVector(this.Scale);
        }

        #endregion

        #region Operators

        public static CustomTransform operator *(CustomTransform t1, CustomTransform t2)
        {
            t1.Position += t2.Position;
            t1.Rotation *= t2.Rotation;
            var v1 = t1.Scale;
            var v2 = t2.Scale;
            v1.x *= v2.x;
            v1.y *= v2.y;
            v1.z *= v2.z;
            t1.Scale = v1;
            return t1;
        }

        public static CustomTransform operator +(CustomTransform t, Vector3 v)
        {
            t.Position += v;
            return t;
        }

        public static CustomTransform operator *(CustomTransform t, Quaternion q)
        {
            t.Rotation *= q;
            return t;
        }

        public static Vector3 operator *(CustomTransform t, Vector3 v)
        {
            return t.Matrix.MultiplyPoint(v);
        }

        #endregion

        #region Get/Set to Transform

        public void SetToLocal(Transform trans)
        {
            trans.localPosition = Position;
            trans.localRotation = Rotation;
            trans.localScale = Scale;
        }

        public void SetToLocal(Transform trans, bool bSetScale)
        {
            trans.localPosition = Position;
            trans.localRotation = Rotation;
            if (bSetScale) trans.localScale = Scale;
        }

        public void SetToGlobal(Transform trans, bool bSetScale)
        {
            if (bSetScale)
            {
                trans.position = Position;
                trans.rotation = Rotation;
                trans.localScale = Vector3.one;
                var m = trans.worldToLocalMatrix;
                m.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
                trans.localScale = m.MultiplyPoint(Scale);
            }
            else
            {
                trans.position = Position;
                trans.rotation = Rotation;
            }
        }

        public void SetToGlobal(Transform trans, bool bSetScale, bool bSetScaleOnGlobalAxes)
        {
            if (bSetScale)
            {
                trans.position = Position;
                trans.rotation = Rotation;
                trans.localScale = Vector3.one;
                var m = trans.worldToLocalMatrix;
                if (bSetScaleOnGlobalAxes)
                {
                    m.SetColumn(0, new Vector4(m.GetColumn(0).magnitude, 0f));
                    m.SetColumn(1, new Vector4(0f, m.GetColumn(1).magnitude));
                    m.SetColumn(2, new Vector4(0f, 0f, m.GetColumn(2).magnitude));
                }
                m.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
                trans.localScale = m.MultiplyPoint(Scale);
            }
            else
            {
                trans.position = Position;
                trans.rotation = Rotation;
            }
        }

        public static CustomTransform GetLocal(Transform trans)
        {
            var t = new CustomTransform();
            t.Position = trans.localPosition;
            t.Rotation = trans.localRotation;
            t.Scale = trans.localScale;
            return t;
        }

        public static CustomTransform GetGlobal(Transform trans)
        {
            var t = new CustomTransform();
            t.Position = trans.position;
            t.Rotation = trans.rotation;
            t.Scale = trans.lossyScale;
            return t;
        }

        #endregion

        #region Static Methods

        public static bool IsNaN(CustomTransform t)
        {
            return VectorExtensions.IsNaN(t.Position) || VectorExtensions.IsNaN(t.Scale) || QuaternionExtensions.IsNaN(t.Rotation);
        }

        public static CustomTransform Lerp(CustomTransform start, CustomTransform end, float t)
        {
            start.Position = Vector3.LerpUnclamped(start.Position, end.Position, t);
            start.Rotation = Quaternion.SlerpUnclamped(start.Rotation, end.Rotation, t);
            start.Scale = Vector3.Lerp(start.Scale, end.Scale, t);
            return start;
        }

        public static CustomTransform Slerp(CustomTransform start, CustomTransform end, float t)
        {
            start.Position = Vector3.SlerpUnclamped(start.Position, end.Position, t);
            start.Rotation = Quaternion.SlerpUnclamped(start.Rotation, end.Rotation, t);
            start.Scale = Vector3.LerpUnclamped(start.Scale, end.Scale, t);
            return start;
        }

        #endregion

    }
}