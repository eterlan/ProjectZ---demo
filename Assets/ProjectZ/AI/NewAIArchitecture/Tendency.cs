using Unity.Entities;


namespace ProjectZ.AI
{
    [InternalBufferCapacity(10)]
    public struct Tendency : IBufferElementData
    {
        public float Value;
    }
}