using Unity.Entities;
using UnityEngine;

public struct Quantity : IComponentData
{
    public int Value;
}

[RequiresEntityConversion]
public class QuantityProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public int Value;

    public void Convert( Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new Quantity
        {
            Value = Value,
        };
        manager.AddComponentData(entity, data);
    }
}