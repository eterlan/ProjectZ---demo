// using System;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;

// namespace Unity.Transforms
// {
//     // why abstract class? 
//     // why componenySystem?
//    public class ParentSystem : ComponentSystem
//    {
//         private EntityQuery m_NewParentsGroup;
//         private EntityQuery m_RemovedParentsGroup;
//         private EntityQuery m_ExistingParentsGroup;
//         private EntityQuery m_DeletedParentsGroup;

//         /// <summary>
//         /// Add a child entity to a parent entity
//         /// </summary>
//         /// <param name="childEntity">childEntity</param>
//         /// <param name="parentEntity">parentEntity</param>
//         void AddChildToParent(Entity childEntity, Entity parentEntity)
//         {
//             // why child add parent as it's previous parent?
//             // for the sake of recording. also avoid query this entity again as m_NewParentsGroup
//             EntityManager.SetComponentData(childEntity, new PreviousParent {Value = parentEntity});
//             // if parent doesn't has any child before.
//             if (!EntityManager.HasComponent(parentEntity, typeof(Child)))
//             {
//                 // add new dynamic buffer. and add this child to this dynamic buffer.
//                 var children = EntityManager.AddBuffer<Child>(parentEntity);
//                 children.Add(new Child {Value = childEntity});
//             }
//             else
//             {
//                 // if parent does has any child before, get dynamic buffer on parent, and add this child to it.
//                 var children = EntityManager.GetBuffer<Child>(parentEntity);
//                 children.Add(new Child {Value = childEntity});
//             }
//         }
//         /// <summary>
//         /// find child index by compare the value of child entity with buffer<children> on parent.
//         /// </summary>
//         /// <param name="children"></param>
//         /// <param name="entity"></param>
//         /// <returns></returns>
//         int FindChildIndex(DynamicBuffer<Child> children, Entity entity)
//         {
//             for (int i = 0; i < children.Length; i++)
//             {
//                 if (children[i].Value == entity)
//                     return i;
//             }

//             throw new InvalidOperationException("Child entity not in parent");
//         }

//         void RemoveChildFromParent(Entity childEntity, Entity parentEntity)
//         {
//             if (!EntityManager.HasComponent<Child>(parentEntity))
//                 return;

//             var children = EntityManager.GetBuffer<Child>(parentEntity);
//             var childIndex = FindChildIndex(children, childEntity);
//             children.RemoveAt(childIndex);
//             if (children.Length == 0)
//             {
//                 EntityManager.RemoveComponent(parentEntity, typeof(Child));
//             }
//         }

//         struct ChangedParent
//         {
//             public Entity ChildEntity;
//             public Entity PreviousParentEntity;
//             public Entity ParentEntity;
//         }

//         // why IJob can be used in componentSystem? 
//         [BurstCompile]
//         struct FilterChangedParents : IJob
//         {
//             public NativeList<ChangedParent> ChangedParents;
//             [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
//             [ReadOnly] public ArchetypeChunkComponentType<PreviousParent> PreviousParentType;
//             [ReadOnly] public ArchetypeChunkComponentType<Parent> ParentType;
//             [ReadOnly] public ArchetypeChunkEntityType EntityType;

//             public void Execute()
//             {
//                 for (int i = 0; i < Chunks.Length; i++)
//                 {
//                     var chunk = Chunks[i];

//                     // If anything write to the ParentTypeComponent, the ComponentVersion would changed.
//                     // Q. How to avoid unnecessary component changed?
//                     if (chunk.DidChange(ParentType, chunk.GetComponentVersion(PreviousParentType)))
//                     {
//                         var chunkPreviousParents = chunk.GetNativeArray(PreviousParentType);
//                         var chunkParents = chunk.GetNativeArray(ParentType);
//                         var chunkEntities = chunk.GetNativeArray(EntityType);

