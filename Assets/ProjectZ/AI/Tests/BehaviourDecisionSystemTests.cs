using NUnit.Framework;
using ProjectZ.Test.SetUp;

namespace ProjectZ.AI.Tests
{
    [TestFixture]
    public class BehaviourDecisionSystemTests : ECSTestsFixture
    {
        [Test]
        public void _0_Decision_System_Can_Update()
        {
            World.GetOrCreateSystem<BehaviourDecisionSystem>().Update();
        }
    }
}