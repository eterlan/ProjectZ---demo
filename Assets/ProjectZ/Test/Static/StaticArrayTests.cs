using NUnit.Framework;
using ProjectZ.Test.SetUp;

namespace ProjectZ.Test.Static
{
    /// <summary>
    /// Static Array in component
    /// </summary>
    /// 1. SA can be store in component.
    /// 2. can be modify.

    [TestFixture]
    public class StaticArrayTests : ECSTestsFixture
    {
        [Test]
        public void _0_Static_Array_In_Component_Legal()
        {
            var target = S.Values[0];
            Assert.AreEqual(1,target);
        }

        [Test]
        public void _0_Update_Static_Array()
        {
            World.GetOrCreateSystem<UpdateStaticArray>().Update();
            var target = S.Values[0];
            Assert.AreEqual(4,target);
        }

        [Test]
        public void _1_Static_Array_In_Buffer_Legal_Test()
        {
            var target = B.Values[0];
            Assert.AreEqual(1,target);
        }

        [Test]
        public void _2_Static_List_In_Buffer_Legal_Test()
        {
            var target = B.F;
            target.Add(3);
            Assert.AreEqual(3, target[2]);
        }
    }
}
