// using Unity.Entities;
// using UnityEngine;
// using Unity.Burst;
// using Unity.Jobs;
// using Unity.Collections;
// using Unity.Mathematics;
// using System.Collections.Generic;
// using Unity.Transforms;

// public class BehaviourSystem : JobComponentSystem
// {
//     private bool IsInitialize = false;
//     public EntityQuery m_ExecuteBehaviourGroup;
//     private List<BehaviourValue> m_UniqueType = new List<BehaviourValue>(5);
    
//     // For add Timer purpose.
//     public EntityQuery m_BeforeAdded;

    
//     // Problem with NativeArray, if you need to deal with data within different system, you have to put it back to component.
//     [RequireComponentTag(typeof(BehaviourValue))]
//     //[BurstCompile]
//     public struct ProcessFactor : IJobForEachWithEntity<HumanStateFactor, HumanStockFactor, BehaviourType, NavigationTag, Timer<BehaviourValue>>
//     {
//         [ReadOnly] public BehaviourValue settings;
        
//         // Various behaviour has different effect.
//         private void CalculateBehaviourEffect(ref int gainFactor, ref int payFactor, int gainValue, int payValue)
//         {
//             gainFactor += gainValue;
//             payFactor  -= payValue;
//         }

//         void PrepareValueForBehaviour(BehaviourTypes behaviourTypes, BehaviourValue behaviourValue, HumanStateFactor stateFactor,  HumanStockFactor stockFactor, out int gainValue, out int payValue, out int coolDownTime)
//         {
//             switch (behaviourTypes)
//             {
//                 case BehaviourTypes.Eat:
//                     gainValue = settings.EatGain;
//                     // 
//                     // gainFactorIndex = gainFactorIndexes[behaviourType]
//                     // costFactorIndex = costFactorIndexes[behaviourType]
//                     // GainFactor = Factors[gainFactorIndex] (buffer)
//                     // CostFactor = Factors[costFactorIndex] (buffer)
//                     // GainValue = settings.Values[behaviourType][ItemIndex];
//                     // or? FoodGainValue = settings.foods[foodIndex]
//                     payValue = settings.EatCost;
//                     coolDownTime = settings.EatCoolDownInMinute;
//                     break;

//                 case BehaviourTypes.Drink:
//                     gainValue = settings.DrinkGain;
//                     payValue = settings.DrinkCost;
//                     coolDownTime = settings.DrinkCoolDownInMinute;
//                     break;

//                 // Do nothing.
//                 default:
//                     gainValue = settings.EatGain;
//                     payValue = settings.EatCost;
//                     coolDownTime = settings.EatCoolDownInMinute;
//                     break;
//             }
//         }
//         // How to implement Cooldown?
//         public void Execute(Entity entity, int index, ref HumanStateFactor stateFactor, ref HumanStockFactor stockFactor, [ReadOnly]ref BehaviourType behaviourType, [ReadOnly]ref NavigationTag navigationTag, ref Timer<BehaviourValue> behavTimer)
//         {
//             //var d= ComponentDataFromEntity<BehaviourType>;
//             // if arrived, behave.
//             if (navigationTag.Arrived)
//             {   
//                 PrepareValueForBehaviour (behaviourType.Behaviour, settings, stateFactor, stockFactor, out int gainValue, out int payValue, out int coolDownTime);
//                 // 0.1 
//                 // If Timer Haven't Start. Record Start Time.
//                 // if ( !IsTimerStarted[index] )   
//                 // { 
//                 //     StartRecordedTimes[index] = time.Game_ElapsedTimeInMinute; 
//                 //     IsTimerStarted[index] = true;
//                 // }
//                 // // Timer Finished
//                 // if ( math.abs (time.Game_ElapsedTimeInMinute - StartRecordedTimes[index]) > coolDownTime )
//                 // {
//                 // 0.2
//                 // if ( !timers.timer.Started)
//                 // {
//                 //     timers.timer.Duration = coolDownTime;
//                 //     timers.timer.Run();
//                 // }
//                 // if ( timers.timer.Finished)
//                 // { 
//                 //     switch (behaviourType.Behaviour)
//                 //     {
//                 //         case BehaviourTypes.Eat :
//                 //         CalculateBehaviourEffect(ref stateFactor.Hungry, ref stockFactor.Food, gainValue, payValue); 
//                 //         break;
//                 //     case BehaviourTypes.Drink :
//                 //         CalculateBehaviourEffect(ref stateFactor.Thirsty, ref stockFactor.Water, gainValue, payValue);
//                 //         break;
//                 //     }
//                 //     timers.timer.Started = false;
//                 // }

