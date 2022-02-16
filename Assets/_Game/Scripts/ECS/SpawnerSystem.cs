using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

// ReSharper disable once InconsistentNaming
public struct SpawnerComponentData : IComponentData
{
    public Entity Prefab;
    public float3 InitRotation_Euler;
    public quaternion InitRotation_Quaternion;
    public float3 InitPosition;
    public float Lifetime;
    public float3 Direction;
    public float3 SpawnPos;
    public float Speed;
    public float AngleHorizontal;
    public float AngleVertical;
    public float AccelSpeed;
    public float AccelTurn;
    public bool Homing;
    public float3 HomingTarget;
    public float HomingAngleSpeed;
    public bool SinWave;
    public float SinWaveSpeed;
    public float SinWaveRangeSize;
    public bool SinWaveInverse;
    public bool UseMaxSpeed;
    public float MaxSpeed;
    public bool UseMinSpeed;
    public float MinSpeed;
}


// Systems can schedule work to run on worker threads.
// However, creating and removing Entities can only be done on the main thread to prevent race conditions.
// The system uses an EntityCommandBuffer to defer tasks that can't be done inside the Job.

// ReSharper disable once InconsistentNaming
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class SpawnerSystem : SystemBase
{
    // BeginInitializationEntityCommandBufferSystem is used to create a command buffer which will then be played back
    // when that barrier system executes.
    //
    // Though the instantiation command is recorded in the SpawnJob, it's not actually processed (or "played back")
    // until the corresponding EntityCommandBufferSystem is updated. To ensure that the transform system has a chance
    // to run on the newly-spawned entities before they're rendered for the first time, the SpawnerSystem_FromEntity
    // will use the BeginSimulationEntityCommandBufferSystem to play back its commands. This introduces a one-frame lag
    // between recording the commands and instantiating the entities, but in practice this is usually not noticeable.
    //
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        // Instead of performing structural changes directly, a Job can add a command to an EntityCommandBuffer to
        // perform such changes on the main thread after the Job has finished. Command buffers allow you to perform
        // any, potentially costly, calculations on a worker thread, while queuing up the actual insertions and
        // deletions for later.
        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        // Schedule the job that will add Instantiate commands to the EntityCommandBuffer.
        // Since this job only runs on the first frame, we want to ensure Burst compiles it before running to get the best performance (3rd parameter of WithBurst)
        // The actual job will be cached once it is compiled (it will only get Burst compiled once).

        Entities
            .WithName("SpawnerSystem")
            .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
            .ForEach((Entity entity, int entityInQueryIndex, in SpawnerComponentData spawner, in LocalToWorld location) =>
            {
                var instance = commandBuffer.Instantiate(entityInQueryIndex, spawner.Prefab);

                commandBuffer.SetComponent(entityInQueryIndex, instance, new Translation { Value = spawner.SpawnPos });
                
                commandBuffer.SetComponent(entityInQueryIndex, instance, new MoveData { 
                    Direction = spawner.Direction, 
                    Speed = spawner.Speed, 
                    MaxSpeed = spawner.MaxSpeed, 
                    MinSpeed = spawner.MinSpeed, 
                    AccelSpeed = spawner.AccelSpeed, 
                    UseMaxSpeed = spawner.UseMaxSpeed, 
                    UseMinSpeed = spawner.UseMinSpeed 
                });

                commandBuffer.SetComponent(entityInQueryIndex, instance, new LifeTime { timeRemainingInSeconds = spawner.Lifetime });
                
                commandBuffer.SetComponent(entityInQueryIndex, instance, new RotationData { 
                    AccelTurn = spawner.AccelTurn, 
                    AngleHorizontal = spawner.AngleHorizontal, 
                    AngleVertical = spawner.AngleVertical, 
                    Homing = spawner.Homing,
                    HomingAngleSpeed = spawner.HomingAngleSpeed,
                    HomingTarget = spawner.HomingTarget,
                    InitPosition = spawner.InitPosition,
                    InitRotation_Euler = spawner.InitRotation_Euler,
                    InitRotation_Quaternion = spawner.InitRotation_Quaternion,
                    SinWave = spawner.SinWave,
                    SinWaveInverse = spawner.SinWaveInverse,
                    SinWaveRangeSize = spawner.SinWaveRangeSize,
                    SinWaveSpeed = spawner.SinWaveSpeed
                });


                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }).ScheduleParallel();

        // SpawnJob runs in parallel with no sync point until the barrier system executes.
        // When the barrier system executes we want to complete the SpawnJob and then play back the commands
        // (Creating the entities and placing them). We need to tell the barrier system which job it needs to
        // complete before it can play back the commands.
        m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
