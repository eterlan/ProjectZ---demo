using Unity.Entities;
using Unity.Mathematics;

namespace ProjectZ.AI
{
    [InternalBufferCapacity(10)]
    public struct Resistance : IBufferElementData
    {
        private float m_value;

        // @Todo Before further implement, it's value should be 0.
        public float Value { get => m_value; set => m_value = math.clamp(value, 0, 1); }
    }
}