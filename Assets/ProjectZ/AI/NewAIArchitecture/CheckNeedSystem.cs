//using ProjectZ.Component;
//using Unity.Entities;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Transforms;
//using UnityEngine;
//
//namespace ProjectZ.AI
//{
//    [DisableAutoCreation]
//    public class CheckNeedSystem : ComponentSystem
//    {
//        protected override void OnUpdate()
//        {
//            // 那是分离成两个系统还是说分开，被手动调用？
//            // 分离系统的问题在于循环会调用多次CheckNeedSystem，根本不用
//            
//        private void CheckNeed(int id)
//        {
//            Entities.ForEach((Entity entity,int index,DynamicBuffer<Tendency> c0, ref CurrentBehaviourInfo c1) =>
//            {
//                var tendencies         = c0.AsNativeArray();
//                var currentLvBehavesID = AIDataSingleton.NeedLvBehavioursType[c1.CurrentNeedLv];
//                
//                for (var i = 0; i < currentLvBehavesID.Length; i++)
//                {
//                    var behaveID = (int)currentLvBehavesID[i];
//                    // 如果行为倾向很严重，说明还有这个层次的需求，也就不需要继续运算
//                    if (tendencies[behaveID].Value > 0.75f)
//                    {
//                        return;
//                    }
//                }
//                c1.CurrentNeedLv += 1;
//            });
//        }
//        }
//
//        protected override void OnCreate()
//        {
//        }
//
//        protected override void OnDestroy()
//        {
//        }
//    }
//}
