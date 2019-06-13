using Unity.Entities;
using UnityEngine;
using ProjectZ.Component.Setting;

namespace ProjectZ.AI
{
    public struct CurrentBehaviour : IComponentData
    {
        public static implicit operator CurrentBehaviour(int e)
        {
            return new CurrentBehaviour
            {
                BehaviourType = (BehaviourType) e
            };
        }

//    public static implicit operator int(Behaviour behaviour)
//    {
//        return behaviour;
//    }

        public BehaviourType BehaviourType;

        public float ExecuteTimer;
    }

    public class CurrentBehaviourProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            var a    = BehaviourType.Drink;
            var data = new CurrentBehaviour {BehaviourType = a};
            manager.AddComponentData(entity, data);
        }
    }

    [InternalBufferCapacity(8)]
    public struct BehaviourTendency : IBufferElementData
    {
        // Use implicit operator OR you have to type a lot.
        // float data;
        // var element = new BehaviourTendency
        // {
        //     BehaviourTendency = data
        // }

        public static implicit operator float(BehaviourTendency e)
        {
            return e.Value;
        }

        public static implicit operator BehaviourTendency(float e)
        {
            return new BehaviourTendency {Value = e};
        }

        public float Value;
    }

    public class BehaviourTendencyProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            manager.AddBuffer<BehaviourTendency>(entity);
        }
    }
}