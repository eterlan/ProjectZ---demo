using System;
using ProjectZ.Component.Items;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace ProjectZ.GamePlay
{
    public class InventorySystem : ComponentSystem
    {
        private EntityQuery m_existingOwnersGroup;

        // who I am or What I have
        private EntityQuery m_newOwnersGroup;
        private EntityQuery m_ownersDeletedGroup;
        private EntityQuery m_removedOwnersGroup;

        private void AddItemToOwner(Entity itemEntity, Entity ownerEntity)
        {
            EntityManager.SetComponentData(itemEntity, new PreviousOwner {Value = ownerEntity});
            if (!EntityManager.HasComponent(ownerEntity, typeof(Items)))
            {
                EntityManager.AddBuffer<Items>(ownerEntity);
            }
            else
            {
                var items = EntityManager.GetBuffer<Items>(ownerEntity);
                items.Add(new Items {Value = itemEntity});
            }
        }

        private int FindItemIndex(DynamicBuffer<Items> items, Entity itemEntity)
        {
            for (var i = 0; i < items.Length; i++)
                if (items[i].Value == itemEntity)
                    return i;
            throw new InvalidOperationException("Item entity not in parent");
        }

        private void RemoveItemFromOwner(Entity itemEntity, Entity ownerEntity)
        {
            // no child
            if (!EntityManager.HasComponent<Items>(ownerEntity)) return;
            // normal
            var items     = EntityManager.GetBuffer<Items>(ownerEntity);
            var itemIndex = FindItemIndex(items, itemEntity);
            items.RemoveAt(itemIndex);
            // after remove no child
            if (items.Length == 0) EntityManager.RemoveComponent(ownerEntity, typeof(Items));
        }

        private void UpdateNewOwners()
        {
            // group CalculateLength less efficient than group ToEntityArray? 
            // TEST it.
            if (m_newOwnersGroup.CalculateLength() > 0)
            {
                // TempJob is required
                var itemEntities = m_newOwnersGroup.ToEntityArray(Allocator.TempJob);
                var owners       = m_newOwnersGroup.ToComponentDataArray<Owner>(Allocator.TempJob);

                EntityManager.AddComponent(m_newOwnersGroup, typeof(PreviousOwner));

                for (var i = 0; i < itemEntities.Length; i++)
                {
                    var itemEntity  = itemEntities[i];
                    var ownerEntity = owners[i].Value;

                    AddItemToOwner(itemEntity, ownerEntity);
                }

                itemEntities.Dispose();
                owners.Dispose();
            }
        }

        private void UpdateRemovedOwners()
        {
            if (m_removedOwnersGroup.CalculateLength() > 0)
            {
                var itemEntities = m_removedOwnersGroup.ToEntityArray(Allocator.TempJob);
                var owners       = m_removedOwnersGroup.ToComponentDataArray<PreviousOwner>(Allocator.TempJob);

                for (var i = 0; i < itemEntities.Length; i++)
                {
                    var itemEntity  = itemEntities[i];
                    var ownerEntity = owners[i].Value;

                    RemoveItemFromOwner(itemEntity, ownerEntity);
                }

                EntityManager.RemoveComponent(m_removedOwnersGroup, typeof(PreviousOwner));
                itemEntities.Dispose();
                owners.Dispose();
            }
        }

        private void UpdateChangedOwners()
        {
            var changedOwnersChunks = m_existingOwnersGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            if (changedOwnersChunks.Length > 0)
            {
                var ownerType         = GetArchetypeChunkComponentType<Owner>(true);
                var previousOwnerType = GetArchetypeChunkComponentType<PreviousOwner>(true);
                var entityType        = GetArchetypeChunkEntityType();
                var changedOwners     = new NativeList<ChangedOwner>(Allocator.TempJob);

                var filterChangedOwnerJob = new FilterChangedOwner
                {
                    Chunks            = changedOwnersChunks,
                    OwnerType         = ownerType,
                    PreviousOwnerType = previousOwnerType,
                    EntityType        = entityType,
                    ChangedOwner      = changedOwners
                };
                var filterChangedOwnerJobHandle = filterChangedOwnerJob.Schedule();
                filterChangedOwnerJobHandle.Complete();

                for (var i = 0; i < changedOwners.Length; i++)
                {
                    var previousOwnerEntity = changedOwners[i].PreviousOwnerEntity;
                    var ownerEntity         = changedOwners[i].OwnerEntity;
                    var itemEntity          = changedOwners[i].ItemEntity;

                    RemoveItemFromOwner(itemEntity, previousOwnerEntity);
                    AddItemToOwner(itemEntity, ownerEntity);
                }

                changedOwners.Dispose();
            }

            changedOwnersChunks.Dispose();
        }

        private void UpdateOwnersDeleted()
        {
            if (m_ownersDeletedGroup.CalculateLength() > 0)
            {
                var previousOwnerEntities = m_ownersDeletedGroup.ToEntityArray(Allocator.TempJob);

                for (var i = 0; i < previousOwnerEntities.Length; i++)
                {
                    var previousOwnerEntity = previousOwnerEntities[i];
                    var itemEntitiesSource  = EntityManager.GetBuffer<Items>(previousOwnerEntity).AsNativeArray();

                    // Why copy to a new NativeArray? forget to dispose
                    var itemEntities = new NativeArray<Entity>(itemEntitiesSource.Length, Allocator.TempJob);

                    for (var j = 0; j < itemEntitiesSource.Length; j++) itemEntities[j] = itemEntitiesSource[j].Value;
                    for (var j = 0; j < itemEntities.Length; j++)
                    {
                        var itemEntity = itemEntities[j];

                        if (!EntityManager.Exists(itemEntity))
                            continue;
                        // why origin system doesn't use generic?
                        if (EntityManager.HasComponent<PreviousOwner>(itemEntity))
                            EntityManager.RemoveComponent<PreviousOwner>(itemEntity);

                        if (EntityManager.HasComponent<Owner>(itemEntity))
                            EntityManager.RemoveComponent<Owner>(itemEntity);
                    }

                    itemEntities.Dispose();
                }

                previousOwnerEntities.Dispose();
            }
        }

        protected override void OnUpdate()
        {
            // why origin order is in reverse?
            // delete
            // remove
            // changed
            // new

            UpdateNewOwners();
            UpdateChangedOwners();
            UpdateRemovedOwners();
            UpdateOwnersDeleted();
        }

        protected override void OnCreate()
        {
            m_newOwnersGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<Owner>()
                },
                None = new ComponentType[]
                {
                    typeof(PreviousOwner)
                }
            });
            m_existingOwnersGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<Owner>(),
                    typeof(PreviousOwner)
                }
            });
            m_removedOwnersGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PreviousOwner)
                },
                None = new ComponentType[]
                {
                    typeof(Owner)
                }
            });
            m_ownersDeletedGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    typeof(Items),
                    ComponentType.ReadOnly<Translation>()
                },
                None = new ComponentType[]
                {
                    // consider it... other item might not have this either.
                    typeof(LocalToWorld)
                }
            });
        }

        private struct ChangedOwner
        {
            public Entity ItemEntity;
            public Entity OwnerEntity;
            public Entity PreviousOwnerEntity;
        }

        private struct FilterChangedOwner : IJob
        {
            public            NativeList<ChangedOwner>                   ChangedOwner;
            public            NativeArray<ArchetypeChunk>                Chunks;
            [ReadOnly] public ArchetypeChunkComponentType<PreviousOwner> PreviousOwnerType;
            [ReadOnly] public ArchetypeChunkComponentType<Owner>         OwnerType;
            [ReadOnly] public ArchetypeChunkEntityType                   EntityType;

            public void Execute()
            {
                // changed?
                for (var i = 0; i < Chunks.Length; i++)
                {
                    var chunk = Chunks[i];
                    if (chunk.DidChange(OwnerType, chunk.GetComponentVersion(PreviousOwnerType)))
                    {
                        var chunkOwners        = chunk.GetNativeArray(OwnerType);
                        var chunkPreviousOwner = chunk.GetNativeArray(PreviousOwnerType);
                        var chunkEntities      = chunk.GetNativeArray(EntityType);

                        for (var j = 0; j < chunk.Count; j++)
                            if (chunkOwners[j].Value != chunkPreviousOwner[j].Value)
                                ChangedOwner.Add(new ChangedOwner
                                {
                                    OwnerEntity         = chunkOwners[j].Value,
                                    PreviousOwnerEntity = chunkPreviousOwner[j].Value,
                                    ItemEntity          = chunkEntities[j]
                                });
                    }
                }
            }
        }
    }
}