using ProjectZ.Component;
using Unity.Entities;
using Unity.Mathematics;


namespace ProjectZ.AI
{
    public struct NeedLv : IComponentData
    {
        public float PeriodCheckTimer ;
        private int m_currentNeedLv;

        public int CurrentNeedLv
        {
            set => m_currentNeedLv = math.clamp(value, 0, AIDataSingleton.NeedLevels.Count);
            get => m_currentNeedLv;
        }
    }
}