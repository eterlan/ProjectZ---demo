using NUnit.Framework;
using ProjectZ.Test.SetUp;

namespace ProjectZ.Test.Buffer
{
    /// <summary>
    /// How the Buffer Constructor works?
    /// </summary>
    /// 1. Default constructor construct all field with default value;
    /// 2. Custom constructor MUST contain every field in struct;
    /// 3. Even though all parameter of Custom constructor has default value, when you pass no parameter in the constructor, it would call the default constructor.
    [TestFixture]
    public class BufferConstructor : ECSTestsFixture
    {
        [Test]
        public void _0_Instantiate_Without_Constructor()
        {
            var entity = m_Manager.CreateEntity(typeof(U));
            m_Manager.GetBuffer<U>(entity).Add(new U());
            var target = m_Manager.GetBuffer<U>(entity)[0].Var1;
            Assert.AreEqual(0,target);
        }

        [Test]
        public void _1_Instantiate_With_Constructor_With_1_Default_Parameter()
        {
            var entity = m_Manager.CreateEntity(typeof(U));
            m_Manager.GetBuffer<U>(entity).Add(new U(var1: 1));
            var target = m_Manager.GetBuffer<U>(entity)[0].Var1;
            Assert.AreEqual(1,target);
        }

        [Test]
        public void _1_Constructor_With_1_Parameter_Another_One_Should_Be_Default()
        {
            var entity = m_Manager.CreateEntity(typeof(U));
            m_Manager.GetBuffer<U>(entity).Add(new U(var1:1));
            var target = m_Manager.GetBuffer<U>(entity)[0].Var2;
            Assert.AreEqual(1,target);
        }
    }
}
