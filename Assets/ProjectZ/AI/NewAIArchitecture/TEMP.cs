//using System;
//using ProjectZ.Component.Setting;
//using ProjectZ.LoadData;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using UnityEngine;
//using static ProjectZ.Component.AIDataSingleton;
//
//namespace ProjectZ.AI
//{
//    [UpdateInGroup(typeof(AIDecisionGroup))]
//    public class PeriodUpdateTendencySystem : JobComponentSystem
//    {
//        private NativeMultiHashMap<int, int>   m_behaviourFactorsModeMap;
//        private NativeMultiHashMap<int, int>   m_behaviourFactorsTypeMap;
//        private NativeMultiHashMap<int, float> m_behaviourFactorsWeightMap;
//        private float                          m_checkNeedPeriods;
//        private NativeArray<int>               m_factorsMax;
//        private NativeArray<int>               m_factorsMin;
//        private NativeArray<float>             m_tendencies;
//
//        protected override JobHandle OnUpdate(JobHandle inputDependency)
//        {
//            // get factor
//            // get info from Singleton
//            // if timer xx, access to tendency
//            var deltaTime = Time.deltaTime;
//            var needLvTimerSingleton = GetSingleton<NeedLvTimerSingleton>();
//            ref var timer = ref needLvTimerSingleton;
//            timer.Value += deltaTime;
//            if (timer.Value > m_checkNeedPeriods)
//            {
//                timer.Value = 0;
//                CalculateTendency();
//            }
////            Entities.ForEach((ref NeedLvTimerSingleton timer) =>
////            {
////                timer.Value += deltaTime;
////                if (timer.Value > m_checkNeedPeriods)
////                {
////                    timer.Value = 0;
////                    CalculateTendency();
////                }
////            });
//            m_factorsMax.Dispose();
//            m_factorsMin.Dispose();
//            m_tendencies.Dispose();
//            // 不进去就不Schedule Job，在外面确认DidChange。这样的话可以使用IJobForEach
//            // Update Tendency
//            // 改成JobComponentSystem
//            return inputDependency;
//        }
//
//        private void CalculateTendency()
//        {
////            Entities.ForEach((DynamicBuffer<Factor> b0, DynamicBuffer<Tendency> b1) =>
////            {
////                Debug.Log("execute");
////                // @Bug 为什么buffer version没有增加？
////                var behaviourCount = 0;
////                while (m_behaviourFactorsTypeMap.TryGetFirstValue(behaviourCount, out var factorIndex, out var itt))
////                {
////                    var factorCount = 1;
////                    m_behaviourFactorsModeMap.TryGetFirstValue(behaviourCount, out var mode, out var itm);
////                    m_behaviourFactorsWeightMap.TryGetFirstValue(behaviourCount, out var weight, out var itw);
////                    var result = ProcessFactor(factorIndex, mode, weight, b0[factorIndex].Value);
////                    while (m_behaviourFactorsTypeMap.TryGetNextValue(out factorIndex, ref itt))
////                    {
////                        factorCount++;
////                        m_behaviourFactorsModeMap.TryGetNextValue(out mode, ref itm);
////                        m_behaviourFactorsWeightMap.TryGetNextValue(out weight, ref itw);
////                        result += ProcessFactor(factorIndex, mode, weight, b0[factorIndex].Value);
////                    }
////
////                    result             /= factorCount;
////                    b1[behaviourCount] =  new Tendency {Value = result};
////                    //m_tendencies[behaviourCount] = result;
////                    behaviourCount++;
////                }
////            });
//
//            float ProcessFactor(int factorIndex, int mode, float weight, int value)
//            {
//                var max    = m_factorsMax[factorIndex];
//                var min    = m_factorsMin[factorIndex];
//                var result = weight * (value - min) / (max - min);
//                switch ((FactorMode) mode)
//                {
//                    case FactorMode.Direct: break;
//                    case FactorMode.Inverse:
//                        result = 1 - result;
//                        break;
//                    case FactorMode.Custom:
//                        break;
//                    default:
//                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
//                }
//
//                return result;
//            }
//        }
//
//        protected override void OnCreate()
//        {
//            Initialize();
//        }
//
//        private void Initialize()
//        {
//            // @Todo A Data Manager is required.
//            World.GetOrCreateSystem<ConvertDataToSingleton>().Update();
//            m_checkNeedPeriods = NeedLevels.CheckPeriods[0];
//            EntityManager.CreateEntity(typeof(NeedLvTimerSingleton));
//            SetSingleton<NeedLvTimerSingleton>(default);
//
//            // FactorsInfo
//            var factorsLength = Factors.Count;
//            m_factorsMax = new NativeArray<int>(factorsLength, Allocator.Persistent);
//            m_factorsMin = new NativeArray<int>(factorsLength, Allocator.Persistent);
//            m_factorsMax.CopyFrom(Factors.FactorsMax);
//            m_factorsMin.CopyFrom(Factors.FactorsMin);
//
//            // BehavioursInfo
//            var behavioursLength = Behaviours.Count;
//            m_tendencies                = new NativeArray<float>(behavioursLength, Allocator.TempJob);
//            m_behaviourFactorsTypeMap   = new NativeMultiHashMap<int, int>(behavioursLength, Allocator.Persistent);
//            m_behaviourFactorsWeightMap = new NativeMultiHashMap<int, float>(behavioursLength, Allocator.Persistent);
//            m_behaviourFactorsModeMap   = new NativeMultiHashMap<int, int>(behavioursLength, Allocator.Persistent);
//            for (var i = 0; i < behavioursLength; i++)
//            {
//                var factorCount = Behaviours.FactorsInfo.Types[i].Length;
//                for (var j = 0; j < factorCount; j++)
//                {
//                    m_behaviourFactorsTypeMap.Add(i, (int) Behaviours.FactorsInfo.Types[i][j]);
//                    m_behaviourFactorsWeightMap.Add(i, Behaviours.FactorsInfo.Weights[i][j]);
//                    m_behaviourFactorsModeMap.Add(i, (int) Behaviours.FactorsInfo.Modes[i][j]);
//                }
//            }
//        }
//
//        protected override void OnDestroy()
//        {
//            m_behaviourFactorsTypeMap.Dispose();
//            m_behaviourFactorsWeightMap.Dispose();
//            m_behaviourFactorsModeMap.Dispose();
//        }
//    }
//}

