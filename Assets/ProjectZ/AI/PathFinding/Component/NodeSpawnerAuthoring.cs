using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectZ.AI.PathFinding
{
    public struct NodeSpawner : ISystemStateComponentData
    {
        public int2 Count;
        public float3 Position;

        public Entity Normal;
        public Entity Obstacle;
        public int Space;
    }

    [RequireComponent(typeof(ConvertToEntity))]
    [RequiresEntityConversion]
    public class NodeSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject Normal;
        public GameObject Obstacle;
        public int2       Count;
        public int Space;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Normal);
            referencedPrefabs.Add(Obstacle);
        }

        public void Convert
        (Entity                     entity,
         EntityManager              manager,
         GameObjectConversionSystem conversionSystem)
        {
            var data = new NodeSpawner
            {
                Count    = Count,
                Normal   = conversionSystem.GetPrimaryEntity(Normal),
                Obstacle = conversionSystem.GetPrimaryEntity(Obstacle),
                Space =  Space,
            };

            manager.AddComponentData(entity, data);
            var query = manager.CreateEntityQuery(typeof(NodeSpawner));

            query.SetSingleton(data);
        }
    }
}