using Unity.Entities;
using UnityEngine;


[InternalBufferCapacity(8)]
public struct BehaviourTendencyBufferElement : IBufferElementData
{
    // Use implicit operator OR you have to type a lot.
    // float data;
    // var element = new BehaviourTendencyBufferElement
    // {
    //     BehaviourTendecy = data
    // }

    public static implicit operator float(BehaviourTendencyBufferElement e)
    {
        return e.BehaviourTendency;
    }
    public static implicit operator BehaviourTendencyBufferElement(float  e)
    {
        return new BehaviourTendencyBufferElement{BehaviourTendency = e};
    }
    public float BehaviourTendency;
}

public class BehaviourTendencyBufferElementProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert ( Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        manager.AddBuffer<BehaviourTendencyBufferElement>(entity);
    }
}