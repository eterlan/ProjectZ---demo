using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using static System.Enum;


[UpdateInGroup(typeof(SimulationSystemGroup))]
public class DecisionSystem : JobComponentSystem
{
    public EntityQuery m_NPCGroup;
    public BufferFromEntity<BehaviourTendency> GetBehaviourTendencyBuffer;

    EndSimulationEntityCommandBufferSystem commandBufferSystem;
    public bool IsIntialized;
    public int npcCount;

    [BurstCompile]
    public struct ProcessTendency : IJobForEachWithEntity<HumanStateFactor, HumanStockFactor>
    {
        [NativeDisableParallelForRestriction]
        public BufferFromEntity<BehaviourTendency> GetBehaviourTendencyBuffer;

        private float CalculateTendencyHasStock(int pFactor, int nFactor,int pMax,int nMax, float pImpactor, float nImpactor, bool IsStockPositiveFactor)
        {
            if (IsStockPositiveFactor)
            {
                pFactor = pMax - pFactor;
            }
            else
            {
                nFactor = nMax - nFactor;
            }
            var pTendency = pImpactor * pFactor / pMax;
            var nTendency = nImpactor * nFactor / nMax;
            return pTendency - nTendency;
        }

        public void Execute(Entity entity, int index, ref HumanStateFactor stateFactor, ref HumanStockFactor stockFactor)
        {
            //stateFactor.d[0] += 1;
            var eatTendency = CalculateTendencyHasStock(stateFactor.Hungry,stockFactor.Food,100,10,1f,0.2f,false);
            var drinkTendency = CalculateTendencyHasStock(stateFactor.Thirsty,stockFactor.Water,100,10,1,0.2f,false);
            
            var behaviourTendencys = GetBehaviourTendencyBuffer[entity];

            behaviourTendencys[(int)BehaviourTypes.Eat]   = eatTendency;
            behaviourTendencys[(int)BehaviourTypes.Drink] = drinkTendency;

        }
    }

    [BurstCompile]
    public struct CompareTendency : IJobForEachWithEntity<BehaviourType, NavigationTag>
    {
        [ReadOnly]
        public BufferFromEntity<BehaviourTendency> GetBehaviourTendencyBuffer;
        private void CompareLargerestTendency(NativeArray<BehaviourTendency> behaviourTendencies , out int largerestTendencyType, out float largerestTendency)
        {
            var length             = behaviourTendencies.Length;

            largerestTendencyType = 0;
            largerestTendency      = behaviourTendencies[0];

            for (int i = 0; i < length; i++)
            {
                var tendency = behaviourTendencies[i];
                var Larger   = tendency > largerestTendency;
                largerestTendencyType = math.select(largerestTendencyType, i, Larger);
            }
            largerestTendency = behaviourTendencies[largerestTendencyType];
        }

        public void Execute(Entity entity, int index, ref BehaviourType BehaviourType, ref NavigationTag navigationTag)
        {
            var prevBehaviour = BehaviourType.Behaviour;

            var behaviourTendencys = GetBehaviourTendencyBuffer[entity].AsNativeArray();

            CompareLargerestTendency (behaviourTendencys, out int largerestTendencyType, out float LargerestTendency); 
            BehaviourType = largerestTendencyType;

            // prevArrived or not, change to false.
            if (prevBehaviour != BehaviourType.Behaviour)
            {
                prevBehaviour = BehaviourType.Behaviour;
                navigationTag.Arrived = false;
            }
        }
    }

    void Initialize()
    {
        var GetBehaviourTendencyBuffer = GetBufferFromEntity<BehaviourTendency>(false);
        var entities                   = m_NPCGroup.ToEntityArray(Allocator.TempJob);
        var behavCount                 = GetNames(typeof(BehaviourTypes)).Length;
        npcCount                       = entities.Length;

        DynamicBuffer<BehaviourTendency> behaviourTendencies;
        for (int index = 0; index < npcCount; index++)
        {
            behaviourTendencies = GetBehaviourTendencyBuffer[entities[index]];
            for (int j = 0; j < behavCount; j++)
            {
                behaviourTendencies.Add(0);
            }
        }
        IsIntialized = true;
        entities.Dispose();
    }
    protected override JobHandle OnUpdate(JobHandle inputDependency)
    {
        if (!IsIntialized)
        {
            Initialize();
        }
        //var entities = m_NPCGroup.ToEntityArray(Allocator.TempJob);
        //Debug.Log(npcCount);
        // hasNavigationTags      = new NativeArray<bool> (npcCount,Allocator.TempJob);
        // for (int index = 0; index < entities.Length; index++)
        // {
        //     hasNavigationTags[index] = EntityManager.HasComponent<NavigationTag>(entities[index]);
        // }
        var ProcessTendencyJob = new ProcessTendency
        {
            GetBehaviourTendencyBuffer = GetBufferFromEntity<BehaviourTendency>(false),
        };

        var ProcessTendencyJobHandle = ProcessTendencyJob.Schedule(m_NPCGroup, inputDependency);

        var CompareTendencyJob = new CompareTendency
        {      
            GetBehaviourTendencyBuffer = GetBufferFromEntity<BehaviourTendency>(true),
            //commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),    
        };

        var CompareTendencyJobHandle = CompareTendencyJob.Schedule(m_NPCGroup, ProcessTendencyJobHandle);

        inputDependency = CompareTendencyJobHandle;
        m_NPCGroup.AddDependency(inputDependency);
        //commandBufferSystem.AddJobHandleForProducer(inputDependency);

        //entities.Dispose();
        
        return inputDependency;
    }
    protected override void OnCreateManager()
    {
        m_NPCGroup = GetEntityQuery(new EntityQueryDesc{
            All = new[] {
                ComponentType.ReadOnly<HumanStateFactor>(),
                ComponentType.ReadOnly<HumanStockFactor>(),
                ComponentType.ReadOnly<BehaviourType>(),
                ComponentType.ReadWrite<NavigationTag>(),
                typeof(BehaviourTendency),        
            },
            None = new[] {
                ComponentType.ReadOnly<Player>(),
            }  
        });
    } 
}