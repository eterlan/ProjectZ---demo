using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;
using System.Collections;
using System.Collections.Generic;

public class NavigationSystem : JobComponentSystem 
{
    private EntityQuery m_NPCHasNavigationTag;
    private EntityQuery m_TreeGroup;
    private EntityQuery m_HouseGroup;
    //public EndSimulationEntityCommandBufferSystem commandBufferSystem;


    [BurstCompile]
    public struct CopyPositions : IJobForEachWithEntity<LocalToWorld>
    {
        public NativeArray<float3> positions;
        public void Execute(Entity entity, int index, [ReadOnly]ref LocalToWorld localToWorld)
        {
            positions[index] = localToWorld.Position;
            
            // TEST = correct. index is group-specify
            // indices[index] = index; 
        }
    }
    [BurstCompile]
    public struct CopyBehaviourTargets : IJobForEachWithEntity<BehaviourType>
    {
        public NativeArray<BehaviourTypes> behaviourTargets;
        public void Execute(Entity entity, int index, [ReadOnly]ref BehaviourType behaviourTarget)
        {
            behaviourTargets[index] = behaviourTarget.Behaviour;
        }
    }

    [BurstCompile]
    public struct FindClosestTarget : IJobForEachWithEntity<Translation>
    {
        [DeallocateOnJobCompletion]
        public NativeArray<int> targetIndices;
        [DeallocateOnJobCompletion]
        public NativeArray<float3> targetPositions;
        public NativeArray<float> targetDistances;
        public NativeArray<float3> targetDir_Nors;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<float3> TreePositions;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<float3> HousePositions;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<BehaviourTypes> behaviourTargets; // use enum?

        void ClosestTarget(NativeArray<float3> Positions, float3 selfPosition, out int closestIndex, out float closestDistance, out float3 closestDir_Nor)
        {
            closestIndex   = 0;
            closestDistance = math.lengthsq(Positions[0] - selfPosition);
            for (int i = 0; i < Positions.Length; i++)
            {
                var target     = Positions[i];
                var distance   = math.lengthsq(target - selfPosition);
                var nearest    = distance < closestDistance;

                closestIndex     = math.select(closestIndex, i, nearest);
                closestDistance  = math.select(closestDistance, distance, nearest);
            }
            closestDistance  = math.sqrt(closestDistance);
            var closestDir   = Positions[closestIndex] - selfPosition;
            closestDir_Nor   = math.normalizesafe(closestDir);
        }

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            // WAIT FOR TEST --- DONE ---
            // targetTypes[0] = 0;
            // targetTypes[1] = 1;
            // targetTypes[2] = 1;
            // targetTypes[3] = 0;

            float3 targetPosition   = float3.zero;
            var selfPosition       = translation.Value;
            var behaviourTarget    = behaviourTargets[index];

