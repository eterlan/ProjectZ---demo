using Unity.Entities;
using UnityEngine;

namespace ProjectZ.AI.PathFinding
{
    public struct NodeInfo : IComponentData
    {
        public int Index;
        public bool Walkable;
    }

    [RequireComponent(typeof(ConvertToEntity))]
    [RequiresEntityConversion]
    public class NodeInfoAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert
        (Entity                     entity,
         EntityManager              manager,
         GameObjectConversionSystem conversionSystem)
        {
            var data = new NodeInfo();
            manager.AddComponentData(entity, data);
        }
    }
}