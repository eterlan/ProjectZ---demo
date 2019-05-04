using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using System;
public class InventorySystem : ComponentSystem
{
    // who I am or What I have
    private EntityQuery m_NewOwnersGroup;
    private EntityQuery m_RemovedOwnersGroup;
    private EntityQuery m_ExistingOwnersGroup;
    private EntityQuery m_OwnersDeletedGroup;

    private void AddItemToOwner(Entity itemEntity, Entity ownerEntity)
    {
        EntityManager.SetComponentData(itemEntity, new PreviousOwner{Value = ownerEntity});
        if (!EntityManager.HasComponent(ownerEntity, typeof(Item)))
        {
            EntityManager.AddBuffer<Item>(ownerEntity);
        }
        else
        {
            var Items = EntityManager.GetBuffer<Item>(ownerEntity);
            Items.Add(new Item{Value = itemEntity});
        }
    }
    private int FindItemIndex(DynamicBuffer<Item> items, Entity itemEntity)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].Value == itemEntity)
            {
                return i;
            }
        }
        throw new InvalidOperationException("Item entity not in parent");
    }
    private void RemoveItemFromOwner(Entity itemEntity, Entity ownerEntity)
    {
        // no child
        if (!EntityManager.HasComponent<Item>(ownerEntity))
        {
            return;
        }
        // normal
        var items = EntityManager.GetBuffer<Item>(ownerEntity);
        var itemIndex = FindItemIndex(items, itemEntity);
        items.RemoveAt(itemIndex);
        // after remove no child
        if (items.Length == 0)
        {
            EntityManager.RemoveComponent(ownerEntity, typeof(Item));
        }
    }
    private struct ChangedOwner
    {
        public Entity ItemEntity;
        public Entity OwnerEntity;
        public Entity PreviousOwnerEntity;
    }
    private struct FilterChangedOwner : IJob
    {
        public NativeList<ChangedOwner> ChangedOwner;
        public NativeArray<ArchetypeChunk> Chunks;
        public ArchetypeChunkComponentType<PreviousOwner> PreviousOwnerType;
        public ArchetypeChunkComponentType<Owner> OwnerType;
        public ArchetypeChunkEntityType EntityType;

        public void Execute()
        {
            // changed?
            for (int i = 0; i < Chunks.Length; i++)
            {
                var chunk = Chunks[i];
                if (chunk.DidChange(OwnerType, chunk.GetComponentVersion(PreviousOwnerType)))
                {
                    var chunkOwners        = chunk.GetNativeArray(OwnerType);
                    var chunkPreviousOwner = chunk.GetNativeArray(PreviousOwnerType);
                    var chunkEntities      = chunk.GetNativeArray(EntityType);

                    for (int j = 0; j < chunk.Count; j++)
                    {
                        if (chunkOwners[j].Value != chunkPreviousOwner[j].Value)
                        {
                            ChangedOwner.Add(new ChangedOwner
                            {
                                OwnerEntity         = chunkOwners[j].Value,
                                PreviousOwnerEntity = chunkPreviousOwner[j].Value,
                                ItemEntity          = chunkEntities[j],
                            });
                        }
                    }
                }
            }
        }
    }
    private void UpdateNewOwners()
    {
        // group CalculateLength less efficient than group ToEntityArray? 
        // TEST it.
        if (m_NewOwnersGroup.CalculateLength() > 0)
        {
            var itemEntities  = m_NewOwnersGroup.ToEntityArray(Allocator.Temp);
            var owners        = m_NewOwnersGroup.ToComponentDataArray<Owner>(Allocator.Temp);

            EntityManager.AddComponent(m_NewOwnersGroup, typeof(PreviousOwner));

            for (int i = 0; i < itemEntities.Length; i++)
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
        if (m_RemovedOwnersGroup.CalculateLength() > 0)
        {
            var itemEntities = m_RemovedOwnersGroup.ToEntityArray(Allocator.Temp);
            var owners = m_RemovedOwnersGroup.ToComponentDataArray<PreviousOwner>(Allocator.Temp);

            for (int i = 0; i < itemEntities.Length; i++)
            {
                var itemEntity  = itemEntities[i];
                var ownerEntity = owners[i].Value;

                RemoveItemFromOwner(itemEntity, ownerEntity);
            }

            EntityManager.RemoveComponent(m_RemovedOwnersGroup, typeof(PreviousOwner));
            itemEntities.Dispose();
            owners.Dispose();
        }
    }
    private void UpdateChangedOwners()
    {
        var changedOwnersChunks = m_ExistingOwnersGroup.CreateArchetypeChunkArray(Allocator.TempJob);
        if (changedOwnersChunks.Length > 0)
        {
            var ownerType = GetArchetypeChunkComponentType<Owner>(true);
            var previousOwnerType = GetArchetypeChunkComponentType<PreviousOwner>(true);
            var entityType = GetArchetypeChunkEntityType();
            var changedOwners = new NativeList<ChangedOwner>(Allocator.TempJob);

            var filterChangedOwnerJob = new FilterChangedOwner
            {
                Chunks            = changedOwnersChunks,
                OwnerType         = ownerType,
                PreviousOwnerType = previousOwnerType,
                EntityType        = entityType,
                ChangedOwner      = changedOwners,
            };
            var filterChangedOwnerJobHandle = filterChangedOwnerJob.Schedule();
            filterChangedOwnerJobHandle.Complete();

            for (int i = 0; i < changedOwners.Length; i++)
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
    private void UpdateDeletedOwners()
    {
        if (m_OwnersDeletedGroup.CalculateLength() > 0)
        {
            var previousOwnerEntities = m_OwnersDeletedGroup.ToEntityArray(Allocator.Temp);
            
            for (int i = 0; i < previousOwnerEntities.Length; i++)
            {
                var previousOwnerEntity = previousOwnerEntities[i];
                var itemEntitiesSource = EntityManager.GetBuffer<Item>(previousOwnerEntity).AsNativeArray();

                // Why copy to a new NativeArray?
                var itemEntities = new NativeArray<Entity>(itemEntitiesSource.Length, Allocator.Temp);

                for (int j = 0; j < itemEntitiesSource.Length; j++)
                {
                    itemEntities[j] = itemEntitiesSource[j].Value;
                }
                for (int j = 0; j < itemEntities.Length; j++)
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
        UpdateDeletedOwners();
    }
    protected override void OnCreate()
    {
        m_NewOwnersGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                ComponentType.ReadOnly<Owner>(),
            },
            None = new ComponentType[]
            {
                typeof(PreviousOwner),
            },
        });
        m_ExistingOwnersGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                ComponentType.ReadOnly<Owner>(),
                typeof(PreviousOwner),
            },
        });
        m_RemovedOwnersGroup = GetEntityQuery(new EntityQueryDesc
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
        m_OwnersDeletedGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(Item)
            },
            None = new ComponentType[]
            {
                // consider it... other item might not have this either.
                typeof(LocalToWorld)
            }
        });
    }
}