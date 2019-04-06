using UnityEngine;
using Unity.Entities;

public class CreateEntity : MonoBehaviour
{
    void Awake()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();
        entityManager.CreateEntity(typeof(Normal));
    }
    private void Start() {
    }
}