// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using UnityEngine;

// public struct Data : IComponentData { public int Value; }

// //[AlwaysUpdateSystem]
// public class ChangedFilterTest : ComponentSystem
// {
//     public int Count;
//     EntityQuery m_DataGroup;
//     EntityQuery m_modifiedDataGroup;
//     EntityQuery m_Listener;
//     protected override void OnUpdate()
//     {
//         if (Input.GetKeyDown(KeyCode.Z))
//         {
//             var eventData = 1;
//             World.Active.GetExistingSystem<FireEventSystem>().ArrivedEvent(new ArrivedEventData{EventData = eventData});
//         }
//         // if there is any event entity.
//         for (int i = 0; i < m_Listener.CalculateLength(); i++)
//         {
//             Debug.Log("Sth happen");
//         }
//     }

//     protected override void OnCreateManager()
//     {
//         m_Listener = GetEntityQuery(new EntityQueryDesc{
//             All = new[] {
//                 ComponentType.ReadWrite<ArrivedEventData>(),
//             }
//         });
//     }

// }

