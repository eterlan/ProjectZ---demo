using Unity.Entities;
using UnityEngine;

public struct House : IComponentData
{
}

public class HouseProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity e, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new House();
        manager.AddComponentData(e, data);
    }
}