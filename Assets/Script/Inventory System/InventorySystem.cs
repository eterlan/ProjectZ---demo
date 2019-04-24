// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using UnityEngine;
// using Unity.Transforms;
// public class InventorySystem : ComponentSystem
// {
//     // who I am or What I have
//     private EntityQuery m_NewOwnersGroup;
//     private EntityQuery m_RemovedOwnersGroup;
//     private EntityQuery m_ExistingOwnersGroup;
//     private EntityQuery m_OwnersDeletedGroup;

//     protected override void OnUpdate()
//     {
        
//     }
//     protected override void OnCreate()
//     {
//         m_NewParentsGroup = GetEntityQuery(new EntityQueryDesc
//         {
//             All = new ComponentType[]
//             {
//                 ComponentType.ReadOnly<Parent>(),
//                 typeof(PreviousParent),
//             },
//             None = new ComponentType[]
//             {
//             },
//         });
//         m_ExistingParentsGroup = GetEntityQuery(new EntityQueryDesc
//         {
//             All = new ComponentType[]
//             {
                
//             },
//             None = new ComponentType[]
//             {
                
//             },
//         });
//     }
//     protected override void OnDestroy()
//     {

//     }
// }