using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class BehaviourSystem : JobComponentSystem
{
    private bool m_isInitialize;

    // For add Timer purpose.
    private           EntityQuery          m_beforeAdded;
    private           EntityQuery          m_executeBehaviourGroup;
    private readonly List<BehaviourValue> m_uniqueType = new List<BehaviourValue>(5);

    protected override JobHandle OnUpdate(JobHandle inputDependency)
    {
        if (!m_isInitialize)
        {
            m_isInitialize = true;

            //Interesting, gather info of entity cannot be allocate with temp memory because it's a job inside.
            
            var entities = m_beforeAdded.ToEntityArray(Allocator.TempJob);
            //Debug.Log(Entities.Length);
            for (var i = 0; i < entities.Length; i++)
            {
                EntityManager.AddBuffer<Timer>(entities[i]);
                var timers = EntityManager.GetBuffer<Timer>(entities[i]);
                timers.Add(new Timer());
                //Debug.Log(Timers.Length);
            }

            entities.Dispose();
        }

        var timersAccssor = GetBufferFromEntity<Timer>();

        EntityManager.GetAllUniqueSharedComponentData(m_uniqueType);


        // index0 has each value 0. 
        // For now I have nothing special in value, so just use 1.
        var settings = m_uniqueType[1];

        // iterate this way might be faster..
        m_executeBehaviourGroup.SetFilter(settings);

        var processFactorJob = new ProcessFactor
        {
            TimersAccssor = timersAccssor,
            Settings      = settings
        };
        var processFactorJobHandle = processFactorJob.Schedule(m_executeBehaviourGroup, inputDependency);
        m_uniqueType.Clear();

        // // TEST
        // test_directly = HumanStateFactor.MaxValue[1];
        // Debug.Log(test[0]+"aaa"+test.Length);
        // Debug.Log("Inupdate"+test_directly);

        inputDependency = processFactorJobHandle;
        return inputDependency;
    }

    // TEST
    // public void LoopDebug(NativeArray<Entity> array)
    // {
    //     for (int i = 0; i < array.Length; i++)
    //     {
    //         var hasComponent = EntityManager.HasComponent<Timer<BehaviourValue>>(array[i]);
    //         Debug.Log($"Array[{i}] = {hasComponent}");
    //     }
    // } 


    protected override void OnCreateManager()
    {
        // valid. 
        // so, use struct or anything else to initialize array? fixed array? how to use ISharedComponentData?
        // test = new NativeArray<int>(HumanStateFactor.MaxValue,Allocator.Persistent);

        // test[0] = 10;
        m_beforeAdded = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<BehaviourType>(),
                ComponentType.ReadOnly<BehaviourValue>(),
                ComponentType.ReadWrite<HumanStateFactor>(),
                ComponentType.ReadWrite<HumanStockFactor>(),
                ComponentType.ReadOnly<NavigationTag>()
                // is it possible RO?
            }
        });
        // Cannot get manager in OnCreateManager?
        m_executeBehaviourGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<BehaviourType>(),
                ComponentType.ReadOnly<BehaviourValue>(),
                ComponentType.ReadWrite<HumanStateFactor>(),
                ComponentType.ReadWrite<HumanStockFactor>(),
                ComponentType.ReadOnly<NavigationTag>(),
                // is it possible RO? yes.. that might be a problem
                ComponentType.ReadWrite<Timer>()
            }
        });
    }


    // Problem with NativeArray, if you need to deal with data within different system, you have to put it back to component.
    [RequireComponentTag(typeof(BehaviourValue))]
    //[BurstCompile]
    public struct
        ProcessFactor : IJobForEachWithEntity<HumanStateFactor, HumanStockFactor, BehaviourType, NavigationTag>
    {
        [NativeDisableParallelForRestriction] public BufferFromEntity<Timer> TimersAccssor;
        [ReadOnly]                            public BehaviourValue          Settings;

        // Various behaviour has different effect.
        private void CalculateBehaviourEffect(ref int gainFactor, ref int payFactor, int gainValue, int payValue)
        {
            gainFactor += gainValue;
            payFactor  -= payValue;
        }

        private void PrepareValueForBehaviour(BehaviourTypes behaviourTypes, BehaviourValue behaviourValue,
            HumanStateFactor stateFactor, HumanStockFactor stockFactor, out int gainValue, out int payValue,
            out int coolDownTime)
        {
            switch (behaviourTypes)
            {
                case BehaviourTypes.Eat:
                    gainValue = Settings.eatGain;
                    // 
                    // gainFactorIndex = gainFactorIndexes[behaviourType]
                    // costFactorIndex = costFactorIndexes[behaviourType]
                    // GainFactor = Factors[gainFactorIndex] (buffer)
                    // CostFactor = Factors[costFactorIndex] (buffer)
                    // GainValue = settings.Values[behaviourType][ItemIndex];
                    // or? FoodGainValue = settings.foods[foodIndex]
                    payValue     = Settings.eatCost;
                    coolDownTime = Settings.eatCoolDownInMinute;
                    break;

                case BehaviourTypes.Drink:
                    gainValue    = Settings.drinkGain;
                    payValue     = Settings.drinkCost;
                    coolDownTime = Settings.drinkCoolDownInMinute;
                    break;

                // Do nothing.
                default:
                    gainValue    = Settings.eatGain;
                    payValue     = Settings.eatCost;
                    coolDownTime = Settings.eatCoolDownInMinute;
                    break;
            }
        }

        // How to implement Cooldown?
        public void Execute(Entity entity, int index, ref HumanStateFactor stateFactor,
            ref HumanStockFactor stockFactor, [ReadOnly] ref BehaviourType behaviourType,
            [ReadOnly] ref NavigationTag navigationTag)
        {
            //Debug.Log("hello");
            //var d= ComponentDataFromEntity<BehaviourType>;
            // if arrived, behave.
            if (navigationTag.Arrived)
            {
                //Debug.Log("arrived");
                PrepareValueForBehaviour(behaviourType.Behaviour, Settings, stateFactor, stockFactor, out var gainValue,
                    out var payValue, out var coolDownTime);
                // 0.1 
                // If Timer Haven't Start. Record Start Time.
                // if ( !IsTimerStarted[index] )   
                // { 
                //     StartRecordedTimes[index] = time.Game_ElapsedTimeInMinute; 
                //     IsTimerStarted[index] = true;
                // }
                // // Timer Finished
                // if ( math.abs (time.Game_ElapsedTimeInMinute - StartRecordedTimes[index]) > coolDownTime )
                // {
                // 0.2
                // if ( !timers.timer.Started)
                // {
                //     timers.timer.Duration = coolDownTime;
                //     timers.timer.Run();
                // }
                // if ( timers.timer.Finished)
                // { 
                //     switch (behaviourType.Behaviour)
                //     {
                //         case BehaviourTypes.Eat :
                //         CalculateBehaviourEffect(ref stateFactor.Hungry, ref stockFactor.Food, gainValue, payValue); 
                //         break;
                //     case BehaviourTypes.Drink :
                //         CalculateBehaviourEffect(ref stateFactor.Thirsty, ref stockFactor.Water, gainValue, payValue);
                //         break;
                //     }
                //     timers.timer.Started = false;
                // }
                // Because it's value type.

                var timers = TimersAccssor[entity];
                // Schedule Timer when not started.
                if (!timers[0].Started)
                    timers[0] = new Timer
                    {
                        Running     = true,
                        Started     = true,
                        ElapsedTime = 0,
                        Duration    = coolDownTime
                    };
                //Debug.Log("started"+timersAccssor[entity][0].Duration+timers[0].duration);

                // Timer finished so start to do sth.
                if (timers[0].Finished)
                {
                    //Debug.Log("finished");
                    switch (behaviourType.Behaviour)
                    {
                        case BehaviourTypes.Eat:
                            CalculateBehaviourEffect(ref stateFactor.Hungry, ref stockFactor.Food, gainValue, payValue);
                            break;
                        case BehaviourTypes.Drink:
                            CalculateBehaviourEffect(ref stateFactor.Thirsty, ref stockFactor.Water, gainValue,
                                payValue);
                            break;
                    }

                    // reset
                    timers[0] = new Timer();
                }
            }
        }
    }
}