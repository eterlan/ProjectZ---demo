using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectZ.AI.PathFinding
{
    [UpdateInGroup(typeof(PathFindingGroup))]
    [UpdateAfter(typeof(SpawnNodeSystem))]
    public class PathFinding : JobComponentSystem
    {
        // @Todo: Move const to other place?
        private       EntityQuery                            m_requestGroup;
        private       EntityQuery                            m_nodeQuery;
        private       EndSimulationEntityCommandBufferSystem m_commandBufferSystem;
        private const int                                    MaximumFindingCount = 3;

        private struct Grid
        {
            public int2 GridSize;

            // Node Index
            public NativeArray<NodeInfo> NodeInfos;
            public NativeArray<int2>     Positions;

            public       NativeArray<Neighbour> Neighbours;
            public const int                    UnitCost       = 10;
            public const int                    Unit           = 1;
            public const float                  InclineUnit    = Unit * math.SQRT2;
            public const int                    NeighbourCount = 8;
            public       int                    NodeCount;
        }

        private Grid                  m_grid;
        private List<PathFindingInfo> m_pathFindingInfos = new List<PathFindingInfo>(10);

        private struct PathFindingInfo
        {
            public NativeArray<float> CostSoFar;
            public NativeArray<int>   ParentIndex;
            public NativeMinHeap      OpenSet;
            public NativeList<int>    CloseSet;
            public NativeList<int2>   Path;
        }


        protected override JobHandle OnUpdate(JobHandle inputDependency)
        {
            var requestsCount = m_requestGroup.CalculateLength();

            // 如果没有请求就返回。
            if (requestsCount == 0) { return inputDependency; }

            for (var scheduledIndex = 0; scheduledIndex < MaximumFindingCount; scheduledIndex++)
            {
                var parentIndex = new NativeArray<int>(m_grid.NodeCount, Allocator.TempJob);
                var costSoFar   = new NativeArray<float>(m_grid.NodeCount, Allocator.TempJob);
                var closeSet    = new NativeList<int>(10, Allocator.TempJob);
                var openSet     = new NativeMinHeap(m_grid.NodeCount, Allocator.TempJob);
                var path        = new NativeList<int2>(10, Allocator.TempJob);

                for (var nodeIndex = 0; nodeIndex < m_grid.NodeCount; nodeIndex++) { costSoFar[nodeIndex] = float.PositiveInfinity; }

                var newPathFindingInfo = new PathFindingInfo
                {
                    ParentIndex = parentIndex,
                    CostSoFar   = costSoFar,
                    CloseSet    = closeSet,
                    OpenSet     = openSet,
                    Path        = path,
                };

                // Only at the beginning frame, when the list is not filled with arrays, this if condition would run.
                if (m_pathFindingInfos.Count - 1 < scheduledIndex) { m_pathFindingInfos.Add(newPathFindingInfo); }
                else
                {
                    m_pathFindingInfos[scheduledIndex].ParentIndex.Dispose();
                    m_pathFindingInfos[scheduledIndex].CostSoFar.Dispose();
                    m_pathFindingInfos[scheduledIndex].CloseSet.Dispose();
                    m_pathFindingInfos[scheduledIndex].OpenSet.Dispose();
                    m_pathFindingInfos[scheduledIndex].Path.Dispose();
                }

                m_pathFindingInfos[scheduledIndex] = newPathFindingInfo;

                var pathFindingJob = new PathFindingJob
                {
                    PathPlannerType        = GetArchetypeChunkBufferType<PathPlanner>(),
                    PathFindingRequestType = GetArchetypeChunkComponentType<PathFindingRequest>(),
                    PathFinderEntityType   = GetArchetypeChunkEntityType(),
                    // @TODO: ToConcurrent
                    CommandBuffer = m_commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                    CostSoFar     = costSoFar,
                    ParentIndex   = parentIndex,
                    OpenSet       = openSet,
                    CloseSet      = closeSet,
                    Path          = path,
                    NodeInfos     = m_grid.NodeInfos,
                    Positions     = m_grid.Positions,
                    GridSize      = m_grid.GridSize,
                    Neighbours    = m_grid.Neighbours,
                };

                var pathFindingJobHandle = pathFindingJob.Schedule(m_requestGroup, inputDependency);
                inputDependency = pathFindingJobHandle;
                // Add previous job to this Query' dependency.
                m_requestGroup.AddDependency(inputDependency);
                // Add every job to commandBufferSystem's dependency.
                m_commandBufferSystem.AddJobHandleForProducer(inputDependency);
                // @Bug: Atomic detect.
            }

            return inputDependency;
        }

        private struct PathFindingJob : IJobChunk
        {
            // @Todo: Move to another Job.
            public ArchetypeChunkBufferType<PathPlanner> PathPlannerType;

            [ReadOnly] public ArchetypeChunkComponentType<PathFindingRequest> PathFindingRequestType;
            [ReadOnly] public ArchetypeChunkEntityType                        PathFinderEntityType;
            public            EntityCommandBuffer.Concurrent                  CommandBuffer;

            public NativeArray<float> CostSoFar;
            public NativeArray<int>   ParentIndex;
            public NativeMinHeap      OpenSet;
            public NativeList<int>    CloseSet;
            public NativeList<int2>   Path;

            [ReadOnly] public NativeArray<NodeInfo>  NodeInfos;
            [ReadOnly] public NativeArray<int2>      Positions;
            [ReadOnly] public int2                   GridSize;
            [ReadOnly] public NativeArray<Neighbour> Neighbours;


            private NativeList<int2> ConstructPath(int startIndex, int endIndex)
            {
                var parentIndex = endIndex;

                while (parentIndex != startIndex)
                {
                    var position = Positions[parentIndex];
                    Path.Add(position);
                    // @Todo: Add to buffer.
                    parentIndex = ParentIndex[parentIndex];
                }

                return Path;
            }

            public void Execute
            (ArchetypeChunk chunk,
             int            chunkIndex,
             int            firstEntityIndex)
            {
                // DidChange
                var b0 = chunk.GetBufferAccessor(PathPlannerType);
                var c0 = chunk.GetNativeArray(PathFindingRequestType);
                var e0 = chunk.GetNativeArray(PathFinderEntityType);

                for (var index = 0; index < chunk.Count; index++)
                {
                    var pathPlanner = b0[index];
                    var request     = c0[index];
                    var pathFinder  = e0[index];

                    // not legal request, return.
                    if (request.StartIndex == request.EndIndex)
                    {
                        CommandBuffer.RemoveComponent<PathFindingRequest>(chunkIndex, pathFinder);
                        return;
                    }

                    var path       = FindPath(request.StartIndex, request.EndIndex);
                    var pathLength = path.Length;

                    // @Todo: isolated area should be determined before path finding.
                    // path not found. return.
                    if (pathLength == 0)
                    {
                        CommandBuffer.RemoveComponent<PathFindingRequest>(chunkIndex, pathFinder);
                        return;
                    }

                    for (var i = pathLength - 1; i >= 0; i--) { pathPlanner.Add(new PathPlanner {NextPosition = path[i]}); }
                    CommandBuffer.RemoveComponent<PathFindingRequest>(chunkIndex, pathFinder);
                }
            }

            private NativeList<int2> FindPath(int startIndex, int endIndex)
            {
                // OpenSet wait to be explore. It should only explore once, and each time should explore the most possible node. Which means the node with lowest F. F = G + H. G is the total cost coming to this point. H is roughly guessing remaining cost to the goal.
                // @Bug: Without this, never work.
                CostSoFar[startIndex] = 0;
                OpenSet.Push(new MinHeapNode(startIndex, 0));
                var endPos = Positions[endIndex];

                while (OpenSet.HasNext())
                {
                    var heapIndex    = OpenSet.Pop();
                    var current      = OpenSet[heapIndex];
                    var currentIndex = current.Index;

                    if (currentIndex == endIndex) { return ConstructPath(startIndex, endIndex); }

                    for (var index = 0; index < Neighbours.Length; index++)
                    {
                        var neighbour        = Neighbours[index];
                        var neighbourIsValid = GetIndexFromValidOffset(currentIndex, neighbour.Offset, out var neighbourIndex);

                        if (!neighbourIsValid)
                            continue;

                        if (CloseSet.Contains(neighbourIndex)) continue;
                        // @Bug: index = -1 is out of range. <- Invalid index is return. <- GetIFVO method
                        var walkable = NodeInfos[neighbourIndex].Walkable;

                        if (!walkable) continue;

                        var newCost = CostSoFar[currentIndex] + neighbour.Cost * Grid.UnitCost;
                        var oldCost = CostSoFar[neighbourIndex];

                        if (newCost >= oldCost) continue;
                        var neighbourPos  = Positions[neighbourIndex];
                        var estimatedCost = newCost + H(neighbourPos, endPos);

                        CostSoFar[neighbourIndex]   = newCost;
                        ParentIndex[neighbourIndex] = currentIndex;
                        OpenSet.Push(new MinHeapNode(neighbourIndex, estimatedCost));
                    }
                }

                float H(int2 current, int2 end)
                {
                    var x   = end.x - current.x;
                    var y   = end.y - current.y;
                    var sqr = x * x + y * y;
                    return sqr;
                    //return math.distancesq(current, end);
                    // @Todo Test Speed
                }

                return Path;
            }

            private bool GetIndexFromValidOffset
            (int     index,
             int2    offset,
             out int neighbourIndex)
            {
                neighbourIndex = 0;
                var sizeX = GridSize.x;
                var sizeY = GridSize.y;
                var x     = index % sizeX;
                var y     = index / sizeX;

                if (x + offset.x < 0
                    || x + offset.x > sizeX - 1
                    || y + offset.y < 0
                    || y + offset.y > sizeY - 1) { return false; }

                neighbourIndex = index + offset.x + offset.y * sizeX;
                return true;
            }
        }

        public void ClearPath(Entity entity) { EntityManager.GetBuffer<PathPlanner>(entity).Clear(); }

        protected override void OnStartRunning()
        {
            FillNodeInfos();
            void FillNodeInfos()
            {
                var translations = m_nodeQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
                var nodeInfos    = m_nodeQuery.ToComponentDataArray<NodeInfo>(Allocator.TempJob);
                
                Debug.Log($"In OnCreate, NodeLength: {nodeInfos.Length}");

                for (var i = 0; i < translations.Length; i++)
                {
                    var index       = nodeInfos[i].Index;
                    var translation = translations[i];
                    var nodeInfo    = nodeInfos[i];
                    m_grid.Positions[index] = new int2((int) translation.Value.x, (int) translation.Value.z);
                    m_grid.NodeInfos[index] = nodeInfo;
                }

                translations.Dispose();
                nodeInfos.Dispose();
            }
        }
        
        protected override void OnCreate()
        {
            m_requestGroup        = GetEntityQuery(ComponentType.ReadWrite<PathPlanner>(), ComponentType.ReadOnly<PathFindingRequest>());
            m_nodeQuery           = GetEntityQuery(ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<NodeInfo>());
            m_commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            InitializeGrid();
            FillNeighbours();

            void InitializeGrid()
            {
                // var spawnerQuery = EntityManager.CreateEntityQuery(typeof(Spawner));
                // Debug.Log(spawnerQuery.CalculateLength());
                //  var size = spawnerQuery.GetSingleton<Spawner>().Count;
                // @Todo: Hard code. Wait for run time to fix it.
                var size = new int2(10, 10);
                m_grid.NodeCount = size.x * size.y;
                m_grid = new Grid
                {
                    GridSize  = size,
                    NodeInfos = new NativeArray<NodeInfo>(m_grid.NodeCount, Allocator.Persistent),
                    Positions = new NativeArray<int2>(m_grid.NodeCount, Allocator.Persistent),
                };

            }
            
            void FillNeighbours()
            {
                m_grid.Neighbours = new NativeArray<Neighbour>(Grid.NeighbourCount, Allocator.Persistent)
                {
                    [0] = new Neighbour(new int2(-Grid.Unit, Grid.Unit), Grid.InclineUnit),
                    [1] = new Neighbour(new int2(0, Grid.Unit), Grid.Unit),
                    [2] = new Neighbour(new int2(Grid.Unit, Grid.Unit), Grid.InclineUnit),
                    [3] = new Neighbour(new int2(-Grid.Unit, 0), Grid.Unit),
                    [4] = new Neighbour(new int2(Grid.Unit, 0), Grid.Unit),
                    [5] = new Neighbour(new int2(-Grid.Unit, -Grid.Unit), Grid.InclineUnit),
                    [6] = new Neighbour(new int2(0, -Grid.Unit), Grid.Unit),
                    [7] = new Neighbour(new int2(Grid.Unit, -Grid.Unit), Grid.InclineUnit)
                };
            }
        }


        protected override void OnDestroy()
        {
            m_grid.NodeInfos.Dispose();
            m_grid.Neighbours.Dispose();
            m_grid.Positions.Dispose();

            for (var i = 0; i < m_pathFindingInfos.Count; i++)
            {
                m_pathFindingInfos[i].ParentIndex.Dispose();
                m_pathFindingInfos[i].CostSoFar.Dispose();
                m_pathFindingInfos[i].CloseSet.Dispose();
                m_pathFindingInfos[i].OpenSet.Dispose();
                m_pathFindingInfos[i].Path.Dispose();
            }

            m_pathFindingInfos.Clear();
        }
    }

    internal struct Neighbour
    {
        public int2  Offset;
        public float Cost;

        public Neighbour(int2 offset, float cost)
        {
            Offset = offset;
            Cost   = cost;
        }
    }
}