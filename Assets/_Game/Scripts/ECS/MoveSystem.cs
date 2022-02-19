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
        float dt = Time.DeltaTime;
        Entities.WithAll<MoveData>().ForEach((ref Translation translation, ref MoveData move, ref Rotation rotation) =>
        {
            float m_speed = move.Speed + (move.AccelSpeed * dt);

            if (move.UseMaxSpeed && m_speed > move.MaxSpeed)
            {
                m_speed = move.MaxSpeed;
            }

            if (move.UseMinSpeed && m_speed < move.MinSpeed)
            {
                m_speed = move.MinSpeed;
            }

            float3 forward = math.mul(rotation.Value, new float3(0f, 0f, 1f));

            translation.Value += (forward * (m_speed * dt));
        }).ScheduleParallel();
    }
}
