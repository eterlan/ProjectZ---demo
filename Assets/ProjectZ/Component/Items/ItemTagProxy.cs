using Unity.Entities;
using UnityEngine;

namespace ProjectZ.Component.Items
{
    public struct ItemTag : IComponentData
    {
    }

    [RequiresEntityConversion]
    public class ItemTagProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            var data = new ItemTag();
            manager.AddComponentData(entity, data);
        }
    }
}