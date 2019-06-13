using NUnit.Framework;
using ProjectZ.Component;
using ProjectZ.Component.Setting;
using ProjectZ.Test.SetUp;

namespace ProjectZ.LoadData.Tests
{
    [TestFixture]
    public class ConvertDataToSingletonTest : ECSTestsFixture
    {
        [Test]
        public void _0_Singleton_Has_Value()
        {
            World.GetOrCreateSystem<ConvertDataToSingleton>().Update();
            var target = AIDataSingleton.Factors.FactorsMax[0];
            Assert.AreEqual(100,target);
        }

        [Test]
        public void _1_Behaviour_Data_Has_Value()
        {
            World.GetOrCreateSystem<ConvertDataToSingleton>().Update();
            var target = AIDataSingleton.Behaviours.FactorsInfo.Modes.Length;
            Assert.AreEqual(8,target);
        }

        [Test]
        public void _2_Behaviour_Data_Has_Level()
        {
            World.GetOrCreateSystem<ConvertDataToSingleton>().Update();
            var target = AIDataSingleton.NeedLevels.BehavioursType[1].Length;
            Assert.AreEqual(8,target);
        }

        [Test]
        public void _3_Behaviour_Data_Has_Weight()
        {
            World.GetOrCreateSystem<ConvertDataToSingleton>().Update();
            var target = AIDataSingleton.Behaviours.FactorsInfo.Weights[1][0];
            Assert.AreEqual(1,target);
        }

        [Test]
        public void _3_Behaviour_Data_Has_Mode()
        {
            World.GetOrCreateSystem<ConvertDataToSingleton>().Update();
            var target = AIDataSingleton.Behaviours.FactorsInfo.Modes[1][0];
            Assert.AreEqual(FactorMode.Direct,target);
        }
    }
}