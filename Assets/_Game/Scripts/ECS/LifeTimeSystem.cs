using Hedronoid.Core;
using Hedronoid.Player;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public struct LifeTime : IComponentData
{
    public float timeRemainingInSeconds;
}

// This system updates all entities in the scene with both a RotationSpeed_SpawnAndRemove and Rotation component.
public partial class LifeTimeSystem : SystemBase
{
    EntityCommandBufferSystem m_Barrier;
    protected override void OnCreate()
    {
        m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    // OnUpdate runs on the main thread.
    protected override void OnUpdate()
    {
        var commandBuffer = m_Barrier.CreateCommandBuffer().AsParallelWriter();

        var deltaTime = Time.DeltaTime;
        Entities.ForEach((Entity entity, int nativeThreadIndex, ref LifeTime lifetime) =>
        {
            lifetime.timeRemainingInSeconds -= deltaTime;

            if (lifetime.timeRemainingInSeconds < 0.0f)
            {
                commandBuffer.DestroyEntity(nativeThreadIndex, entity);
            }
        }).ScheduleParallel();

        m_Barrier.AddJobHandleForProducer(Dependency);
    }
}
