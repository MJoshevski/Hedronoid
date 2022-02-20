using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

// ReSharper disable once InconsistentNaming
[AddComponentMenu("DOTS Samples/SpawnAndRemove/Spawner")]
[ConverterVersion("joe", 1)] 
public class SpawnerAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject Prefab;
    public Vector3 InitRotation_Euler;
    public Quaternion InitRotation_Quaternion;
    public Vector3 InitPosition;
    public float Lifetime;
    public Vector3 SpawnPos;
    public float Speed;
    public float AngleHorizontal;
    public float AngleVertical;
    public float AccelSpeed;
    public float AccelTurn;
    public bool Homing;
    public Vector3 HomingTarget;
    public float HomingAngleSpeed;
    public bool SinWave;
    public float SinWaveSpeed;
    public float SinWaveRangeSize;
    public bool SinWaveInverse;
    public bool UseMaxSpeed;
    public float MaxSpeed;
    public bool UseMinSpeed;
    public float MinSpeed;

    // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }

    // Lets you convert the editor data representation to the entity optimal runtime representation

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new SpawnerComponentData
        {
            // The referenced prefab will be converted due to DeclareReferencedPrefabs.
            // So here we simply map the game object to an entity reference to that prefab.
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            SinWaveSpeed = SinWaveSpeed,
            SinWaveRangeSize = SinWaveRangeSize,
            SinWaveInverse = SinWaveInverse,
            SinWave = SinWave,
            InitRotation_Quaternion = InitRotation_Quaternion,
            InitRotation_Euler = InitRotation_Euler,
            InitPosition = InitPosition,
            HomingTarget = HomingTarget,
            HomingAngleSpeed = HomingAngleSpeed,
            AccelSpeed = AccelSpeed,
            AccelTurn = AccelTurn,
            AngleHorizontal = AngleHorizontal,
            AngleVertical = AngleVertical,
            Homing = Homing,
            Lifetime = Lifetime,
            MaxSpeed = MaxSpeed,
            MinSpeed = MinSpeed,
            SpawnPos = SpawnPos,
            Speed = Speed,
            UseMaxSpeed = UseMaxSpeed,
            UseMinSpeed = UseMinSpeed
        };
        dstManager.AddComponentData(entity, spawnerData);
    }
}
