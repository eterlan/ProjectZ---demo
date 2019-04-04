using Unity.Entities;
using UnityEngine;

public struct Timers : IComponentData
{
    public Timer timer;
}

public class TimersProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert ( Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new Timers{};
        manager.AddComponentData(entity, data);
    }
}