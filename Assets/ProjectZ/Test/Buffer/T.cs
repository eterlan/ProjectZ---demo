using System;
using Unity.Collections;
using Unity.Entities;

namespace ProjectZ.Test.Buffer
{
    [Serializable]
    public struct T : IComponentData
    {
        public Point point;
        public int   forTest;
    }

    [InternalBufferCapacity(8)]
    public struct U : IBufferElementData
    {
        public int Var1;
        public int Var2;

        public U(int var1 = 1, int var2 = 1)
        {
            Var1 = var1;
            Var2 = var2;
        }
    }

    public struct Point
    {
        public int X;
        public int Y;
    } 
    
    // CANNOT contain nested array.
    //    public struct NA
    //    {
    //        public int[] Values;
    //    }
}