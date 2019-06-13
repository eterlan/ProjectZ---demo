using Unity.Entities;
using UnityEngine;

namespace ProjectZ.AI
{
    public struct Npc : IComponentData
    {
    }

//[RequireComponent(typeof(CurrentBehaviour),typeof(BehaviourSetting),ty)]
    public class NpcProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            var data = new Npc();
            manager.AddComponentData(entity, data);
        }
    }
}