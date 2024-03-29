
```C#
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Samples.Boids
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class BoidSystem : JobComponentSystem
    {
        private ComponentGroup  m_BoidGroup; // Declare group.
        private ComponentGroup  m_TargetGroup; // Declare group.
        private ComponentGroup  m_ObstacleGroup; // Declare group.

        private List<Boid>      m_UniqueTypes = new List<Boid>(10); // How many different maps.
        private List<PrevCells> m_PrevCells   = new List<PrevCells>(); //

        struct PrevCells // It's a group of fish divided by region.
        {
            public NativeMultiHashMap<int, int> hashMap; // ?
            public NativeArray<int>             cellIndices; // index for fish in this group.
            public NativeArray<float3>          copyTargetPositions;
            public NativeArray<float3>          copyObstaclePositions;
            public NativeArray<float3>          cellAlignment;
            public NativeArray<float3>          cellSeparation;
            public NativeArray<int>             cellObstaclePositionIndex;
            public NativeArray<float>           cellObstacleDistance;
            public NativeArray<int>             cellTargetPositionIndex;
            public NativeArray<int>             cellCount;
        }

        [BurstCompile]
        struct CopyPositions : IJobForEachWithEntity<LocalToWorld>
        {
            public NativeArray<float3> positions;

            public void Execute(Entity entity, int index, [ReadOnly]ref LocalToWorld localToWorld)
            {
                positions[index] = localToWorld.Position;
            }
        }

        [BurstCompile]
        struct CopyHeadings : IJobForEachWithEntity<LocalToWorld>
        {
            public NativeArray<float3> headings;

            public void Execute(Entity entity, int index, [ReadOnly]ref LocalToWorld localToWorld)
            {
                headings[index] = localToWorld.Forward;
            }
        }


        [BurstCompile]
        [RequireComponentTag(typeof(Boid))]
        struct HashPositions : IJobForEachWithEntity<LocalToWorld>
        {
            public NativeMultiHashMap<int, int>.Concurrent hashMap; // ( hash , index)
            public float                                   cellRadius;

            public void Execute(Entity entity, int index, [ReadOnly]ref LocalToWorld localToWorld)
            {
                var hash = (int)math.hash(new int3(math.floor(localToWorld.Position / cellRadius)));
                // math.floor(localToWorld.Position / cellRadius))
                // if radius is 3, x is 90, than floor(90~119 / 3) = 3. same to the y & z axis.
                // So, it basically divide the space with a box which r
                // math.hash(int3)
                //
                hashMap.Add(hash, index);
            }
        }

        [BurstCompile]
        struct MergeCells : IJobNativeMultiHashMapMergedSharedKeyIndices
        {
            public NativeArray<int>                 cellIndices;
            public NativeArray<float3>              cellAlignment;
            public NativeArray<float3>              cellSeparation;
            public NativeArray<int>                 cellObstaclePositionIndex;
            public NativeArray<float>               cellObstacleDistance;
            public NativeArray<int>                 cellTargetPositionIndex;
            public NativeArray<int>                 cellCount;
            [ReadOnly] public NativeArray<float3>   targetPositions;
            [ReadOnly] public NativeArray<float3>   obstaclePositions;

            void NearestPosition(NativeArray<float3> targets, float3 position, out int nearestPositionIndex, out float nearestDistance )
            {
                nearestPositionIndex = 0;
                nearestDistance      = math.lengthsq(position-targets[0]);
                for (int i = 1; i < targets.Length; i++)
                {
                    var targetPosition = targets[i];
                    var distance       = math.lengthsq(position-targetPosition);
                    var nearest        = distance < nearestDistance;

                    nearestDistance      = math.select(nearestDistance, distance, nearest);
                    nearestPositionIndex = math.select(nearestPositionIndex, i, nearest);
                }
                nearestDistance = math.sqrt(nearestDistance);
            }

            public void ExecuteFirst(int index)
            {
                var position = cellSeparation[index] / cellCount[index];

                int obstaclePositionIndex;
                float obstacleDistance;
                NearestPosition(obstaclePositions, position, out obstaclePositionIndex, out obstacleDistance);
                cellObstaclePositionIndex[index] = obstaclePositionIndex;
                cellObstacleDistance[index]      = obstacleDistance;

                int targetPositionIndex;
                float targetDistance;
                NearestPosition(targetPositions, position, out targetPositionIndex, out targetDistance);
                cellTargetPositionIndex[index] = targetPositionIndex;

                cellIndices[index] = index;
            }

            /// <summary>
            /// What next? Why combine?
            /// </summary>
            /// <param name="cellIndex"></param>
            /// <param name="index"></param>
            public void ExecuteNext(int cellIndex, int index)
            {
                cellCount[cellIndex]      += 1;
                cellAlignment[cellIndex]  = cellAlignment[cellIndex] + cellAlignment[index];
                cellSeparation[cellIndex] = cellSeparation[cellIndex] + cellSeparation[index];
                cellIndices[index]        = cellIndex;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Boid))]
        struct Steer : IJobForEachWithEntity<LocalToWorld>
        {
            [ReadOnly] public NativeArray<int>             cellIndices;
            [ReadOnly] public Boid                         settings;
            [ReadOnly] public NativeArray<float3>          targetPositions;
            [ReadOnly] public NativeArray<float3>          obstaclePositions;
            [ReadOnly] public NativeArray<float3>          cellAlignment;
            [ReadOnly] public NativeArray<float3>          cellSeparation;
            [ReadOnly] public NativeArray<int>             cellObstaclePositionIndex;
            [ReadOnly] public NativeArray<float>           cellObstacleDistance;
            [ReadOnly] public NativeArray<int>             cellTargetPositionIndex;
            [ReadOnly] public NativeArray<int>             cellCount;
            public float                                   dt;

            public void Execute(Entity entity, int index, ref LocalToWorld localToWorld)
            {
                var forward                           = localToWorld.Forward;
                var currentPosition                   = localToWorld.Position;
                var cellIndex                         = cellIndices[index];
                var neighborCount                     = cellCount[cellIndex];
                var alignment                         = cellAlignment[cellIndex];
                var separation                        = cellSeparation[cellIndex];
                var nearestObstacleDistance           = cellObstacleDistance[cellIndex];
                var nearestObstaclePositionIndex      = cellObstaclePositionIndex[cellIndex];
                var nearestTargetPositionIndex        = cellTargetPositionIndex[cellIndex];
                var nearestObstaclePosition           = obstaclePositions[nearestObstaclePositionIndex];
                var nearestTargetPosition             = targetPositions[nearestTargetPositionIndex];

                var obstacleSteering                  = currentPosition - nearestObstaclePosition;
                var avoidObstacleHeading              = (nearestObstaclePosition + math.normalizesafe(obstacleSteering)
                                                        * settings.ObstacleAversionDistance)- currentPosition;
                var targetHeading                     = settings.TargetWeight
                                                        * math.normalizesafe(nearestTargetPosition - currentPosition);
                var nearestObstacleDistanceFromRadius = nearestObstacleDistance - settings.ObstacleAversionDistance;
                var alignmentResult                   = settings.AlignmentWeight
                                                        * math.normalizesafe((alignment/neighborCount)-forward);
                var separationResult                  = settings.SeparationWeight
                                                        * math.normalizesafe((currentPosition * neighborCount) - separation);
                var normalHeading                     = math.normalizesafe(alignmentResult + separationResult + targetHeading);
                var targetForward                     = math.select(normalHeading, avoidObstacleHeading, nearestObstacleDistanceFromRadius < 0);
                var nextHeading                       = math.normalizesafe(forward + dt*(targetForward-forward));

                localToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(
                        new float3(localToWorld.Position + (nextHeading * settings.MoveSpeed * dt)),
                        quaternion.LookRotationSafe(nextHeading, math.up()),
                        new float3(1.0f, 1.0f, 1.0f))
                };
            }
        }

        protected override void OnStopRunning()
        {
            for (var i = 0; i < m_PrevCells.Count; i++)
            {
                m_PrevCells[i].hashMap.Dispose();
                m_PrevCells[i].cellIndices.Dispose();
                m_PrevCells[i].copyTargetPositions.Dispose();
                m_PrevCells[i].copyObstaclePositions.Dispose();
                m_PrevCells[i].cellAlignment.Dispose();
                m_PrevCells[i].cellSeparation.Dispose();
                m_PrevCells[i].cellObstacleDistance.Dispose();
                m_PrevCells[i].cellObstaclePositionIndex.Dispose();
                m_PrevCells[i].cellTargetPositionIndex.Dispose();
                m_PrevCells[i].cellCount.Dispose();
            }
            m_PrevCells.Clear();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityManager.GetAllUniqueSharedComponentData(m_UniqueTypes); // get all maps and put it in a List.

            var obstacleCount = m_ObstacleGroup.CalculateLength(); // calculate how many obstacle using GroupQuery.
            var targetCount = m_TargetGroup.CalculateLength();  // calculate how many target using GroupQuery.


            // Ignore typeIndex 0, can't use the default for anything meaningful.
            for (int typeIndex = 1; typeIndex < m_UniqueTypes.Count; typeIndex++)
            {

                var settings = m_UniqueTypes[typeIndex]; // get the first map
                m_BoidGroup.SetFilter(settings); // query for fish who keep map[1]

                var boidCount  = m_BoidGroup.CalculateLength(); // 25000 fish who keep first map. (two spawners)
                // cacheIndex is
                var cacheIndex                = typeIndex - 1; // we minus 1 because typeIndex is added 1 before.
                var cellIndices               = new NativeArray<int>(boidCount, Allocator.TempJob,NativeArrayOptions.UninitializedMemory); // Index in each cell?
                //
                var hashMap                   = new NativeMultiHashMap<int,int>(boidCount,Allocator.TempJob);
                // How far is the obstacle from cell?
                var cellObstacleDistance      = new NativeArray<float>(boidCount, Allocator.TempJob,NativeArrayOptions.UninitializedMemory);
                // How far is the obstacle from cell?
                var cellObstaclePositionIndex = new NativeArray<int>(boidCount, Allocator.TempJob,NativeArrayOptions.UninitializedMemory);
                var cellTargetPositionIndex   = new NativeArray<int>(boidCount, Allocator.TempJob,NativeArrayOptions.UninitializedMemory);
                var cellCount                 = new NativeArray<int>(boidCount, Allocator.TempJob,NativeArrayOptions.UninitializedMemory);


                var cellAlignment = new NativeArray<float3>(boidCount, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);
                var cellSeparation = new NativeArray<float3>(boidCount, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);
                var copyTargetPositions = new NativeArray<float3>(targetCount, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);
                var copyObstaclePositions = new NativeArray<float3>(obstacleCount, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);

                var initialCellAlignmentJob = new CopyHeadings
                {
                    headings = cellAlignment
                };
                var initialCellAlignmentJobHandle = initialCellAlignmentJob.Schedule(m_BoidGroup, inputDeps);

                var initialCellSeparationJob = new CopyPositions
                {
                    positions = cellSeparation
                };
                var initialCellSeparationJobHandle = initialCellSeparationJob.Schedule(m_BoidGroup, inputDeps);

                var copyTargetPositionsJob = new CopyPositions
                {
                    positions = copyTargetPositions
                };
                var copyTargetPositionsJobHandle = copyTargetPositionsJob.Schedule(m_TargetGroup, inputDeps);

                var copyObstaclePositionsJob = new CopyPositions
                {
                    positions = copyObstaclePositions
                };
                var copyObstaclePositionsJobHandle = copyObstaclePositionsJob.Schedule(m_ObstacleGroup, inputDeps);

                var nextCells = new PrevCells
                {
                    cellIndices               = cellIndices,
                    hashMap                   = hashMap,
                    copyObstaclePositions     = copyObstaclePositions,
                    copyTargetPositions       = copyTargetPositions,
                    cellAlignment             = cellAlignment,
                    cellSeparation            = cellSeparation,
                    cellObstacleDistance      = cellObstacleDistance,
                    cellObstaclePositionIndex = cellObstaclePositionIndex,
                    cellTargetPositionIndex   = cellTargetPositionIndex,
                    cellCount                 = cellCount
                };

                Debug.Log($"Before Prev.Add()  boidCount{boidCount}, UniqueTypes.Count{m_UniqueTypes.Count}, typeIndex{typeIndex}, cacheIndex{cacheIndex}, m_PrevCells.Count{m_PrevCells.Count}");

                if (cacheIndex > (m_PrevCells.Count - 1)) //
                {
                    m_PrevCells.Add(nextCells);
                }
                else
                {
                    m_PrevCells[cacheIndex].hashMap.Dispose();
                    m_PrevCells[cacheIndex].cellIndices.Dispose();
                    m_PrevCells[cacheIndex].cellObstaclePositionIndex.Dispose();
                    m_PrevCells[cacheIndex].cellTargetPositionIndex.Dispose();
                    m_PrevCells[cacheIndex].copyTargetPositions.Dispose();
                    m_PrevCells[cacheIndex].copyObstaclePositions.Dispose();
                    m_PrevCells[cacheIndex].cellAlignment.Dispose();
                    m_PrevCells[cacheIndex].cellSeparation.Dispose();
                    m_PrevCells[cacheIndex].cellObstacleDistance.Dispose();
                    m_PrevCells[cacheIndex].cellCount.Dispose();
                }
                m_PrevCells[cacheIndex] = nextCells; // if cacheIndex>=Prev.Count Add Position just == cacheIndex.
                // else (<) dispose the older PrevCells[cachedIndex] and override a nextCells to it.

                // ?
                var hashPositionsJob = new HashPositions
                {
                    hashMap        = hashMap.ToConcurrent(),
                    cellRadius     = settings.CellRadius
                };
                var hashPositionsJobHandle = hashPositionsJob.Schedule(m_BoidGroup, inputDeps);


                // ?
                var initialCellCountJob = new MemsetNativeArray<int>
                {
                    Source = cellCount,
                    Value  = 1
                };

                var initialCellCountJobHandle = initialCellCountJob.Schedule(boidCount, 64, inputDeps);

                // Schedule means schedule who should already run before this job.
                // A depends on B, which means A depends on B result.
                // CombineDepencies means A & B should run before C.
                var initialCellBarrierJobHandle = JobHandle.CombineDependencies(initialCellAlignmentJobHandle, initialCellSeparationJobHandle, initialCellCountJobHandle);
                var copyTargetObstacleBarrierJobHandle = JobHandle.CombineDependencies(copyTargetPositionsJobHandle, copyObstaclePositionsJobHandle);
                var mergeCellsBarrierJobHandle = JobHandle.CombineDependencies(hashPositionsJobHandle, initialCellBarrierJobHandle, copyTargetObstacleBarrierJobHandle);

                var mergeCellsJob = new MergeCells
                {
                    cellIndices               = cellIndices,
                    cellAlignment             = cellAlignment,
                    cellSeparation            = cellSeparation,
                    cellObstacleDistance      = cellObstacleDistance,
                    cellObstaclePositionIndex = cellObstaclePositionIndex,
                    cellTargetPositionIndex    = cellTargetPositionIndex,
                    cellCount                 = cellCount,
                    targetPositions           = copyTargetPositions,
                    obstaclePositions         = copyObstaclePositions
                };
                var mergeCellsJobHandle = mergeCellsJob.Schedule(hashMap,64,mergeCellsBarrierJobHandle);
                // See, here's what the hashMap used for.

                var steerJob = new Steer
                {
                    cellIndices               = nextCells.cellIndices,
                    settings                  = settings,
                    cellAlignment             = cellAlignment,
                    cellSeparation            = cellSeparation,
                    cellObstacleDistance      = cellObstacleDistance,
                    cellObstaclePositionIndex = cellObstaclePositionIndex,
                    cellTargetPositionIndex    = cellTargetPositionIndex,
                    cellCount                 = cellCount,
                    targetPositions           = copyTargetPositions,
                    obstaclePositions         = copyObstaclePositions,
                    dt                        = Time.deltaTime,
                };
                var steerJobHandle = steerJob.Schedule(m_BoidGroup, mergeCellsJobHandle);

                inputDeps = steerJobHandle;
                m_BoidGroup.AddDependency(inputDeps);
            }
            m_UniqueTypes.Clear();

            return inputDeps;
        }

        protected override void OnCreateManager()
        {
            m_BoidGroup = GetEntityQuery(new EntityArchetypeQuery
            {
                All = new [] { ComponentType.ReadOnly<Boid>(), ComponentType.ReadWrite<LocalToWorld>() },
                Options = EntityArchetypeQueryOptions.FilterWriteGroup
            });

            m_TargetGroup = GetEntityQuery(new EntityArchetypeQuery
            {
                All = new [] { ComponentType.ReadOnly<BoidTarget>(), ComponentType.ReadOnly<LocalToWorld>() },
            });

            m_ObstacleGroup = GetEntityQuery(new EntityArchetypeQuery
            {
                All = new [] { ComponentType.ReadOnly<BoidObstacle>(), ComponentType.ReadOnly<LocalToWorld>() },
            });
        }
    }
}
```