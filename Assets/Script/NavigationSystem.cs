using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class NavigationSystem : JobComponentSystem
{
    private EntityQuery m_houseGroup;
    private EntityQuery m_npcHasNavigationTag;
    private EntityQuery m_treeGroup;

    protected override JobHandle OnUpdate(JobHandle inputDependency)
    {
        var treeCount  = m_treeGroup.CalculateLength();
        var npcCount   = m_npcHasNavigationTag.CalculateLength();
        var houseCount = m_houseGroup.CalculateLength();

        // Tree Group
        var copyTreePositions = new NativeArray<float3>(treeCount, Allocator.TempJob);

        // House Group
        var copyHousePositions = new NativeArray<float3>(houseCount, Allocator.TempJob);

        // NPC Group
        var targetDistances  = new NativeArray<float>(npcCount, Allocator.TempJob);
        var targetPositions  = new NativeArray<float3>(npcCount, Allocator.TempJob);
        var targetIndices    = new NativeArray<int>(npcCount, Allocator.TempJob);
        var targetDirNors   = new NativeArray<float3>(npcCount, Allocator.TempJob);
        var behaviourTargets = new NativeArray<BehaviourTypes>(npcCount, Allocator.TempJob);

        // What to do if there is two job to schedule? Combine?
        var copyTreePositionsJob = new CopyPositions
        {
            Positions = copyTreePositions
        };
        var copyTreePositionsJobHandle = copyTreePositionsJob.Schedule(m_treeGroup, inputDependency);

        var copyHousePositionsJob = new CopyPositions
        {
            Positions = copyHousePositions
        };
        var copyHousePositionsJobHandle = copyHousePositionsJob.Schedule(m_houseGroup, inputDependency);

        var copyBehaviourTargetsJob = new CopyBehaviourTargets
        {
            BehaviourTargets = behaviourTargets
        };
        var copyBehaviourTargetsJobHandle = copyBehaviourTargetsJob.Schedule(m_npcHasNavigationTag, inputDependency);

        var copyTargetAndPositionsBarrierJobHandle = JobHandle.CombineDependencies(copyTreePositionsJobHandle,
            copyHousePositionsJobHandle, copyBehaviourTargetsJobHandle);

        var findtargetJob = new FindClosestTarget
        {
            TreePositions  = copyTreePositions,
            HousePositions = copyHousePositions,

            TargetDistances  = targetDistances,
            TargetIndices    = targetIndices,
            TargetPositions  = targetPositions,
            TargetDirNors   = targetDirNors,
            BehaviourTargets = behaviourTargets
        };
        var findTargetJobHandle = findtargetJob.Schedule(m_npcHasNavigationTag, copyTargetAndPositionsBarrierJobHandle);
        var movementJob = new Movement
        {
            DeltaTime       = Time.deltaTime,
            TargetDirNors  = targetDirNors,
            TargetDistances = targetDistances
            //commandBuffer   = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
        };
        var movementJobHandle = movementJob.Schedule(m_npcHasNavigationTag, findTargetJobHandle);

        inputDependency = movementJobHandle;
        //Why add dependency to Group?
        m_npcHasNavigationTag.AddDependency(inputDependency);

        //TEST
        // LoopDebug(copyTreePositions);
        // LoopDebug(targetPositions);
        // LoopDebug(targetDir_Nors);
        // LoopDebug(targetTypes);

        return inputDependency;
    }

    public void LoopDebug(NativeArray<float3> inputArray)
    {
        for (var i = 0; i < inputArray.Length; i++) Debug.Log($"Array[{i}] = {inputArray[i]}");
    }

    public void LoopDebug(NativeArray<int> inputArray)
    {
        for (var i = 0; i < inputArray.Length; i++) Debug.Log($"Array[{i}] = {inputArray[i]}");
    }

    protected override void OnCreateManager()
    {
        //commandBufferSystem   = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        m_npcHasNavigationTag = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<Npc>(),
                ComponentType.ReadWrite<Translation>(),
                ComponentType.ReadWrite<Rotation>(),
                ComponentType.ReadOnly<MovementSpeed>(),
                ComponentType.ReadOnly<BehaviourType>(),
                ComponentType.ReadOnly<NavigationTag>()
            }
        });
        m_treeGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<Tree>(),
                ComponentType.ReadOnly<LocalToWorld>() // static object doesn't has Translation component ?
            }
        });
        m_houseGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<House>(),
                ComponentType.ReadOnly<LocalToWorld>()
            }
        });
    }
    //public EndSimulationEntityCommandBufferSystem commandBufferSystem;


    [BurstCompile]
    public struct CopyPositions : IJobForEachWithEntity<LocalToWorld>
    {
        public NativeArray<float3> Positions;

        public void Execute(Entity entity, int index, [ReadOnly] ref LocalToWorld localToWorld)
        {
            Positions[index] = localToWorld.Position;

            // TEST = correct. index is group-specify
            // indices[index] = index; 
        }
    }

    [BurstCompile]
    public struct CopyBehaviourTargets : IJobForEachWithEntity<BehaviourType>
    {
        public NativeArray<BehaviourTypes> BehaviourTargets;

        public void Execute(Entity entity, int index, [ReadOnly] ref BehaviourType behaviourTarget)
        {
            BehaviourTargets[index] = behaviourTarget.Behaviour;
        }
    }

    [BurstCompile]
    public struct FindClosestTarget : IJobForEachWithEntity<Translation>
    {
        [DeallocateOnJobCompletion] public            NativeArray<int>            TargetIndices;
        [DeallocateOnJobCompletion] public            NativeArray<float3>         TargetPositions;
        public                                        NativeArray<float>          TargetDistances;
        public                                        NativeArray<float3>         TargetDirNors;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3>         TreePositions;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3>         HousePositions;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<BehaviourTypes> BehaviourTargets; // use enum?

        private void ClosestTarget(NativeArray<float3> positions, float3 selfPosition, out int closestIndex,
            out float closestDistance, out float3 closestDirNor)
        {
            closestIndex    = 0;
            closestDistance = math.lengthsq(positions[0] - selfPosition);
            for (var i = 0; i < positions.Length; i++)
            {
                var target   = positions[i];
                var distance = math.lengthsq(target - selfPosition);
                var nearest  = distance < closestDistance;

                closestIndex    = math.select(closestIndex, i, nearest);
                closestDistance = math.select(closestDistance, distance, nearest);
            }

            closestDistance = math.sqrt(closestDistance);
            var closestDir = positions[closestIndex] - selfPosition;
            closestDirNor = math.normalizesafe(closestDir);
        }

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            // WAIT FOR TEST --- DONE ---
            // targetTypes[0] = 0;
            // targetTypes[1] = 1;
            // targetTypes[2] = 1;
            // targetTypes[3] = 0;

            var targetPosition  = float3.zero;
            var selfPosition    = translation.Value;
            var behaviourTarget = BehaviourTargets[index];

            //Default = Tree
            var positions = TreePositions;
            switch (behaviourTarget)
            {
                case BehaviourTypes.Eat:
                {
                    positions = HousePositions;
                    break;
                }

                case BehaviourTypes.Drink:
                {
                    positions = TreePositions;
                    break;
                }
            }

            if (positions.Length > 0)
            {
                ClosestTarget(positions, selfPosition, out var targetIndex, out var targetDistance,
                    out var targetDirNor);
                TargetIndices[index]   = targetIndex;
                TargetDistances[index] = targetDistance;
                TargetPositions[index] = positions[targetIndex];
                TargetDirNors[index]  = targetDirNor;
            }
        }
    }

    [BurstCompile]
    public struct Movement : IJobForEachWithEntity<Translation, MovementSpeed, Rotation, NavigationTag>
    {
        public                                        float               DeltaTime;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> TargetDirNors;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float> TargetDistances;
        //public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int index, ref Translation translation,
            [ReadOnly] ref MovementSpeed movementSpeed, ref Rotation rotation, ref NavigationTag navigationTag)
        {
            if (!navigationTag.Arrived)
            {
                var speed         = DeltaTime * movementSpeed.Speed;
                var targetDirNor = TargetDirNors[index];
                var position      = translation.Value;
                var movePosition  = position + targetDirNor * speed; //direction_normalize * speed;

                //hard-code, might change it to radius later?
                var keepPosition = 0.5f;
                var arrived      = TargetDistances[index] < keepPosition;
                //If has other component, you cannot assign Value to LTW.
                translation.Value = movePosition; //math.select(movePosition, position, tooClose);
                rotation.Value    = quaternion.LookRotationSafe(targetDirNor, math.up());

                // if (Arrived)               commandBuffer.RemoveComponent<NavigationTag>(index,entity);
                if (arrived)
                    navigationTag.Arrived = true;
            }
        }
    }
}