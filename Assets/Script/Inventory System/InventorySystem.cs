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

    private void AddItemToOwner(Entity ownerEntity, Entity itemEntity)
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
    private void RemoveChildFromParent(Entity itemEntity, Entity ownerEntity)
    {
        
    }
    protected override void OnUpdate()
    {
        
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
                ComponentType.ReadWrite<PreviousOwner>(),
                ComponentType.ReadWrite<Owner>(),
            },
            None = new ComponentType[]
            {
                
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
                typeof(Items)
            },
            None = new ComponentType[]
            {
                typeof(LocalToWorld)
            }
        });
    }
    protected override void OnDestroy()
    {

    }
}