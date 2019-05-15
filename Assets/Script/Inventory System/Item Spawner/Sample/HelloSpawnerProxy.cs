using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Samples.HelloCube_06
{
    [RequiresEntityConversion]
    public class HelloSpawnerProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        [FormerlySerializedAs("CountX")] public int        countX;
        [FormerlySerializedAs("CountY")] public int        countY;
        [FormerlySerializedAs("Prefab")] public GameObject prefab;

        // Lets you convert the editor data representation to the entity optimal runtime representation

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var spawnerData = new HelloSpawner
            {
                // The referenced prefab will be converted due to DeclareReferencedPrefabs.
                // So here we simply map the game object to an entity reference to that prefab.
                // GetPrimaryEntity?
                Prefab = conversionSystem.GetPrimaryEntity(prefab),
                CountX = countX,
                CountY = countY
            };
            dstManager.AddComponentData(entity, spawnerData);
        }

        // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
        public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
        {
            gameObjects.Add(prefab);
        }
    }
}