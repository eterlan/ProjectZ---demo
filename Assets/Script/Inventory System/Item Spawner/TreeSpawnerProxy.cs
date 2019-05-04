using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
public struct TreeSpawner : IComponentData
{
    public Entity Prefab;
    public int Count;
    public float SpawnRadius;
    public float ObjectRadius;
}

[RequiresEntityConversion]
public class TreeSpawnerProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject Prefab;
    public int Count;
    public float SpawnRadius;
    public float ObjectRadius;

    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(Prefab);
    }
    public void Convert( Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new TreeSpawner
        {
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            Count = Count,
            SpawnRadius = SpawnRadius,
            ObjectRadius = ObjectRadius,
        };
        manager.AddComponentData(entity, data);
    }
}