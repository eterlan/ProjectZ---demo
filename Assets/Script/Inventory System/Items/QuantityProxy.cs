using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

public struct Quantity : IComponentData
{
    public int Value;
}

[RequiresEntityConversion]
public class QuantityProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [FormerlySerializedAs("Value")] public int value;

    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new Quantity
        {
            Value = value
        };
        manager.AddComponentData(entity, data);
    }
}