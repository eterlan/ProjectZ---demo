using System;
using NUnit.Framework;
using ProjectZ.Component;
using ProjectZ.Component.Setting;
using ProjectZ.LoadData;
using ProjectZ.Test.SetUp;
using Unity.Entities;

namespace ProjectZ.AI.Tests
{
    [TestFixture]
    public class PeriodCheckNeedsSystemTests : ECSTestsFixture
    {
        [SetUp]
        public void SetUp()
        {
            var archetype = m_Manager.CreateArchetype(typeof(CurrentBehaviourInfo), typeof(Factor), typeof
                (Tendency));
            m_entity = m_Manager.CreateEntity(archetype);
            World.GetOrCreateSystem<ConvertDataToSingleton>().Update();
        }

        private Entity m_entity;

        [Test]
        public void _0_When_Not_Exceed_Timer_System_Not_Modify_Need_Lv()
        {
            m_Manager.SetComponentData(m_entity, new CurrentBehaviourInfo {CurrentNeedLv = 1});
            World.GetOrCreateSystem<PeriodCheckNeedsSystem>().Update();
            var target = m_Manager.GetComponentData<CurrentBehaviourInfo>(m_entity).CurrentNeedLv;
            Assert.AreEqual(1, target);
        }

        [Test]
        public void _1_When_Timer_Exceed_And_Tendency_Exceed_Needs_Lv_Should_Be_1()
        {
            var origin = 1;
            m_Manager.SetComponentData(m_entity,
                new CurrentBehaviourInfo {CurrentNeedLv = origin, PeriodCheckTimer = 2});
            var buffer = m_Manager.GetBuffer<Tendency>(m_entity);
            var length = Enum.GetValues(typeof(BehaviourType)).Length;
            // @Todo Set Tendency bound from 0 to 1;
            // @Todo Tendency value should / n.
            var testValue = 0.1f;
            for (var i = 0; i < length; i++)
            {
                buffer.Add(new Tendency {Value = testValue});
                testValue += 0.1f;
            }

            World.GetOrCreateSystem<PeriodCheckNeedsSystem>().Update();
            var target = m_Manager.GetComponentData<CurrentBehaviourInfo>(m_entity).CurrentNeedLv;
            Assert.AreEqual(origin, target);
        }

        [Test]
        public void _2_When_Timer_Exceed_And_Tendency_Not_Exceed_System_Modify_Need_Lv()
        {
            var origin = 1;
            m_Manager.SetComponentData(m_entity,
                new CurrentBehaviourInfo {CurrentNeedLv = origin, PeriodCheckTimer = 2});

            var buffer    = m_Manager.GetBuffer<Tendency>(m_entity);
            var length    = AIDataSingleton.Behaviours.Count;
            var testValue = 0.1f;
            for (var i = 0; i < length; i++) buffer.Add(new Tendency {Value = testValue});
            World.GetOrCreateSystem<PeriodCheckNeedsSystem>().Update();
            var target = m_Manager.GetComponentData<CurrentBehaviourInfo>(m_entity).CurrentNeedLv;
            Assert.AreEqual(origin + 1, target);
        }

//        [Test]
//        public void _3_Test_Job()
//        {
//            m_Manager.SetComponentData(m_entity,new CurrentBehaviourInfo{CurrentNeedLv = 1,PeriodCheckTimer = 2});
//            var buffer = m_Manager.GetBuffer<Tendency>(m_entity);
//            var length = Enum.GetValues(typeof(BehaviourType)).Length;
//            var testValue = 0.1f;
//            for (int i = 0; i < length; i++)
//            {
//                buffer.Add(new Tendency{Value = testValue});
//                testValue += 0.1f;
//            }
//
//            var job = new PeriodCheckNeedsSystem.PeriodCheck
//            {
//                FixedDeltaTime        = fixedDeltaTime,
//                CheckNeedPeriods      = m_checkNeedPeriods,
//                NeedLvBehavioursIndex = m_needLvBehavioursIndex
//            }.Run();
//            //World.GetOrCreateSystem<PeriodCheckNeedsSystem>().
//            var target = m_Manager.GetComponentData<CurrentBehaviourInfo>(m_entity).CurrentNeedLv;
//            Assert.AreEqual(1,target);
//        }
    }
}