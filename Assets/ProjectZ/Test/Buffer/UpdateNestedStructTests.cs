using NUnit.Framework;
using ProjectZ.Test.SetUp;

namespace ProjectZ.Test.Buffer
{
    [TestFixture]
    public class UpdateNestedStructTests : ECSTestsFixture
    {
        [Test]
        public void _0_Update_Normal_Var()
        {
            var entity = m_Manager.CreateEntity(typeof(T));

            World.GetOrCreateSystem<UpdateNestedStruct>().Update();
            var target = m_Manager.GetComponentData<T>(entity).forTest;
            Assert.AreEqual(1,target);
        }

        [Test]
        public void _1_Update_Nested_Struct()
        {
            var entity = m_Manager.CreateEntity(typeof(T));
        
            World.GetOrCreateSystem<UpdateNestedStruct>().Update();
            var target = m_Manager.GetComponentData<T>(entity).point.X;
            Assert.AreEqual(1,target);
        }
    }
}
