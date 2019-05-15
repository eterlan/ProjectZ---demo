// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using UnityEngine;
// using Unity.Transforms;
// using Unity.Physics;
// using Unity.Physics.Systems;
// using Unity.Physics.Extensions;
// public class CollectionSystem : JobComponentSystem
// {
//     EntityQuery m_PlayerGroup;
//     BuildPhysicsWorld BuildPhysicsWorldSystem;
//     bool IsCollecting;
//     // hardcode for now
//     float collectionExtent = 1;

//     [RequireComponentTag(typeof(Player))]
//     public struct RayCast : IJobForEachWithEntity<Translation>
//     {
//         [ReadOnly] public CollisionFilter PlayerFilter;
//         [ReadOnly] public float CollectionExtent;        
//         public CollisionWorld CollisionWorld;
//         [NativeDisableParallelForRestriction]
//         public NativeList<int> AllHits;
//         public void Execute(Entity entity, int index, [ReadOnly]ref Translation translation)
//         {
//             var position = translation.Value;
//             Aabb collectionBound = new Aabb
//             {
//                 Min = position - CollectionExtent,
//                 Max = position + CollectionExtent,
//             };
//             var input = new OverlapAabbInput
//             {
//                 Aabb = collectionBound,
//                 Filter = PlayerFilter,
//             };
//             CollisionWorld.OverlapAabb(input, ref AllHits);
//         }
//     }
//     protected override JobHandle OnUpdate(JobHandle InputDependency)
//     {
//         // necessary?
//         BuildPhysicsWorldSystem.FinalJobHandle.Complete();
//         var physicsWorld = BuildPhysicsWorldSystem.PhysicsWorld;
//         var AllHits = new NativeList<int>(Allocator.TempJob);
//         var playerEntities = m_PlayerGroup.ToEntityArray(Allocator.TempJob);
//         CollisionFilter playerFilter = CollisionFilter.Default;

//         for (int i = 0; i < playerEntities.Length; i++)
//         {
//             var player = playerEntities[i];
//             var rbIndex = physicsWorld.GetRigidBodyIndex(player);
//             playerFilter = physicsWorld.GetCollisionFilter(rbIndex);
//         }

//         IsCollecting = false;
//         if (Input.GetKeyDown(KeyCode.F))
//         {
//             IsCollecting = true;   
//         }

//         if (IsCollecting)
//         {
//             Debug.Log("pressF");
//             var RayCastJob = new RayCast
//             {
//                 CollisionWorld = physicsWorld.CollisionWorld,
//                 CollectionExtent = collectionExtent,
//                 AllHits = AllHits,
//                 PlayerFilter = playerFilter,
//             };
//             var RayCastJobHandle = RayCastJob.Schedule(this, InputDependency);
//             InputDependency = RayCastJobHandle;
//             RayCastJobHandle.Complete();
//         }
//         for (int itemIndex = 0; itemIndex < AllHits.Length; itemIndex++)
//         {
//             Debug.Log("collision");
//         }
//         playerEntities.Dispose();
//         AllHits.Dispose();

//         return InputDependency;
//     }
//     protected override void OnCreate()
//     {
//         BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
//         m_PlayerGroup = GetEntityQuery(new EntityQueryDesc
//         {
//             All = new ComponentType[]
//             {
//                 ComponentType.ReadOnly<Player>(),
//                 ComponentType.ReadOnly<Translation>(),
//             },
//         });
//     }
//     protected override void OnDestroy()
//     {

//     }
// }

// seems overlap doesn't work well with Job. try hybrid way.

