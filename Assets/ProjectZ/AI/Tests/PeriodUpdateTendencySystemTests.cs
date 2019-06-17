using NUnit.Framework;
using ProjectZ.Component;
using ProjectZ.Component.Setting;
using ProjectZ.Test.SetUp;
using Unity.Entities;
using UnityEngine;

namespace ProjectZ.AI.Tests
{
    [TestFixture]
    public class PeriodUpdateTendencySystemTests : ECSTestsFixture
    {
        [SetUp]
        public void SetUp()
        {
            var timerEntity = World.GetOrCreateSystem<PeriodUpdateTendencySystem>()
                .GetSingletonEntity<NeedLvTimerSingleton>();

            m_tendencyType = m_Manager.GetArchetypeChunkBufferType<Tendency>(true);

            var entity = m_Manager.CreateEntity(typeof(Tendency), typeof(Factor));
            m_chunk = m_Manager.GetChunk(entity);
            var factorsCount = AIDataSingleton.Factors.Count;
            var factors      = m_Manager.GetBuffer<Factor>(entity);
            // @Todo Factor should have bound.
            var factor = 0.1f;

            for (var i = 0; i < factorsCount; i++)
            {
                var value = factor * AIDataSingleton.Factors.FactorsMax[i];
                factors.Add(new Factor {Value = (int) value});
                factor += 0.1f;
            }

            m_tendencies = m_Manager.GetBuffer<Tendency>(entity);
            var behavioursCount = AIDataSingleton.Behaviours.Count;
            var tendency        = 0.1f;

            for (var i = 0; i < behavioursCount; i++)
            {
                m_tendencies.Add(new Tendency {Value = tendency});
                tendency += 0.1f;
            }
        }

        private ArchetypeChunk                     m_chunk;
        private ArchetypeChunkBufferType<Tendency> m_tendencyType;

        private DynamicBuffer<Tendency> m_tendencies;

        // 空测试会导致SetUp创建世界后，在initial新建的NativeContainer，如果释放逻辑在Update()，就会无法释放。
        //[Test]
        public void _1_Load_Data_Correctly()
        {
            // @TODO How to test it ?
        }

        [Test]
        public void _0_Component_Not_Changed_Before_Time_Up()
        {
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>().SetSingleton(new NeedLvTimerSingleton {Value = 0.1f});
            var before = m_chunk.GetComponentVersion(m_tendencyType);
            Debug.Log("");
            Debug.Log("");
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>().Update();
            var after = m_chunk.GetComponentVersion(m_tendencyType);

            Assert.AreEqual(before, after);
        }

        // @Bug buffer如果是add，那么componentVersion不会改变？
        [Test]
        public void _0_Component_Version_Change_After_Time_Up()
        {
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>()
                .SetSingleton(new NeedLvTimerSingleton {Value = 0.51f});

            var before = m_chunk.GetComponentVersion(m_tendencyType);
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>().Update();
            Debug.Log("");
            Debug.Log("");
            var after = m_chunk.GetComponentVersion(m_tendencyType);
            Assert.AreNotEqual(before, after);
        }

        [Test]
        public void _0_Timer_Work_Correctly()
        {
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>().SetSingleton(new NeedLvTimerSingleton {Value = 2});
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>().Update();
            var target = World.GetOrCreateSystem<PeriodCheckNeedsSystem>().GetSingleton<NeedLvTimerSingleton>().Value;
            Assert.AreEqual(0, target);
        }

        [Test]
        public void _2_Calculate_Mode_D_D_Tendency_Correctly()
        {
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>().SetSingleton(new NeedLvTimerSingleton {Value = 0.6f});

            var indexToCheck = (int) BehaviourType.Drink;
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>().Update();
            Debug.Log("");
            Debug.Log("");
            var after = m_tendencies[indexToCheck].Value;

            var estimate = 0.14f;
            Assert.AreEqual(estimate, after);
        }

        [Test]
        public void _2_Calculate_Mode_I_D_Tendency_Correctly()
        {
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>().SetSingleton(new NeedLvTimerSingleton {Value = 0.6f});

            var indexToCheck = (int) BehaviourType.GetWater;
            World.GetOrCreateSystem<PeriodUpdateTendencySystem>().Update();
            Debug.Log("");
            Debug.Log("");
            var after = m_tendencies[indexToCheck].Value;

            var estimate = 0.6f;
            Assert.AreEqual(estimate, after);
        }

        // @Todo HashMap是按顺序添加的吗？如果不是的话错误在何处？该如何改进好像是反着的？
    }
}