            //Default = Tree
            var Positions          = TreePositions;
            switch (behaviourTarget)
            {
                case BehaviourTypes.Eat :{
                    Positions = HousePositions;
                    break;
                }
                case BehaviourTypes.Drink :{
                    Positions = TreePositions;
                    break;
                }               
            }
            if (Positions.Length > 0)
            {
                ClosestTarget(Positions, selfPosition, out int targetIndex, out float targetDistance, out float3 targetDir_Nor);
                targetIndices[index]   = targetIndex;
                targetDistances[index] = targetDistance;
                targetPositions[index] = Positions[targetIndex];    
                targetDir_Nors[index]  = targetDir_Nor;
            }               
        }
    }

    [BurstCompile]
    public struct Movement : IJobForEachWithEntity<Translation, MovementSpeed,Rotation, NavigationTag>
    {
        public float DeltaTime;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<float3> targetDir_Nors;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<float>  targetDistances;
        //public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int index, ref Translation Translation, [ReadOnly] ref MovementSpeed movementSpeed,ref Rotation Rotation, ref NavigationTag navigationTag)
        {
            if (!navigationTag.Arrived)
            {
                var speed               =   DeltaTime * movementSpeed.Speed;
                var targetDir_Nor       =   targetDir_Nors[index];
                var position            =   Translation.Value;
                var movePosition        =   position + targetDir_Nor*speed;//direction_normalize * speed;

                //hard-code, might change it to radius later?
                var keepPosition        =   0.5f;
                var Arrived             =   targetDistances[index] < keepPosition;
                //If has other component, you cannot assign Value to LTW.
                Translation.Value       =   movePosition;//math.select(movePosition, position, tooClose);
                Rotation.Value          =   quaternion.LookRotationSafe(targetDir_Nor, math.up());

                // if (Arrived)               commandBuffer.RemoveComponent<NavigationTag>(index,entity);
                if (Arrived)               
                    navigationTag.Arrived = true;
            }
            

        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependency)
    {
        var treeCount = m_TreeGroup.CalculateLength();
        var NPCCount  = m_NPCHasNavigationTag.CalculateLength();
        var HouseCount = m_HouseGroup.CalculateLength();
        
        // Tree Group
        var copyTreePositions = new NativeArray<float3>(treeCount, Allocator.TempJob); 
        
        // House Group
        var copyHousePositions = new NativeArray<float3>(HouseCount, Allocator.TempJob);

        // NPC Group
        var targetDistances   = new NativeArray<float>(NPCCount,Allocator.TempJob);
        var targetPositions   = new NativeArray<float3>(NPCCount,Allocator.TempJob);
        var targetIndices     = new NativeArray<int>(NPCCount, Allocator.TempJob);
        var targetDir_Nors    = new NativeArray<float3>(NPCCount, Allocator.TempJob);
        var behaviourTargets  = new NativeArray<BehaviourTypes>(NPCCount,Allocator.TempJob);
       
        // What to do if there is two job to schedule? Combine?
        CopyPositions CopyTreePositionsJob = new CopyPositions
        {
            positions = copyTreePositions,
        };     
        var CopyTreePositionsJobHandle = CopyTreePositionsJob.Schedule(m_TreeGroup, inputDependency);

        CopyPositions CopyHousePositionsJob = new CopyPositions
        {
            positions = copyHousePositions,
        };
        var copyHousePositionsJobHandle   = CopyHousePositionsJob.Schedule(m_HouseGroup, inputDependency);

        CopyBehaviourTargets copyBehaviourTargetsJob = new CopyBehaviourTargets
        {
            behaviourTargets = behaviourTargets,
        };
        var copyBehaviourTargetsJobHandle = copyBehaviourTargetsJob.Schedule(m_NPCHasNavigationTag, inputDependency);

        var CopyTargetAndPositionsBarrierJobHandle = JobHandle.CombineDependencies(CopyTreePositionsJobHandle,copyHousePositionsJobHandle,copyBehaviourTargetsJobHandle);

        FindClosestTarget findtargetJob = new FindClosestTarget
        {
            TreePositions    = copyTreePositions,
            HousePositions   = copyHousePositions,
            
            targetDistances  = targetDistances,
            targetIndices    = targetIndices,
            targetPositions  = targetPositions,
            targetDir_Nors   = targetDir_Nors,
            behaviourTargets = behaviourTargets,
        };
        var FindTargetJobHandle = findtargetJob.Schedule(m_NPCHasNavigationTag, CopyTargetAndPositionsBarrierJobHandle);     
        Movement MovementJob = new Movement
        {
            DeltaTime       = Time.deltaTime,
            targetDir_Nors  = targetDir_Nors,
            targetDistances = targetDistances,
            //commandBuffer   = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
        };
        var MovementJobHandle = MovementJob.Schedule(m_NPCHasNavigationTag, FindTargetJobHandle);

        inputDependency       = MovementJobHandle;
        //Why add dependency to Group?
        m_NPCHasNavigationTag.AddDependency(inputDependency);

        //TEST
        // LoopDebug(copyTreePositions);
        // LoopDebug(targetPositions);
        // LoopDebug(targetDir_Nors);
        // LoopDebug(targetTypes);

        return inputDependency;
    }

    public void LoopDebug(NativeArray<float3> inputArray)
    {
        for (int i = 0; i < inputArray.Length; i++)
        {
            Debug.Log($"Array[{i}] = {inputArray[i]}");
        }
    } 
    public void LoopDebug(NativeArray<int> inputArray)
    {
        for (int i = 0; i < inputArray.Length; i++)
        {
            Debug.Log($"Array[{i}] = {inputArray[i]}");
        }
    }

    protected override void OnCreateManager()
    {
        //commandBufferSystem   = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        m_NPCHasNavigationTag = GetEntityQuery(new EntityQueryDesc
        {
            All = new []
            {
                ComponentType.ReadOnly<Npc>(),
                ComponentType.ReadWrite<Translation>(),
                ComponentType.ReadWrite<Rotation>(),
                ComponentType.ReadOnly<MovementSpeed>(),
                ComponentType.ReadOnly<BehaviourType>(),
                ComponentType.ReadOnly<NavigationTag>(),
            }
        });
        m_TreeGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new []
            {
                ComponentType.ReadOnly<Tree>(),
                ComponentType.ReadOnly<LocalToWorld>(), // static object doesn't has Translation component ?
            }
        });
        m_HouseGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new []
            {
                ComponentType.ReadOnly<House>(),
                ComponentType.ReadOnly<LocalToWorld>(),
            }
        });
    }
}