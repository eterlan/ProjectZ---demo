using Unity.Entities;

namespace ProjectZ.Component.Items
{
    [InternalBufferCapacity(10)]
    public struct Items : ISystemStateBufferElementData
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
}