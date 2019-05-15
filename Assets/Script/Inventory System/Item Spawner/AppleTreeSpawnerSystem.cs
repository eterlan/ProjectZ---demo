using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

// for the sake of overlap detaction. When update in simulationGroup, WorldRenderBounds wouldn't be added on the entity.
// @TODO: Update only once (by add/remove tag? might have other way such as attribute.

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(RenderBoundsUpdateSystem))]
public class AppleSpawnerSystem : ComponentSystem
{
    private EntityQuery         m_appleSpawnersGroup;
    private EntityQuery         m_appleTreeSpawnerGroup;
    private EntityQuery         m_overlapCheckGroup;
    private Random              m_random;
    private NativeArray<float3> m_treePositions;

    private float3 NewRandomPointInUnitSquare()
    {
        var point = m_random.NextFloat3() * 2 - 1;
        return point;
    }

    protected override void OnUpdate()
    {
        // instantiate tree
        var sourceEntities = m_appleTreeSpawnerGroup.ToEntityArray(Allocator.TempJob);
        var treeSpawners   = m_appleTreeSpawnerGroup.ToComponentDataArray<TreeSpawner>(Allocator.TempJob);
        m_overlapCheckGroup.SetFilterChanged(typeof(House));
        var overlapChecks = m_overlapCheckGroup.ToComponentDataArray<WorldRenderBounds>(Allocator.TempJob);


        for (var i = 0; i < sourceEntities.Length; i++)
        {
            var treeSpawner      = treeSpawners[i];
            var count            = treeSpawner.Count;
            var prefab           = treeSpawner.Prefab;
            var spawnRadius      = treeSpawner.SpawnRadius;
            var instances        = new NativeArray<Entity>(count, Allocator.TempJob);
            var randomUnitPoints = new NativeArray<float3>(count, Allocator.TempJob);
            m_treePositions = new NativeArray<float3>(count, Allocator.Persistent);

            GeneratePoints.RandomPointsInUnitSphere(randomUnitPoints);

            EntityManager.Instantiate(prefab, instances);
            for (var j = 0; j < count; j++)
            {
                var position = randomUnitPoints[j] * new float3(spawnRadius, 0, spawnRadius);
                position.y += 0.01f;
                for (var index = 0; index < overlapChecks.Length; index++)
                {
                    var bound = overlapChecks[index]; // what the bound value? how's it detact containing?
                    if (bound.Value.Contains(position))
                    {
                        index      =  0;
                        position   =  NewRandomPointInUnitSquare() * new float3(spawnRadius, 0, spawnRadius);
                        position.y += 0.01f;
                    }
                }

                EntityManager.SetComponentData(instances[j], new Translation {Value = position});
                m_treePositions[j] = position;
            }

            instances.Dispose();
            randomUnitPoints.Dispose();
        }

        overlapChecks.Dispose();
        sourceEntities.Dispose();
        treeSpawners.Dispose();
        EntityManager.DestroyEntity(m_appleTreeSpawnerGroup);

        var appleSpawnerCount = m_appleSpawnersGroup.CalculateLength();

        for (var i = 0; i < appleSpawnerCount; i++)
        {
            var treeCount                  = m_treePositions.Length;
            var sourceAppleSpawnerEntities = m_appleSpawnersGroup.ToEntityArray(Allocator.TempJob);
            var appleSpawners              = m_appleSpawnersGroup.ToComponentDataArray<AppleSpawner>(Allocator.TempJob);
            var appleSpawner               = appleSpawners[i];
            var appleHeight                = appleSpawner.AppleRadius;
            var countPerTree               = appleSpawner.CountPerTree;
            var prefab                     = appleSpawner.Prefab;
            var radius                     = appleSpawner.SpawnRadius;

            for (var treeIndex = 0; treeIndex < treeCount; treeIndex++)
            {
                var center           = m_treePositions[treeIndex];
                var randomUnitPoints = new NativeArray<float3>(countPerTree * treeCount, Allocator.TempJob);
                GeneratePoints.RandomPointsInUnitSphere(randomUnitPoints);
                var instances = new NativeArray<Entity>(countPerTree, Allocator.TempJob);
                EntityManager.Instantiate(prefab, instances);

                for (var applePerTreeIndex = 0; applePerTreeIndex < countPerTree; applePerTreeIndex++)
                {
                    var instance        = instances[applePerTreeIndex];
                    var appleTotalIndex = treeIndex * countPerTree + applePerTreeIndex;
                    var position        = center + randomUnitPoints[appleTotalIndex] * radius;
                    var groundPosition  = new float3(position.x, appleHeight / 2, position.z);
                    EntityManager.SetComponentData(instance, new Translation
                    {
                        Value = groundPosition
                    });
                }

                randomUnitPoints.Dispose();
                instances.Dispose();
            }

            sourceAppleSpawnerEntities.Dispose();
            appleSpawners.Dispose();
            EntityManager.DestroyEntity(m_appleSpawnersGroup);
            Enabled = false;
        }
    }

    protected override void OnCreate()
    {
        m_random.InitState();
        m_appleTreeSpawnerGroup =
            GetEntityQuery(ComponentType.ReadOnly<TreeSpawner>(), ComponentType.ReadOnly<LocalToWorld>());

        m_appleSpawnersGroup =
            GetEntityQuery(ComponentType.ReadOnly<AppleSpawner>(), ComponentType.ReadOnly<LocalToWorld>());

        m_overlapCheckGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<WorldRenderBounds>(),
                typeof(House)
            }
        });
    }

    protected override void OnDestroy()
    {
        if (m_treePositions.IsCreated) m_treePositions.Dispose();
    }
}