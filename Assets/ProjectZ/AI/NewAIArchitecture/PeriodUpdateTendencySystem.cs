using ProjectZ.Component.Setting;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using ProjectZ.LoadData;
using static ProjectZ.Component.AIDataSingleton;

namespace ProjectZ.AI
{
    [UpdateInGroup(typeof(AIDecisionGroup))]
    public class PeriodUpdateTendencySystem : ComponentSystem
    {
        private float                          m_checkNeedPeriods;
        private NativeMultiHashMap<int, int>   m_behaviourFactorsTypeMap;
        private NativeMultiHashMap<int, float> m_behaviourFactorsWeightMap;
        private NativeMultiHashMap<int, int> m_behaviourFactorsModeMap;
        private NativeArray<int>               m_factorsMax;
        private NativeArray<int>               m_factorsMin;
        private NativeArray<float> m_tendencies;

        protected override void OnUpdate()
        {
            // get factor
            // get info from Singleton
            // if timer xx, access to tendency
            var deltaTime = Time.deltaTime;
            Entities.ForEach((ref NeedLvTimerSingleton timer) =>
            {
                timer.Value += deltaTime;
                if (timer.Value > m_checkNeedPeriods)
                {
                    timer.Value = 0;
                    CalculateTendency();
                }
            });
            // 进去就是DidChange 或者 ChangeFilter
            // 不进去就不Schedule Job，在外面确认DidChange
            // Update Tendency
        }

        private void CalculateTendency()
        {
            Entities.ForEach((DynamicBuffer<Factor> b0, DynamicBuffer<Tendency> b1) =>
            {
                var behaviourCount = 0;
                while (m_behaviourFactorsTypeMap.TryGetFirstValue(behaviourCount,out int factorIndex, out var itt))
                {
                    var factorCount = 1;
                    m_behaviourFactorsModeMap.TryGetFirstValue(behaviourCount, out var mode, out var itm);
                    m_behaviourFactorsWeightMap.TryGetFirstValue(behaviourCount, out var weight, out var itw);
                    var result = ProcessFactor(factorIndex,mode,weight,b0[factorIndex].Value);
                    while (m_behaviourFactorsTypeMap.TryGetNextValue(out factorIndex, ref itt))
                    {
                        factorCount ++;
                        m_behaviourFactorsModeMap.TryGetNextValue(out mode, ref itm);
                        m_behaviourFactorsWeightMap.TryGetNextValue(out weight,ref itw);
                        result += ProcessFactor(factorIndex,mode,weight,b0[factorIndex].Value);
                    }

                    result /= factorCount;
                    m_tendencies[behaviourCount] = result;
                }
            });

            float ProcessFactor(int factorIndex, int mode, float weight, int value)
            {
                var max = m_factorsMax[factorIndex];
                var min = m_factorsMin[factorIndex];
                var result = weight * (value - min) / (max - min);
                switch ((FactorMode)mode)
                {
                    case FactorMode.Direct : break;
                    case FactorMode.Inverse :
                        result = 1 - result; break;
                }

                return result;
            }

            float CalculateTendencyHasStock(int pFactor, int nFactor, int pMax, int nMax, float pImpactor, float nImpactor, bool isStockPositiveFactor)
            {
                if (isStockPositiveFactor)
                    pFactor = pMax - pFactor;
                else
                    nFactor = nMax - nFactor;
                var pTendency = pImpactor * pFactor / pMax;
                var nTendency = nImpactor * nFactor / nMax;
                return pTendency - nTendency;
            }
        }

        protected override void OnCreate()
        {
            Initialize();
        }

        private void Initialize()
        {
            // @Todo A Data Manager is required.
            World.GetOrCreateSystem<ConvertDataToSingleton>().Update();
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
            m_tendencies = new NativeArray<float>(behavioursLength,Allocator.TempJob);
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
        }
    }
}