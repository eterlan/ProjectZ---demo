using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
public struct AppleSpawner : IComponentData
{
    public Entity Prefab;
    public int CountPerTree;
    public float SpawnRadius;
    public float AppleRadius;
}
[RequiresEntityConversion]
public class AppleSpawnerProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject Prefab;
    public int CountPerTree;
    public float SpawnRadius;
    public float AppleRadius;

    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(Prefab);
    }
    public void Convert( Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new AppleSpawner
        {
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            CountPerTree = CountPerTree,
            SpawnRadius = SpawnRadius,
            AppleRadius = AppleRadius,
        };
        manager.AddComponentData(entity, data);
    }
}