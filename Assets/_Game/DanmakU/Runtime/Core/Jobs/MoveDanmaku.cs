using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

namespace DanmakU
{
    internal struct MoveDanmaku : IJobBatchedFor
    {
        public float DeltaTime;

        public NativeArray<Vector3> Positions;
        public NativeArray<Quaternion> Rotations;

        [ReadOnly] public NativeArray<float> Speeds;
        [ReadOnly] public NativeArray<float> AngularSpeeds;

        public MoveDanmaku(DanmakuPool pool)
        {
            DeltaTime = Time.deltaTime;
            Positions = pool.Positions;
            Rotations = pool.Rotations;
            Speeds = pool.Speeds;
            AngularSpeeds = pool.AngularSpeeds;
        }

        public unsafe void Execute(int start, int end)
        {
            var positionPtr = (Vector3*)Positions.GetUnsafePtr() + start;
            var rotationPtr = (float*)Rotations.GetUnsafePtr() + start;
            var speedPtr = (float*)Speeds.GetUnsafeReadOnlyPtr() + start;
            var angularSpeedPtr = (float*)AngularSpeeds.GetUnsafeReadOnlyPtr() + start;
            var rotationEnd = rotationPtr + (end - start);

            while (rotationPtr < rotationEnd)
            {
                var speed = *speedPtr++;
                var yawRotation = *rotationPtr + *angularSpeedPtr++ * DeltaTime;
                //var pitchRotation = *pitchRotationPtr + *angularSpeedPtr++ * DeltaTime;
                *rotationPtr = yawRotation;
                //*pitchRotationPtr = pitchRotation;
                //Debug.LogErrorFormat("YAW: {0}, PITCH: {1}", yawRotation, pitchRotation);
                positionPtr->x += speed * Mathf.Cos(yawRotation) * DeltaTime;
                positionPtr->y += speed * Mathf.Sin(yawRotation) * DeltaTime;
                //positionPtr->x += speed * Mathf.Cos(yawRotation) * Mathf.Cos(pitchRotation) * DeltaTime;
                //positionPtr->y += speed * Mathf.Sin(yawRotation) * Mathf.Cos(pitchRotation) * DeltaTime;
                //positionPtr->z += speed * Mathf.Sin(pitchRotation) * DeltaTime;
                rotationPtr++;
                //pitchRotationPtr++;
                positionPtr++;
            }
        }
    }
}