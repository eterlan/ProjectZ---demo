using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;


namespace ProjectZ.AI.PathFinding
{
    [UpdateInGroup(typeof(PathFindingGroup))]
    [UpdateAfter(typeof(SpawnNode))]
    public class SpawnPathFinder : ComponentSystem
    {
        private Random      m_random = new Random();
        private EntityQuery m_spawnerQuery;

        protected override void OnUpdate()
        {
            // @Todo: 100 Finder has different start and end index.
            // @Todo: Comment out Debug. Test efficiency.
            // @Todo: extend the map to 30 * 30
            // Set FinderSpawnerPosition according to node spawner pos.
            var nodeSpawner = GetSingleton<NodeSpawner>();
            var nodeSpawnerUnit = nodeSpawner.Space;
            var offsetX = nodeSpawnerUnit * nodeSpawner.Count.x / 2;
            var offsetY = nodeSpawnerUnit * nodeSpawner.Count.y / 2;
            var finderSpawnerPos = nodeSpawner.Position + new float3(offsetX,0f, offsetY);
            
            // Set finderInitPos according to finder spawner pos.
            var spawners  = m_spawnerQuery.ToComponentDataArray<FinderSpawner>(Allocator.TempJob);
            var entities  = m_spawnerQuery.ToEntityArray(Allocator.TempJob);

            for (var spawnerIndex = 0; spawnerIndex < spawners.Length; spawnerIndex++)
            {
                var spawner         = spawners[spawnerIndex];
                var spawnerEntity   = entities[spawnerIndex];
                EntityManager.SetComponentData(spawnerEntity, new Translation{Value = finderSpawnerPos});

                var finder          = spawner.Finder;
                var finderCount     = spawner.Count;
                var finderEntities  = new NativeArray<Entity>(finderCount, Allocator.TempJob);
                var randomPositions = new NativeArray<float3>(2 * finderCount, Allocator.TempJob);
                // @TEST: NSlice 是引用还是复制， 我猜测是引用，不然需要释放。
                var finderStartPositions = new NativeSlice<float3>(randomPositions,0, finderCount);
                var finderEndPositions = new NativeSlice<float3>(randomPositions,finderCount,finderCount);
                
                GeneratePoints.RandomPointsInSphere(finderSpawnerPos, spawner.Radius, randomPositions);
                EntityManager.Instantiate(finder, finderEntities);

                for (var i = 0; i < finderCount; i++)
                {
                    var finderEntity = finderEntities[i];
                    var startPos = finderStartPositions[i];
                    var endPos = finderEndPositions[i];
                    startPos.y = 0;
                    endPos.y = 0;
                    EntityManager.SetComponentData(finderEntity,new Translation{Value = startPos});
                    EntityManager.SetComponentData(finderEntity, new PathFindingRequest
                    {
                        StartPosition = startPos,
                        EndPosition = endPos
                    });
                }

                finderEntities.Dispose();
                randomPositions.Dispose();

                EntityManager.DestroyEntity(spawnerEntity);
            }

            spawners.Dispose();
            entities.Dispose();
        }

        protected override void OnCreate()
        {
            m_random.InitState();
            m_spawnerQuery = GetEntityQuery(typeof(FinderSpawner), typeof(Translation));
        }

        protected override void OnDestroy() { }
    }
}