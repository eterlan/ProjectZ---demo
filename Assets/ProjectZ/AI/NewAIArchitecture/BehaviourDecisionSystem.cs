using ProjectZ.Component.Setting;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace ProjectZ.AI
{
    [UpdateInGroup(typeof(AIDecisionGroup))]
    public class BehaviourDecisionSystem : JobComponentSystem
    {
        private EntityQuery m_group;

        protected override JobHandle OnUpdate(JobHandle inputDependency)
        {
            // @Todo 尝试用FilterChanged来追踪是否产生变化。#Q：Query是如何影响到Job的？是通过Dependency注册的。
            //if (TendencyHasChanged)
            var maximumIndexes = World.Active.GetOrCreateSystem<FindMaximumSystem>()
                .FindMaximum<Tendency>(out var jobHandle);
            var modifyCurrBehaveJob = new ModifyCurrentBehaviourJob
            {
                MaximumIndexes = maximumIndexes,
                // test
                gsv = GlobalSystemVersion,
                lsv = LastSystemVersion
            };
            var modifyCurrBehaveJobHandle = modifyCurrBehaveJob.Schedule(m_group, jobHandle);
            inputDependency = modifyCurrBehaveJobHandle;
            // @Bug 没有把最后一环交给系统注册，系统弄错最后一个Job是谁，导致还未跑完所有Job，EM就被释放。
            m_group.AddDependency(inputDependency);

            return inputDependency;
        }

//        // 分成两个Job的原因是因为这样可以让上个Job用Burst运行，同时把修改当前行为和事件功能分离开
//        // @Todo 考虑如何实现通知事件。用Queue吗？
//        private void CheckIfBehaviourChange(NativeList<int> changedIndexes)
//        {
//            var i = 0;
//            Entities.ForEach((ref CurrentBehaviourInfo c0) =>
//            {
//                if (c0.CurrBehaviourType != c0.PrevBehaviourType)
//                {
//                    changedIndexes.Add(i);
//                    c0.PrevBehaviourType = c0.CurrBehaviourType;
//                }
//            });
//        }
        protected override void OnCreate()
        {
            m_group = GetEntityQuery(
                ComponentType.ReadOnly<Tendency>(),
                ComponentType.ReadWrite<BehaviourInfo>());
            m_group.SetFilterChanged(typeof(Tendency));
        }

        protected override void OnDestroy()
        {
        }

        private struct ModifyCurrentBehaviourJob : IJobForEachWithEntity<BehaviourInfo>
        {
            [DeallocateOnJobCompletion]
            public NativeArray<int> MaximumIndexes;

            // test
            public uint gsv;
            public uint lsv;

            // @Bug 错误的给不是触发条件的Component使用【ChangedFilter】，因此无效。
            // @Bug 没有标记作为监测变化Filter的Component。Group只对手动遍历如IJobChunk等生效。
            public void Execute(Entity entity, int index, ref BehaviourInfo c0)
            {
                Debug.Log("GSV" + gsv + "LSV" + lsv);
                c0.PrevBehaviourType = c0.CurrBehaviourType;
                c0.CurrBehaviourType = (BehaviourType) MaximumIndexes[index];
            }
        }
    }
}