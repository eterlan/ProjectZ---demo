using Unity.Entities;
using UnityEngine;

public struct TimeRecord : IComponentData
{
    public int StartTimeInMinute;
    public int StartElapsedTimeInHour;
    public int StartElapsedTimeInDay;
    public int StartElapsedTimeInMonth;
    public int StartElapsedTimeInYear;

    public float RealElapsedTimeInSecond;

    public float GameElapsedTimeInSecond;
    public int   GameElapsedTimeInMinute;
    public int   GameElapsedTimeInHour;
    public int   GameElapsedTimeInDay;
    public int   GameElapsedTimeInMonth;
    public int   GameElapsedTimeInYear;

    public int DefaultTimeElapsedSpeed;
    public int ModifiedTimeElapsedSpeed;
}

public class TimeRecordProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new TimeRecord
        {
            DefaultTimeElapsedSpeed  = ConstValue.DefaultTimeElapsedSpeed,
            ModifiedTimeElapsedSpeed = 1
        };
        manager.AddComponentData(entity, data);
    }
}