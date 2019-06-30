using UnityEngine;
using NUnit.Framework;
using ProjectZ.Test.SetUp;
using Unity.Collections;

namespace ProjectZ.Test
{
    [TestFixture]
    public class CopyNativeArrayTest : ECSTestsFixture
    {
        [Test]
        public void _0_Copy_NativeArray_Doesnt_Need_Clear()
        {
            NativeArray<int> hi = new NativeArray<int>(3,Allocator.TempJob);
            hi[0] = 3;
            var hello = hi;
            hi.Dispose();
            Assert.AreEqual(3,hello[0]);
        }
    }
}