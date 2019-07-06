using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectZ.AI.PathFinding
{
    public class MoveForward : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // @Todo NavigatePlan -> Targets
            var dT = UnityEngine.Time.deltaTime;

            Entities.ForEach(
                (ref LocalToWorld  localToWorld,
                 ref Translation   translation,
                 ref MoveSpeed movSpeed) =>
                {
                    var dPos   = localToWorld.Forward * dT * movSpeed.Speed;
                    var newPos = translation.Value + dPos;
                    translation.Value = newPos;
                    //Debug.Log($"newPos: {newPos}");
                });
            
            return;
        }
        // Modify rotation speed
        // Modify move forward speed

        protected override void OnCreate() { }

        protected override void OnDestroy() { }
    }
}