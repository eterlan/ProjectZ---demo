// using System;
// using Unity.Collections;
// using Unity.Entities;
//
// namespace ProjectZ.AI.GOAP
// {
//     public enum ActionType
//     {
//     }
//
//     public enum PreconditionType
//     {
//     }
//
//     public enum EffectType
//     {
//     }
//
//     public enum StateType
//     {
//     }
//
//     public enum GoalType
//     {
//     }
//
//     public enum CompareType
//     {
//         True,
//         False,
//         Larger,
//         Smaller,
//         Equal,
//     }
//
//     public struct Goal
//     {
//         public GoalType         GoalType;
//         public PreconditionType Precondition;
//     }
//
//     public struct CurrentGoal : IComponentData
//     {
//         public int Index;
//     }
//
//     public class Goap : ComponentSystem
//     {
//         struct Action
//         {
//             // Action Index
//             public NativeMultiHashMap<int, StateType> Preconditions;
//             public NativeMultiHashMap<int, StateType> Effects;
//         }
//
//         struct State
//         {
//             // State Index
//             public NativeArray<int>                    Preconditions;
//             public NativeMultiHashMap<int, ActionType> StateSatisfyActions;
//             public NativeMultiHashMap<int, ActionType> StateRequireActions;
//             public NativeArray<CompareType>            CompareTypes;
//             public NativeArray<int>                    Effects;
//             public NativeArray<int>                    CurrentStates;
//         }
//
//         struct Goal
//         {
//             // Goal Index
//             public NativeArray<StateType> Preconditions;
//         }
//
//         Action m_action = new Action
//         {
//             Preconditions = new NativeMultiHashMap<int, StateType>(), 
//             Effects =new NativeMultiHashMap<int, StateType>() ,
//         };
//         State m_state = new State
//         {
//             Preconditions            = new NativeArray<int>(),
//             StateSatisfyActions      = new NativeMultiHashMap<int, ActionType>(),
//             StateRequireActions      = new NativeMultiHashMap<int, ActionType>(),
//             CompareTypes             = new NativeArray<CompareType>(),
//             Effects                  = new NativeArray<int>(),
//             CurrentStates            = new NativeArray<int>(),
//         };
//         Goal m_goal = new Goal
//         {
//             Preconditions = new NativeArray<StateType>(),
//         };
//
//         protected override void OnUpdate()
//         {
//             // @Todo 世界状态与precondition都是同样的Index
//             // Find the prevAction's precondition == currAction's effect
//             var currGoal         = new CurrentGoal { }.Index;
//             var goalPrecondition = m_goal.Preconditions[currGoal];
//             var action           = FindMatchingAction(goalPrecondition);
//
//             // @Todo should operator be considered ? sure later.
//             // 如何存放比较条件？还是说放弃简单的数据类型，转而用struct？
//             //bool match = Compare(goalPrecondition);
//
//             // @Todo 似乎可以写进物品相关信息里。或者把行为直接用编号串联起来。
//             ActionType FindMatchingAction(StateType type)
//             {
//                 var stateIndex = (int) type;
//                 var state             = m_state.CurrentStates[stateIndex];
//                 var precondition      = m_state.Preconditions[stateIndex];
//                 var compareType       = m_state.CompareTypes[stateIndex];
//
//                 // 遍历Action，如果谁的Effect = precondition，返回这个Action
//
//                 // @Todo 尝试每个Action的Effect, and cache it to the NMHM<> for different states.
//                 while (actionE.TryGetFirstValue(0, out var item, out var it))
//                 {
//                     var effectIndex = (int) item;
//                     var effect      = effects[effectIndex];
//                     var match       = Compare(state, effect, compareType, precondition);
//
//                     // @Todo How to deal with different part didn't match? 
//                     if (match) { }
//
//                     while (actionE.TryGetNextValue(out item, ref it)) { }
//                 }
//
//                 return;
//             }
//
//
//             bool Compare(
//                 int         state,
//                 CompareType compareType,
//                 int         precondition)
//             {
//                 switch (compareType)
//                 {
//                     case CompareType.True:    return state == 1;
//                     case CompareType.False:   return state != 1;
//                     case CompareType.Larger:  return state >= precondition;
//                     case CompareType.Smaller: return state < precondition;
//                     case CompareType.Equal:   return state == precondition;
//                     default:                  throw new IndexOutOfRangeException(compareType.ToString());
//                 }
//             }
//         }
//
//         protected override void OnCreate() { }
//
//         protected override void OnDestroy() { }
//     }
// }