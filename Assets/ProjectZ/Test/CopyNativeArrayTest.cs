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
            Assert.AreEqual(false,hi.IsCreated);
            // @Conclusion: NativeArray直接赋值相当于传递引用，因此释放拷贝者后，原容器无须释放，但原容器IsCreated却返回true
        }
    }
}