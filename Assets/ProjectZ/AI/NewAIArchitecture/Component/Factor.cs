using Unity.Entities;

namespace ProjectZ.AI
{
    [InternalBufferCapacity(10)]
    public struct Factor : IBufferElementData
    {
        public int Value;
    }
}