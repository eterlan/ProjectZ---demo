using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectZ.AI
{
    public struct MovementSpeed : IComponentData
    {
        public float Speed;
    }

// Why use this attribute? Seems doesn't necessary/

    [RequiresEntityConversion]
// Why it need to derive from monobehavior? 
// else it can't be added to inspector
    public class MovementSpeedProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [FormerlySerializedAs("MetersPerSecond")] public float metersPerSecond;

        public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            var movementSpeed = new MovementSpeed {Speed = metersPerSecond};
            manager.AddComponentData(entity, movementSpeed);
        }
    }
}