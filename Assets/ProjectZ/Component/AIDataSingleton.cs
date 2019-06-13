using System;
using ProjectZ.Component.Setting;
using Unity.Entities;

namespace ProjectZ.Component
{
    public struct AIDataSingleton : IComponentData
    {
        public struct NeedLevels
        {
            public const int Count = 6;
            public const float NeedsCriticalVar = 0.75f;
            public static readonly float[] CheckPeriods = {0.5f,1,2,4,16,32};
            public static readonly BehaviourType[][]
                BehavioursType = new BehaviourType[Setting.Setting.NeedLvCount][];
        }
        
        public struct Factors
        {
            public static readonly int          Count       = Enum.GetValues(typeof(FactorType)).Length;
            public static readonly FactorType[] FactorsType = new FactorType[Count];
            public static readonly int[]        FactorsMax  = new int[Count];
            public static readonly int[]        FactorsMin  = new int[Count];
        }

        public struct Behaviours
        {
            public static readonly int             Count          = Enum.GetValues(typeof(BehaviourType)).Length;
            public static readonly BehaviourType[] BehavioursType = new BehaviourType[Count];
            public struct FactorsInfo
            {
                public static readonly FactorType[][] Types   = new FactorType[Count][];
                public static readonly FactorMode[][] Modes   = new FactorMode[Count][];
                public static readonly float[][]      Weights = new float[Count][];
            }
        }
    }

    // @Todo Parse类型与Mode

}