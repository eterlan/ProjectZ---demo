# How to write a Timer?

计时器这个东西以前学过怎么写，但当时只是觉得老师写的特好用，并没有搞明白为什么要这样写，这次写ECS的时候本想再写一个，结果发现，事情似乎并没有这么简单。

# What's the problem?

一开始我想的是，我用一个变量数组储存entity到达目的地的这个时间，记为初始时间，如果即时时间 > 初始时间，运行结束，执行行为。

# How many Bool is required?

1. 一个bool
```C#
if (inSomeState)
{
    if (!initialized)
    {
        initialized = true;
        elapsedTime = 0;
        duration = x;
    }
    elapsedTime += deltaTime;
    if (elapsedTime > duration)
    {
        initialized = false;
        // do sth cool, turn to another state.
    }
}
```
2. 两个bool
```C#
if (inSomeState)
{
    if (!started)
    {
        started = true;
        elapsedTime = 0;
        duration = x;
    }
    elapsedTime += deltaTime;
    finished = elapsedTime > duration;
    if (finished)
    {
        started = false;
        // do sth cool, turn to another state.
    }
}
```

3. 3个bool
```C#
if (inSomeState)
{
    if (!running)
    {
        started = true;
        running = true;
        elapsedTime = 0;
        duration = x;
    }
    elapsedTime += deltaTime;
    if ( elapsedTime > duration)
        running = false;
    var finished = started && !running;
    if (finished)
    {
        // do sth cool, turn to another state.
    }
}
```

记录非Generic Timer
```c#
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
```

Finally 
我认为不应该实现做一个Timer，因为如果需要，那么难以避免暴露很多属性给外界，这样的话一旦忘记如何实现的，面对很多变量会给程序员增加负担。而简单