//                         for (int j = 0; j < chunk.Count; j++)
//                         {
//                             if (chunkParents[j].Value != chunkPreviousParents[j].Value)
//                                 ChangedParents.Add(new ChangedParent
//                                 {
//                                     ChildEntity = chunkEntities[j],
//                                     ParentEntity = chunkParents[j].Value,
//                                     PreviousParentEntity = chunkPreviousParents[j].Value
//                                 });
//                         }
//                     }
//                 }
//             }
//         }

//         protected override void OnCreate()
//         {
//             // ALl these group are query for child entity.

//             // no previous parent = NewParent
//             // ---cannot be used in my system, because it has localtoworld.---
//             // I don't care
//             m_NewParentsGroup = GetEntityQuery(new EntityQueryDesc
//             {
//                 All = new ComponentType[]
//                 {
//                     ComponentType.ReadOnly<Parent>(), 
//                     ComponentType.ReadOnly<LocalToWorld>(), 
//                     ComponentType.ReadOnly<LocalToParent>()
//                 },
//                 None = new ComponentType[]
//                 {
//                     // not RO because of structral change.
//                     typeof(PreviousParent)
//                 },
//                 // Why do this ?
//                 // Parent in LocalToWorld WriteGroup. so other component in this WriteGroup which not declare here wouldn't be show up in this query
//                 // so -> who is in this WriteGroup? why they can't show up?
//                 Options = EntityQueryOptions.FilterWriteGroup
//             });

//             // only has previous parent.
//             // when delete?
//             m_RemovedParentsGroup = GetEntityQuery(new EntityQueryDesc
//             {
//                 All = new ComponentType[]
//                 {
//                     // not RO because of structral change.
//                     typeof(PreviousParent)
//                 },
//                 None = new ComponentType[]
//                 {
//                     typeof(Parent)
//                 },
//                 Options = EntityQueryOptions.FilterWriteGroup
//             });
//             m_ExistingParentsGroup = GetEntityQuery(new EntityQueryDesc
//             {
//                 All = new ComponentType[]
//                 {
//                     ComponentType.ReadOnly<Parent>(), 
//                     ComponentType.ReadOnly<LocalToWorld>(), 
//                     ComponentType.ReadOnly<LocalToParent>(),
//                     typeof(PreviousParent)
//                 },
//                 Options = EntityQueryOptions.FilterWriteGroup
//             });

//             // SystemStateBufferElement -> so this group is parent which was been deleted. only buffer<Child> was remained.
//             m_DeletedParentsGroup = GetEntityQuery(new EntityQueryDesc
//             {
//                 All = new ComponentType[]
//                 {
//                     typeof(Child)
//                 },
//                 None = new ComponentType[]
//                 {
//                     typeof(LocalToWorld)
//                 },
//                 Options = EntityQueryOptions.FilterWriteGroup
//             });
//         }

//         /// <summary>
//         /// for each entity in the m_NewParentsGroup{no prevParent},
//         /// add 
//         /// </summary>
//         void UpdateNewParents()
//         {
//             // children in m_NewParentsGroup == group.ToEntityArray
//             var childEntities = m_NewParentsGroup.ToEntityArray(Allocator.TempJob);
//             // parents in m_NewParentsGroup == group.ToComponentDataArray<Parent> 
//             var parents = m_NewParentsGroup.ToComponentDataArray<Parent>(Allocator.TempJob);

//             // For every enetity without prevParent, add prevParent component.
//             EntityManager.AddComponent(m_NewParentsGroup, typeof(PreviousParent));

//             for (int i = 0; i < childEntities.Length; i++)
//             {
//                 var childEntity = childEntities[i];
//                 var parentEntity = parents[i].Value;

//                 AddChildToParent(childEntity, parentEntity);
//             }

//             childEntities.Dispose();
//             parents.Dispose();
//         }

//         // when parent is removed, notify parent to delete the child buffer element
//         void UpdateRemoveParents()
//         {
//             var childEntities = m_RemovedParentsGroup.ToEntityArray(Allocator.TempJob);
//             var previousParents = m_RemovedParentsGroup.ToComponentDataArray<PreviousParent>(Allocator.TempJob);

