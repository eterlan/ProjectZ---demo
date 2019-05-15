using Unity.Entities;
using UnityEngine;

public struct NavigationTag : IComponentData
{
    public bool Arrived;
}

public class NavigationTagProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new NavigationTag {Arrived = false};
        manager.AddComponentData(entity, data);
    }
}