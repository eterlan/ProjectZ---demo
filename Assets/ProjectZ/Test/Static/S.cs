using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ProjectZ.Test.Static
{
    public struct S : IComponentData
    {
        public static int[]  Values  = new[] {1,2};
        public static int[,] Values2 = new[,] {{1,2},{2,3}};
    }
    public struct B : IComponentData
    {
        public static int[]     Values = new[] {1,2};
        public static List<int> F      = new List<int>{1,2};
        public static List<S>   ss     = new List<S>();
    }

    public struct F
    {
        public static int[] v;
        public        int[] f;
        private       Mesh  m_mesh;
    }
}