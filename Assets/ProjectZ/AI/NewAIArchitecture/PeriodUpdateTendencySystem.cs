using System;
using ProjectZ.Component.Setting;
using ProjectZ.LoadData;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static ProjectZ.Component.AIDataSingleton;

namespace ProjectZ.AI
{
    [UpdateInGroup(typeof(AIDecisionGroup))]
    public class PeriodUpdateTendencySystem : JobComponentSystem
    {
        private NativeMultiHashMap<int, int>   m_behaviourFactorsModeMap;
        private NativeMultiHashMap<int, int>   m_behaviourFactorsTypeMap;
        private NativeMultiHashMap<int, float> m_behaviourFactorsWeightMap;
        private float                          m_checkNeedPeriods;
        private NativeArray<int>               m_factorsMax;
        private NativeArray<int>               m_factorsMin;

        protected override JobHandle OnUpdate(JobHandle inputDependency)
        {
            var deltaTime = Time.deltaTime;
            var timer     = GetSingleton<NeedLvTimerSingleton>().Value;
            timer += deltaTime;

            if (timer > m_checkNeedPeriods)
            {
                timer           = 0;
                inputDependency = ScheduleCalculateTendencyJob(inputDependency);
            }

            SetSingleton(new NeedLvTimerSingleton {Value = timer});

            inputDependency.Complete();
            return inputDependency;
        }

        private JobHandle ScheduleCalculateTendencyJob(JobHandle inputDependency)
        {
            var calculateTendencyJob = new CalculateTendency
            {
                BehaviourFactorsModeMap   = m_behaviourFactorsModeMap,
                BehaviourFactorsTypeMap   = m_behaviourFactorsTypeMap,
                BehaviourFactorsWeightMap = m_behaviourFactorsWeightMap,
                FactorsMax                = m_factorsMax,
                FactorsMin                = m_factorsMin
            };

            return calculateTendencyJob.Schedule(this, inputDependency);
        }

        private struct CalculateTendency : IJobForEach_BBB<Factor, Resistance, Tendency>
        {
            [ReadOnly] public NativeMultiHashMap<int, int>   BehaviourFactorsModeMap;
            [ReadOnly] public NativeMultiHashMap<int, int>   BehaviourFactorsTypeMap;
            [ReadOnly] public NativeMultiHashMap<int, float> BehaviourFactorsWeightMap;
            [ReadOnly] public NativeArray<int>               FactorsMax;
            [ReadOnly] public NativeArray<int>               FactorsMin;

            public void Execute(
                [ReadOnly] DynamicBuffer<Factor> b0,
                DynamicBuffer<Resistance>        b1,
                DynamicBuffer<Tendency>          b2)
            {
                var behaviourCount = 0;

                while (BehaviourFactorsTypeMap.TryGetFirstValue(behaviourCount, out var factorIndex, out var itt))
                {
                    var factorCount = 1;
                    BehaviourFactorsModeMap.TryGetFirstValue(behaviourCount, out var mode, out var itm);
                    BehaviourFactorsWeightMap.TryGetFirstValue(behaviourCount, out var weight, out var itw);
                    var result = ProcessFactor(factorIndex, mode, weight, b0[factorIndex].Value);

                    while (BehaviourFactorsTypeMap.TryGetNextValue(out factorIndex, ref itt))
                    {
                        factorCount++;
                        BehaviourFactorsModeMap.TryGetNextValue(out mode, ref itm);
                        BehaviourFactorsWeightMap.TryGetNextValue(out weight, ref itw);
                        result += ProcessFactor(factorIndex, mode, weight, b0[factorIndex].Value);
                    }

                    // avoid influence by number of factors.
                    result /= factorCount;
                    // Take resistance factor into consideration. 
                    result             *= 1 - b1[behaviourCount].Value;
                    b2[behaviourCount] =  new Tendency {Value = result};
                    behaviourCount++;
                }
            }

            private float ProcessFactor(
                int   factorIndex,
                int   mode,
                float weight,
                int   value)
            {
                var max = FactorsMax[factorIndex];
                var min = FactorsMin[factorIndex];
                value = math.clamp(value, min, max);
                var result = weight * (value - min) / (max - min);

                switch ((FactorMode) mode)
                {
                    case FactorMode.Direct: break;
                    case FactorMode.Inverse:
                        result = 1 - result;
                        break;

                    case FactorMode.Custom: break;
                    default:                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }

                return result;
            }
        }

        protected override void OnCreate()
        {
            World.GetOrCreateSystem<ConvertDataToSingleton>().Update();
            Initialize();
        }

        private void Initialize()
        {
            // @Todo A Data Manager is required.
            m_checkNeedPeriods = NeedLevels.CheckPeriods[0];

            EntityManager.CreateEntity(typeof(NeedLvTimerSingleton));
            SetSingleton<NeedLvTimerSingleton>(default);

            // FactorsInfo
            var factorsLength = Factors.Count;
            m_factorsMax = new NativeArray<int>(factorsLength, Allocator.Persistent);
            m_factorsMin = new NativeArray<int>(factorsLength, Allocator.Persistent);
            m_factorsMax.CopyFrom(Factors.FactorsMax);
            m_factorsMin.CopyFrom(Factors.FactorsMin);

            // BehavioursInfo
            var behavioursLength = Behaviours.Count;
            m_behaviourFactorsTypeMap   = new NativeMultiHashMap<int, int>(behavioursLength, Allocator.Persistent);
            m_behaviourFactorsWeightMap = new NativeMultiHashMap<int, float>(behavioursLength, Allocator.Persistent);
            m_behaviourFactorsModeMap   = new NativeMultiHashMap<int, int>(behavioursLength, Allocator.Persistent);

            for (var i = 0; i < behavioursLength; i++)
            {
                var factorCount = Behaviours.FactorsInfo.Types[i].Length;

                for (var j = 0; j < factorCount; j++)
                {
                    m_behaviourFactorsTypeMap.Add(i, (int) Behaviours.FactorsInfo.Types[i][j]);
                    m_behaviourFactorsWeightMap.Add(i, Behaviours.FactorsInfo.Weights[i][j]);
                    m_behaviourFactorsModeMap.Add(i, (int) Behaviours.FactorsInfo.Modes[i][j]);
                }
            }
        }

        protected override void OnDestroy()
        {
            m_behaviourFactorsTypeMap.Dispose();
            m_behaviourFactorsWeightMap.Dispose();
            m_behaviourFactorsModeMap.Dispose();
            m_factorsMax.Dispose();
            m_factorsMin.Dispose();
        }
    }
}