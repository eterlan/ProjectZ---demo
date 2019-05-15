using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

public struct HumanStockFactor : IComponentData
{
    public int Food;
    public int Water;
}

public class HumanStockFactorProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [FormerlySerializedAs("Food")] [Range(0, ConstValue.MaxFood)] public int food;

    [FormerlySerializedAs("Water")] [Range(0, ConstValue.MaxWater)] public int water;

    public void Convert(Entity e, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new HumanStockFactor
        {
            Food  = food,
            Water = water
        };
        manager.AddComponentData(e, data);
    }
}