using System.Collections.Generic;
using System.Linq;

namespace Tests.Infrastructure
{
    public class HeartContainerBuilder : TestDataBuilder<HeartContainer>
    {
        private List<Heart> m_hearts;

        public HeartContainerBuilder(List<Heart> hearts)
        {
            m_hearts = hearts;
        }

        public HeartContainerBuilder() : this(new List<Heart> {A.Heart()}){}

        public HeartContainerBuilder With(params Heart[] hearts)
        {
            m_hearts = hearts.ToList();
            return this;
        }
        public override HeartContainer Build() =>new HeartContainer(m_hearts);
    }
}