//                 // Schedule Timer when not started.
//                 if ( !behavTimer.Started )
//                 {
//                     behavTimer.Duration = coolDownTime;
//                     behavTimer.Run();
//                 } 
//                 // Timer finished so start to do sth.
//                 if ( behavTimer.Finished)
//                 { 
//                     switch (behaviourType.Behaviour)
//                     {
//                         case BehaviourTypes.Eat :
//                         CalculateBehaviourEffect(ref stateFactor.Hungry, ref stockFactor.Food, gainValue, payValue); 
//                         break;
//                     case BehaviourTypes.Drink :
//                         CalculateBehaviourEffect(ref stateFactor.Thirsty, ref stockFactor.Water, gainValue, payValue);
//                         break;
//                     }
//                     behavTimer.Stop();
//                 }
//             }       
//         }
//     }

//     protected override JobHandle OnUpdate(JobHandle inputDependency)
//     {
//         if (!IsInitialize)
//         {
//             IsInitialize       = true;
//             EntityManager.AddComponent(m_BeforeAdded, ComponentType.ReadWrite<Timer<BehaviourValue>>());
//         }

//         EntityManager.GetAllUniqueSharedComponentData(m_UniqueType);

//         // index0 has each value 0. 
//         // For now I have nothing special in value, so just use 1.
//         var settings = m_UniqueType[1];

//         // iterate this way might be faster..
//         m_ExecuteBehaviourGroup.SetFilter(settings);

//         var ProcessFactorJob = new ProcessFactor
//         {
//             settings           = settings,
//         };
//         var ProcessFactorJobHandle = ProcessFactorJob.Schedule(m_ExecuteBehaviourGroup,inputDependency);
//         m_UniqueType.Clear();

//         // // TEST
//         // test_directly = HumanStateFactor.MaxValue[1];
//         // Debug.Log(test[0]+"aaa"+test.Length);
//         // Debug.Log("Inupdate"+test_directly);

//         inputDependency = ProcessFactorJobHandle;
//         return inputDependency;
//     }

//     // TEST
//     // public void LoopDebug(NativeArray<Entity> array)
//     // {
//     //     for (int i = 0; i < array.Length; i++)
//     //     {
//     //         var hasComponent = EntityManager.HasComponent<Timer<BehaviourValue>>(array[i]);
//     //         Debug.Log($"Array[{i}] = {hasComponent}");
//     //     }
//     // } 


//     protected override void OnCreateManager()
//     {
//         // valid. 
//         // so, use struct or anything else to initialize array? fixed array? how to use ISharedComponentData?
//         // test = new NativeArray<int>(HumanStateFactor.MaxValue,Allocator.Persistent);
        
//         // test[0] = 10;
//         m_BeforeAdded = GetEntityQuery(new EntityQueryDesc{
//             All = new[] {
//                 ComponentType.ReadOnly<BehaviourType>(),
//                 ComponentType.ReadOnly<BehaviourValue>(),
//                 ComponentType.ReadWrite<HumanStateFactor>(),
//                 ComponentType.ReadWrite<HumanStockFactor>(),
//                 ComponentType.ReadOnly<NavigationTag>(),
//                 // is it possible RO?
//             },
//         }); 
//         // Cannot get manager in OnCreateManager?
//         m_ExecuteBehaviourGroup = GetEntityQuery(new EntityQueryDesc{
//             All = new[] {
//                 ComponentType.ReadOnly<BehaviourType>(),
//                 ComponentType.ReadOnly<BehaviourValue>(),
//                 ComponentType.ReadWrite<HumanStateFactor>(),
//                 ComponentType.ReadWrite<HumanStockFactor>(),
//                 ComponentType.ReadOnly<NavigationTag>(),
//                 // is it possible RO? yes.. that might be a problem
//                 ComponentType.ReadWrite<Timer<BehaviourValue>>(),
//             },
//         }); 
//     }
// }