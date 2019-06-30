using NUnit.Framework;
using ProjectZ.Test.SetUp;
using ProjectZ.AI.PathFinding;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectZ.AI.PathFinding.Tests
{
    [TestFixture]
    public class MoveForwardSystemTests : ECSTestsFixture
    {
        private Entity m_player;

        [SetUp]
        public void SetUp()
        {
            //@Bug 自带Translation组件。？
            m_player = m_Manager.CreateEntity(typeof(MoveSpeed), typeof(Translation), typeof(LocalToWorld), typeof(NavigateTarget));
            m_Manager.SetComponentData(m_player, new NavigateTarget {Position = new float3(10, 0, 10)});
            m_Manager.SetComponentData(m_player, new MoveSpeed {Speed         = 3, MaximumSpeed = 5});
            Spawner spawner = new Spawner {Count = new int2(10, 10)};
            m_Manager.CreateEntity(typeof(Spawner));
            var spawnerQuery = m_Manager.CreateEntityQuery(typeof(Spawner));
            Debug.Log(spawnerQuery.CalculateLength());
            spawnerQuery.SetSingleton(spawner);
        }

        [Test]
        public void _0_PathFinding_System_Update()
        {
            TestDelegate target = World.GetOrCreateSystem<PathFinding>().Update;
            Assert.DoesNotThrow(target);
        }
    }
}

// @Bug: 由于LocalToWorldSystem在测试时不会自动运行，也无法创建，导致LocalToWorld.Forward为0，所以无法测试相关代码。