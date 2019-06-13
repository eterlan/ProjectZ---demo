using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectZ.Component
{
    public struct PlayerInput : IComponentData
    {
        public bool LeftMouse;
        public bool RightMouse;
    }

    public class PlayerInputProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity e, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            var data = new PlayerInput();
            manager.AddComponentData(e, data);
        }
    }
}