// using System;
// using System.Collections.Generic;
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
//     // @Bug NativeArray cannot contain managed type. NativeArray itself is managed. 
//     struct Action // half SOA OOP？跟下面那种都需要放在NativeArray<Action> 
//     {
//         public ActionType             Type;
//         public NativeArray<StateType> Preconditions;
//         public NativeArray<int>       PreconditionValues;
//         
//         public NativeArray<StateType> Effects;
//         public NativeArray<int>       EffectValues;
//     }
//
//     struct Action_OOP // pure AOS I guess.
//     {
//         public NativeArray<Precondition> Preconditions;
//         public NativeArray<Effect>       Effects;
//     }
//
//     public struct Effect
//     {
//         public int        Value;
//         public StateType  Type;
//     }
//
//     public struct Precondition
//     {
//         public int         Value;
//         public StateType   Type;
//         public CompareType CompareType;
//     }
//
//     public struct Goal
//     {
//         public GoalType         Type;
//         public PreconditionType Precondition;
//     }
//     public struct State
//     {
//         public StateType               Type;
//         public NativeArray<ActionType> SatisfyActions;
//         public NativeArray<ActionType> RequireActions;
//     }
//
//     public struct CurrentGoal : IComponentData
//     {
//         public int Index;
//     }
//
//     public struct WorldState
//     {
//         public StateType Type;
//         public int       Value;
//     } //: IBufferElementData { public int Value;}
//
//     public class GOAP : ComponentSystem
//     {
//         private NativeArray<Action> m_actions;
//         private NativeArray<Goal>   m_goals;
//         private NativeArray<State>  m_states;
//
//         protected override void OnUpdate()
//         {
//             // precondition of current node. 
//             var currentGoal      = new CurrentGoal { }.Index;
//             var goalPrecondition = m_goals[currentGoal].Precondition;
//
//             // ForEach actions of which effect relate to the goal precondition. check if it fit the requirement. 
//             var actions = m_states[(int) goalPrecondition].SatisfyActions;
//
//             for (var i = 0; i < actions.Length; i++)
//             {
//                 var actionIndex       = (int) actions[i];
//                 var preconditions     = m_actions[actionIndex].Preconditions;
//                 var preconditionCount = preconditions.Length;
//
//                 for (var j = 0; j < preconditionCount; j++)
//                 {
//                     var preconditionIndex = preconditions[j];
//                 }
//             }
//
//
//             bool Compare(
//                 int         state,
//                 int         effect,
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
//                     default:                  throw new IndexOutOfRangeException("CompareType");
//                 }
//             }
//         }
//
//         protected override void OnCreate()
//         {
//             var actionsNum = Enum.GetValues(typeof(ActionType)).Length;
//             var goalsNum   = Enum.GetValues(typeof(GoalType)).Length;
//             var statesNum  = Enum.GetValues(typeof(StateType)).Length;
//             m_actions = new NativeArray<Action>(actionsNum, Allocator.Persistent);
//             m_goals   = new NativeArray<Goal>(goalsNum, Allocator.Persistent);
//             m_states  = new NativeArray<State>(statesNum, Allocator.Persistent);
//
//             // @Todo 似乎可以写进物品相关信息里。或者把行为直接用编号串联起来。Sure，do it.
//             // Cache connection between each State and it's Satisfy Action.
//             // 用Struct的时候注意传值问题。
//             ConnectStatesToActions();
//         }
//
//         private void ConnectStatesToActions()
//         {
//             for (var i = 0; i < m_actions.Length; i++)
//             {
//                 var action  = m_actions[i].Type;
//                 var effects = m_actions[i].Effects;
//
//                 for (var j = 0; j < effects.Length; j++)
//                 {
//                     var effect         = effects[j];
//                     var satisfyActions = m_states[(int) effect].SatisfyActions;
//                     // @Bug 这里应该就是复制的struct了. 解决，不传struct，而传NativeArray
//                     // @Todo NativeList.
//                     var existingNum = satisfyActions.Length;
//                     satisfyActions[existingNum] = action;
//                 }
//             }
//         }
//
//         protected override void OnDestroy() { }
//     }
// }