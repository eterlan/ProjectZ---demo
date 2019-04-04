using Unity.Entities;
using UnityEngine;

public struct NpcTag : IComponentData
{

}
public class NpcTagProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new NpcTag { };
        manager.AddComponentData(entity, data);
    }
}