using Unity.Entities;
using UnityEngine;

public struct BehaviourValue : ISharedComponentData
{
    public int MaxWater                ;
    public int MaxFood                 ;
    public int MaxHungry               ;
    public int MaxThirsty              ;
    public int MaxSleepiness           ;
    public int MaxStamina              ;
    public int DrinkCost               ;
    public int DrinkGain               ;
    public int EatCost                 ;
    public int EatGain                 ;
    public int EatCoolDownInMinute     ;
    public int DrinkCoolDownInMinute   ;
    public int GetWaterCoolDownInMinute;
    public int HuntCoolDownInMinute    ;

}

public class BehaviourValueProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert( Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new BehaviourValue
        {
            MaxWater                 = ConstValue.MaxWater,
            MaxFood                  = ConstValue.MaxFood,
            MaxHungry                = ConstValue.MaxHungry,
            MaxSleepiness            = ConstValue.MaxSleepiness,
            MaxStamina               = ConstValue.MaxSleepiness,
            MaxThirsty               = ConstValue.MaxThirsty,
            DrinkCost                = ConstValue.DrinkCostWater,
            DrinkGain                = ConstValue.DrinkGainThirsty,
            EatCost                  = ConstValue.EatCostFood,
            EatGain                  = ConstValue.EatGainHungry,
            EatCoolDownInMinute      = ConstValue.EatCoolDownInMinute,
            DrinkCoolDownInMinute    = ConstValue.DrinkCoolDownInMinute,
            GetWaterCoolDownInMinute = ConstValue.GetWaterCoolDownInMinute,
            HuntCoolDownInMinute     = ConstValue.HuntCoolDownInMinute,
        };
        manager.AddSharedComponentData(entity,data);
    }
}