using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

public struct IndexOnUi : IComponentData
{
    public int Value;
}

[RequiresEntityConversion]
public class IndexOnUiProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [FormerlySerializedAs("Value")] public int value;

    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new IndexOnUi
        {
            Value = value
        };
        manager.AddComponentData(entity, data);
    }
}