using NUnit.Framework;
using ProjectZ.Test.SetUp;
using Unity.Entities;
using UnityEngine;

namespace ProjectZ.Test.PlayerMode.ChangeDetection
{
    [TestFixture]
    public class ChangeSystemBehaviourTests
    {
        public class ChunkDidChangeTests : ECSTestsFixture
        {
            private ArchetypeChunk                                      m_chunk;
            private ArchetypeChunkComponentType<ForChangeTestComponent> m_componentType;

            [SetUp]
            public void SetUp()
            {
                var entity = m_Manager.CreateEntity(typeof(ForChangeTestComponent));
                m_componentType = m_Manager.GetArchetypeChunkComponentType<ForChangeTestComponent>(false);
                m_chunk         = m_Manager.GetChunk(entity);
            }

            [Test]
            public void _0_CS_Component_Version_Not_Changed_Without_Access()
            {
                var version = World.GetOrCreateSystem<ChangeComponentSystem>().LastSystemVersion;
                var target  = m_chunk.DidChange(m_componentType, version);
                var cv      = m_chunk.GetComponentVersion(m_componentType);
                Debug.Log($"cv:{cv}, version{version}");
                Assert.IsTrue(target);
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
    }
}