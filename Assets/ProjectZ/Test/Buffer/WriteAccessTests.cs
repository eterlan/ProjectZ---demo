using UnityEngine;
using NUnit.Framework;
using ProjectZ.Test.SetUp;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ProjectZ.Test.Buffer
{
    [TestFixture]
    public class WriteAccessTests : ECSTestsFixture
    {
        private DynamicBuffer<RWTestBuffer> m_buffer;

        [SetUp]
        public void SetUp()
        {
            var entity = m_Manager.CreateEntity(typeof(RWTestBuffer));
            m_buffer = m_Manager.GetBuffer<RWTestBuffer>(entity);
            m_buffer.Add(new RWTestBuffer{Value = 1});
            m_buffer.Add(new RWTestBuffer{Value = 2});

        }
        [Test]
        public void _0_Two_RW_Access_To_Buffer_In_Component_System()
        {
            World.GetOrCreateSystem<TwoAccessToBufferComponentSystem>().Update();
            
        }

        [Test]
        public void _1_Single_RW_Access_To_Buffer_In_JCS()
        {
            World.GetOrCreateSystem<SingleAccessToBufferJobComponentSystem>().Update();   
        }

        [Test]
        public void _2_Two_RW_Access_To_Buffer_In_JCS()
        {
            World.GetOrCreateSystem<TwoAccessToBufferJobComponentSystem>().Update();
        }

        [Test]
        public void _3_Two_RW_Access_To_Buffer_With_IJob()
        {
            World.GetOrCreateSystem<TwoRawAccessToBufferWithIJob>().Update();
        }
    }

    public class TwoAccessToBufferComponentSystem : ComponentSystem
    {
        // @Todo: To JobSystem
        protected override void OnUpdate()
        {
            Entities.ForEach((DynamicBuffer<RWTestBuffer> buffer) =>
            {
                
            });
            Entities.ForEach((DynamicBuffer<RWTestBuffer> buffer) =>
            {
                
            });
        }
    }

    public class SingleAccessToBufferJobComponentSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var accessJob = new RW1();
            var handle = accessJob.Schedule(this);
            return handle;
        }
        public struct RW1 : IJobForEach_B<RWTestBuffer>
        {
            public void Execute(DynamicBuffer<RWTestBuffer> b0)
            {
                
            }
        }
    }
    
    public class TwoAccessToBufferJobComponentSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var accessJob = new RW1();
            var handle    = accessJob.Schedule(this,inputDeps);
            var access2Job = new RW2();
            var handle2 = access2Job.Schedule(this,handle);
            return handle2;
        }
        public struct RW1 : IJobForEach_B<RWTestBuffer>
        {
            public void Execute(DynamicBuffer<RWTestBuffer> b0)
            {
                
            }
        }
        public struct RW2 : IJobForEach_B<RWTestBuffer>
        {
            public void Execute(DynamicBuffer<RWTestBuffer> b0)
            {
                
            }
        }
    }
    public class TwoRawAccessToBufferWithIJob : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var query = GetEntityQuery(typeof(RWTestBuffer));
            Debug.Log($"Query Length: {query.CalculateLength()}");
            var entities = query.ToEntityArray(Allocator.TempJob);
            var buffer = EntityManager.GetBuffer<RWTestBuffer>(entities[0]);
            var accessJob  = new RW1{Buffer = buffer};
            // @Note: IJob 不能像JobForEach一样在schedule里面传system信息。
            // for (var i = 0; i < UPPER; i++)
            // {
            //     
            // }
            var handle     = accessJob.Schedule(inputDeps);
            // @Bug: Second access would lead to problem.
            //var buffer2NdAccess = EntityManager.GetBuffer<RWTestBuffer>(entities[0]);

            var access2Job = new RW2{Buffer = buffer};
            var handle2    = access2Job.Schedule(handle);
            inputDeps = handle2;
            
            entities.Dispose();
            inputDeps.Complete();
            return inputDeps;
        }

        private struct RW1 : IJob
        {
            public DynamicBuffer<RWTestBuffer> Buffer;
            public void Execute()
            {
                Buffer.Add(new RWTestBuffer{Value = 3});
            }
        }

        private struct RW2 : IJob
        {
            public DynamicBuffer<RWTestBuffer> Buffer;
            public void Execute()
            {
                Buffer.Add(new RWTestBuffer{Value = 3});

            }
        }
    }

    public struct RWTestBuffer : IBufferElementData
    {
        public int Value;
    }

}