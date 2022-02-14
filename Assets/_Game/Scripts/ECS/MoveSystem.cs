using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using System;

[Serializable]
public struct MoveData : IComponentData
{
    public float3 Direction;
    public float Speed;
    public float MaxSpeed;
    public float MinSpeed;
    public float AccelSpeed;
    public bool UseMaxSpeed;
    public bool UseMinSpeed;

}
public partial class MoveSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<MoveData>().ForEach((ref Translation translation, ref MoveData move) =>
        {
            float m_speed = move.Speed + (move.AccelSpeed * Time.DeltaTime);

            if (move.UseMaxSpeed && m_speed > move.MaxSpeed)
            {
                m_speed = move.MaxSpeed;
            }

            if (move.UseMinSpeed && m_speed < move.MinSpeed)
            {
                m_speed = move.MinSpeed;
            }


            translation.Value += (move.Direction * (m_speed * Time.DeltaTime));
        }).ScheduleParallel();
    }
}
