using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace ProjectZ.AI.PathFinding
{
    public struct FinderSpawner : IComponentData
    {
        public int Count;
        public int Radius;
        public Entity Finder;
    }

    [RequireComponent(typeof(ConvertToEntity))]
    [RequiresEntityConversion]
    public class FinderSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public int Count;
        public int Radius;
        public GameObject Finder;

        public void Convert
        (Entity                     entity,
         EntityManager              manager,
         GameObjectConversionSystem conversionSystem)
        {
            var data = new FinderSpawner
            {
                Count = Count,
                Radius = Radius,
                Finder = conversionSystem.GetPrimaryEntity(Finder),
            };
            manager.AddComponentData(entity, data);
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Finder);
        }
    }
}