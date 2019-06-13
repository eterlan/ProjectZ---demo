using System;
using ProjectZ.Component;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using ProjectZ.Component.Setting;

namespace ProjectZ.AI
{
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
            var behavCount                 = Enum.GetNames(typeof(BehaviourType)).Length;
            NpcCount = entities.Length;

            DynamicBuffer<BehaviourTendency> behaviourTendencies;
            for (var index = 0; index < NpcCount; index++)
            {
                behaviourTendencies = getBehaviourTendencyBuffer[entities[index]];
                for (var j = 0; j < behavCount; j++) 
                    behaviourTendencies.Add(0);
            }

            IsIntialized = true;
            entities.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDependency)
        {
            if (!IsIntialized) Initialize();

            var processTendencyJob = new ProcessTendency
            {
                GetBehaviourTendencyBuffer = GetBufferFromEntity<BehaviourTendency>()
            };

            var processTendencyJobHandle = processTendencyJob.Schedule(NpcGroup, inputDependency);

            var compareTendencyJob = new CompareTendency
            {
                GetBehaviourTendencyBuffer = GetBufferFromEntity<BehaviourTendency>(true)
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
                    ComponentType.ReadOnly<HumanState>(),
                    ComponentType.ReadOnly<HumanStock>(),
                    ComponentType.ReadOnly<CurrentBehaviour>(),
                    ComponentType.ReadWrite<Navigation>(),
                    typeof(BehaviourTendency)
                },
                None = new[]
                {
                    ComponentType.ReadOnly<Player>()
                }
            });
        }

        [BurstCompile]
        public struct ProcessTendency : IJobForEachWithEntity<HumanState, HumanStock>
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

            public void Execute(Entity entity, int index, ref HumanState state,
                ref HumanStock stock)
            {
                //stateFactor.d[0] += 1;
                var eatTendency = CalculateTendencyHasStock(state.Hungry, stock.Food, 100, 10, 1f, 0.2f, false);
                var drinkTendency =
                    CalculateTendencyHasStock(state.Thirsty, stock.Water, 100, 10, 1, 0.2f, false);

                var behaviourTendencies = GetBehaviourTendencyBuffer[entity];

                behaviourTendencies[(int) BehaviourType.Eat]   = eatTendency;
                behaviourTendencies[(int) BehaviourType.Drink] = drinkTendency;
            }
        }

        [BurstCompile]
        public struct CompareTendency : IJobForEachWithEntity<CurrentBehaviour, Navigation>
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

            public void Execute(Entity entity, int index, ref CurrentBehaviour currentBehaviour, ref Navigation navigation)
            {
                var prevBehaviour = currentBehaviour.BehaviourType;

                var behaviourTendencys = GetBehaviourTendencyBuffer[entity].AsNativeArray();

                CompareLargerestTendency(behaviourTendencys, out var largerestTendencyType, out var largerestTendency);
                currentBehaviour = largerestTendencyType;

                // prevArrived or not, change to false.
                if (prevBehaviour != currentBehaviour.BehaviourType)
                {
                    prevBehaviour      = currentBehaviour.BehaviourType;
                    navigation.Arrived = false;
                }
            }
        }
    }
}