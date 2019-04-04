using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;

public class BehaviourSystem : JobComponentSystem
{
    private bool IsInitialize = false;
    public ComponentGroup m_ExecuteBehaviourGroup;
    private List<BehaviourValue> m_UniqueType = new List<BehaviourValue>(5);
    private NativeArray<ByteBool> IsTimerStarted;
    private NativeArray<int> StartRecordedTimes;
    public int executingCount;
    


    // Problem with NativeArray, if you need to deal with data within different system, you have to put it back to component.
    [RequireComponentTag(typeof(BehaviourValue))]
    [BurstCompile]
    public struct ProcessFactor : IJobProcessComponentDataWithEntity<HumanStateFactor, HumanStockFactor, BehaviourType, NavigationTag, Timers>
    {
        // Settings
        [ReadOnly] public BehaviourValue settings;
        public NativeArray<ByteBool> IsTimerStarted;
        public NativeArray<int> StartRecordedTimes;
        // int gainFactor;
        // int payFactor;
        
        // Various behaviour has different effect.
        private void CalculateBehaviourEffect(ref int gainFactor, ref int payFactor, int gainValue, int payValue)
        {
            gainFactor += gainValue;
            payFactor  -= payValue;
        }

        void PrepareValueForBehaviour(BehaviourTypes behaviourTypes, BehaviourValue behaviourValue, HumanStateFactor stateFactor,  HumanStockFactor stockFactor, out int gainValue, out int payValue, out int coolDownTime)
        {
            switch (behaviourTypes)
            {
                case BehaviourTypes.Eat:
                    //gainFactor = stateFactor.Hungry;
                    gainValue = settings.EatGain;
                    payValue = settings.EatCost;
                    coolDownTime = settings.EatCoolDownInMinute;
                    break;

                case BehaviourTypes.Drink:
                    gainValue = settings.DrinkGain;
                    payValue = settings.DrinkCost;
                    coolDownTime = settings.DrinkCoolDownInMinute;
                    break;

                // Do nothing.
                default:
                    gainValue = settings.EatGain;
                    payValue = settings.EatCost;
                    coolDownTime = settings.EatCoolDownInMinute;
                    break;
            }
        }
        // How to implement Cooldown?
        public void Execute(Entity entity, int index, ref HumanStateFactor stateFactor, ref HumanStockFactor stockFactor, [ReadOnly]ref BehaviourType behaviourType, [ReadOnly]ref NavigationTag navigationTag, [ReadOnly]ref Timers timers)
        {
            
            // if arrived, behave.
            if (navigationTag.Arrived)
            {   
                PrepareValueForBehaviour (behaviourType.Behaviour, settings, stateFactor, stockFactor, out int gainValue, out int payValue, out int coolDownTime);
                // If Timer Haven't Start. Record Start Time.
                // if ( !IsTimerStarted[index] )   
                // { 
                //     StartRecordedTimes[index] = time.Game_ElapsedTimeInMinute; 
                //     IsTimerStarted[index] = true;
                // }
                // // Timer Finished
                // if ( math.abs (time.Game_ElapsedTimeInMinute - StartRecordedTimes[index]) > coolDownTime )
                // {
                if ( !timers.timer.Started)
                {
                    timers.timer.Duration = coolDownTime;
                    timers.timer.elapsedTime = 0;
                    timers.timer.Started = true;
                    timers.timer.Running = true;
                }
                if ( timers.timer.Finished)
                { 
                    switch (behaviourType.Behaviour)
                    {
                        case BehaviourTypes.Eat :
                        CalculateBehaviourEffect(ref stateFactor.Hungry, ref stockFactor.Food, gainValue, payValue); 
                        break;
                    case BehaviourTypes.Drink :
                        CalculateBehaviourEffect(ref stateFactor.Thirsty, ref stockFactor.Water, gainValue, payValue);
                        break;
                    }
                    timers.timer.Started = false;
                }
            }       
        }
    }

    protected override void OnStopRunning()
    {
        StartRecordedTimes.Dispose();

        IsTimerStarted.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependency)
    {
        if (!IsInitialize)
        {
            executingCount     = m_ExecuteBehaviourGroup.CalculateLength();
            StartRecordedTimes = new NativeArray<int>(executingCount, Allocator.Persistent);
            IsTimerStarted     = new NativeArray<ByteBool>(executingCount, Allocator.Persistent);
            IsInitialize       = true;
        }

        EntityManager.GetAllUniqueSharedComponentData(m_UniqueType);

        // index0 has each value 0. 
        // For now I have nothing special in value, so just use 1.
        var settings = m_UniqueType[1];

        // iterate this way might be faster..
        m_ExecuteBehaviourGroup.SetFilter(settings);

        var ProcessFactorJob = new ProcessFactor
        {
            settings           = settings,
            StartRecordedTimes = StartRecordedTimes,
            IsTimerStarted        = IsTimerStarted
        };
        var ProcessFactorJobHandle = ProcessFactorJob.ScheduleGroup(m_ExecuteBehaviourGroup, inputDependency); 

        m_UniqueType.Clear();

        inputDependency = ProcessFactorJobHandle;
        return inputDependency;
    }

    // TEST
    public void LoopDebug(NativeArray<int> inputArray)
    {
        for (int i = 0; i < inputArray.Length; i++)
        {
            Debug.Log($"Array[{i}] = {inputArray[i]}");
        }
    } 


    protected override void OnCreateManager()
    {
        m_ExecuteBehaviourGroup = GetComponentGroup(new EntityArchetypeQuery{
            All = new[] {
                ComponentType.ReadOnly<BehaviourType>(),
                ComponentType.ReadOnly<BehaviourValue>(),
                ComponentType.ReadWrite<HumanStateFactor>(),
                ComponentType.ReadWrite<HumanStockFactor>(),
                ComponentType.ReadOnly<NavigationTag>(),
                ComponentType.ReadOnly<Timers>(),
            },
        });     
        // Cannot get manager in OnCreateManager?
    }

}