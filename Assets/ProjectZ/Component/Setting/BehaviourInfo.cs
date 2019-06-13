using Unity.Entities;

namespace ProjectZ.Component.Setting
{
    [System.Serializable]
    public struct BehaviourInfo
    {
        public string Name;
        public FactorsInfo[] FactorsInfo;
        public int NeedLevel;
    }

    [System.Serializable]
    public struct FactorsInfo
    {
        public string FactorName;
        public float  FactorWeight;
        public string FactorMode;
    }
}