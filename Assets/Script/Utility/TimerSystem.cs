using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct Timer<T> : IComponentData
{
    public float elapsedTime;
    private float duration;
    private bool running;
    private bool started;
    
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

public class TimerUpdateGroup : ComponentSystemGroup { }
[UpdateInGroup(typeof(TimerUpdateGroup))]
public abstract class TimerSystem<T> : JobComponentSystem where T : struct
{
    public struct TimerUpdateJob : IJobProcessComponentData<Timer<T>>
    {
        public float deltaTime;
        public void Execute(ref Timer<T> c0)
        {
            if (c0.Running)
            {
                c0.elapsedTime += deltaTime;
                if (c0.elapsedTime > c0.Duration)
                {
                    c0.Running = false;
                }
            } 
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new TimerUpdateJob{
            deltaTime = Time.deltaTime,
        }.Schedule(this, inputDeps);
    }
}

//somewhere...
//EntityManager.AddComponentData(entity, new Timer<MySpecificTimer>());

public class BehaviourTimerSystem : TimerSystem<BehaviourValue> 
{
    
}