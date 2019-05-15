using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerMovementSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDependency)
    {
        var input = new float3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        var playerMovementJob = new PlayerMovement
        {
            DeltaTime   = Time.deltaTime,
            PlayerInput = input
        };
        return playerMovementJob.Schedule(this, inputDependency);
    }

    public struct PlayerMovement : IJobForEach<Translation, Player, MovementSpeed, Rotation>
    {
        public float  DeltaTime;
        public float3 PlayerInput;

        public void Execute(ref Translation translation, [ReadOnly] ref Player player,
            [ReadOnly] ref MovementSpeed movementSpeed, ref Rotation rotation)
        {
            var moveSpeed     = DeltaTime * movementSpeed.Speed;
            var moveDirection = math.normalizesafe(PlayerInput);
            var movePosition  = translation.Value + moveSpeed * moveDirection;

            translation.Value = movePosition;

            if (moveDirection.x != 0 || moveDirection.z != 0)
                rotation.Value = quaternion.LookRotationSafe(moveDirection, math.up());
        }
    }
}