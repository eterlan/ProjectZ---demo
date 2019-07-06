using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectZ.AI.PathFinding
{
    [UpdateInGroup(typeof(PathFindingGroup))]
    [UpdateAfter(typeof(PathFinding))]
    public class HandleTargetArrived : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // @Todo: 是否应该把Rotate也放过来？
            // @Todo: RotateSpeed改名。
            // @Todo: 一开始target为0，0 导致全部跑过来了
            Entities.ForEach(
                (ref MoveSpeed      movementSpeed,
                 ref NavigateTarget target,
                 ref Translation    translation) =>
                {
                    var length = math.lengthsq(target.Position - translation.Value);
                    movementSpeed.Speed = math.lerp(movementSpeed.Speed, length > 1f ? movementSpeed.MaximumSpeed : 0f, movementSpeed.LerpSpeed);
                });

            Entities.ForEach(
                (DynamicBuffer<PathPlanner> path,
                 ref Translation            translation,
                 ref NavigateTarget         target) =>
                {
                    var count = target.Count;
                    if (count == 0 && path.Length != 0)
                    {
                        // has to set target to self position to read next target.
                        target.Position.x = path[count].NextPosition.x;
                        target.Position.z = path[count].NextPosition.y;
                    }
                    // while pathPlanner has NextPosition.
                    // @Bug: else if 之后一直卡在上个条件，没有出去的方法。
                    if (count < path.Length)
                    {
                        // if arrived, change to Next target.
                        if (math.lengthsq(target.Position - translation.Value) > 3f) return;
                        target.Position.x = path[count].NextPosition.x;
                        target.Position.z = path[count].NextPosition.y;
                        target.Count++;
                    }
                    // Path Finding Complete. Reset counter and path.
                    else
                    {
                        target.Count = 0;
                        path.Clear();
                    }

                  
                });
        }

        protected override void OnCreate() { }

        protected override void OnDestroy() { }
    }
}