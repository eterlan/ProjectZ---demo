using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Tests
{
    public class HeartTests
    {
        
        private Image m_image;
        private Heart m_heart;
            
        [SetUp]
        public void BeforeEveryTest()
        {
            m_image = new GameObject().AddComponent<Image>();
            m_heart = new Heart(m_image);
        }

        public class CurrentNumberOfHeartPieces : HeartTests
        {
            [Test]
            public void _0_Image_Fill_Is_0_Heart_Piece()
            {
                m_image.fillAmount = 0;

                m_heart.Replenish(0);
                Assert.AreEqual(0, m_heart.CurrentHeartPieces);
            }

            [Test]
            public void _1_Set_Image_With_0_Fill_To_25_Percent_Fill_Is_1_Heart_Piece()
            {
                m_image.fillAmount = 0f;
                
                m_heart.Replenish(1);
                Assert.AreEqual(1,m_heart.CurrentHeartPieces);
            }
        }
        public class TheReplenishMethod : HeartTests
        {
            [Test]
            public void _0_Set_Image_With_0_Percent_Fill_To_0_Percent_Fill()
            {
                m_image.fillAmount = 0;

                m_heart.Replenish(0);
                Assert.AreEqual(0, m_image.fillAmount);
            }

            [Test]
            public void _1_Set_Image_With_0_Percent_Fill_To_25_Percent_Fill()
            {
                m_image.fillAmount = 0f;
                
                m_heart.Replenish(1);
                Assert.AreEqual(0.25,m_image.fillAmount);
            }
            
            [Test]
            public void _1_Set_Image_With_25_Percent_Fill_To_50_Percent_Fill()
            {
                m_image.fillAmount = 0.25f;
                
                m_heart.Replenish(1);
                Assert.AreEqual(0.5f,m_image.fillAmount);
            }

            [Test]
            public void _2_Throw_ArgumentOutOfRange_Exception_With_Negative_Argument()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => m_heart.Replenish(-1));
            }
        }
        public class TheDepleteMethod : HeartTests
        {
            [Test]
            public void _0_Set_Image_With_1_Fill_To_1_Fill()
            {
                m_image.fillAmount = 1;
                m_heart.Deplete(0);
                Assert.AreEqual(1,m_image.fillAmount);
            }
            
            [Test]
            public void _1_Set_Image_With_1_Fill_To_75_Percent_Fill()
            {
                m_image.fillAmount = 1;
                m_heart.Deplete(1);
                Assert.AreEqual(0.75f,m_image.fillAmount);
            }
            
            [Test]
            public void _1_Set_Image_With_75_Percent_Fill_To_50_Percent_Fill()
            {
                m_image.fillAmount = 0.75f;
                m_heart.Deplete(1);
                Assert.AreEqual(0.5f,m_image.fillAmount);
            }
            
            [Test]
            public void _2_Throw_ArgumentOutOfRange_Exception_With_Negative_Argument()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => m_heart.Replenish(-1));
            }
        }
    }
}