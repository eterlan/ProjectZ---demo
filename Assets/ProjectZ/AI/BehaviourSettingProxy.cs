using System;
using ProjectZ.Component.Setting;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectZ.AI
{
    [Serializable]
    public struct BehaviourSetting : ISharedComponentData
    {
        public int maxWater;
        public int maxFood;
        public int maxHungry;
        public int maxThirsty;
        public int maxSleepiness;
        public int maxStamina;
        public int drinkCost;
        public int drinkGain;
        public int eatCost;
        public int eatGain;
        public int eatCoolDownInMinute;
        public int drinkCoolDownInMinute;
        public int getWaterCoolDownInMinute;
        public int huntCoolDownInMinute;
    }

    public class BehaviourSettingProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            var data = new BehaviourSetting
            {
                maxWater                 = Setting.MaxWater,
                maxFood                  = Setting.MaxFood,
                maxHungry                = Setting.MaxHungry,
                maxSleepiness            = Setting.MaxSleepiness,
                maxStamina               = Setting.MaxSleepiness,
                maxThirsty               = Setting.MaxThirsty,
                drinkCost                = Setting.DrinkCostWater,
                drinkGain                = Setting.DrinkGainThirsty,
                eatCost                  = Setting.EatCostFood,
                eatGain                  = Setting.EatGainHungry,
                eatCoolDownInMinute      = Setting.EatCoolDownInMinute,
                drinkCoolDownInMinute    = Setting.DrinkCoolDownInMinute,
                getWaterCoolDownInMinute = Setting.GetWaterCoolDownInMinute,
                huntCoolDownInMinute     = Setting.HuntCoolDownInMinute
            };
            manager.AddSharedComponentData(entity, data);
        }
    }
}