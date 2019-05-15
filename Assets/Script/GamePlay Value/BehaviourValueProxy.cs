using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct BehaviourValue : ISharedComponentData
{
    [FormerlySerializedAs("MaxWater")] public int maxWater;
    [FormerlySerializedAs("MaxFood")] public int maxFood;
    [FormerlySerializedAs("MaxHungry")] public int maxHungry;
    [FormerlySerializedAs("MaxThirsty")] public int maxThirsty;
    [FormerlySerializedAs("MaxSleepiness")] public int maxSleepiness;
    [FormerlySerializedAs("MaxStamina")] public int maxStamina;
    [FormerlySerializedAs("DrinkCost")] public int drinkCost;
    [FormerlySerializedAs("DrinkGain")] public int drinkGain;
    [FormerlySerializedAs("EatCost")] public int eatCost;
    [FormerlySerializedAs("EatGain")] public int eatGain;
    [FormerlySerializedAs("EatCoolDownInMinute")] public int eatCoolDownInMinute;
    [FormerlySerializedAs("DrinkCoolDownInMinute")] public int drinkCoolDownInMinute;
    [FormerlySerializedAs("GetWaterCoolDownInMinute")] public int getWaterCoolDownInMinute;
    [FormerlySerializedAs("HuntCoolDownInMinute")] public int huntCoolDownInMinute;
}

public class BehaviourValueProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new BehaviourValue
        {
            maxWater                 = ConstValue.MaxWater,
            maxFood                  = ConstValue.MaxFood,
            maxHungry                = ConstValue.MaxHungry,
            maxSleepiness            = ConstValue.MaxSleepiness,
            maxStamina               = ConstValue.MaxSleepiness,
            maxThirsty               = ConstValue.MaxThirsty,
            drinkCost                = ConstValue.DrinkCostWater,
            drinkGain                = ConstValue.DrinkGainThirsty,
            eatCost                  = ConstValue.EatCostFood,
            eatGain                  = ConstValue.EatGainHungry,
            eatCoolDownInMinute      = ConstValue.EatCoolDownInMinute,
            drinkCoolDownInMinute    = ConstValue.DrinkCoolDownInMinute,
            getWaterCoolDownInMinute = ConstValue.GetWaterCoolDownInMinute,
            huntCoolDownInMinute     = ConstValue.HuntCoolDownInMinute
        };
        manager.AddSharedComponentData(entity, data);
    }
}