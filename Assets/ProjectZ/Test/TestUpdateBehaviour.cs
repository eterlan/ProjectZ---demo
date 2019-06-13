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
//    public class TestUpdateBehaviour : ComponentSystem
//    {
//        private Entity m_entity;
//        protected override void OnUpdate()
//        {
//            Entities.ForEach((ref CurrentBehaviourInfo info) =>
//            {
//                Debug.Log(1);
//            });
//        }
//
//        protected override void OnCreate()
//        {
//            // manager exist?
//            m_entity = EntityManager.CreateEntity(typeof(CurrentBehaviourInfo));
//        }
//    }
//    
//    [AlwaysUpdateSystem]
//    public class CallUpdate : ComponentSystem
//    {
//        protected override void OnUpdate()
//        {
//            if (Input.GetKeyDown(KeyCode.A))
//            {
//                World.GetOrCreateSystem<TestUpdateBehaviour>().Update();
//            }
//
//            if (Input.GetKeyDown(KeyCode.S))
//            {
//                World.GetOrCreateSystem<TestUpdateBehaviour>().Enabled ^= true;
//            }
//        }
//    }
//}
//
