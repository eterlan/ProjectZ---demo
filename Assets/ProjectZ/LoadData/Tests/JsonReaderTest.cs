using NUnit.Framework;
using ProjectZ.Component.Setting;

namespace ProjectZ.LoadData.Tests
{
    [TestFixture]
    public class JsonReaderTest
    {
        private FactorInfo[] m_factorsInfo;
        private BehaviourInfo[] m_behavioursInfo;
        [SetUp]
        public void SetUp()
        {
            m_factorsInfo = JsonReader.LoadData<FactorInfo>();
            m_behavioursInfo = JsonReader.LoadData<BehaviourInfo>();
        }
        [Test]
        public void _0_Load_Data_Return_True()
        {
            var target = m_factorsInfo[0];
            Assert.NotNull(target);
        }

        [Test]
        public void _1_Load_Data_Has_Value()
        {
            var target = m_factorsInfo[0].Max;
            Assert.AreEqual(100,target);
        }

        [Test]
        public void _2_Load_Data_Has_Enum()
        {
            var target = m_factorsInfo[0].Name;
            Assert.AreEqual(FactorType.Hungry.ToString(),target);
        }

        [Test]
        public void _2_5th_Enum_Test()
        {
            var target = m_factorsInfo[4].Name;
            Assert.AreEqual(FactorType.Lumber.ToString(),target);
        }

        [Test]
        public void _3_Test_Behaviour_Length()
        {
            var target = m_behavioursInfo[5].FactorsInfo.Length;
            Assert.AreEqual(2,target);
        }

        [Test]
        public void _3_Test_Behaviour_Name()
        {
            var target = m_behavioursInfo[3].Name;
            Assert.NotNull(target);
        }
        
        [Test]
        public void _3_Test_Behaviour_Lv_Data()
        {
            var target = m_behavioursInfo[5].NeedLevel;
            Assert.AreEqual(1,target);
        }
        
        [Test]
        public void _3_Test_Behaviour_Factors_Name_Not_Null()
        {
            var target = m_behavioursInfo[3].FactorsInfo[0].FactorName;
            Assert.NotNull(target);
        }
    }
}
