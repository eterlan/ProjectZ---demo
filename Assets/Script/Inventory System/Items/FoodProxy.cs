using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

public struct Food : ISystemStateSharedComponentData
{
    public int      Restoration;
    public FoodType FoodType;
    public SpType   SpType;
    public int      SpValue;
    public int      MaxInCell;
}

[RequiresEntityConversion]
public class FoodProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [FormerlySerializedAs("FoodType")] public FoodType foodType;
    [FormerlySerializedAs("MaxInCell")] public int      maxInCell;
    [FormerlySerializedAs("Restoration")] public int      restoration;
    [FormerlySerializedAs("SpType")] public SpType   spType;
    [FormerlySerializedAs("SpValue")] public int      spValue;

    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new Food
        {
            Restoration = restoration,
            FoodType    = foodType,
            SpType      = spType,
            SpValue     = spValue,
            MaxInCell   = maxInCell
        };
        manager.AddSharedComponentData(entity, data);
    }
}