using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public class CollectionSystem : ComponentSystem
{
    private BuildPhysicsWorld m_buildPhysicsWorldSystem;

    // hardcode for now
    private readonly float m_collectionExtent = 1;

    private bool m_isCollecting;

    // hardcode for now
    private readonly uint        m_itemCategoryBits = 2;
    private          EntityQuery m_playerGroup;

    protected override void OnUpdate()
    {
        // necessary?
        m_buildPhysicsWorldSystem.FinalJobHandle.Complete();

        var physicsWorld   = m_buildPhysicsWorldSystem.PhysicsWorld;
        var allHits        = new NativeList<int>(Allocator.TempJob);
        var playerEntities = m_playerGroup.ToEntityArray(Allocator.TempJob);

        for (var playerIndex = 0; playerIndex < playerEntities.Length; playerIndex++)
        {
            var playerEntity   = playerEntities[playerIndex];
            var rbIndex        = physicsWorld.GetRigidBodyIndex(playerEntity);
            var playerFilter   = physicsWorld.GetCollisionFilter(rbIndex);
            var playerPosition = EntityManager.GetComponentData<Translation>(playerEntity).Value;

            m_isCollecting = false;

            // hard code. for AI purpose, it need to be refract.
            // FilterGroup Input
            // InputSys . Filter Any = Player / NPC 
            // PlayerInput: ForEach.With(Player) {//set bool}
            // NPCInput: ForEach.With(NPC) {//set bool}
            if (Input.GetKeyDown(KeyCode.F)) m_isCollecting = true;
            Debug.Log("AllHitsLength" + allHits.Length);

            if (m_isCollecting)
            {
                var position = playerPosition;
                var collectionBound = new Aabb
                {
                    Min = position - m_collectionExtent,
                    Max = position + m_collectionExtent
                };
                var overlapFlter = new CollisionFilter
                {
                    CategoryBits = playerFilter.CategoryBits, // 1 = 0001
                    MaskBits     = m_itemCategoryBits           // 2 = 0010
                };
                var input = new OverlapAabbInput
                {
                    Aabb = collectionBound,
                    // cannot simply same as player filter, or it will collect anything not only item.
                    Filter = overlapFlter
                };

                // TEST
                // var testFilter1 = CollisionFilter.IsCollisionEnabled(playerFilter, overlapFlter);
                // var testFilter2 = CollisionFilter.IsCollisionEnabled(playerFilter, overlapFlter);

                // Debug.Log($"playerFilter.MaskBits: {playerFilter.MaskBits}, CategoryBits: {playerFilter.CategoryBits}");
                // Debug.Log($"overlapFlter.MaskBits: {overlapFlter.MaskBits}, CategoryBits: {overlapFlter.CategoryBits}");
                // Debug.Log(testFilter1+""+testFilter2);
                // Debug.Log("AllHitsLength"+AllHits.Length);

                if (physicsWorld.CollisionWorld.OverlapAabb(input, ref allHits))
                {
                    //
                    Debug.Log("AllHitsLength" + allHits.Length);
                    for (var itemIndex = 0; itemIndex < allHits.Length; itemIndex++)
                    {
                        //
                        Debug.Log("collision");
                        //get entity
                        var item = physicsWorld.Bodies[itemIndex].Entity;
                        // Might change later if Unity Official fix filter.
                        // because the OverlapFilter doesn't work, I have to manually check if it's item. 
                        if (EntityManager.HasComponent<ItemTag>(item))
                        {
                            Debug.Log(1111);
                            // destroy entity, item value would remain(ISharedSysStateComponent)
                            EntityManager.DestroyEntity(item);
                            //
                            //     // remember the previous owner..? That's stealing.
                            // if (!EntityManager.HasComponent<Owner>(item))
                            // {
                            //     var owner = EntityManager.GetComponentData<Owner>(item);
                            //     //
                            //     // Debug.log($"You are stealing, owner is {owner.Value}");
                            // }

                            // then add owner
                            EntityManager.AddComponentData(item, new Owner {Value = playerEntity});


                            //Actually InventorySystem should handle the renderer things.

                            // var inventory = EntityManager.GetBuffer<Items>(playerEntity).AsNativeArray();
                            // foreach (var itemElement in inventory)
                            // {
                            //     Debug.Log("hi");
                            //     var entity = itemElement.Value;
                            //     if (EntityManager.HasComponent<Food>(entity))
                            //     {
                            //         var foodType = EntityManager.GetSharedComponentData<Food>(entity).FoodType;
                            //         Debug.Log(foodType+"is Added.");
                            //     }
                            //     else
                            //     {
                            //         // Why not trigger this one?
                            //         Debug.Log("no food");
                            //     }
                            // }
                        }
                        //limitation: 1 per time. cool down 


                        //change group to has owner.
                        //how to deal with F? depart to Input component & AI component
                        //test AI collection
                    }
                }
            }

            playerEntities.Dispose();
            allHits.Dispose();
        }
    }

    protected override void OnCreate()
    {
        m_buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_playerGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<Player>(),
                ComponentType.ReadOnly<Translation>()
            }
        });
    }

    protected override void OnDestroy()
    {
    }
}