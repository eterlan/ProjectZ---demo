using ProjectZ.Component.Setting;
using Unity.Entities;
using UnityEngine;

namespace ProjectZ.AI
{
    public struct HumanState : IComponentData
    {
        public int Sleepiness;
        public int Hungry;
        public int Thirsty;
        public int Stamina;
    }

    public class HumanStateFactorProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Range(0, Setting.MaxHungry)]     public int hungry;
        [Range(0, Setting.MaxSleepiness)] public int sleepiness;
        [Range(0, Setting.MaxStamina)]    public int stamina;
        [Range(0, Setting.MaxThirsty)]    public int thirsty;

        public void Convert(Entity e, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            var data = new HumanState
            {
                //helloo = new NativeArray<int>(10,Allocator.TempJob),
                Sleepiness = sleepiness,
                Hungry     = hungry,
                Thirsty    = thirsty,
                Stamina    = stamina
            };
            manager.AddComponentData(e, data);
        }
    }
}