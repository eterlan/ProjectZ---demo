using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace ProjectZ.AI.PathFinding
{
    [InternalBufferCapacity(10)]
    public struct PathPlanner : IBufferElementData
    {
        public int2 NextPosition;
    }

    [RequireComponent(typeof(ConvertToEntity))]
    [RequiresEntityConversion]
    public class PathPlannerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert
        (Entity                     entity,
         EntityManager              manager,
         GameObjectConversionSystem conversionSystem)
        {
            manager.AddBuffer<PathPlanner>(entity);
        }
    }
}