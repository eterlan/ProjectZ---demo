using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct PlayerInput : IComponentData
{
    public float3 Value;
}

public class PlayerInputProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity e, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new PlayerInput();
        manager.AddComponentData(e, data);
    }
}

