using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct TimeRecord : IComponentData
{
    public int Start_TimeInMinute;
    public int Start_ElapsedTimeInHour;
    public int Start_ElapsedTimeInDay;
    public int Start_ElapsedTimeInMonth;
    public int Start_ElapsedTimeInYear;
    
    public float Real_ElapsedTimeInSecond;

    public float Game_ElapsedTimeInSecond;
    public int Game_ElapsedTimeInMinute;
    public int Game_ElapsedTimeInHour;
    public int Game_ElapsedTimeInDay;
    public int Game_ElapsedTimeInMonth;
    public int Game_ElapsedTimeInYear;

    public int Default_TimeElapsedSpeed; 
    public int Modified_TimeElapsedSpeed;

}

public class TimeRecordProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert ( Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var data = new TimeRecord
        {
            Default_TimeElapsedSpeed = ConstValue.Default_TimeElapsedSpeed,
            Modified_TimeElapsedSpeed = 1,
        };
        manager.AddComponentData(entity, data);
    }
}