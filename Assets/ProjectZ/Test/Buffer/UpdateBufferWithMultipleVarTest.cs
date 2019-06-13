using NUnit.Framework;
using ProjectZ.Test.SetUp;

namespace ProjectZ.Test.Buffer
{
    [TestFixture]
    public class UpdateBufferVarTest : ECSTestsFixture
    {
        [Test]
        public void _0_Update_One_Var()
        {
            var entity = m_Manager.CreateEntity(typeof(U));

            World.GetOrCreateSystem<UpdateBufferWithMultipleElement>().Update();
            var target = m_Manager.GetBuffer<U>(entity)[0].Var1;
            Assert.AreEqual(1, target);
        }

        [Test]
        public void _1_Update_Second_Var()
        {
            var entity = m_Manager.CreateEntity(typeof(U));

            World.GetOrCreateSystem<UpdateBufferWithMultipleElement>().Update();
            var target = m_Manager.GetBuffer<U>(entity)[0].Var2;
            Assert.AreEqual(2,target);
        }

        [Test]
        public void _2_Modify_Existing_Element()
        {
            var entity = m_Manager.CreateEntity(typeof(U));
            World.GetOrCreateSystem<ModifyExistingElement>().Update();
            var target = m_Manager.GetBuffer<U>(entity)[0].Var2;
            Assert.AreEqual(2,target);
        }

        [Test]
        public void _3_Modify_Nested_Buffer()
        {
            var entity = m_Manager.CreateEntity(typeof(NestedBuffer));
            var buffer = m_Manager.GetBuffer<NestedBuffer>(entity);
            buffer.Add(new NestedBuffer{N = new EcsIntElement2{Value0 = 1}});
            var target = m_Manager.GetBuffer<NestedBuffer>(entity)[0].N.Value0;
            Assert.AreEqual(1,target);
        }
        // GetBuffer 会自动更新吗？ Yes.
        // 嵌套Struct的数组会更新吗？不允许嵌套数组。
        [Test]
        public void _4_Buffer_Auto_Update_After_GetBuffer_Test()
        {
            var entity = m_Manager.CreateEntity(typeof(U));
            var buffer = m_Manager.GetBuffer<U>(entity);
            var elem   = new U(1,3);
            buffer.Add(elem);
            Assert.AreEqual(3,buffer[0].Var2);
        }
    }
}