using UnityEngine;
using Unity.Entities;
using System;

public struct BehaviourType : IComponentData
{
    public static implicit operator BehaviourType(int e)
    {
        return new BehaviourType
        {
            Behaviour = (BehaviourTypes)e,
        };
    }
    public static implicit operator int (BehaviourType BehaviourType)
    {
        return (int)BehaviourType;
    }
    public BehaviourTypes Behaviour;
    //public int Behaviour;
}

public class BehaviourTypeProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert( Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
    {
        var a = BehaviourTypes.Drink;
        var data = new BehaviourType{Behaviour = a};
        manager.AddComponentData(entity, data);
    }
}