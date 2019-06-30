using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace ProjectZ.AI.PathFinding
{
    public struct NavigateTarget : IComponentData
    {
        public float3 Position;
        public int Count;
    }

    [RequireComponent(typeof(ConvertToEntity))]
    [RequiresEntityConversion]
    public class NavigateTargetAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float3 TargetPosition; 
        public void Convert
        (Entity                     entity,
         EntityManager              manager,
         GameObjectConversionSystem conversionSystem)
        {
            //@Todo, Jst for test.
            var data = new NavigateTarget
            {
                Position = TargetPosition,
            };

            manager.AddComponentData(entity, data);
        }
    }
}