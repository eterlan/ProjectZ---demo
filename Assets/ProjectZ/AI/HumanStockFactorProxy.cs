using ProjectZ.Component.Setting;
using Unity.Entities;
using UnityEngine;

namespace ProjectZ.AI
{
    public struct HumanStock : IComponentData
    {
        public int Food;
        public int Water;
    }

    public class HumanStockFactorProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Range(0, Setting.MaxFood)] public int food;

        [Range(0, Setting.MaxWater)] public int water;

        public void Convert(Entity e, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            var data = new HumanStock
            {
                Food  = food,
                Water = water
            };
            manager.AddComponentData(e, data);
        }
    }
}