using ProjectZ.Component;
using ProjectZ.Component.Setting;
using Unity.Entities;
using Unity.Mathematics;


namespace ProjectZ.AI
{
    public struct CurrentBehaviourInfo : IComponentData
    {
        private int m_currentNeedLv;

        public BehaviourType BehaviourType;
        public float         PeriodCheckTimer;
        public int           CurrentNeedLv
        {
            set => m_currentNeedLv = math.clamp(value, 0, AIDataSingleton.NeedLevels.Count);
            get => m_currentNeedLv;
        }
        
    }
}