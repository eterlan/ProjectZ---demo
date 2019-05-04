using Unity.Entities;
using UnityEngine;
public struct Food : ISharedComponentData
{
    public  int Restoration;
    public FoodType FoodType;
    public SpType SpType;
    public int SpValue;
    public int MaxInCell; 
}

[RequiresEntityConversion]
public class FoodProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public int Restoration;
    public FoodType FoodType;
    public SpType SpType;
    public int SpValue;
    public int MaxInCell; 
    public void Convert( Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new Food
        {
            Restoration = Restoration,
            FoodType    = FoodType,
            SpType      = SpType,
            SpValue     = SpValue,
            MaxInCell   = MaxInCell,
        };
        manager.AddSharedComponentData(entity, data);
    }
}