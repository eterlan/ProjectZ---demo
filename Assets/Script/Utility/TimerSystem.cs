using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct Timer : IBufferElementData
{
    public float elapsedTime;
    public float duration;
    public bool running;
    public bool started;
    
    public bool Started
    {
        set { started = value; }
        get { return started; }
    }
    public float Duration
    {
        get { return duration; }
        set 
        {
            if ( value > 0 && !Running )
            {
                duration = value;                 
            }
        }
    }
    public bool Finished
    {
        get { return Started && !Running; }
    }

    public bool Running
    {
        get { return running; }
        set { running = value; }
    }
    
    public void Run()
    {
        elapsedTime = 0;
        started = true;
        running = true;
    }
    public void Stop()
    {
        elapsedTime = 0;
        started = false;
        running = false;
    }
}

// foreach buffer

// update each element in it.
public class TimerSystem : JobComponentSystem
{
    private EntityQuery m_TimerGroup;
   
    public struct TimerUpdateJob : IJob
    {
        [DeallocateOnJobCompletion]
        public NativeArray<ArchetypeChunk> chunks;
        public ArchetypeChunkBufferType<Timer> timersType;
        public float deltaTime;
        public void Execute()
        {
            // how many chunks?
            for (int i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                var timersAccessor = chunk.GetBufferAccessor<Timer>(timersType);

                // I think chunk.Count == timersAccessor.Length == naArray<Archetype>.Length
                // how many entities in this chunk?
                for (int j = 0; j < chunk.Count; j++)
                {
                    var timers = timersAccessor[j];
                    // how many timer in Timers Array?
                    for (int k = 0; k < timers.Length; k++)
                    {
                        var copyTimer = timers[k];
                        //Debug.Log("not running");
                        if (copyTimer.Running)
                        {
                            //Debug.Log("running");
                            timers[k] = new Timer
                            {
                                started = timers[k].started,
                                running = timers[k].running,
                                elapsedTime = timers[k].elapsedTime + deltaTime,
                                duration = timers[k].duration,
                            };
                            if (copyTimer.elapsedTime > copyTimer.Duration)
                            {
                                timers[k] = new Timer
                                {
                                    running = false,
                                    started = true,
                                };
                            }
                        }   
                    }                    
                }
            }            
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var chunks = m_TimerGroup.CreateArchetypeChunkArray(Allocator.TempJob);

        return new TimerUpdateJob{
            chunks = chunks,
            timersType = GetArchetypeChunkBufferType<Timer>(false),
            deltaTime = Time.deltaTime,
        }.Schedule(inputDeps);
    }
    protected override void OnCreate()
    {
        m_TimerGroup = GetEntityQuery(new EntityQueryDesc{
            All = new ComponentType[] {
                typeof(Timer),
            }
        });
    }
}
