using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using System;
using Unity.Collections;

public class TimeControllingSystem : JobComponentSystem
{
    ComponentGroup m_HumanGroup;
    ComponentGroup m_TimerGroup;
    // public struct RunTimer : IJobProcessComponentData<Timers>
    // {
    //     public float deltaTime;
    //     public void Execute( ref Timers timers)
    //     {
    //         if (timers.timer.Running)
    //         {
    //             timers.timer.elapsedTime += deltaTime;
    //             if (timers.timer.elapsedTime > timers.timer.Duration)
    //             {
    //                 timers.timer.Running = false;
    //             }
    //         }      
    //     }
    // }
    [BurstCompile]
    public struct RecordTime : IJobProcessComponentData<TimeRecord>
    {
        public float deltaTime;

        public void Execute(ref TimeRecord time)
        {
            time.Real_ElapsedTimeInSecond += deltaTime;

            // If it's 0, use 1 instead.
            math.select(time.Modified_TimeElapsedSpeed , 1, time.Modified_TimeElapsedSpeed == 0);
            
            time.Game_ElapsedTimeInSecond += deltaTime * time.Default_TimeElapsedSpeed * time.Modified_TimeElapsedSpeed;

            if ( time.Game_ElapsedTimeInSecond >= 60 )
            {
                time.Game_ElapsedTimeInSecond = 0;
                time.Game_ElapsedTimeInMinute += 1;
            }
            if ( time.Game_ElapsedTimeInMinute >= 60 )
            {
                time.Game_ElapsedTimeInMinute = 0;
                time.Game_ElapsedTimeInHour   += 1;
            }
            if ( time.Game_ElapsedTimeInHour >= 24)
            {
                time.Game_ElapsedTimeInHour = 0;
                time.Game_ElapsedTimeInDay += 1;
            }
            if ( time.Game_ElapsedTimeInDay >= 30)
            {
                time.Game_ElapsedTimeInDay = 0;
                time.Game_ElapsedTimeInMonth += 1;
            }
            if (time.Game_ElapsedTimeInMonth >= 1)
            {
                time.Game_ElapsedTimeInMonth = 0;
                time.Game_ElapsedTimeInYear += 1;
            }

        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependency)
    {
        var deltaTime = Time.deltaTime;
        var RecordTimeJob = new RecordTime
        {
            deltaTime = deltaTime,
        };
        var RecordTimeJobHandle = RecordTimeJob.ScheduleGroup(m_HumanGroup, inputDependency);

        // var RunTimerJob = new RunTimer
        // {
        //     deltaTime = deltaTime,
        // };
        // var RunTimerJobHandle = RunTimerJob.ScheduleGroup(m_TimerGroup, inputDependency);

        //var TimeJobHandleBarrier = JobHandle.CombineDependencies( RecordTimeJobHandle, RunTimerJobHandle );
        
        inputDependency = RecordTimeJobHandle;
        return inputDependency;
    }
    protected override void OnCreateManager()
    {
        m_HumanGroup = GetComponentGroup( new EntityArchetypeQuery {
            All = new[] {
                ComponentType.ReadWrite<TimeRecord>(),
                ComponentType.ReadOnly<HumanStateFactor>(),
                ComponentType.ReadOnly<HumanStockFactor>(),
            },
        });
        // m_TimerGroup = GetComponentGroup( new EntityArchetypeQuery {
        //     All =  new[] {
        //         ComponentType.ReadWrite<Timers>(),
        //     }
        // });
    }
}