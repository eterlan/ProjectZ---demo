using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public partial class HeartContainerTest
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
        m_image = new GameObject().AddComponent<Image>();
        m_image2 = new GameObject().AddComponent<Image>();
        m_image3 = new GameObject().AddComponent<Image>();
        m_images = new List<Image> {m_image,m_image2,m_image3};
        m_hearts = new List<Heart>(new []
        {
            new Heart(m_image),
            new Heart(m_image2),
            new Heart(m_image3),
        });

        m_heartContainer = new HeartContainer(m_hearts);
    }

    public class TestReplenishedMethod : HeartContainerTest
    {
        [Test]
        public void _0_Set_Image_With_0_Fill_To_0_Fill()
        {
            m_images[0].fillAmount = 0;
            m_heartContainer.Replenish(0);
            Assert.AreEqual(0,m_image.fillAmount);
        }

        [Test]
        public void _1_Set_Image_With_0_Fill_To_50_Fill()
        {
            m_images[0].fillAmount = 0;
            m_heartContainer.Replenish(2);
            Assert.AreEqual(0.5,m_image.fillAmount);
        }
//        
//        [Test]
//        public void _2_Set_Image_With_100_Fill_To_150_Fill()
//        {
//            m_images[0].fillAmount = 1;
//            m_images[1].fillAmount = 0;
//            m_heartContainer.Replenish(2);
//            Assert.AreEqual(0.5,m_images[1].fillAmount);
//        }

        [Test]
        public void _2_Heart_Are_Replenished_In_Order()
        {
            m_images[0].fillAmount = 0;
            m_images[1].fillAmount = 0;
            m_heartContainer.Replenish(2);
            Assert.AreEqual(0.5,m_images[0].fillAmount);
            Assert.AreEqual(0,m_images[1].fillAmount);
        }

        [Test]
        public void _3_Replenish_Exceeding_Heart_Pieces_To_Next_Heart()
        {
            m_images[0].fillAmount = 0.75f;
            m_images[1].fillAmount = 0f;
            m_heartContainer.Replenish(2);
            Assert.AreEqual(1,m_images[0].fillAmount);
            Assert.AreEqual(0.25,m_images[1].fillAmount);
        }
        
        [Test]
        public void _3_Replenish_Exceeding_Heart_Pieces_To_Next_Next_Heart()
        {
            m_images[0].fillAmount = 0.75f;
            m_images[1].fillAmount = 0f;
            m_images[2].fillAmount = 0f;
            m_heartContainer.Replenish(6);
            Assert.AreEqual(1,m_images[0].fillAmount);
            Assert.AreEqual(1,m_images[1].fillAmount);
            Assert.AreEqual(0.25,m_images[2].fillAmount);
        }
    }
}