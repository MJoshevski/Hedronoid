using Unity.Entities;
using UnityEngine;

// ReSharper disable once InconsistentNaming
[AddComponentMenu("DOTS Samples/HybridComponent/Lifetime")]
[ConverterVersion("joe", 1)]
public class LifeTimeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float timeRemainingInSeconds = 1200000;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new LifeTime { timeRemainingInSeconds = timeRemainingInSeconds });
    }
}
