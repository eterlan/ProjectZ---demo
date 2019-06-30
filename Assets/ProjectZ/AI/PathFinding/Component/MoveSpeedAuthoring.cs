using Unity.Entities;
using UnityEngine;

namespace ProjectZ.AI.PathFinding
{
    public struct MoveSpeed : IComponentData
    {
        public float Speed;
        public float MaximumSpeed;
        public float LerpSpeed;
    }

    [RequireComponent(typeof(ConvertToEntity))]
    [RequiresEntityConversion]
    public class MoveSpeedAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float MaximumSpeed;
        public float LerpSpeed;

        public void Convert
        (Entity                     entity,
         EntityManager              manager,
         GameObjectConversionSystem conversionSystem)
        {
            var data = new MoveSpeed
            {
                MaximumSpeed = MaximumSpeed,
                LerpSpeed    = LerpSpeed,
            };

            manager.AddComponentData(entity, data);
        }
    }
}