#### SetSingleton<T>(T)

Sets the value of a singleton component.



##### Declaration

```c#
public void SetSingleton<T>(T value)
    where T : struct, IComponentData
```

##### Parameters

| Type | Name    | Description                                         |
| :--- | :------ | :-------------------------------------------------- |
| T    | *value* | An instance of type T containing the values to set. |

##### Type Parameters

| Name | Description         |
| :--- | :------------------ |
| *T*  | The component type. |

##### Remarks

For a component to be a singleton, there can be only one instance of that component in a [World](https://docs.unity3d.com/Packages/com.unity.entities@0.0/api/Unity.Entities.World.html). The component must be the only component in its archetype and you cannot use the same type of component as a normal component.

To create a singleton, create an entity with the singleton component as its only component, and then use `SetSingleton()` to assign a value.

For example, if you had a component defined as:

```C#
public struct Singlet: IComponentData{ public int Value; }
```

You could create a singleton as follows:

```C#
var entityManager = World.Active.EntityManager;
var singletonEntity = entityManager.CreateEntity(typeof(Singlet));
var singletonGroup = entityManager.CreateEntityQuery(typeof(Singlet));
singletonGroup.SetSingleton<Singlet>(new Singlet {Value = 1});
```

You can set and get the singleton value from a EntityQuery or a ComponentSystem.

##### Exceptions

| Type                             | Condition                                                    |
| :------------------------------- | :----------------------------------------------------------- |
| System.InvalidOperationException | Thrown if more than one instance of this component type exists in the world or the component type appears in more than one archetype. |