using NUnit.Framework;
using ProjectZ.Component;
using ProjectZ.Test.SetUp;
using Unity.Entities;
using UnityEngine;

namespace ProjectZ.AI.Tests
{
    [TestFixture]
    public class BehaviourDecisionSystemTests : ECSTestsFixture
    {
        [SetUp]
        public void SetUp()
        {
            m_entity0    = m_Manager.CreateEntity(typeof(Tendency), typeof(BehaviourInfo));
            m_entity1    = m_Manager.CreateEntity(typeof(Tendency), typeof(BehaviourInfo));
            m_buffer0    = m_Manager.GetBuffer<Tendency>(m_entity0);
            m_buffer1    = m_Manager.GetBuffer<Tendency>(m_entity1);
            m_chunk      = m_Manager.GetChunk(m_entity0);
            m_bufferType = m_Manager.GetArchetypeChunkBufferType<Tendency>(true);
            m_currType   = m_Manager.GetArchetypeChunkComponentType<BehaviourInfo>(false);
            var count0 = 0.1f;
            var count1 = 0.98f;
            for (var i = 0; i < AIDataSingleton.Behaviours.Count; i++)
            {
                m_buffer0.Add(new Tendency {Value = count0});
                m_buffer1.Add(new Tendency {Value = count1});
                count0 += 0.1f;
                count1 -= 0.05f;
            }
        }

        private Entity                                            m_entity0;
        private Entity                                            m_entity1;
        private DynamicBuffer<Tendency>                           m_buffer0;
        private DynamicBuffer<Tendency>                           m_buffer1;
        private ArchetypeChunk                                    m_chunk;
        private ArchetypeChunkBufferType<Tendency>                m_bufferType;
        private ArchetypeChunkComponentType<BehaviourInfo> m_currType;

        [Test]
        public void _0_Decision_System_Work_Properly()
        {
            World.GetOrCreateSystem<BehaviourDecisionSystem>().Update();
        }

        [Test]
        public void _1_System_Find_Correct_Index_For_Buffer0()
        {
            World.GetOrCreateSystem<BehaviourDecisionSystem>().Update();
            var target = m_Manager.GetComponentData<BehaviourInfo>(m_entity0).CurrBehaviourType;
            Assert.AreEqual(7, (int) target);
        }

        [Test]
        public void _1_System_Find_Correct_Index_For_Buffer1()
        {
            World.GetOrCreateSystem<BehaviourDecisionSystem>().Update();
            var target = m_Manager.GetComponentData<BehaviourInfo>(m_entity1).CurrBehaviourType;
            Assert.AreEqual(0, (int) target);
        }

        [Test]
        public void _2_System_Would_Not_Pass_Changed_Filter_After_The_First_Run()
        {
            World.GetOrCreateSystem<BehaviourDecisionSystem>().Update();
            var origin = m_chunk.GetComponentVersion(m_bufferType);
            World.GetOrCreateSystem<BehaviourDecisionSystem>().Update();
            var after = m_chunk.GetComponentVersion(m_bufferType);
            Assert.AreEqual(origin, after);
        }

        // @Bug _2_System_Would_Not_Update_Before_Tendency_Changed Is wrong. Because any system which doesn't update before would 
        // The logic is: Although Sys has [ChangedFilter], but considering it's First run, so the ChangedFilter pass, so the Job is run then the Buffer is modified, at last Buffer version is bumped.
        [Test]
        public void _2_System_Would_Pass_Changed_Filter_At_The_First_Run()
        {
            //m_buffer0[1]
            var origin = m_Manager.GetComponentData<BehaviourInfo>(m_entity0).CurrBehaviourType;
            World.GetOrCreateSystem<BehaviourDecisionSystem>().Update();
            var after = m_Manager.GetComponentData<BehaviourInfo>(m_entity0).CurrBehaviourType;
            Assert.AreNotEqual(origin, after);
        }

        [Test]
        public void _3_System_Would_Execute_Job_After_Set_Changed_Filter_At_First_Run()
        {
            var origin = m_chunk.GetComponentVersion(m_currType);
            World.GetOrCreateSystem<BehaviourDecisionSystem>().Update();
            var target = m_Manager.GetComponentData<BehaviourInfo>(m_entity0).CurrBehaviourType;
            var after  = m_chunk.GetComponentVersion(m_currType);

            Assert.AreNotEqual(origin, after);
        }

        [Test]
        public void _3_System_Would_Not_Execute_Job_After_Set_Change_Filter_After_First_Run()
        {
            World.GetOrCreateSystem<BehaviourDecisionSystem>().Update();
            Debug.Log(m_chunk.GetComponentVersion(m_currType));
            Debug.Log(m_chunk.GetComponentVersion(m_currType));
            World.GetOrCreateSystem<BehaviourDecisionSystem>().Update();
            Debug.Log("LSV:" + World.GetOrCreateSystem<BehaviourDecisionSystem>().LastSystemVersion);
            World.GetOrCreateSystem<BehaviourDecisionSystem>().Update();
            Debug.Log(m_chunk.GetComponentVersion(m_currType));
            var origin = m_chunk.GetComponentVersion(m_currType);

            Debug.Log(m_chunk.GetComponentVersion(m_currType));


            var after = m_chunk.GetComponentVersion(m_currType);

            Assert.AreEqual(origin, after);
        }
    }
}