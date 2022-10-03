using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Unity.Burst;

namespace DanmakU
{
    internal struct MoveDanmaku : IJobBatchedFor
    {
        public float DeltaTime;

        public NativeArray<Vector3> Positions;
        public NativeArray<float> Yaws;
        public NativeArray<float> Pitches;
        [ReadOnly] public NativeArray<float> Speeds;
        [ReadOnly] public NativeArray<float> AngularSpeeds;

        public MoveDanmaku(DanmakuPool pool)
        {
            DeltaTime = Time.deltaTime;
            Positions = pool.Positions;
            Yaws = pool.Yaws;
            Pitches = pool.Pitches;
            Speeds = pool.Speeds;
            AngularSpeeds = pool.AngularSpeeds;
        }

        public unsafe void Execute(int start, int end)
        {
            var positionPtr = (Vector3*)Positions.GetUnsafePtr() + start;
            var yawRotationPtr = (float*)Yaws.GetUnsafePtr() + start;
            var pitchRotationPtr = (float*)Pitches.GetUnsafePtr() + start;
            var speedPtr = (float*)Speeds.GetUnsafeReadOnlyPtr() + start;
            var angularSpeedPtr = (float*)AngularSpeeds.GetUnsafeReadOnlyPtr() + start;
            var yawRotationEnd = yawRotationPtr + (end - start);
            while (yawRotationPtr < yawRotationEnd)
            {
                var speed = *speedPtr++;
                var yawRotation = *yawRotationPtr + *angularSpeedPtr++ * DeltaTime;
                var pitchRotation = *pitchRotationPtr + *angularSpeedPtr++ * DeltaTime;
                *yawRotationPtr = yawRotation;
                *pitchRotationPtr = pitchRotation;
                positionPtr->x += speed * Mathf.Cos(yawRotation) * DeltaTime;
                positionPtr->y += speed * Mathf.Sin(yawRotation) * DeltaTime;
                //positionPtr->x += speed * Mathf.Cos(yawRotation) * Mathf.Cos(pitchRotation) * DeltaTime;
                //positionPtr->y += speed * Mathf.Sin(yawRotation) * Mathf.Cos(pitchRotation) * DeltaTime;
                //positionPtr->z += speed * Mathf.Sin(pitchRotation) * DeltaTime;
                yawRotationPtr++;
                positionPtr++;
            }
        }

    }

}