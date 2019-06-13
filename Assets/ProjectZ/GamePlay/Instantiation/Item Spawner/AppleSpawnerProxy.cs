using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

public struct AppleSpawner : IComponentData
{
    public int    Count { get; set; }
    public Entity Prefab;
    public int    CountPerTree;
    public float  SpawnRadius;
    public float  AppleRadius;
}

[RequiresEntityConversion]
public class AppleSpawnerProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    [FormerlySerializedAs("AppleRadius")] public float      appleRadius;
    [FormerlySerializedAs("CountPerTree")] public int        countPerTree;
    [FormerlySerializedAs("Prefab")] public GameObject prefab;
    [FormerlySerializedAs("SpawnRadius")] public float      spawnRadius;

    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new AppleSpawner
        {
            Prefab       = conversionSystem.GetPrimaryEntity(prefab),
            CountPerTree = countPerTree,
            SpawnRadius  = spawnRadius,
            AppleRadius  = appleRadius
        };
        manager.AddComponentData(entity, data);
    }

    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(prefab);
    }
}