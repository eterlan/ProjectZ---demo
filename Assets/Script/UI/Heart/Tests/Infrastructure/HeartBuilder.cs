using UnityEngine;
using UnityEngine.UI;

namespace Tests.Infrastructure
{
    public class HeartBuilder : TestDataBuilder<Heart>
    {
        private Image m_image;
        
        public HeartBuilder(Image image)
        {
            m_image = image;
        }

        public HeartBuilder() : this(An.Image()){}
        
        public override Heart Build()
        {
            var heart = new Heart(m_image);
            return heart;
        }

        public HeartBuilder With(Image image)
        {
            m_image = image;
            return this;
        }
    }
}