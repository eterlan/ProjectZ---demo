using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

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
    public float      appleRadius;
    public int        countPerTree;
    public GameObject prefab;
    public float      spawnRadius;

    public void Convert(
        Entity                     entity,
        EntityManager              manager,
        GameObjectConversionSystem conversionSystem)
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

    public void DeclareReferencedPrefabs(List<GameObject> gameObjects) { gameObjects.Add(prefab); }
}