//             for (int i = 0; i < childEntities.Length; i++)
//             {
//                 var childEntity = childEntities[i];
//                 var previousParentEntity = previousParents[i].Value;

//                 RemoveChildFromParent(childEntity, previousParentEntity);
//             }

//             EntityManager.RemoveComponent(m_RemovedParentsGroup, typeof(PreviousParent));            
//             childEntities.Dispose();
//             previousParents.Dispose();
//         }

//         void UpdateChangeParents()
//         {
//             // Might need changed group == m_ExistingParentsGroup
//             var changeParentsChunks = m_ExistingParentsGroup.CreateArchetypeChunkArray(Allocator.TempJob);
//             if (changeParentsChunks.Length > 0)
//             {
//                 var parentType = GetArchetypeChunkComponentType<Parent>(true);
//                 var previousParentType = GetArchetypeChunkComponentType<PreviousParent>(true);
//                 // ?
//                 var entityType = GetArchetypeChunkEntityType();
//                 //var e = GetArchetypeChunkEntityType();
//                 var changedParents = new NativeList<ChangedParent>(Allocator.TempJob);

//                 // goal -> return a List<struct ChangedParents>
//                 var filterChangedParentsJob = new FilterChangedParents
//                 {
//                     Chunks = changeParentsChunks,
//                     ChangedParents = changedParents,
//                     ParentType = parentType,
//                     PreviousParentType = previousParentType,
//                     EntityType = entityType
//                 };
//                 var filterChangedParentsJobHandle = filterChangedParentsJob.Schedule();
//                 filterChangedParentsJobHandle.Complete();

//                 for (int i = 0; i < changedParents.Length; i++)
//                 {
//                     var childEntity = changedParents[i].ChildEntity;
//                     var previousParentEntity = changedParents[i].PreviousParentEntity;
//                     var parentEntity = changedParents[i].ParentEntity;

//                     RemoveChildFromParent(childEntity, previousParentEntity);
//                     AddChildToParent(childEntity, parentEntity);
//                 }    
//                 changedParents.Dispose();
//             }
//             changeParentsChunks.Dispose();
//         }

//         void UpdateDeletedParents()
//         {
//             var previousParents = m_DeletedParentsGroup.ToEntityArray(Allocator.TempJob);

//             for (int i = 0; i < previousParents.Length; i++)
//             {
//                 var parentEntity = previousParents[i];
//                 var childEntitiesSource = EntityManager.GetBuffer<Child>(parentEntity).AsNativeArray();
//                 var childEntities = new NativeArray<Entity>(childEntitiesSource.Length, Allocator.Temp);

//                 // double loop get each child
//                 for (int j = 0; j < childEntitiesSource.Length; j++)
//                     childEntities[j] = childEntitiesSource[j].Value;

//                 for (int j = 0; j < childEntities.Length; j++)
//                 {
//                     var childEntity = childEntities[j];

//                     if (!EntityManager.Exists(childEntity))
//                         continue;

//                     if (EntityManager.HasComponent(childEntity, typeof(Parent)))
//                         EntityManager.RemoveComponent(childEntity, typeof(Parent));
//                     if (EntityManager.HasComponent(childEntity, typeof(PreviousParent)))
//                         EntityManager.RemoveComponent(childEntity, typeof(PreviousParent));
//                     if (EntityManager.HasComponent(childEntity, typeof(LocalToParent)))
//                         EntityManager.RemoveComponent(childEntity, typeof(LocalToParent));
//                 }

//                 childEntities.Dispose();
//             }
//             EntityManager.RemoveComponent(m_DeletedParentsGroup, typeof(Child));            
//             previousParents.Dispose();
//         }

//         protected override void OnUpdate()
//         {
//             UpdateDeletedParents();
//             UpdateRemoveParents();

//             UpdateChangeParents();
//             UpdateNewParents();
//         }
//     }
// }

