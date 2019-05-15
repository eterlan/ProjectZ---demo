using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

public struct TreeSpawner : IComponentData
{
    public Entity Prefab;
    public int    Count;
    public float  SpawnRadius;
    public float  ObjectRadius;
}

[RequiresEntityConversion]
public class TreeSpawnerProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    [FormerlySerializedAs("Count")] public int        count;
    [FormerlySerializedAs("ObjectRadius")] public float      objectRadius;
    [FormerlySerializedAs("Prefab")] public GameObject prefab;
    [FormerlySerializedAs("SpawnRadius")] public float      spawnRadius;

    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new TreeSpawner
        {
            Prefab       = conversionSystem.GetPrimaryEntity(prefab),
            Count        = count,
            SpawnRadius  = spawnRadius,
            ObjectRadius = objectRadius
        };
        manager.AddComponentData(entity, data);
    }

    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(prefab);
    }
}