using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using static System.Enum;


[UpdateInGroup(typeof(SimulationSystemGroup))]
public class DecisionSystem : JobComponentSystem
{
    public bool        IsIntialized;
    public EntityQuery NpcGroup;
    public int         NpcCount;

    private void Initialize()
    {
        var getBehaviourTendencyBuffer = GetBufferFromEntity<BehaviourTendency>();
        var entities                   = NpcGroup.ToEntityArray(Allocator.TempJob);
        var behavCount                 = GetNames(typeof(BehaviourTypes)).Length;
        NpcCount = entities.Length;

        DynamicBuffer<BehaviourTendency> behaviourTendencies;
        for (var index = 0; index < NpcCount; index++)
        {
            behaviourTendencies = getBehaviourTendencyBuffer[entities[index]];
            for (var j = 0; j < behavCount; j++) behaviourTendencies.Add(0);
        }

        IsIntialized = true;
        entities.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependency)
    {
        if (!IsIntialized) Initialize();
        //var entities = m_NPCGroup.ToEntityArray(Allocator.TempJob);
        //Debug.Log(npcCount);
        // hasNavigationTags      = new NativeArray<bool> (npcCount,Allocator.TempJob);
        // for (int index = 0; index < entities.Length; index++)
        // {
        //     hasNavigationTags[index] = EntityManager.HasComponent<NavigationTag>(entities[index]);
        // }
        var processTendencyJob = new ProcessTendency
        {
            GetBehaviourTendencyBuffer = GetBufferFromEntity<BehaviourTendency>()
        };

        var processTendencyJobHandle = processTendencyJob.Schedule(NpcGroup, inputDependency);

        var compareTendencyJob = new CompareTendency
        {
            GetBehaviourTendencyBuffer = GetBufferFromEntity<BehaviourTendency>(true)
            //commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),    
        };

        var compareTendencyJobHandle = compareTendencyJob.Schedule(NpcGroup, processTendencyJobHandle);

        inputDependency = compareTendencyJobHandle;
        NpcGroup.AddDependency(inputDependency);
        //commandBufferSystem.AddJobHandleForProducer(inputDependency);

        //entities.Dispose();

        return inputDependency;
    }

    protected override void OnCreateManager()
    {
        NpcGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<HumanStateFactor>(),
                ComponentType.ReadOnly<HumanStockFactor>(),
                ComponentType.ReadOnly<BehaviourType>(),
                ComponentType.ReadWrite<NavigationTag>(),
                typeof(BehaviourTendency)
            },
            None = new[]
            {
                ComponentType.ReadOnly<Player>()
            }
        });
    }

    [BurstCompile]
    public struct ProcessTendency : IJobForEachWithEntity<HumanStateFactor, HumanStockFactor>
    {
        [NativeDisableParallelForRestriction] public BufferFromEntity<BehaviourTendency> GetBehaviourTendencyBuffer;

        private float CalculateTendencyHasStock(int pFactor, int nFactor, int pMax, int nMax, float pImpactor,
            float nImpactor, bool isStockPositiveFactor)
        {
            if (isStockPositiveFactor)
                pFactor = pMax - pFactor;
            else
                nFactor = nMax - nFactor;
            var pTendency = pImpactor * pFactor / pMax;
            var nTendency = nImpactor * nFactor / nMax;
            return pTendency - nTendency;
        }

        public void Execute(Entity entity, int index, ref HumanStateFactor stateFactor,
            ref HumanStockFactor stockFactor)
        {
            //stateFactor.d[0] += 1;
            var eatTendency = CalculateTendencyHasStock(stateFactor.Hungry, stockFactor.Food, 100, 10, 1f, 0.2f, false);
            var drinkTendency =
                CalculateTendencyHasStock(stateFactor.Thirsty, stockFactor.Water, 100, 10, 1, 0.2f, false);

            var behaviourTendencys = GetBehaviourTendencyBuffer[entity];

            behaviourTendencys[(int) BehaviourTypes.Eat]   = eatTendency;
            behaviourTendencys[(int) BehaviourTypes.Drink] = drinkTendency;
        }
    }

    [BurstCompile]
    public struct CompareTendency : IJobForEachWithEntity<BehaviourType, NavigationTag>
    {
        [ReadOnly] public BufferFromEntity<BehaviourTendency> GetBehaviourTendencyBuffer;

        private void CompareLargerestTendency(NativeArray<BehaviourTendency> behaviourTendencies,
            out int largerestTendencyType, out float largerestTendency)
        {
            var length = behaviourTendencies.Length;

            largerestTendencyType = 0;
            largerestTendency     = behaviourTendencies[0];

            for (var i = 0; i < length; i++)
            {
                var tendency = behaviourTendencies[i];
                var larger   = tendency > largerestTendency;
                largerestTendencyType = math.select(largerestTendencyType, i, larger);
            }

            largerestTendency = behaviourTendencies[largerestTendencyType];
        }

        public void Execute(Entity entity, int index, ref BehaviourType behaviourType, ref NavigationTag navigationTag)
        {
            var prevBehaviour = behaviourType.Behaviour;

            var behaviourTendencys = GetBehaviourTendencyBuffer[entity].AsNativeArray();

            CompareLargerestTendency(behaviourTendencys, out var largerestTendencyType, out var largerestTendency);
            behaviourType = largerestTendencyType;

            // prevArrived or not, change to false.
            if (prevBehaviour != behaviourType.Behaviour)
            {
                prevBehaviour         = behaviourType.Behaviour;
                navigationTag.Arrived = false;
            }
        }
    }
}