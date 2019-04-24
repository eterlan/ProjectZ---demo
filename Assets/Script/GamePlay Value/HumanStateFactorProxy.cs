using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public struct HumanStateFactor : IComponentData
{
    // public static int[] m;
    // public static int[][] MaxValue = 
    // {
    //     m = new int[2] {1,3},
    //     new int[] {1,2,3}, 
    //     new int[] {4,5,6}, 
    //     new int[] {7,8,9},
    // };

    // which kind of array is supported?
    // static looks fine and can be initialize.
    // public int ad {get;set;}
    // property looks fine
    // public NativeHashMap<int,int> hello; 
    // public NativeMultiHashMap<int,int> das;
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
            //helloo = new NativeArray<int>(10,Allocator.TempJob),
            Sleepiness = Sleepiness,
            Hungry     = Hungry,
            Thirsty    = Thirsty,
            Stamina    = Stamina,
        };
        manager.AddComponentData(e, data);
    }
}