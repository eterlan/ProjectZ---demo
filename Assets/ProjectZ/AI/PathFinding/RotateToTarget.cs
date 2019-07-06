using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectZ.AI.PathFinding
{
    public class RotateToTarget : ComponentSystem
    {
        protected override void OnUpdate()
        {
            //var dt = Time.deltaTime;
            Entities.ForEach(
                (ref NavigateTarget navigateTarget,
                 ref Rotation       rotation,
                 ref LocalToWorld   localToWorld,
                 ref MoveRotSpeed   rotSpeed) =>
                {
                    // Inverse x & z, because unity is left hand coordinate.
                    // var forwardRad = math.atan2(localToWorld.Forward.x, localToWorld.Forward.z);
                    // var targetRad  = math.atan2(target.Position.x, target.Position.z);
                    // var diff       = targetRad - forwardRad;
                    // Debug.Log($"forwardVec: {localToWorld.Forward}, targetVec{target.Position}; forwardRad{forwardRad}, tarRad{targetRad}");
                    // var rotDir           = 1;
                    // if (diff < 0) rotDir = -1;
                    //
                    // rotSpeed.RotSpeed = math.lerp(rotSpeed.RotSpeed, math.abs(diff) < 0.1f ? 0f : rotSpeed.MaxRotSpeed, 0.5f);
                    // var dRot   = rotDir * dt * rotSpeed.RotSpeed;
                    // var newRot = forwardRad + dRot;
                    // Debug.Log($"");
                    //rotation.Value = quaternion.AxisAngle(math.up(), newRot);
                    // @Todo: Quaternion Lerp.
                    var targetVec = navigateTarget.Position - localToWorld.Position;
                    targetVec.y = 0;
                    var length = math.lengthsq(targetVec);
                    //Debug.Log($"rotLength: {length}");
                    if (length < 1f)
                        return;

                    //Debug.Log($"forward: {localToWorld.Forward}, Pos: {localToWorld.Position}, tarPos: {navigateTarget.Position}");
                    var forwardQua = quaternion.LookRotation(localToWorld.Forward, math.up());
                    var targetQua = quaternion.LookRotation(targetVec, math.up());
                    var newRot = math.nlerp(forwardQua, targetQua, rotSpeed.LerpSpeed);
                    rotation.Value = newRot;
                });
        }

        protected override void OnCreate() { }

        protected override void OnDestroy() { }
    }
}