// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Transforms;

// [AlwaysUpdateSystem]
// public class TestRandom : ComponentSystem
// {
//     Random random = new Random();
//     protected override void OnUpdate()
//     {
//         random.InitState();
//         var count = 5;
//         for (int i = 0; i < count; i++)
//         {
//             var number = random.NextFloat();
//             UnityEngine.Debug.Log(number);
//         }
//     }
//     protected override void OnCreate()
//     {

//     }
//     protected override void OnDestroy()
//     {

//     }
// }

// // Conclusion : Each InitState would Initialize the the state or random. 
// // So it would repeat again and again.
// // You should init at OnCreate if that's not intented.

