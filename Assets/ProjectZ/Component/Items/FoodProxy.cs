using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectZ.Component.Items
{
    public struct Food : ISystemStateSharedComponentData
    {
        public int      Restoration;
        public FoodType FoodType;
        public int      SpValue;
        public int      MaxInCell;
    }

    [RequiresEntityConversion]
    public class FoodProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public FoodType foodType;
        public int      maxInCell;
        public int      restoration;
        public int      spValue;

        public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            var data = new Food
            {
                Restoration = restoration,
                FoodType    = foodType,
                SpValue     = spValue,
                MaxInCell   = maxInCell
            };
            manager.AddSharedComponentData(entity, data);
        }
    }
}