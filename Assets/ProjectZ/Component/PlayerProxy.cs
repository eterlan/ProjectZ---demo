using Unity.Entities;
using UnityEngine;

namespace ProjectZ.Component
{
    public struct Player : IComponentData
    {
    }

//
    public class PlayerProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            var data = new Player();
            manager.AddComponentData(entity, data);
            // manager.AddBuffer<Items>(entity);
            // cannot be singleton because the component in entity is not singleton. It's same with NPC
        }
    }
}