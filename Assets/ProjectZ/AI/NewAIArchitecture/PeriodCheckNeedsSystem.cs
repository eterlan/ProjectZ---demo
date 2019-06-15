using ProjectZ.Component;
using ProjectZ.LoadData;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace ProjectZ.AI
{
    [UpdateInGroup(typeof(AIDecisionGroup))]
    public class PeriodCheckNeedsSystem : JobComponentSystem
    {
        private NativeArray<float>           m_checkNeedPeriods;
        private int                          m_lvCount;
        private NativeMultiHashMap<int, int> m_needLvBehavioursIndex;
        private float                        m_needsCriticalVar = 0.75f;

        protected override JobHandle OnUpdate(JobHandle inputDependency)
        {
            var fixedDeltaTime = Time.fixedDeltaTime;
            var periodCheckJob = new PeriodCheck
            {
                FixedDeltaTime       = fixedDeltaTime,
                CheckNeedPeriods     = m_checkNeedPeriods,
                NeedLvBehavioursInfo = m_needLvBehavioursIndex,
                LvCount              = m_lvCount,
                NeedsCriticalVar     = m_needsCriticalVar
            };
            var periodCheckJobHandle = periodCheckJob.Schedule(this, inputDependency);

            inputDependency = periodCheckJobHandle;
            return inputDependency;
        }


        protected override void OnCreate()
        {
            //@TODO Some Data singleton manager might be better? It's not bad to check again though.
            World.GetOrCreateSystem<ConvertDataToSingleton>().Update();
            Initialize();
        }

        private void Initialize()
        {
            m_lvCount          = AIDataSingleton.NeedLevels.Count;
            m_needsCriticalVar = AIDataSingleton.NeedLevels.NeedsCriticalVar;
            m_checkNeedPeriods
                = new NativeArray<float>(AIDataSingleton.NeedLevels.CheckPeriods, Allocator.Persistent);
            m_needLvBehavioursIndex
                = new NativeMultiHashMap<int, int>(AIDataSingleton.NeedLevels.Count, Allocator.Persistent);
            for (var i = 0; i < AIDataSingleton.NeedLevels.Count; i++)
            {
                var behavesCount = AIDataSingleton.NeedLevels.BehavioursType[i].Length;
                for (var j = 0; j < behavesCount; j++)
                {
                    var index = (int) AIDataSingleton.NeedLevels.BehavioursType[i][j];
                    m_needLvBehavioursIndex.Add(i, index);
                }
            }

            //@TODO, For the sake of Let While Loop Work.Should be removed if Lv1 behaviour is refined.
            // 意思是属于需求等级0的行为id是1
            m_needLvBehavioursIndex.Add(0, 1);
        }

        protected override void OnDestroy()
        {
            m_checkNeedPeriods.Dispose();
            m_needLvBehavioursIndex.Dispose();
            Debug.Log("Destroy");
        }

        [BurstCompile]
        public struct PeriodCheck : IJobForEachWithEntity_EBC<Tendency, CurrentBehaviourInfo>
        {
            [ReadOnly]
            public float FixedDeltaTime;

            [ReadOnly]
            public NativeArray<float> CheckNeedPeriods;

            [ReadOnly]
            public NativeMultiHashMap<int, int> NeedLvBehavioursInfo;

            [ReadOnly]
            public int LvCount;

            [ReadOnly]
            public float NeedsCriticalVar;

            // 我要知道是谁时间到了，需要更新行为
            public void Execute(Entity entity, int index, DynamicBuffer<Tendency> b0, ref CurrentBehaviourInfo c1)
            {
                // 每0.5s检查最初级需求，满足则向上
                var checkPeriod = CheckNeedPeriods[0];
                c1.PeriodCheckTimer += FixedDeltaTime;
                if (c1.PeriodCheckTimer > checkPeriod) NeedsCheck(b0, ref c1);
            }

            private void NeedsCheck(DynamicBuffer<Tendency> b0, ref CurrentBehaviourInfo c1)
            {
                c1.PeriodCheckTimer = 0;
                var lv = 0;
                while (Satisfied(b0, lv) && lv < LvCount) lv++;

                c1.CurrentNeedLv = lv;
            }

            private bool Satisfied(DynamicBuffer<Tendency> b0, int needsLv)
            {
                var tendencies = b0.AsNativeArray();
                if (!NeedLvBehavioursInfo.TryGetFirstValue(needsLv, out var behaveIndex, out var it))
                    return false;
                if (tendencies[behaveIndex].Value > NeedsCriticalVar)
                    return false;

                while (NeedLvBehavioursInfo.TryGetNextValue(out behaveIndex, ref it))
                    if (tendencies[behaveIndex].Value > NeedsCriticalVar)
                        return false;

                return true;
            }
        }
    }
}