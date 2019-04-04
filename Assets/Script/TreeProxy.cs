using Unity.Entities;
using UnityEngine;

public struct Tree : IComponentData
{

}
public class TreeProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new Tree { };
        manager.AddComponentData(entity, data);
    }
}