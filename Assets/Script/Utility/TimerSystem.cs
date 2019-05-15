using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public struct Timer : IBufferElementData
{
    public float ElapsedTime;
    private float m_duration;
    public bool  Running;
    public bool  Started;

    public float Duration
    {
        get => m_duration;
        set
        {
            if (value > 0 && !Running) m_duration = value;
        }
    }

    public bool Finished => Started && !Running;

    public void Run()
    {
        ElapsedTime = 0;
        Started     = true;
        Running     = true;
    }

    public void Stop()
    {
        ElapsedTime = 0;
        Started     = false;
        Running     = false;
    }
}

// foreach buffer

// update each element in it.
public class TimerSystem : JobComponentSystem
{
    private EntityQuery m_timerGroup;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var chunks = m_timerGroup.CreateArchetypeChunkArray(Allocator.TempJob);

        return new TimerUpdateJob
        {
            Chunks     = chunks,
            TimersType = GetArchetypeChunkBufferType<Timer>(),
            DeltaTime  = Time.deltaTime
        }.Schedule(inputDeps);
    }

    protected override void OnCreate()
    {
        m_timerGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(Timer)
            }
        });
    }

    public struct TimerUpdateJob : IJob
    {
        [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk>     Chunks;
        public                             ArchetypeChunkBufferType<Timer> TimersType;
        public                             float                           DeltaTime;

        public void Execute()
        {
            // how many chunks?
            for (var i = 0; i < Chunks.Length; i++)
            {
                var chunk          = Chunks[i];
                var timersAccessor = chunk.GetBufferAccessor(TimersType);

                // I think chunk.Count == timersAccessor.Length == naArray<Archetype>.Length
                // how many entities in this chunk?
                for (var j = 0; j < chunk.Count; j++)
                {
                    var timers = timersAccessor[j];
                    // how many timer in Timers Array?
                    for (var k = 0; k < timers.Length; k++)
                    {
                        var copyTimer = timers[k];
                        //Debug.Log("not running");
                        if (copyTimer.Running)
                        {
                            //Debug.Log("running");
                            timers[k] = new Timer
                            {
                                Started     = timers[k].Started,
                                Running     = timers[k].Running,
                                ElapsedTime = timers[k].ElapsedTime + DeltaTime,
                                Duration    = timers[k].Duration
                            };
                            if (copyTimer.ElapsedTime > copyTimer.Duration)
                                timers[k] = new Timer
                                {
                                    Running = false,
                                    Started = true
                                };
                        }
                    }
                }
            }
        }
    }
}