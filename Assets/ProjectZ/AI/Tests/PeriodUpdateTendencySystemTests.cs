using UnityEngine;
using NUnit.Framework;
using ProjectZ.Test.SetUp;
using ProjectZ.LoadData;
using Unity.Entities;

namespace ProjectZ.AI.Tests
{
    [TestFixture]
    public class PeriodUpdateTendencySystemTests : ECSTestsFixture
    {
        private ArchetypeChunk m_chunk;
        private ArchetypeChunkBufferType<Tendency> m_type;
        [SetUp]
        public void SetUp()
        {
            var entity = World.GetOrCreateSystem<PeriodUpdateTendencySystem>().GetSingletonEntity<NeedLvTimerSingleton>();
            m_chunk= m_Manager.GetChunk(entity);
            m_type = m_Manager.GetArchetypeChunkBufferType<Tendency>(true);

        }
        [Test]
        public void _0_Timer_Work_Correctly()
        {
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>().SetSingleton(new NeedLvTimerSingleton{Value = 2});
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>().Update();
            var target = World.GetOrCreateSystem<PeriodCheckNeedsSystem>().GetSingleton<NeedLvTimerSingleton>().Value;
            Assert.AreEqual(0,target);
        }

        [Test]
        public void _0_Component_Not_Changed_Before_Time_Up()
        {
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>().SetSingleton(new NeedLvTimerSingleton{Value = 0.4f});
            var before = m_chunk.GetComponentVersion(m_type);
            Debug.Log("");
            Debug.Log("");
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>().Update();
            var after = m_chunk.GetComponentVersion(m_type);

            Assert.AreEqual(before,after);
        }

        [Test]
        public void _1_Load_Data_Correctly()
        {
            
        }

        [Test]
        public void _2_CalculateTendencyCorrectly()
        {
            
        }
    }
}