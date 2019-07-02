using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace ProjectZ.AI
{
    public class FindMaximumSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
        }

        // HYBRID ECS
        public NativeList<int> FindMaximum<T>()
            where T : struct, IBufferElementData
        {
            return SortInt<T>();
        }

        private NativeList<int> SortInt<T>()
            where T : struct, IBufferElementData
        {
            var output = new NativeList<int>(8, Allocator.TempJob);

            Entities.ForEach((DynamicBuffer<T> b0) =>
            {
                var largestIndex = 0;
                var buffer       = b0.Reinterpret<float>().AsNativeArray();
                var largest      = buffer[0];

                for (var currIndex = 0; currIndex < b0.Length; currIndex++)
                {
                    var current = buffer[currIndex];
                    var larger  = current > largest;
                    largestIndex = math.select(largestIndex, currIndex, larger);
                }

                output.Add(largestIndex);
            });
            return output;
        }

        // PURE ECS
        // @Todo 为了计算buffer长度，花了这么多功夫，是否应该直接传值进来？
        [BurstCompile]
        public NativeArray<int> FindMaximum<T>(out JobHandle handle) 
            where T : struct, IBufferElementData
        {
            var query    = GetEntityQuery(ComponentType.ReadOnly<T>());
            var entities = query.ToEntityArray(Allocator.TempJob);
            var entity   = entities[0];
            var length =
                EntityManager.GetChunk(entity).GetBufferAccessor(GetArchetypeChunkBufferType<Tendency>(true))[0].Length;
            entities.Dispose();

            // @Bug THIS ONE. EM.BUFFER WOULD HAVE RW ACCESS.EntityManager.GetBuffer<T>(entity).Length;
            // var length = 10;
            var output = new NativeArray<int>(length, Allocator.TempJob);
            var job    = new Sort<T> {Output = output};
            handle = job.Schedule(query);
            return output;
        }

        private struct Sort<T> : IJobForEachWithEntity_EB<T> where T : struct, IBufferElementData
        {
            public NativeArray<int> Output;

            public void Execute(Entity entity, int index, [ReadOnly] DynamicBuffer<T> b0)
            {
                var largestIndex = 0;
                var buffer       = b0.Reinterpret<float>().AsNativeArray();
                var length       = b0.Length;
                var largest      = buffer[0];

                for (var currIndex = 0; currIndex < length; currIndex++)
                {
                    var tendency = buffer[currIndex];
                    var larger   = tendency > largest;
                    largestIndex = math.select(largestIndex, currIndex, larger);
                }

                Output[index] = largestIndex;
            }
        }
    }
}