using Unity.Entities;
using UnityEngine;

public struct Player : IComponentData
{

}

//
public class PlayerProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new Player();
        manager.AddComponentData(entity, data);
    }
}