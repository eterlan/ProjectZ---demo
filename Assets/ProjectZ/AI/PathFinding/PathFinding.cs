using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectZ.AI.PathFinding
{
    [UpdateInGroup(typeof(PathFindingGroup))]
    [UpdateAfter(typeof(SpawnNodeSystem))]
    public class PathFinding : ComponentSystem
    {
        // @Todo: Move const to other place?

        private struct Grid
        {
            public int2 GridSize;

            // Node Index
            public NativeArray<NodeInfo> NodeInfos;
            public NativeArray<int2> Positions;
            
            public NativeArray<Neighbour> m_neighbours;
            public const int UnitCost = 10;
            public const int Unit = 1;
            public const float InclineUnit = Grid.Unit * math.SQRT2;
            public const int NeighbourCount = 8;
            public int NodeCount;
        }

        // Neighbour Index， 上下左右 正+斜 = 8
        private Grid m_grid;
        private struct PathFindingInfo
        {
            
        }
        public NativeArray<float> CostSoFar;
        public NativeArray<int> ParentIndex;
        private NativeMinHeap    m_openSet;
        private NativeList<int>  m_closeSet;
        private NativeList<int2> m_path;

        protected override void OnUpdate()
        {
            Entities.ForEach((DynamicBuffer<PathPlanner> pathPlanner, ref PathFindingRequest request) =>
            {
                // @Todo: 不应该是有一个不需要就返回，而是大家都不需要才返回。
                // @Todo: 不应该是通用一个数组，而是每人一个数组。
                // @Todo: 提供清除旧路的方法。
                // @Todo: 是否应该一个区域的FINDER公用一个数组？
                if (request.StartIndex == request.EndIndex)
                    return;// 如何只跳过当前不需要寻路的人？
                Reset();
                var path       = FindPath(request.StartIndex, request.EndIndex);
                var pathLength = path.Length;
                if (pathLength == 0) return;
                // @Todo: Terminate Path Finding by set request OR remove request?
                request.StartIndex = request.EndIndex;

                for (var i = pathLength - 1; i >= 0; i--) { pathPlanner.Add(new PathPlanner {NextPosition = path[i]}); }
                CleanUp();
            });
        }

        public void ClearPath(Entity entity)
        {
            EntityManager.GetBuffer<PathPlanner>(entity).Clear();
        }
        private NativeList<int2> FindPath(int startIndex, int endIndex)
        {
            // OpenSet wait to be explore. It should only explore once, and each time should explore the most possible node. Which means the node with lowest F. F = G + H. G is the total cost coming to this point. H is roughly guessing remaining cost to the goal.
            // @Bug: Without this, never work.
            CostSoFar[startIndex] = 0;
            m_openSet.Push(new MinHeapNode(startIndex, 0));
            var endPos = m_grid.Positions[endIndex];

            while (m_openSet.HasNext())
            {
                var heapIndex    = m_openSet.Pop();
                var current      = m_openSet[heapIndex];
                var currentIndex = current.Index;

                if (currentIndex == endIndex)
                {
                    return ConstructPath();
                }

                for (var index = 0; index < m_grid.m_neighbours.Length; index++)
                {
                    var neighbour      = m_grid.m_neighbours[index];
                    var neighbourIsValid = GetIndexFromValidOffset(currentIndex, neighbour.Offset, out var 
                    neighbourIndex);

                    if (!neighbourIsValid)
                        continue;
                    
                    if (m_closeSet.Contains(neighbourIndex)) continue;
                    // @Bug: index = -1 is out of range. <- Invalid index is return. <- GetIFVO method
                    var walkable = m_grid.NodeInfos[neighbourIndex].Walkable;

                    if (!walkable) continue;

                    var newCost = CostSoFar[currentIndex] + neighbour.Cost * Grid.UnitCost;
                    var oldCost = CostSoFar[neighbourIndex];

                    if (newCost >= oldCost) continue;
                    var neighbourPos  = m_grid.Positions[neighbourIndex];
                    var estimatedCost = newCost + H(neighbourPos, endPos);

                    CostSoFar[neighbourIndex]   = newCost;
                    ParentIndex[neighbourIndex] = currentIndex;
                    m_openSet.Push(new MinHeapNode(neighbourIndex, estimatedCost));

                    // var neighbourPosition = current + neighbour.Offset;
                    // if in the explored Set, continue
                    // if in the toExplored Set, 
                    // if has a higher cost than record, continue
                    // else, push to the toExplored Set
                    // Update 

                    // @Todo 从位置获得Index？WTF。为什么要获得Index？ 因为要知道是否为障碍物。如果障碍物处没有Node，那就不用Index。不对，NodeIndex还能统领全局，每个结点至今最快记录，和父节点都靠Index获得。
                    // @Todo 那有什么办法能不用Position获得Index？ 那得看Index怎么编码的。如果先是OOP组织起来，再把不同数据放进不同数组的话，就不用担心了。
                    // #Todo 
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

            bool GetIndexFromValidOffset(int index, int2 offset, out int neighbourIndex)
            {
                neighbourIndex = 0;
                var sizeX = m_grid.GridSize.x;
                var sizeY = m_grid.GridSize.y;
                var x = index % sizeX;
                var y = index / sizeX;
                if (x+offset.x < 0 || x+offset.x>sizeX-1 || y+offset.y < 0 || y + offset.y > sizeY-1) { return false; }
                neighbourIndex = index + offset.x + offset.y * m_grid.GridSize.x;
                return true;
            }

            NativeList<int2> ConstructPath()
            {
                var parentIndex = endIndex;

                while (parentIndex != startIndex)
                {
                    var position = m_grid.Positions[parentIndex];
                    m_path.Add(position);
                    // @Todo: Add to buffer.
                    parentIndex = ParentIndex[parentIndex];
                }

                return m_path;
            }

            return m_path;
        }

        // @Todo: 改成Job后，收到Request（Entity）后入列，一起寻路也许好点。
        private void CleanUp()
        {
            ParentIndex.Dispose();
            CostSoFar.Dispose();
            m_closeSet.Dispose();
            m_openSet.Dispose();
            m_path.Dispose();
        }

        private void Reset()
        {
            ParentIndex = new NativeArray<int>(m_grid.NodeCount, Allocator.TempJob);
            CostSoFar   = new NativeArray<float>(m_grid.NodeCount, Allocator.TempJob);
            m_closeSet         = new NativeList<int>(10,Allocator.TempJob);
            m_openSet          = new NativeMinHeap(m_grid.NodeCount, Allocator.TempJob);
            m_path             = new NativeList<int2>(10, Allocator.TempJob);

            for (var i = 0; i < m_grid.NodeCount; i++) { CostSoFar[i] = float.PositiveInfinity; }

            var index = 0;
            Entities.ForEach((ref NodeInfo nodeInfo, ref Translation translation) =>
            {
                index = nodeInfo.Index;
                m_grid.Positions[index] = new int2((int) translation.Value.x, (int) translation.Value.z);
                CostSoFar[index] = float.PositiveInfinity;
                m_grid.NodeInfos[index] = nodeInfo;
                index++;
            });

            // var query = GetEntityQuery(typeof(NodeInfo));
            // var array = query.ToComponentDataArray<NodeInfo>(Allocator.TempJob);
            // Debug.Log("");
            // array.Dispose();
        }

        protected override void OnCreate()
        {
            Initialize();

            void Initialize()
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

                InstantiateNeighbour();
            }

            void InstantiateNeighbour()
            {
                m_grid.m_neighbours = new NativeArray<Neighbour>(Grid.NeighbourCount, Allocator.Persistent)
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
            m_grid.m_neighbours.Dispose();
            m_grid.Positions.Dispose();
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