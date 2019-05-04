using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using System.Collections.Generic;
using Unity.Rendering;

// for the sake of overlap detaction. When update in simulationGroup, WorldRenderBounds wouldn't be added on the entity.
// @TODO: Update only once (by add/remove tag? might have other way such as attribute.

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(RenderBoundsUpdateSystem))]
public class AppleSpawnerSystem : ComponentSystem
{
    private EntityQuery m_AppleTreeSpawnerGroup;
    private EntityQuery m_AppleSpawnersGroup;
    private EntityQuery m_OverlapCheckGroup;
    private Unity.Mathematics.Random random;
    NativeArray<float3> treePositions = new NativeArray<float3>();

    float3 NewRandomPointInUnitSquare()
    {
        var point = random.NextFloat3() * 2 - 1;
        return point;
    }
    protected override void OnUpdate()
    {
        // instantiate tree
        var sourceEntities = m_AppleTreeSpawnerGroup.ToEntityArray(Allocator.TempJob);
        var treeSpawners = m_AppleTreeSpawnerGroup.ToComponentDataArray<TreeSpawner>(Allocator.TempJob);
        m_OverlapCheckGroup.SetFilterChanged(typeof(House));
        var overlapChecks = m_OverlapCheckGroup.ToComponentDataArray<WorldRenderBounds>(Allocator.TempJob);
        

        for (int i = 0; i < sourceEntities.Length; i++)
        {
            var treeSpawner      = treeSpawners[i];
            var count            = treeSpawner.Count;
            var prefab           = treeSpawner.Prefab;
            var spawnRadius      = treeSpawner.SpawnRadius;
            var instances        = new NativeArray<Entity>(count, Allocator.TempJob);
            var randomUnitPoints = new NativeArray<float3>(count, Allocator.TempJob);
            treePositions        = new NativeArray<float3>(count, Allocator.Persistent);

            GeneratePoints.RandomPointsInUnitSphere(randomUnitPoints);

            EntityManager.Instantiate(prefab, instances);            
            for (int j = 0; j < count; j++)
            {
                var position = randomUnitPoints[j] * new float3(spawnRadius, 0, spawnRadius);
                position.y += 0.01f;
                for (int index = 0; index < overlapChecks.Length; index++)
                {
                    var bound = overlapChecks[index]; // what the bound value? how's it detact containing?
                    if (bound.Value.Contains(position))
                    {
                        index = 0;
                        position = NewRandomPointInUnitSquare() * new float3(spawnRadius, 0, spawnRadius);
                        position.y += 0.01f;
                    }
                }
                EntityManager.SetComponentData(instances[j], new Translation{Value = position});
                treePositions[j] = position;
            }
            instances.Dispose();
            randomUnitPoints.Dispose();
        }
        overlapChecks.Dispose();
        sourceEntities.Dispose();
        treeSpawners.Dispose();
        EntityManager.DestroyEntity(m_AppleTreeSpawnerGroup);

        var appleSpawnerCount    = m_AppleSpawnersGroup.CalculateLength();
        
        for (int i = 0; i < appleSpawnerCount; i++)
        {
            var treeCount = treePositions.Length;
            var sourceAppleSpawnerEntities = m_AppleSpawnersGroup.ToEntityArray(Allocator.TempJob);
            var appleSpawners = m_AppleSpawnersGroup.ToComponentDataArray<AppleSpawner>(Allocator.TempJob);
            var appleSpawner = appleSpawners[i];
            var appleHeight = appleSpawner.AppleRadius;
            var countPerTree = appleSpawner.CountPerTree;
            var prefab = appleSpawner.Prefab;
            var radius = appleSpawner.SpawnRadius;

            for (int treeIndex = 0; treeIndex < treeCount; treeIndex++)
            {
                var center = treePositions[treeIndex];
                var RandomUnitPoints = new NativeArray<float3>(countPerTree * treeCount, Allocator.TempJob);
                GeneratePoints.RandomPointsInUnitSphere(RandomUnitPoints);
                var instances = new NativeArray<Entity>(countPerTree, Allocator.TempJob);  
                EntityManager.Instantiate(prefab, instances);

                for (int applePerTreeIndex = 0; applePerTreeIndex < countPerTree; applePerTreeIndex++)
                {
                    var instance = instances[applePerTreeIndex];
                    var appleTotalIndex = treeIndex * countPerTree + applePerTreeIndex;
                    var position = center + RandomUnitPoints[appleTotalIndex] * radius;
                    var groundPosition = new float3(position.x, appleHeight/2, position.z);
                    EntityManager.SetComponentData(instance, new Translation
                    {
                        Value = groundPosition,
                    });
                }
                RandomUnitPoints.Dispose();
                instances.Dispose();
            }
            sourceAppleSpawnerEntities.Dispose();
            appleSpawners.Dispose();
            EntityManager.DestroyEntity(m_AppleSpawnersGroup);
            Enabled = false;
        }
    }
    protected override void OnCreate()
    {      
        random.InitState();
        m_AppleTreeSpawnerGroup = GetEntityQuery(new ComponentType[]{
            ComponentType.ReadOnly<TreeSpawner>(),
            ComponentType.ReadOnly<LocalToWorld>(),
        });

        m_AppleSpawnersGroup = GetEntityQuery(new ComponentType[]{
            ComponentType.ReadOnly<AppleSpawner>(),
            ComponentType.ReadOnly<LocalToWorld>(),
        });

        m_OverlapCheckGroup = GetEntityQuery(new EntityQueryDesc{
            All = new[]{
                ComponentType.ReadOnly<WorldRenderBounds>(),
                typeof(House),
            }
        });
    }
    protected override void OnDestroy()
    {
        treePositions.Dispose();
    }
}