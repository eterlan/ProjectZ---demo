using Unity.Entities;
using UnityEngine;

public struct PlayerTag : IComponentData
{

}

//
public class PlayerTagProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new PlayerTag { };
        manager.AddComponentData(entity, data);
    }
}