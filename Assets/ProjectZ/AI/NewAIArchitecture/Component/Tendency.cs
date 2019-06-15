using Unity.Entities;
using Unity.Mathematics;

namespace ProjectZ.AI
{
    [InternalBufferCapacity(10)]
    public struct Tendency : IBufferElementData
    {
        private float m_value;

        public float Value
        {
            get => m_value;
            set => m_value = math.clamp(value, 0, 1);
        }
    }
}