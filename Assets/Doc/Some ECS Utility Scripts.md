# Some ECS Utility Scripts
## Fast copy from NativeArray to managed array
Unity’s ECS is still in its infancy. We’re not getting rid of the old way yet. One of the old ways is dealing with the Mesh class. This class has the interface for setting the vertices, normals, uv, colors, etc. However, these interfaces accepts only managed arrays. In ECS, you can only use the Native collection if you plan to use the Jobs system and Burst compiler. In my case, I keep a NativeArray and copy the contents to a managed array and use this managed array to set to the mesh. We don’t use a for loop here and copy each element. It’s slow especially when the array elements grow large. I use the following extension method instead.

``` C#
public static unsafe void CopyToFast<T>(this NativeArray<T> nativeArray, T[] array) where T : struct {
    int byteLength = nativeArray.Length * UnsafeUtility.SizeOf(typeof(T));
    void* managedBuffer = UnsafeUtility.AddressOf(ref array[0]);
    void* nativeBuffer = nativeArray.GetUnsafePtr();
    UnsafeUtility.MemCpy(managedBuffer, nativeBuffer, byteLength);
}
 
// Usage
NativeArray<Vector3> nativeVertices = new NativeArray<Vector3>(vertexCount, Allocator.Persistent);
Vector3[] managedVertices = new Vector3[vertexCount];
nativeVertices.CopyToFast(managedVertices);
```

## ByteBool
Unity’s ECS uses structs as components. One limitation is that the variables in it should be “blittable”.Blittable types are types that do not require conversion when they are passed between managed and unmanaged code. Under the hood, Unity’s ECS copies a lot of data around. They require blittable types to make this copying as fast as possible.

This means that you can’t have reference types like classes and arrays. Surprisingly, you also can’t use bool. Yep, bool is not a blittable type. But boolean values are so useful. You can get around this by using a byte (one or zero) as the boolean value and wrap it in a struct. Here’s my take on it:
```C#
public struct ByteBool : IEquatable {
    private byte value;
 
    public ByteBool(bool value) {
        this.value = (byte)(value ? 1 : 0);
    }
 
    public bool Value {
        get {
            return this.value != 0;
        }
 
        set {
            this.value = (byte)(value ? 1 : 0);
        }
    }
 
    public bool Equals(ByteBool other) {
        return this.value == other.value;
    }
 
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
            return false;
        }
 
        return obj is ByteBool &amp;&amp; Equals((ByteBool) obj);
    }
 
    public override int GetHashCode() {
        return this.value.GetHashCode();
    }
     
    /// <summary>
    /// Converts a bool to a ByteBool
    /// </summary>
    /// 
    /// 
    public static implicit operator ByteBool(bool value) {
        return new ByteBool(value);
    }
     
    /// <summary>
    /// Converts a ByteBool to a bool
    /// </summary>
    /// 
    /// 
    public static implicit operator bool(ByteBool source) {
        return source.Value;
    }
}
```
You can use this struct like how you would use a bool variable.
```C#
struct MyComponent : IComponentData {
    public ByteBool isSomething;
}
 
MyComponent component = new MyComponent();
component.isSomething = true;
 
...
 
if(component.isSomething) {
    // Do something cool here
}
```

## ComponentDataFromEntity for ISharedComponentData
ComponentDataFromEntity is like Dictionary<Entity, T> where T can only be IComponentData. There’s no such thing for ISharedComponentData. The solution I found is to collect them into a container inside a system. During my studies, I have found that I do this collection of ISharedComponentData instances a lot in different places. I thought maybe I could refactor this. Here’s what I made:

```C#
public abstract class CollectSharedComponentsSystem<T> : ComponentSystem where T : struct, ISharedComponentData {
    protected struct Collected : ISystemStateComponentData {
    }
     
    protected struct Data {
        public readonly int Length;
 
        [ReadOnly]
        public EntityArray Entity;
         
        [ReadOnly]
        public SharedComponentDataArray<T> Component;
 
        [ReadOnly]
        public SubtractiveComponent<Collected> Collected;
    }
 
    [Inject]
    protected Data data;
     
    private readonly Dictionary<Entity, T> map = new Dictionary<Entity, T>(1);
     
    protected override void OnUpdate() {
        for (int i = 0; i < this.data.Length; ++i) {
            Entity entity = this.data.Entity[i];
            this.map[entity] = this.data.Component[i];
             
            // Add this component so it will no longer be processed by this system
            this.PostUpdateCommands.AddComponent(entity, new Collected());
        }
    }
     
    public Maybe<T> Get(Entity entity) {
        if (this.map.TryGetValue(entity, out T value)) {
            return new Maybe<T>(value);
        }
         
        return Maybe<T>.Nothing;
    }
}
 
// Usage
// Say you have a MySharedComponent
 
// Create the concrete system
public class MySharedComponentCollectionSystem : CollectSharedComponentsSystem<MySharedComponent> {
}
 
// Inject in another system and use it
public class SomeSystem : ComponentSystem {
    [Inject]
    private MySharedComponentCollectionSystem mySharedComponents;
 
    protected override void OnUpdate() {
        for(...) {
            Entity someEntity = // get it somewhere
            Maybe<MySharedComponent> sharedComponent = this.mySharedComponents.Get(someEntity);
            if(sharedComponent.HasValue) {
                // Use sharedComponent here
            }
        }
    }
}
```
Note here that I didn't handle removal because I have no use for it yet. You can also see the usage of Maybe<T>. This is a pattern that I have come to adopt lately. If you have a method which can return a value or nothing (say for non existence), return a Maybe<T> instead. This way, you're clear with your intention that the method may return nothing. The caller of the method can see this intention very well and can adjust their code. You can read more here.