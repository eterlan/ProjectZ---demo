// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using UnityEngine;

// //[AlwaysUpdateSystem]
// public class FireEventSystem : ComponentSystem 
// {
//     public NativeQueue<ArrivedEventData> queue;

//     protected override void OnUpdate()
//     {
//         // destroy event entity
//         Entities.ForEach((Entity entity, ref ArrivedEventData eventData) =>
//         {
//             PostUpdateCommands.DestroyEntity(entity);
            
//         });
//         // create event entity
//         while (queue.TryDequeue(out ArrivedEventData eventData))
//         {
//             var eventEntity = PostUpdateCommands.CreateEntity();
//             PostUpdateCommands.AddComponent(eventEntity, eventData);
//         }
//     }
//     protected override void OnCreateManager()
//     {
//         queue = new NativeQueue<ArrivedEventData>(Allocator.Persistent);
//     }
//     protected override void OnDestroyManager()
//     {
//         queue.Dispose();
//     }
//     public void ArrivedEvent(ArrivedEventData eventData)
//     {
//         queue.Enqueue(eventData);
//     }
// }

// public struct ArrivedEventData : IComponentData
// {
//     public int EventData;
// }