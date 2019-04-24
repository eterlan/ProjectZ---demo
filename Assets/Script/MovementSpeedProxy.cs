using System;
using Unity.Entities;
using UnityEngine;

public struct MovementSpeed : IComponentData
{
    public float Speed;
}

// Why use this attribute? Seems doesn't necessary/

[RequiresEntityConversion]
// Why it need to derive from monobehavior? 
// else it can't be added to inspector
public class MovementSpeedProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public float MetersPerSecond;
    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var MovementSpeed = new MovementSpeed { Speed = MetersPerSecond };
        manager.AddComponentData(entity, MovementSpeed);
    }
}