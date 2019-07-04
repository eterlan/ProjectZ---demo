using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectZ.AI.PathFinding
{
    public struct PathFindingRequest : IComponentData
    {
        public float3 StartPosition;
        public float3 EndPosition;
    }

    [RequireComponent(typeof(ConvertToEntity))]
    [RequiresEntityConversion]
    public class PathFindingRequestAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float3 StartIndex;
        public float3 EndIndex;
        public void Convert
        (Entity                     entity,
         EntityManager              manager,
         GameObjectConversionSystem conversionSystem)
        {
            var data = new PathFindingRequest
            {
                StartPosition = StartIndex,
                EndPosition = EndIndex,
            };

            manager.AddComponentData(entity, data);
        }
    }
}