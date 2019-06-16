using ProjectZ.Component;
using ProjectZ.Component.Setting;
using Unity.Entities;
using Unity.Mathematics;

namespace ProjectZ.AI
{
    public struct BehaviourInfo : IComponentData
    {
        public BehaviourType CurrBehaviourType;
        public BehaviourType PrevBehaviourType;
    }
}