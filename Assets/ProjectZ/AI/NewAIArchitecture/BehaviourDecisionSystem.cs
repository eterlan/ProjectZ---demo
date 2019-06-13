using ProjectZ.Component;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace ProjectZ.AI
{
    public class BehaviourDecisionSystem : JobComponentSystem
    {
        private struct CopyFactor : IJobForEachWithEntity_EB<Factor>
        {
            public int[] m_factorsMax;
            public int[] m_factorsMin;
            public int[] m_factorsValue;

            public void Execute(Entity entity, int i, [ReadOnly]DynamicBuffer<Factor> factor)
            {
                var factors = factor.AsNativeArray();
                m_factorsValue[i] = factors[i].Value;
            }
        }
        private struct CalculateTendency : IJobForEachWithEntity<Rotation>
        {
        
            public void Execute(Entity entity,int index, ref Rotation rotation)
            {
            
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDependency)
        {
        
            return inputDependency;
        }

        protected override void OnCreate()
        {
        }

        protected override void OnDestroy()
        {
        }
    }
}
