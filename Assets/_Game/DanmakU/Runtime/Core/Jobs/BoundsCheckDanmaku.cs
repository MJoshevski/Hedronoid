using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DanmakU
{
    internal struct BoundsCheckDanmaku : IJobBatchedFor
    {

        public float DeltaTime;
        [ReadOnly] public NativeArray<Vector3> Positions;
        public NativeArray<float> Times;

        Vector3 min;
        Vector3 max;

        public BoundsCheckDanmaku(DanmakuPool pool)
        {
            DeltaTime = Time.deltaTime;
            Times = pool.Times;
            Positions = pool.Positions;

            var bounds = DanmakuManager.Instance.Bounds;
            min = bounds.min;
            max = bounds.max;
        }

        public unsafe void Execute(int start, int end)
        {
            var positionPtr = (Vector3*)Positions.GetUnsafeReadOnlyPtr() + start;
            var timePtr = (float*)Times.GetUnsafePtr() + start;
            var positionEnd = positionPtr + (end - start);
            while (positionPtr < positionEnd)
            {
                var x = positionPtr->x;
                var y = positionPtr->y;
                if (x >= min.x && x <= max.x && y >= min.y && y <= max.y)
                {
                    *timePtr++ += DeltaTime;
                }
                else
                {
                    *timePtr++ = float.MinValue;
                }
                positionPtr++;
            }
        }

    }
}