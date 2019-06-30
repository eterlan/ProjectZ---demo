using Unity.Entities;
using UnityEngine;

namespace ProjectZ.AI.PathFinding
{
    public struct PathFindingRequest : IComponentData
    {
        public int StartIndex;
        public int EndIndex;
    }

    [RequireComponent(typeof(ConvertToEntity))]
    [RequiresEntityConversion]
    public class PathFindingRequestAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int StartIndex;
        public int EndIndex;
        public void Convert
        (Entity                     entity,
         EntityManager              manager,
         GameObjectConversionSystem conversionSystem)
        {
            var data = new PathFindingRequest
            {
                StartIndex = StartIndex,
                EndIndex = EndIndex,
            };

            manager.AddComponentData(entity, data);
        }
    }
}