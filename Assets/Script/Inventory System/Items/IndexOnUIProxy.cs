using Unity.Entities;
using UnityEngine;

public struct IndexOnUI : IComponentData
{
    public int Value;
}

[RequiresEntityConversion]
public class IndexOnUIProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public int Value;

    public void Convert( Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new IndexOnUI
        {
            Value = Value,
        };
        manager.AddComponentData(entity, data);
    }
}