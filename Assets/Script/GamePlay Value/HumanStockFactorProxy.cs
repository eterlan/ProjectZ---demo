using UnityEngine;
using Unity.Entities;

public struct HumanStockFactor : IComponentData
{
    public int Food;
    public int Water;
}
public class HumanStockFactorProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [Range(0,ConstValue.MaxFood)]
    public int Food;
    [Range(0,ConstValue.MaxWater)]
    public int Water;
    public void Convert(Entity e, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new HumanStockFactor
        {
            Food  = Food,
            Water = Water,
        };
        manager.AddComponentData(e, data);
    }
}