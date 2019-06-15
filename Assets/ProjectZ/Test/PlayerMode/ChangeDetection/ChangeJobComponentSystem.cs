using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ProjectZ.Test.PlayerMode.ChangeDetection
{
    public class ChangeJobComponentSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDependency)
        {
            return inputDependency;
        }

        public JobHandle RW()
        {
            var writeAccessJob = new WAccess();
            return writeAccessJob.Schedule(this);
        }

        public JobHandle RO()
        {
            var writeAccessJob = new WAccess();
            return writeAccessJob.Schedule(this);
        }

        protected override void OnCreate()
        {
        }

        protected override void OnDestroy()
        {
        }

        public JobHandle W()
        {
            var wAccessJob = new WAccess();
            return wAccessJob.Schedule(this);
        }

        private struct RWAccess : IJobForEach<ForChangeTestComponent>
        {
            public void Execute(ref ForChangeTestComponent c0)
            {
            }
        }

        private struct ROAccess : IJobForEach<ForChangeTestComponent>
        {
            public void Execute([ReadOnly] ref ForChangeTestComponent c0)
            {
            }
        }

        private struct WAccess : IJobForEach<ForChangeTestComponent>
        {
            public void Execute(ref ForChangeTestComponent c0)
            {
                c0.Value += 1;
            }
        }
    }
}