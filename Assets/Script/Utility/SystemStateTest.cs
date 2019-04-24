// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using UnityEngine;

// public struct State : ISystemStateComponentData { }
// public struct Normal : IComponentData { }

// public class Test : ComponentSystem
// {
//     public int Count;
//     EntityQuery m_CreateGroup;
//     EntityQuery m_DestroyGroup;
//     protected override void OnUpdate()
//     {
//         for (int i = 0; i < m_CreateGroup.CalculateLength(); i++)
//         {
//             Debug.Log("Added component");
//             EntityManager.AddComponent(m_CreateGroup, ComponentType.ReadOnly<State>());
//             EntityManager.DestroyEntity(GetEntityQuery(new[] { ComponentType.ReadWrite<Normal>(), typeof(State)}));
//         }
//         for (int i = 0; i < m_DestroyGroup.CalculateLength(); i++)
//         {   
//             Debug.Log("Remove state");
//             EntityManager.RemoveComponent(m_DestroyGroup, typeof(State));
//         }
//     }

//     protected override void OnCreateManager()
//     {
//         m_CreateGroup = GetEntityQuery( new EntityQueryDesc {
//             All = new[] {
//                 ComponentType.ReadWrite<Normal>()
//             },
//             None = new[] {
//                 ComponentType.ReadWrite<State>(),
//             }
//         });
//         Count = m_CreateGroup.CalculateLength();
//         m_DestroyGroup = GetEntityQuery( new EntityQueryDesc {
//             All = new[] {
//                 ComponentType.ReadWrite<State>(),
//             },
//             None = new[] {
//                 ComponentType.ReadWrite<Normal>(),
//             },
//         });
//     }

// }