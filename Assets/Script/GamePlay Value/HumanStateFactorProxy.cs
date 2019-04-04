using UnityEngine;
using Unity.Entities;

public struct HumanStateFactor : IComponentData
{
    public int Sleepiness;
    public int Hungry;
    public int Thirsty;
    public int Stamina;
}
public class HumanStateFactorProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [Range(0,ConstValue.MaxSleepiness)]
    public int Sleepiness;
    [Range(0,ConstValue.MaxHungry)]
    public int Hungry;
    [Range(0,ConstValue.MaxThirsty)]
    public int Thirsty;
    [Range(0,ConstValue.MaxStamina)]
    public int Stamina;   
    public void Convert(Entity e, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new HumanStateFactor
        {
            Sleepiness = Sleepiness,
            Hungry     = Hungry,
            Thirsty    = Thirsty,
            Stamina    = Stamina,
        };
        manager.AddComponentData(e, data);
    }
}