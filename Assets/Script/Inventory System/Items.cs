using UnityEngine;
using Unity.Entities;

[InternalBufferCapacity(10)]
public struct Item : ISystemStateBufferElementData
{
    public Entity Value;
}

public struct Owner : IComponentData
{
    public Entity Value;
}

public struct PreviousOwner : ISystemStateComponentData
{
    public Entity Value;
}