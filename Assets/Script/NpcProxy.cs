using Unity.Entities;
using UnityEngine;

public struct Npc : IComponentData
{
}

public class NpcProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new Npc();
        manager.AddComponentData(entity, data);
    }
}