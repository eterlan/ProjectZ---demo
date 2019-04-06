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
    public ComponentGroup m_NPCGroup;
    public BufferFromEntity<BehaviourTendencyBufferElement> GetBehaviourTendencyBuffer;

    EndSimulationEntityCommandBufferSystem commandBufferSystem;
    public bool IsIntialized;
    public int npcCount;

    [BurstCompile]
    public struct ProcessTendency : IJobProcessComponentDataWithEntity<HumanStateFactor, HumanStockFactor>
    {
        [NativeDisableParallelForRestriction]
        public BufferFromEntity<BehaviourTendencyBufferElement> GetBehaviourTendencyBuffer;

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
            
            var eatTendency = CalculateTendencyHasStock(stateFactor.Hungry,stockFactor.Food,100,10,1f,0.2f,false);
            var drinkTendency = CalculateTendencyHasStock(stateFactor.Thirsty,stockFactor.Water,100,10,1,0.2f,false);
            
            var behaviourTendencys = GetBehaviourTendencyBuffer[entity];

            behaviourTendencys[(int)BehaviourTypes.Eat]   = eatTendency;
            behaviourTendencys[(int)BehaviourTypes.Drink] = drinkTendency;

        }
    }

    [BurstCompile]
    public struct CompareTendency : IJobProcessComponentDataWithEntity<BehaviourType, NavigationTag>
    {
        [ReadOnly]
        public BufferFromEntity<BehaviourTendencyBufferElement> GetBehaviourTendencyBuffer;
        [ReadOnly]
        private DynamicBuffer<BehaviourTendencyBufferElement>   behaviourTendencys;
        // public EntityCommandBuffer.Concurrent commandBuffer;
        // [DeallocateOnJobCompletion]
        // public NativeArray<bool> hasNavigationTags;

        private void CompareLargerestTendency(DynamicBuffer<BehaviourTendencyBufferElement> behaviourTendencys, out int largerestTendencyType, out float largerestTendency)
        {
            var length             = behaviourTendencys.Length;

            largerestTendencyType = 0;
            largerestTendency      = behaviourTendencys[0];

            for (int i = 0; i < length; i++)
            {
                var tendency = behaviourTendencys[i];
                var Larger   = tendency > largerestTendency;
                largerestTendencyType = math.select(largerestTendencyType, i, Larger);
            }
            largerestTendency = behaviourTendencys[largerestTendencyType];
        }

        public void Execute(Entity entity, int index, ref BehaviourType BehaviourType, ref NavigationTag navigationTag)
        {
            var prevBehaviour = BehaviourType.Behaviour;

            behaviourTendencys = GetBehaviourTendencyBuffer[entity];
            CompareLargerestTendency (behaviourTendencys, out int largerestTendencyType, out float LargerestTendency); 
            BehaviourType = largerestTendencyType;

            //try -> compare components directly rather than values inside of it.
            // if (prevBehaviour != BehaviourType.Behaviour && !hasNavigationTags[index])
            // {
            //     commandBuffer.AddComponent(index,entity,new NavigationTag());
            //     prevBehaviour = BehaviourType.Behaviour;
            //     //hasNavigationTags[index] = true;
            // }

            // prevArrived or not, change to false.
            if (prevBehaviour != BehaviourType.Behaviour)
            {
                prevBehaviour = BehaviourType.Behaviour;
                navigationTag.Arrived = false;
            }
        }
    }

    protected override void OnStopRunning()
    {
        //hasNavigationTags.Dispose();
    }

    void Initialize()
    {
        var GetBehaviourTendencyBuffer = GetBufferFromEntity<BehaviourTendencyBufferElement>(false);
        var entities                   = m_NPCGroup.ToEntityArray(Allocator.TempJob);
        var behavCount                 = GetNames(typeof(BehaviourTypes)).Length;
        npcCount                       = entities.Length;

        DynamicBuffer<BehaviourTendencyBufferElement> BehaviourTendencyElements;
        for (int index = 0; index < npcCount; index++)
        {
            BehaviourTendencyElements = GetBehaviourTendencyBuffer[entities[index]];
            for (int j = 0; j < behavCount; j++)
            {
                BehaviourTendencyElements.Add(0);
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
            GetBehaviourTendencyBuffer = GetBufferFromEntity<BehaviourTendencyBufferElement>(false),
        };

        var ProcessTendencyJobHandle = ProcessTendencyJob.ScheduleGroup(m_NPCGroup, inputDependency);

        var CompareTendencyJob = new CompareTendency
        {      
            GetBehaviourTendencyBuffer = GetBufferFromEntity<BehaviourTendencyBufferElement>(true),
            //commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),    
        };

        var CompareTendencyJobHandle = CompareTendencyJob.ScheduleGroup(m_NPCGroup, ProcessTendencyJobHandle);

        inputDependency = CompareTendencyJobHandle;
        m_NPCGroup.AddDependency(inputDependency);
        //commandBufferSystem.AddJobHandleForProducer(inputDependency);

        //entities.Dispose();
        
        return inputDependency;
    }
    protected override void OnCreateManager()
    {
        m_NPCGroup = GetComponentGroup(new EntityArchetypeQuery{
            All = new[] {
                ComponentType.ReadOnly<HumanStateFactor>(),
                ComponentType.ReadOnly<HumanStockFactor>(),
                ComponentType.ReadOnly<BehaviourType>(),
                ComponentType.ReadWrite<NavigationTag>(),
                typeof(BehaviourTendencyBufferElement),        
            },
            None = new[] {
                ComponentType.ReadOnly<PlayerTag>(),
            }  
        });
    } 
}