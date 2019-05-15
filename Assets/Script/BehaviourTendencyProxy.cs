using Unity.Entities;
using UnityEngine;

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