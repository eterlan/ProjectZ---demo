using NUnit.Framework;
using ProjectZ.Test.SetUp;
using Unity.Entities;
using UnityEngine;

namespace ProjectZ.Test.ChangeDetection
{
    [TestFixture]
    public class ChangeSystemBehaviourTests
    {
        public class ComponentSystemChangeTest : ECSTestsFixture
        {
            private ArchetypeChunk                                      m_chunk;
            private ArchetypeChunkComponentType<ForChangeTestComponent> m_componentType;
            private Entity                                              m_entity;

            [SetUp]
            public void SetUp()
            {
                m_entity        = m_Manager.CreateEntity(typeof(ForChangeTestComponent));
                m_componentType = m_Manager.GetArchetypeChunkComponentType<ForChangeTestComponent>(false);
                m_chunk         = m_Manager.GetChunk(m_entity);
            }

            [Test]
            public void _0_CS_Component_Version_Larger_Than_1_At_First_Time_So_Did_Change_Return_True()
            {
                var version = World.GetOrCreateSystem<ChangeComponentSystem>().LastSystemVersion;
                var target  = m_chunk.DidChange(m_componentType, version);
                var cv      = m_chunk.GetComponentVersion(m_componentType);
                Debug.Log($"cv:{cv}, version{version}");
                Assert.IsTrue(target);
            }

            [Test]
            public void _1_Direct_Access_Would_Change_Version()
            {
                var origin = m_chunk.GetComponentVersion(m_componentType);
                World.GetOrCreateSystem<ChangeComponentSystem>().Update();
                m_Manager.SetComponentData(m_entity, new ForChangeTestComponent {Value = 2});
                var after = m_chunk.GetComponentVersion(m_componentType);
                Assert.AreNotEqual(origin, after);
            }

            // @Bug _2_ForEach_With_RO_Access_Would_Not_Change_Component_Version() Is Wrong
            [Test]
            public void _2_ForEach_With_RO_Access_Would_Not_Change_Component_Version()
            {
                var origin = m_chunk.GetComponentVersion(m_componentType);
                World.GetOrCreateSystem<ChangeComponentSystem>().Update();
                World.GetOrCreateSystem<ChangeComponentSystem>().RO();
                var after = m_chunk.GetComponentVersion(m_componentType);
                Assert.AreEqual(origin, after);
            }

            [Test]
            public void _2_ForEach_With_RW_Access_Would_Change_Component_Version()
            {
                var origin = m_chunk.GetComponentVersion(m_componentType);
                World.GetOrCreateSystem<ChangeComponentSystem>().Update();
                World.GetOrCreateSystem<ChangeComponentSystem>().RW();
                var after = m_chunk.GetComponentVersion(m_componentType);
                Assert.AreNotEqual(origin, after);
            }

            [Test]
            public void _3_CS_Component_Version_Changed_With_Actually_W_Access()
            {
                var version = World.GetOrCreateSystem<ChangeComponentSystem>().LastSystemVersion;
                var cv      = m_chunk.GetComponentVersion(m_componentType);
                Debug.Log($"cv:{cv}, version{version}");
                World.GetOrCreateSystem<ChangeComponentSystem>().Update();
                version = World.GetOrCreateSystem<ChangeComponentSystem>().LastSystemVersion;
                cv      = m_chunk.GetComponentVersion(m_componentType);
                Debug.Log($"cv:{cv}, version{version}");
                World.GetOrCreateSystem<ChangeComponentSystem>().Update();
                version = World.GetOrCreateSystem<ChangeComponentSystem>().LastSystemVersion;
                cv      = m_chunk.GetComponentVersion(m_componentType);
                Debug.Log($"cv:{cv}, version{version}");
                World.GetOrCreateSystem<ChangeComponentSystem>().Update();
                cv = m_chunk.GetComponentVersion(m_componentType);
                Debug.Log($"cv:{cv}, version{version}");
                World.GetOrCreateSystem<ChangeComponentSystem>().RW();
                var target = m_chunk.DidChange(m_componentType, version);
                cv = m_chunk.GetComponentVersion(m_componentType);
                Debug.Log($"cv:{cv}, version{version}");
                Assert.IsTrue(target);
            }
        }

        public class JobComponentSystemChunkDidChangeTests : ECSTestsFixture
        {
            private ArchetypeChunk                                      m_chunk;
            private ArchetypeChunkComponentType<ForChangeTestComponent> m_componentType;
            private Entity                                              m_entity;

            [SetUp]
            public void SetUp()
            {
                m_entity        = m_Manager.CreateEntity(typeof(ForChangeTestComponent));
                m_componentType = m_Manager.GetArchetypeChunkComponentType<ForChangeTestComponent>(false);
                m_chunk         = m_Manager.GetChunk(m_entity);
            }

            [Test]
            public void _0_RO_Access_Component_Version_Not_Change()
            {
                var before = m_chunk.GetComponentVersion(m_componentType);
                World.GetOrCreateSystem<ChangeJobComponentSystem>().Update();
                var jobHandle = World.GetOrCreateSystem<ChangeJobComponentSystem>().RO();
                jobHandle.Complete();
                var after = m_chunk.GetComponentVersion(m_componentType);
                Assert.AreEqual(before, after);
            }

            [Test]
            public void _1_RW_Access_Component_Version_Not_Change()
            {
                var before = m_chunk.GetComponentVersion(m_componentType);
                World.GetOrCreateSystem<ChangeJobComponentSystem>().Update();
                var jobHandle = World.GetOrCreateSystem<ChangeJobComponentSystem>().RW();
                jobHandle.Complete();
                var after = m_chunk.GetComponentVersion(m_componentType);
                Assert.AreNotEqual(before, after);
            }
        }
    }
}