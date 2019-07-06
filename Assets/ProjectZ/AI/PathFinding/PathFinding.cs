using ProjectZ.AI.PathFinding.Container;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectZ.AI.PathFinding
{
    // @Todo: 只在所有query都不为0的情况下才运行。
    [UpdateInGroup(typeof(PathFindingGroup))]
    [UpdateAfter(typeof(SpawnNode))]
    public class PathFinding : JobComponentSystem
    {
        // @Todo: Move const to other place?
        private       EntityQuery                            m_requestGroup;
        private       EntityQuery                            m_nodeQuery;
        private       EndSimulationEntityCommandBufferSystem m_commandBufferSystem;
        private const int                                    MaxProccessCount = 3;
        private       Grid                                   m_grid;

        private struct Grid
        {
            // Node Index
            public NativeArray<NodeInfo>  NodeInfos;
            public NativeArray<int2>      Positions;
            public NativeArray<Neighbour> Neighbours;

            public int  NodeCount;
            public int2 GridSize;
            public int2 Zero;
            public int  UnitCost;
            public int  DistancePerUnit;

            public const int   Unit           = 1;
            public const float InclineUnit    = Unit * math.SQRT2;
            public const int   NeighbourCount = 8;
        }

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
            var requestEntities        = m_requestGroup.ToEntityArray(Allocator.TempJob);
            var requests               = m_requestGroup.ToComponentDataArray<PathFindingRequest>(Allocator.TempJob);
            var processingRequestCount = math.min(MaxProccessCount, requestEntities.Length);
            var pathPlanners =
                new NativeArray<DynamicBuffer<PathPlanner>>(MaxProccessCount, Allocator.TempJob);
            for (var i = 0;
                i < processingRequestCount;
                i++)
            {
                var entity      = requestEntities[i];
                var pathPlanner = EntityManager.GetBuffer<PathPlanner>(entity);
                pathPlanners[i] = pathPlanner;
            }

            for (var i = 0;
                i < processingRequestCount;
                i++)
            {
                var requestEntity = requestEntities[i];
                // @Bug 因为在主线程获取Component没有dependency，因此在loop中使用会导致报错：上一个Job还未完成又被获取了写入权限。
                // var pathPlanner = EntityManager.GetBuffer<PathPlanner>(requestEntity);
                var pathPlanner = pathPlanners[i];
                var request     = requests[i];

                var parentIndex = new NativeArray<int>(m_grid.NodeCount, Allocator.TempJob);
                var costSoFar   = new NativeArray<float>(m_grid.NodeCount, Allocator.TempJob);
                var closeSet    = new NativeList<int>(10, Allocator.TempJob);
                var openSet     = new NativeMinHeap(m_grid.NodeCount, Allocator.TempJob);
                var path        = new NativeList<int2>(10, Allocator.TempJob);

                for (var nodeIndex = 0; nodeIndex < m_grid.NodeCount; nodeIndex++)
                {
                    costSoFar[nodeIndex] = float.PositiveInfinity;
                }

                var newPathFindingInfo = new PathFindingInfo
                {
                    CostSoFar   = costSoFar,
                    ParentIndex = parentIndex,
                    OpenSet     = openSet,
                    CloseSet    = closeSet,
                    Path        = path,
                };

                var pathFindingJob = new PathFindingJob
                {
                    Unit2Dist          = m_grid.DistancePerUnit,
                    UnitCost           = m_grid.UnitCost,
                    GridZero           = m_grid.Zero,
                    PathFindingRequest = request,
                    // PathFinderEntity   = requestEntity,
                    // CommandBuffer      = m_commandBufferSystem.CreateCommandBuffer(),
                    CostSoFar   = newPathFindingInfo.CostSoFar,
                    ParentIndex = newPathFindingInfo.ParentIndex,
                    OpenSet     = newPathFindingInfo.OpenSet,
                    CloseSet    = newPathFindingInfo.CloseSet,
                    Path        = newPathFindingInfo.Path,
                    NodeInfos   = m_grid.NodeInfos,
                    Positions   = m_grid.Positions,
                    GridSize    = m_grid.GridSize,
                    Neighbours  = m_grid.Neighbours,
                };

                var pathFindingJobHandle = pathFindingJob.Schedule(inputDependency);

                var writePathJob = new WritePath
                {
                    RequestEntity = requestEntity,
                    CommandBuffer = m_commandBufferSystem.CreateCommandBuffer(),
                    PathPlanner   = pathPlanner,
                    Path          = path,
                };

                var writePathJobHandle = writePathJob.Schedule(pathFindingJobHandle);

                closeSet.Dispose(pathFindingJobHandle);
                path.Dispose(writePathJobHandle);
                inputDependency = writePathJobHandle;
                //inputDependency.Complete();

                m_commandBufferSystem.AddJobHandleForProducer(inputDependency);
                // @Bug: Atomic detect.
            }

            requestEntities.Dispose();
            requests.Dispose();
            pathPlanners.Dispose();

            return inputDependency;
        }

        [BurstCompile]
        private struct PathFindingJob : IJob
        {
            public int  Unit2Dist;
            public int  UnitCost;
            public int2 GridZero;

            [ReadOnly] public PathFindingRequest PathFindingRequest;
            // [ReadOnly] public Entity              PathFinderEntity;
            // public            EntityCommandBuffer CommandBuffer;

            [DeallocateOnJobCompletion]
            public NativeArray<float> CostSoFar;

            [DeallocateOnJobCompletion]
            public NativeArray<int> ParentIndex;

            [DeallocateOnJobCompletion]
            public NativeMinHeap OpenSet;

            public NativeList<int>  CloseSet;
            public NativeList<int2> Path;

            [ReadOnly] public NativeArray<NodeInfo>  NodeInfos;
            [ReadOnly] public NativeArray<int2>      Positions;
            [ReadOnly] public int2                   GridSize;
            [ReadOnly] public NativeArray<Neighbour> Neighbours;

            public void Execute()
            {
                // not legal request, return.
                if (math.distancesq(PathFindingRequest.StartPosition, PathFindingRequest.EndPosition) < 1f)
                {
                    // PathFound(false, PathFindingFailedReason.StartEqualEnd);
                    return;
                }

                var startIndex = ConvertPosToIndex(PathFindingRequest.StartPosition);
                var endIndex   = ConvertPosToIndex(PathFindingRequest.EndPosition);
                endIndex = SelectValidEndIndex(startIndex,endIndex);
                FindPath(startIndex, endIndex);
            }

            private void ConstructPath(int startIndex, int endIndex)
            {
                var parentIndex = endIndex;
                //var sb = new StringBuilder();
                //sb.Append("PathList: end <-");

                while (parentIndex != startIndex)
                {
                    var position = Positions[parentIndex];
                    Path.Add(position);
                    //sb.Append($" {position} <-");
                    parentIndex = ParentIndex[parentIndex];
                }

                //sb.Append($" start");
                //Debug.Log(sb);
            }

            private int ConvertPosToIndex(float3 position)
            {
                var x     = ConvertPosToIndex(position.x - GridZero.x);
                var y     = ConvertPosToIndex(position.z - GridZero.y);
                var index = x + y * GridSize.x;
                math.clamp(index, 0, GridSize.x * GridSize.y);
                return index;
            }

            private int SelectValidEndIndex(int startIndex, int endIndex)
            {
                if (startIndex == endIndex) { endIndex++; }
                while (!NodeInfos[endIndex].Walkable)
                {
                    endIndex++;
                }
                return endIndex;
            }

            private int ConvertPosToIndex(float position)
            {
                var temp = (int) position;
                var ones = temp % Unit2Dist;
                temp -= ones + math.select(Unit2Dist, 0, ones < Unit2Dist / 2);
                return temp / Unit2Dist;
            }

            private void FindPath(int startIndex, int endIndex)
            {
                // OpenSet wait to be explore. It should only explore once, and each time should explore the most possible node. Which means the node with lowest F. F = G + H. G is the total cost coming to this point. H is roughly guessing remaining cost to the goal.
                // @Bug: Must manually assign initial cost to 0, else it would be infinity.
                CostSoFar[startIndex] = 0;
                OpenSet.Push(new MinHeapNode(startIndex, 0));
                var endPos = Positions[endIndex];

                while (OpenSet.HasNext())
                {
                    var heapIndex    = OpenSet.Pop();
                    var current      = OpenSet[heapIndex];
                    var currentIndex = current.Index;

                    if (currentIndex == endIndex)
                    {
                        ConstructPath(startIndex, endIndex); 
                        return;
                    }

                    for (var index = 0; index < Neighbours.Length; index++)
                    {
                        var neighbour = Neighbours[index];
                        var neighbourIsValid =
                            GetIndexFromValidOffset(currentIndex, neighbour.Offset, out var neighbourIndex);

                        if (!neighbourIsValid)
                            continue;

                        if (CloseSet.Contains(neighbourIndex)) continue;
                        // @Bug: index = -1 is out of range. <- Invalid index is return. <- GetIFVO method
                        var walkable = NodeInfos[neighbourIndex].Walkable;

                        if (!walkable) continue;

                        var newCost = CostSoFar[currentIndex] + neighbour.Cost * UnitCost;
                        var oldCost = CostSoFar[neighbourIndex];

                        if (newCost >= oldCost) continue;
                        var neighbourPos  = Positions[neighbourIndex];
                        var estimatedCost = newCost + H(neighbourPos, endPos);

                        CostSoFar[neighbourIndex]   = newCost;
                        ParentIndex[neighbourIndex] = currentIndex;
                        OpenSet.Push(new MinHeapNode(neighbourIndex, estimatedCost));
                    }
                }

                // PathFound(false, PathFindingFailedReason.EndNotAccessible);

                float H(int2 current, int2 end)
                {
                    var x   = end.x - current.x;
                    var y   = end.y - current.y;
                    var sqr = x * x + y * y;
                    return sqr;
                    //return math.distancesq(current, end);
                    // @Todo Test Speed
                }
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

            // @Todo: burst doesn't support Debug. Change to exception?
            // private void PathFound(bool success, PathFindingFailedReason reason = PathFindingFailedReason.None)
            // {
            //     if (!success)
            //         Debug.Log($"PathFinding Failed, reason is {reason.ToString()}");
            // }
        }

        private struct WritePath : IJob
        {
            public Entity                     RequestEntity;
            public EntityCommandBuffer        CommandBuffer;
            public DynamicBuffer<PathPlanner> PathPlanner;
            public NativeList<int2>           Path;

            public void Execute()
            {
                var pathLength = Path.Length;

                for (var i = pathLength - 1; i >= 0; i--) { PathPlanner.Add(new PathPlanner {NextPosition = Path[i]}); }
                CommandBuffer.RemoveComponent<PathFindingRequest>(RequestEntity);
            }
        }

        public void ClearPath(Entity entity) { EntityManager.GetBuffer<PathPlanner>(entity).Clear(); }

        protected override void OnStartRunning()
        {
            var nodeSpawner = GetSingleton<NodeSpawner>();
            InitializeGrid();
            FillNodeInfos();
            FillNeighbours();

            void InitializeGrid()
            {
                var size = nodeSpawner.Count;

                var nodeCount = size.x * size.y;
                m_grid = new Grid
                {
                    NodeCount = nodeCount,
                    GridSize  = size,
                    NodeInfos = new NativeArray<NodeInfo>(nodeCount, Allocator.Persistent),
                    Positions = new NativeArray<int2>(nodeCount, Allocator.Persistent),
                };
            }

            void FillNodeInfos()
            {
                var translations = m_nodeQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
                var nodeInfos    = m_nodeQuery.ToComponentDataArray<NodeInfo>(Allocator.TempJob);
                var nodeCount    = translations.Length;


                for (var i = 0; i < nodeCount; i++)
                {
                    var index       = nodeInfos[i].Index;
                    var translation = translations[i];
                    var nodeInfo    = nodeInfos[i];
                    m_grid.Positions[index] = new int2((int) translation.Value.x, (int) translation.Value.z);
                    m_grid.NodeInfos[index] = nodeInfo;
                }

                m_grid.Zero            = m_grid.Positions[0];
                m_grid.DistancePerUnit = nodeSpawner.NodeSpace;
                m_grid.UnitCost        = nodeSpawner.NodeSpace * Grid.Unit;
                translations.Dispose();
                nodeInfos.Dispose();
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

        protected override void OnCreate()
        {
            m_requestGroup = GetEntityQuery(ComponentType.ReadWrite<PathPlanner>(),
                ComponentType.ReadOnly<PathFindingRequest>());

            m_nodeQuery =
                GetEntityQuery(ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<NodeInfo>());

            //RequireForUpdate(m_requestGroup);
            // @Todo: Should I move initialize grid to OnCreate()? Because I doesn't need to reinitialize grid after this system stop running from a period.
            m_commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }


        protected override void OnDestroy()
        {
            m_grid.NodeInfos.Dispose();
            m_grid.Neighbours.Dispose();
            m_grid.Positions.Dispose();
        }
    }

    public struct Neighbour
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

public enum PathFindingFailedReason
{
    None,
    StartEqualEnd,
    EndNotAccessible,
}