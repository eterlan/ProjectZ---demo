    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using ProjectZ.UI.Heart;
    using Tests.Infrastructure;
    using UnityEngine;
    using UnityEngine.UI;

namespace Tests
{
    public class HeartContainerTest
    {
        private Image m_image;
        private Image m_image2;
        private Image m_image3;
        private List<Heart> m_hearts; 
        private List<Image> m_images;
        HeartContainer m_heartContainer;

        [SetUp]
        public void BeforeEveryTest()
        {
            m_image = An.Image();
            m_image2 = An.Image();
            m_image3 = An.Image();
            m_images = new List<Image> {m_image,m_image2,m_image3};
            m_hearts = new List<Heart>
            {
                A.Heart(),
                A.Heart(),
                A.Heart(),
            };

            m_heartContainer = A.HeartContainerBuilder();
        }

        public class TestReplenishedMethod : HeartContainerTest
        {
            [Test]
            public void _0_Set_Image_With_0_Fill_To_0_Piece()
            {
                A.HeartContainerBuilder().Build().Replenish(0);
                Assert.AreEqual(0,m_hearts[0].CurrentHeartPieces);
            }

            [Test]
            public void _1_Set_Image_With_0_Fill_To_2_Pieces()
            {
                var target = A.Heart();
                A.HeartContainerBuilder().With(target).Build().Replenish(2);
                Assert.AreEqual(2,((Heart)target).CurrentHeartPieces);
            }
            
            [Test]
            public void _2_Set_Image_With_100_Fill_To_150_Fill()
            {
                m_hearts = new List<Heart>
                {
                    A.Heart().With(An.Image().WithFillAmount(1)),
                    A.Heart(),
                };
                m_heartContainer = new HeartContainer(m_hearts);

                m_heartContainer.Replenish(2);
                Assert.AreEqual(2,m_hearts[1].CurrentHeartPieces);
            }

            [Test]
            public void _2_Heart_Are_Replenished_In_Order()
            {
                var target1 = A.Heart();
                var target2 = A.Heart();
                ((HeartContainer)A.HeartContainerBuilder().With(target1,target2)).Replenish(2);
                Assert.AreEqual(2,((Heart)target1).CurrentHeartPieces);
                Assert.AreEqual(0,((Heart)target2).CurrentHeartPieces);
            }

            [Test]
            public void _3_Replenish_Exceeding_Heart_Pieces_To_Next_Heart()
            {
                m_hearts = new List<Heart>
                {
                    A.Heart().With(An.Image().WithFillAmount(0.75f)),
                    A.Heart(),
                };
                m_heartContainer = new HeartContainer(m_hearts);

                m_heartContainer.Replenish(2);
                Assert.AreEqual(4,m_hearts[0].CurrentHeartPieces);
                Assert.AreEqual(1,m_hearts[1].CurrentHeartPieces);
            }
            
            [Test]
            public void _3_Replenish_Exceeding_Heart_Pieces_To_Next_Next_Heart()
            {
                var target = A.Heart();
                
                ((HeartContainer)A.HeartContainerBuilder().With(
                    A.Heart().With(An.Image().WithFillAmount(0.75f)),
                    A.Heart().With(An.Image().WithFillAmount(0)),
                    target)).Replenish(6);
                
                Assert.AreEqual(1,((Heart)target).CurrentHeartPieces);
            }
        }
        public class TestDepleteMethod : HeartContainerTest
        {
            [Test]
            public void _0_Set_Heart_With_0_Fill_To_0_Fill()
            {
                var target = A.Heart().Build();
                ((HeartContainer)A.HeartContainerBuilder().With(target)).Deplete(0);
                Assert.AreEqual(0,target.CurrentHeartPieces);
            }

            [Test]
            public void _1_Set_Heart_With_1_Fill_To_75_Percent_Fill()
            {
                var target = A.Heart().With(An.Image().WithFillAmount(1)).Build();
                ((HeartContainer) A.HeartContainerBuilder().With(target)).Deplete(1);
                Assert.AreEqual(3,target.CurrentHeartPieces);
            }

            [Test]
            public void _2_Set_3rd_Heart_With_3_Pieces_Deplete_To_1st_With_2_Pieces()
            {
                var target = A.Heart().With(An.Image().WithFillAmount(1f)).Build();
                var setUp = A.Heart().With(An.Image().WithFillAmount(1f).Build());
                var setUp2 = A.Heart().With(An.Image().WithFillAmount(0.75f)).Build();
                var heartContainer = ((HeartContainer)A.HeartContainerBuilder().With(target,setUp,setUp2));
                heartContainer.Deplete(9);
                Assert.AreEqual(2,target.CurrentHeartPieces);
            }

            [Test]
            public void _2_Set_2nd_Heart_With_4_Pieces_Deplete_1_Piece_Check_1st_Heart_With_4_Pieces()
            {
                var target = A.Heart().With(An.Image().WithFillAmount(1f)).Build();
                var setUp = A.Heart().With(An.Image().WithFillAmount(1F)).Build();
                A.HeartContainerBuilder().With(target,setUp).Build().Deplete(1);
                Assert.AreEqual(4,target.CurrentHeartPieces);
                Assert.AreEqual(3,setUp.CurrentHeartPieces);
            }
        }
    }
}
