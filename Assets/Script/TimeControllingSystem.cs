using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class TimeControllingSystem : JobComponentSystem
{
    private EntityQuery m_humanGroup;
    private EntityQuery m_timerGroup;

    protected override JobHandle OnUpdate(JobHandle inputDependency)
    {
        var deltaTime = Time.deltaTime;
        var recordTimeJob = new RecordTime
        {
            DeltaTime = deltaTime
        };
        var recordTimeJobHandle = recordTimeJob.Schedule(m_humanGroup, inputDependency);

        // var RunTimerJob = new RunTimer
        // {
        //     deltaTime = deltaTime,
        // };
        // var RunTimerJobHandle = RunTimerJob.Schedule(m_TimerGroup, inputDependency);

        //var TimeJobHandleBarrier = JobHandle.CombineDependencies( RecordTimeJobHandle, RunTimerJobHandle );

        inputDependency = recordTimeJobHandle;
        return inputDependency;
    }

    protected override void OnCreateManager()
    {
        m_humanGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadWrite<TimeRecord>(),
                ComponentType.ReadOnly<HumanStateFactor>(),
                ComponentType.ReadOnly<HumanStockFactor>()
            }
        });
        // m_TimerGroup = GetEntityQuery( new EntityArchetypeQuery {
        //     All =  new[] {
        //         ComponentType.ReadWrite<Timers>(),
        //     }
        // });
    }

    // public struct RunTimer : IJobForEach<Timers>
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
    public struct RecordTime : IJobForEach<TimeRecord>
    {
        public float DeltaTime;

        public void Execute(ref TimeRecord time)
        {
            time.RealElapsedTimeInSecond += DeltaTime;

            // If it's 0, use 1 instead.
            math.select(time.ModifiedTimeElapsedSpeed, 1, time.ModifiedTimeElapsedSpeed == 0);

            time.GameElapsedTimeInSecond += DeltaTime * time.DefaultTimeElapsedSpeed * time.ModifiedTimeElapsedSpeed;

            if (time.GameElapsedTimeInSecond >= 60)
            {
                time.GameElapsedTimeInSecond =  0;
                time.GameElapsedTimeInMinute += 1;
            }

            if (time.GameElapsedTimeInMinute >= 60)
            {
                time.GameElapsedTimeInMinute =  0;
                time.GameElapsedTimeInHour   += 1;
            }

            if (time.GameElapsedTimeInHour >= 24)
            {
                time.GameElapsedTimeInHour =  0;
                time.GameElapsedTimeInDay  += 1;
            }

            if (time.GameElapsedTimeInDay >= 30)
            {
                time.GameElapsedTimeInDay   =  0;
                time.GameElapsedTimeInMonth += 1;
            }

            if (time.GameElapsedTimeInMonth >= 1)
            {
                time.GameElapsedTimeInMonth =  0;
                time.GameElapsedTimeInYear  += 1;
            }
        }
    }
}