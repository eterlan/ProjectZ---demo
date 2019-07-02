using Unity.Entities;
using UnityEngine;


namespace ProjectZ.AI.PathFinding
{
    public struct FinderSpawner : IComponentData
    {
        public int Count;
        public int Radius;
    }

    [RequireComponent(typeof(ConvertToEntity))]
    [RequiresEntityConversion]
    public class FinderSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert
        (Entity                     entity,
         EntityManager              manager,
         GameObjectConversionSystem conversionSystem)
        {
            var data = new FinderSpawner();
            manager.AddComponentData(entity, data);
        }
    }
}