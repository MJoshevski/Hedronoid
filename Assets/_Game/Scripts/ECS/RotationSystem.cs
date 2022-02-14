using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using System;
using System.Runtime.CompilerServices;


// ReSharper disable once InconsistentNaming
[Serializable]
public struct RotationData : IComponentData
{
    public float3 InitRotation_Euler;
    public quaternion InitRotation_Quaternion;
    public float3 InitPosition;
    public float AngleHorizontal;
    public float AngleVertical;
    public float AccelTurn;
    public bool Homing;
    public float3 HomingTarget;
    public float HomingAngleSpeed;
    public bool SinWave;
    public float SinWaveSpeed;
    public float SinWaveRangeSize;
    public bool SinWaveInverse;

}
// This system updates all entities in the scene with both a RotationSystem and Rotation component.

// ReSharper disable once InconsistentNaming
public partial class RotationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        throw new NotImplementedException();
    }

    //// OnUpdate runs on the main thread.
    //protected override void OnUpdate()
    //{
    //    var deltaTime = Time.DeltaTime;

    //    // The in keyword on the RotationSystem component tells the job scheduler that this job will not write to rotation
    //    Entities
    //        .WithName("RotationSystem")
    //        .ForEach((ref Rotation rotation, in RotationData rotationData) =>
    //        {
    //            float3 myAngles = rotationData.InitRotation_Euler;

    //            quaternion newRotation = rotationData.InitRotation_Quaternion;

    //            if (rotationData.Homing)
    //            {
    //                // homing target.
    //                if (0f < rotationData.HomingAngleSpeed)
    //                {
    //                    var rotation = quaternion.LookRotation(rotationData.HomingTarget - rotationData.InitPosition);

    //                    var toRotation = RotateTowards(rotationData.InitRotation_Quaternion, rotation, Time.DeltaTime * rotationData.HomingAngleSpeed);

    //                    newRotation = toRotation;
    //                }
    //            }
    //            else if (rotationData.SinWave)
    //            {
    //                //// acceleration turning.
    //                ///
    //                var AngleHorizontal = rotationData.AngleHorizontal;
    //                var AngleVertical = rotationData.AngleVertical;

    //                AngleHorizontal += (rotationData.AccelTurn * deltaTime);
    //                AngleVertical += (rotationData.AccelTurn * deltaTime);

    //                // sin wave.
    //                if (0f < rotationData.SinWaveSpeed && 0f < rotationData.SinWaveRangeSize)
    //                {
    //                    float waveAngleXZ = AngleHorizontal + (rotationData.SinWaveRangeSize / 2f * (Mathf.Sin(m_selfFrameCnt * m_sinWaveSpeed / 100f) * (m_sinWaveInverse ? -1f : 1f)));

    //                    newRotation = quaternion.Euler(
    //                        m_baseAngles.x + m_angleVertical, m_baseAngles.y + waveAngleXZ, myAngles.z);

    //                }
    //                m_selfFrameCnt += UbhTimer.instance.deltaFrameCount;
    //            }
    //            else
    //            {
    //                // acceleration turning.
    //                float addAngle = rotationData.AccelTurn * deltaTime;

    //                newRotation = quaternion.Euler(
    //                    myAngles.x, myAngles.y - addAngle, myAngles.z + addAngle);
    //            }


    //            // Rotate something about its up vector at the speed given by RotationSpeed_SpawnAndRemove.
    //            //rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), rotSpeedSpawnAndRemove.RadiansPerSecond * deltaTime));
    //        }).
    //        ScheduleParallel();
    //}

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static quaternion RotateTowards(quaternion from, quaternion to, float maxDegreesDelta)
    //{
    //    float num = Angle(from, to);
    //    return num < float.Epsilon ? to : math.slerp(from, to, math.min(1f, maxDegreesDelta / num));
    //}

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static float Angle(this quaternion q1, quaternion q2)
    //{
    //    var dot = math.dot(q1, q2);
    //    return !(dot > 0.999998986721039) ? (float)(math.acos(math.min(math.abs(dot), 1f)) * 2.0) : 0.0f;
    //}
}
