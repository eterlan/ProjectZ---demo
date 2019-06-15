using NUnit.Framework;
using ProjectZ.Component;
using ProjectZ.Test.SetUp;
using UnityEngine;

namespace ProjectZ.AI.Tests
{
    public class FindMaximumSystemTests : ECSTestsFixture
    {
        public class ComponentSystemTests : ECSTestsFixture
        {
            [SetUp]
            public void SetUp()
            {
                var entity = m_Manager.CreateEntity(typeof(Tendency));
                var buffer = m_Manager.GetBuffer<Tendency>(entity);
                var count  = 0.1f;
                for (var i = 0; i < AIDataSingleton.Behaviours.Count; i++)
                {
                    buffer.Add(new Tendency {Value = count});
                    count += 0.1f;
                }
            }

            [Test]
            public void _0_Test_Find_Maximum_Method()
            {
                var output = World.GetOrCreateSystem<FindMaximumSystem>().FindMaximum<Tendency>();
                var target = output[0];
                output.Dispose();
                Assert.AreEqual(7, target);
            }
        }

        public class JobTest : ECSTestsFixture
        {
            [SetUp]
            public void SetUp()
            {
                World.GetOrCreateSystem<FindMaximumSystem>();
                var entity = m_Manager.CreateEntity(typeof(Tendency));
                var buffer = m_Manager.GetBuffer<Tendency>(entity);
                var length = AIDataSingleton.Behaviours.Count;
                var count  = 0.1f;
                for (var i = 0; i < length; i++)
                {
                    buffer.Add(new Tendency {Value = count});
                    count += 0.1f;
                }
            }

            [Test]
            public void _0_Test_Find_Maximum_Job_Method()
            {
                var output = World.GetOrCreateSystem<FindMaximumSystem>().FindMaximum<Tendency>(out var outputHandle);
                // Require complete? Yes.
                outputHandle.Complete();
                Debug.Log("outputHandle.IsCompleted: " + outputHandle.IsCompleted);
                var target = output[0];
                output.Dispose();
                Assert.AreEqual(7, target);
            }
        }
    }
}