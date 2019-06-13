using Unity.Entities;
using UnityEngine;

namespace ProjectZ.AI
{
    public struct Navigation : IComponentData
    {
        public bool Arrived;
    }

    public class NavigationTagProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            var data = new Navigation {Arrived = false};
            manager.AddComponentData(entity, data);
        }
    }
}