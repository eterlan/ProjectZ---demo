using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;

public class PlayerMovementSystem : JobComponentSystem
{
    public struct PlayerMovement : IJobForEach<Translation, Player, MovementSpeed, Rotation>
    {
        public float deltaTime;
        public float3 playerInput;
        public void Execute(ref Translation translation,[ReadOnly]ref Player player,[ReadOnly]ref MovementSpeed movementSpeed,ref Rotation rotation)  
        {
            var moveSpeed     = deltaTime * movementSpeed.Speed;
            var moveDirection = math.normalizesafe(playerInput);
            var movePosition  = translation.Value + moveSpeed * moveDirection;

            translation.Value = movePosition; 

            if ( moveDirection.x != 0 || moveDirection.z != 0 )
            {
                rotation.Value    = quaternion.LookRotationSafe(moveDirection, math.up());
            }
        }
    }
    protected override JobHandle OnUpdate( JobHandle InputDependency)
    {
        var input = new float3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        var playerMovementJob = new PlayerMovement
        {
            deltaTime   = Time.deltaTime,
            playerInput = input,
        };
        return playerMovementJob.Schedule(this,InputDependency);

    }
}