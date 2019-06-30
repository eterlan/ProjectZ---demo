using Unity.Entities;
using UnityEngine;

namespace ProjectZ.AI.PathFinding
{
    public struct MoveRotSpeed : IComponentData
    {
        public float RotSpeed;
        public float MaxRotSpeed;
        public float LerpSpeed;
    }

    [RequireComponent(typeof(ConvertToEntity))]
    [RequiresEntityConversion]
    public class MoveRotSpeedAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float MaxRotSpeed;        
        public float LerpSpeed;

        public void Convert
        (Entity                     entity,
         EntityManager              manager,
         GameObjectConversionSystem conversionSystem)
        {
            var data = new MoveRotSpeed
            {
                MaxRotSpeed = MaxRotSpeed,
                LerpSpeed =   LerpSpeed,
            };

            manager.AddComponentData(entity, data);
        }
    }
}