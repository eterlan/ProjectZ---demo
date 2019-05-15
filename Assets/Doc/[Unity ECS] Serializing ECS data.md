-   
-   
-   
-   
-   
-   
-   
-   

-   



# [Unity ECS] Serializing ECS data

[![Go to the profile of 5argon](https://cdn-images-1.medium.com/fit/c/50/50/1*IThyiP47QpvVqEsWFclyCQ.png)](https://medium.com/@5argon?source=post_header_lockup)

[5argon](https://medium.com/@5argon)Follow

Dec 4, 2018



![img](https://cdn-images-1.medium.com/max/800/0*vprjPP-ZlQ7koIWw)

Photo by [chuttersnap](https://unsplash.com/@chuttersnap?utm_source=medium&utm_medium=referral) on [Unsplash](https://unsplash.com/?utm_source=medium&utm_medium=referral)

ECS provides a way to write all chunk data as-is to binary and back. Currently the API feels pretty much WIP but let’s learn what we can before it is finalized. You may use it as to save and restore a world’s state.

### You can only serialize an entire world

Because you tell `EntityManager` which is a per-world thing to serialize.

And so to “select” what to serialize just create a new `World` and move entities to it. To do that, use EM of your to be serialized world and `EntityManager.MoveEntitiesFrom` where you could use `ComponentGroup` as to select what to move to this world.

Of course `EntityArchetypeQuery` could be used to create that component group. So you get only the correct chunks.

#### Move

You lose those entities from your original world, it is not a copy. So you may want to move back when you have finished with the serialize.

### What are being serialized?

To learn about this let’s see `SerializeUtility.cs` > `SerializeWorld`

This is the type collection phase (preview 21) :



![img](https://cdn-images-1.medium.com/max/800/1*qTMJK4dyG8O3_qVBouNyZg.png)![img](https://cdn-images-1.medium.com/max/1400/1*qTMJK4dyG8O3_qVBouNyZg.png)

Writing phase



![img](https://cdn-images-1.medium.com/max/800/1*VSKzPwCIo3uYdzo-EVdAIQ.png)

-   Type index for each type paired with ASCII bytes representing its `AssemblyQualifiedName` . Hash for compact representation of its data **layout**.
-   For each archetype, bunch of indexes about type contained and entity count too. (When serialize back then we could get the name back)
-   Then after that it simply iterates through chunks and write all byte data.

That’s amazingly simple! Now if ECS is widely used you can hack other people’s game just by following the bytes according to this code.. (Do not come to hack my game)

#### Chunk’s archetype

Each chunk strictly belongs to 1 `EntityArchetype` / `Archetype` . This information is a pointer in the `Chunk` data structure. Surely we could not just serialize this pointer as-is or when it loads back, it would be meaningless.

Instead, that pointer field is treated as an int, replace the address by archetype index. We will use this number to map back on deserialize.



![img](https://cdn-images-1.medium.com/max/800/1*5P2-d8OdU3z2a75m2KY2dg.png)

2 fields above contains next and prev pointers. So the archetype index is positioned by `pointer size` *4 later from the beginning of the chunk. This is important in the bug section later.

### SharedComponentData

If you use `SerializeUtility` you notice you get shared component “indexes”. That’s useless unless you know what it means!

So if you use `SerializeUtilityHybrid` instead you get something back to map those indexes back to.. as a game object. How??



![img](https://cdn-images-1.medium.com/max/800/1*WbYYt56p3_RQ6QYWCPw7pw.png)

So it searches for your shared type name and append “Component” and look for the `SharedComponentDataWrapper` If you do not have that component then you are screwed.. so if you have `Lane : ISharedComponentData { int lane; }`then you better prepare `LaneComponent : SharedComponentDataWrapper`

**Update** : This “Component” suffix pattern seems to be changed.

[**Unity-Technologies/EntityComponentSystemSamples**
*Contribute to Unity-Technologies/EntityComponentSystemSamples development by creating an account on GitHub.*github.com](https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/ReleaseNotes.md#fixes)

>   HybridSerializeUtility no longer has an implicit naming convention requirement for SharedComponentDataProxy (i.e. wrapper) classes.

After that it keeps the data in that, attach to your game object. Your game object is being `new` ed, so that probably pops up instantly in your scene? Then you do whatever you can to keep it around for the load? It feels really weird yet reasonable way to serialize IShared… lol

According to this deserialize code, you should keep something called “shared scene”. Which should contains your magic game object that you got. You got to have this scene loaded additively before attempting to deserialize. But the deserialize method take just `GameObject` so you could keep it in whatever way. Maybe using the new Addressable Asset System?



![img](https://cdn-images-1.medium.com/max/800/1*kKB4_Dv9qAhimaOPnAJCAg.png)

Loading back if you do it in an empty world everything should be fine? Since you serialized the **whole** world’s shared component data.

#### What it looks like



![img](https://cdn-images-1.medium.com/max/800/1*7qZEOMxpwmc4AatgpEfRZQ.png)

You see this game object contains every unique values of `ISharedComponentData` I used. Note the error, of course if we run the game with this object **active** it would cause problem as GOE will try to add multiple components of the same type. However it is just a storage, we will keep it disabled in order to feed to `SerializeUtilityHybrid` on deserialize.

Remember that in my entities they would be attached with `LaneIndex` and `Measure` shared component, Unity will append “Component” string and find its equivalent wrappers by that name.

#### Shared component data purification

I decided to “purify” all shared component because I don’t want to keep a separated serialization for the game object, then I am back to “pure” serialization. By purifying does not mean removing the component, but use set component making it a **default** value. (Literally, `default` )

And by the rule of `ISharedComponentData` it does not count as unique index when the value is at its default! This way the game object will not pop up if you used `SerializeUtilityHybrid` , or if just `SerializeUtility` the returned `int[]` indicating how many shared components would be at length 0. (You could put some `throw` here if you fear you missed some component.)

You still have those shared components serialized as its archetype, but not each unique values, which is what I wanted since I can recompute them later from normal components. No hassle…

Also if you want to use my `PurifierSystem.cs`

<https://github.com/5argon/E7ECS/blob/master/UtilitySystems/PurifierSystem.cs>

### Entity remapping

#### EntityRemapInfo

The serialize command optionally take `NativeArray<EntityRemapUtility.EntityRemapInfo>` . What is “entity remapping”? If by the code literally,

-   You store a bunch of `Entity` pair.
-   Then to use the remapper give it one entity and its pair will come out.
-   Gives `Entity.Null` when pair not found. In order to get a pair both `Index`and `Version` needs to be correct.

`EntityRemapUtility.RemapEntity` can do this if you give it that `NativeArray` .
*(This could actually be useful in a gameplay code?)

You don’t have to allocate this `NativeArray` if you just simply use the overload without it. But if you do, then you get to keep the created remapper!

Do not allocate it manually, use the method `CreateEntityRemapArray` of the `EntityManager` WHERE YOU WILL MOVE FROM and you will get just enough size to work.

#### What’s in the remapper?

You will get this remap : “turns all entities you have into Version = 0 and Index = nice running number”

So you see the purpose, Unity wants the serialized file to have a nicely arranged indexes and all at version 0.

#### Patching entities

The serializer has this amazing function : not just that all of your entities are being remapped into a new index, all of your `Entity` **fields** in **all of your** `IComponentData` will be remapped! So you can safely serialize all the `Entity` -as-reference as-is and have a peace of mind that they are “intelligently” still linked in the serialized file. Magic!

If you want to do this “patching” manually there are some methods in EntityRemapUtility, but involving unsafe codes. And also when you move entities from your original world this patching already happened once.

Now you might have some reason to use the remapper overload. You may want to further patch other unserialized entities to something in the serialized file… essentially you know things your world right now will “reborn” into which `Entity` on deserialize.

### How it writes?

`BinaryWriter` is a Unity’s interface not C# class. It requires unsafe code to implement as you have to say how to **unsafe void WriteBytes(void\* data, int bytes);**

However Unity gives `StreamBinaryWriter` for you to use. It’s weakness is that it is not really a stream writer, it is a file writer. So you cannot bring your own stream.



![img]()

I have to make a code copy as `RealStreamBinaryWriter` that accepts just plain `Stream` . (Honestly this should be named `FileBinaryWriter` or something…) Then maybe I could put some `CryptoStream` or compression stream in it. **using ICSharpCode.SharpZipLib.BZip2;** recommended! (MIT license) I got the size smaller than Lv. 9 compression of ZIP and GZIP.

The copy-paste hacked systems : <https://github.com/5argon/E7ECS/tree/master/Serialization> it requires `unsafe` , if you don’t want to make your game’s unsafe assembly you may use my entire `asmdef` in that GitHub repo. (Unity Package Manager can pull from GitHub too!!)

### Deserialization

Now you will realize why this serialization thing is based on the whole world, because otherwise the deserialized entities would have a conflicting indexes.

This is a critical step because we want to know what could break deserialization, we want to avoid non-backward compatible changes with serialized files.

#### File format version



![img]()

The first thing the reader read is an `int` . When you save you keep an `int` of serializer’s version. If Entities lib bump this version then you can get incompatibility error.

Note that the version is `static` and not `const` , so it is possible to set it to your file’s version to allows read. But in that case the read might have problems because reader code is already at new version. (So to migrate you might have to copy paste the reader code of old Entities package and reserialize as the new one?)

#### Type matching back

Now the highlight!



![img]()

-   Reflection back from ASCII serialized name with `Type.GetType` . If you changed your assembly qualified type name then you are screwed.
-   Including changing your `asmdef` file name, that changes the assembly qualified name.. obviously.
-   Even if you got the name right if its layout changes even one bit then the read fails because hash mismatch.
-   Protobuf for example is better in this department since it labels each field with an integer. As long as you don’t touch old label and just deprecate it, and use new numbers for new fields it is backward compatible.
-   Keep the old version’s layout for migration? It could be great if we can “redirect name” to old struct with correct layout. *there is a code comment in the type hashing program that is saying **//@TODO: Encode type in hashcode…**, so if that happens then you will not get away with just equal layout.
-   TypeManager will be populated with all types from your serialized data at this point.

### Type layout hashing algorithm

It is a big pressure to not touch the type in a way that changes its layout hash. In the future it should be resolvable more nicely, but for now the hash needs to be dead exact because the deserialize algorithm wants to do a direct pointer math to write **chunk data** (not even each struct data) without carefully looking at the actual data structure. So it is like ok the hash matches so it is probably fine HERE GOES and pours everything back.

If you have heard about **FlatBuffers** from Google, the concept is almost the same but Google also serialized “vtable” in order to allow forward compatibility (able to add new fields) and use “(deprecated)” on the schema for backward compatibility (kinda like Protobuf). ECS could use the same strategy in the future, maybe serialize more sophisticated layout and not just a hash.

[**FlatBuffers: FlatBuffers white paper**
*A FlatBuffer is a binary buffer containing nested objects (structs, tables, vectors,..) organized using offsets so that…*google.github.io](https://google.github.io/flatbuffers/flatbuffers_white_paper.html)

You can imagine how one tiny struct change can throw everything off in that big, contiguous chunk of data where multiple type lives together. But in exchange, serialize and read back should be blazing fast. As in nothing should be able to do faster than blindingly read/write with minimal protection and no backward support like this…

And here is the layout creation routine which will be hashed.



![img]()

-   Only instance fields with public or non-public, so properties are fine to use. (*You may utilize property to allows backward compatibility or add functionality without introducing layout change.)
-   Field’s offset is there. You cannot rearrange fields. (OR you can use C# `[FieldOffset]` to make the offset the same after doing some surgery to your struct?)
-   It seems to support `[FixedBuffer]` <https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.fixedbufferattribute?view=netframework-4.7.2> I don’t know where Unity used this or is it supposed to be used by us?
-   Some types are not interchangable because of type code. <https://docs.microsoft.com/en-us/dotnet/api/system.typecode?view=netframework-4.7.2> For example you cannot rename your enum into an int and have it “just works”. The hash will change.
-   Next stage is field’s `sizeof` . Pointer and class fields have fixed pointer size but they are not used in `IComponentData` anyways. Enum also should have the same size across the board, so swapping one enum type to another enum type should be possible here? (Or simply add more values to the old enum, that should be fine.)
-   Other fields will use standard `sizeof` . But still types with the same `sizeof` would be caught by the type code and so not interchangable. (Like `uint` and `int` )
-   Renaming fields is possible. No names has been hashed. Remember that renaming the `IComponentData` struct name where it will be a part of archetype is NOT possible as ASCII name will be there together with its layout hash.

Good read about struct’s layout :

[**Mastering C# structs - C# tutorial - developer Fusion**
*As structs are aggregates of any other data type you care to invent, learning how to work with them is important, and…*www.developerfusion.com](https://www.developerfusion.com/article/84519/mastering-structs-in-c/)

### Checking serialized types

To prevent surprise I suggest you go into the source code and log out all ASCII types that goes into your serialized file. I once have one unexpected type slip in, then I changed some structure of that `struct` thinking that it has no relationship to the serialized files and boom..

### Serialized data format

This is based on preview 21. You may want to do some surgery if forward compatibility breaks.

```
4 [Amount of types]
4 [Total type ASCII names length including number 0 for each] = A
--- Type names xA
? [ASCII name]
4 [Number 0 as a separator]
---
4 [Amount of archetypes] = B
----- Archetype xB
4 [Entity count]
4 [Type count] = C
4C [Type indexes, maps to type names prior in order]
-----
4 [Amount of chunks]
I have no idea below this but just don't mess with them
```

Because there is no “total file size” serialized anywhere in the beginning, it should be possible to hack and change ASCII names for example if you just renamed your struct name, then just modify the total ASCII length a bit above that.

### Bugs

#### Preview 21 : Negative Entity Index

Make the `World` to receive deserialized data from scratch. Then after you got your data, **do not touch EntityManager.** In other words you cannot continue using that deserialized world in any way other than quickly migrate those entities to your main world using your main world’s MoveEntitiesFrom.

I have create more entities to the loaded world but I got a new entity with `Index` = -1 (???), making the world complain when it is disposing its own `EntityManager` .

#### Preview 21 : Zero chunk capacity

This one I am not sure, but I think it is related to the prior case. The serialized chunk contains only the data and archetype index hooked with it. On deserialize back, the archetype is restored along with its data, shared index, and dynamic buffers. The capacity was not restored. The method that adds the newly deserialized chunk to ECS system `AddExistingChunk` of `ExclusiveEntityTransaction` also do not refresh the capacity. And so when you do something that modifies that chunk funny things happen.

Luckily moving chunks to a new world will refresh the capacity as I said in the previous point. Just think that the deserialized chunk is very dangerous.

#### Preview 21 : Unrelated archetypes serialized

I found that many more archetypes with 0 entities got in the file. This surprised me when I change one unrelated`IComponentData` not contained in the file and unable to read back because layout changed.

GetAllArchetypes of SerializeUtility.cs contains entity check ≥ 0. IMO I think the correct choice should be > 0 so archetypes without entities will be stripped out.

#### Preview 21 : 32/64 bit pointer size problem

On deserialize back, it reads chunk data and **assume** that the shape is exactly of type `Chunk` . This type contains 4 pointers outright before it can reach the `Archetype*` field.



![img]()

And on deserialize it asked for `chunk->Archetype` . What it does not know is that we might have serialized the file with 64-bit machine causing the offset to be 8*4 = 32 but on things like Android, if it is 32 bit then `chunk->Archetype`will offset only 4*4 = 16 bytes following the sequential layout rule. And you get 0 index every time in situation like this! `Chunk` ‘s shape is not the same in the player’s phone and our dev machine.

Then it access `chunk->Buffer` . How many pointers are preceding that field?



![img]()

`sizeof(Chunk)` on a 64-bit machine is 88. The end of `Padding2` is 80 plus 4 more `byte` allocated as `fixed` buffer. (Where’s 4 more?)

Now you can see more problem, the `Buffer` needs to be moved earlier to work with Android’s struct. There are many preceding pointers. That `Buffer`field seems to be doing “4-consecutive-byte-as-beginning-of-big-memory” strategy, so `->` on it works as dereferencing to the real memory. (regardless of architecture) I think?

That means, this very point `Buffer` is the real start of massive chunk data to follows, and not just a pointer that point to some memory area. So remapping this field is not simple. The serialized machine would have this memory area shifted further than what 32-bit machine want.

And in turn 32-bit machine would have a bit more chunk data space storage. That means 64-bit to 32-bit is safe because it introduces blank data, but if you work on 32-bit machine and then play on 64-bit Android it is not safe, as it could cause data truncate.

So, to make this work we will shift back the entire content of `Buffer` . But not until the end! Since “changed version” and “shared index” must be at the end of the chunk as per Unity’s designed chunk layout. We can guess that is the case by looking at these methods :



![img]()

Fixed chunk size = 16 * 1024 -256 = 16384–256 = 16128

Chunk buffer size = 16128-(88–4) = 16044 + chunk tails based on how many component in its archetype and how many shared types it has. The -4 is probably to remove that “4 more” I don’t know where it came from.

Wait! Isn’t that means chunk buffer size can go over 16128 if it has a lot of components?

It seems the buffer can “overflow” into the heap, and the serialization is already taking care of that in the case that the architecture is the same.

To be clear here’s what we are doing, moving back just the data while not touching the chunk’s tail. Remeber that chunk size is fixed regardless of the variable header size caused by CPU architecture. Ideally, I would like the yellow area to be 0.



![img]()

But knowing that there are even more heap overflow mechanism I don’t understand, this simple cut-up might not work as expected for those remaning memories. **So I think… I will just wait for UT to fix this up.**

Because of all these problems I think it is not wise to use the built-in serializer right now (preview 21), if your project is not running in the same architecture as the machine you are making the game (basically mobile games!, etc.)

However, if you are doing just a quick serialize then deserialize in the same machine then it is fully functional! Even in the same game session, maybe to “hibernate” some big worlds and offload it to the disk temporarily.

#### What’s inside `Buffer` not required a fix as well?

As far as I read the `Buffer` field is used by ECS by casting to `Entity*` , and that `Entity` structure is the same for 32 or 64 bit machine.

#### Can we just make both 32 and 64 bit version?

Then we can `if` on the size of `void*` to check architecture?

The problem is, how can I make a 32-bit file from my 64-bit machine? The C# program containing `Chunk` struct would have to think that it is currently on 32-bit. Unity is all 64-bit nowadays, so I don’t know how to do that. For now I will back off to my own game-specific serialization while keeping an eye on serialization code now that I have (have to) read it.

### Why go through all these horrible problems? Just use BinaryFormatter / JSON / Protobuf / OdinSerializer / FlatBuffer / XML / ???

At some point I thought, I would be better off writing my own game-specific serialization lol

But taking data out of ECS for serialization presents many problems :

-   The tedious deserialize plan : each `Entity` treated as OOP object. When deserialize back, use `EntityManager` to create an entity and attach each component back one by one. Then I need some kind of header saying this entity has which component, which will be translated to `switch case` on my deserializer since ECS is guarded by generic methods. ( `ComponentType` , or `Type` , or assembly qualified `string` of type would not suffice!)
-   `Entity` is not remapped. If I use C# binary serializer to serialize `IComponentData` for example, the `Entity` field inside would be meaningless on deserialize. Need to relink manually later.
-   Theoretically nothing could be faster than what Unity is doing right now.
-   Every time I think if something to increase speed, it always come with other complexity trade off. For example, I considered serializing `NativeArray<T>` which comes from chunk iteration. Then it could be based on only one chunk. However the API to specify which `T` I want is not elegant enough when playing around with chunk iteration. …it is hard to explain, but please try designing such API in your head and you will see why the “dumb chunk read/write” Unity is using actually make sense and not that dumb.
-   `DynamicBuffer` automatically/magically works. This is the memory which can actually go out of a chunk. I have tested that my dynamic buffers was deserialized correctly. (I use them to keep strings as buffers of bytes) This is the gateway to forward compatibility as it allows you to add more later. (In the scope of the same buffer type)
-   No “general purpose” serializer can be written on top of ECS (without reflection hack to use internal methods) because anything related to getting/setting data is “gated by generic”. That is the code must explicitly containing the **type of your game** somewhere. The code I write could never work for your game. Unless I use a scheme like `IJobProcessComponentData` with multiple amount of generics, so you can serialize upto that amount. Still not completely flexible, what if you want to serialize 6 components, 2 shared, and 2 dynamic buffer? The serialization wrapper probably don’t have that exact generic combination waiting.

I agree that chunk writing is the way forward for ECS serialization since it utilize ECS’s special properties that no other general purpose serializer could ever match, but it needs to be much more robust. We are in preview anyways!

### What’s the other solution we have?

My current (temporary) solution is to just use C# `BinaryFormatter` . However, I format a collection of `IComponentData` directly. Put `[Serializable]` on the `struct` then make a wrapper class to serialize them with chunk iteration.

Let’s say I have 1000 entities. All of them has component A and any B or C. I do chunk iteration on component group query all A and any B C. I then made a class `SerializedData` containing `List<(A,B,C,Flag,Sentity)>` which the flag shows availability of component of an entity, left the unrelated component as `default` .

On deserialize back I add component for only those available. This way we might waste a lot of space if only a few entities has C but C space has to be reserved for all, but it is easy enough for a temporary solution.

The stored `Sentity` is just `Entity` with `[Serializable]` on top.. it is because in the `A B C` I have stored `Entity` with relation to other serialized together entities, on deserializing if it knows what itself was then I can remap all `Entity` field manually, like the entity remapping explained earlier. Because `Entity` does not have `[Serializable]` , I put `[NonSerialize]` on them so C# ignores it.

Finally size comparison. C# serializer almost tripled in size from a chunk serialization! (Same encryption and compression)



![img]()

You see you have to got balls and be a masochist if you want to dive into undocumented shiny features like this.



-   [Programming](https://medium.com/tag/programming?source=post)
-   [Unity](https://medium.com/tag/unity?source=post)
-   [Ecs](https://medium.com/tag/ec?source=post)
-   [English Article](https://medium.com/tag/english-articles?source=post)



67 claps



Follow

[![Go to the profile of 5argon](https://cdn-images-1.medium.com/fit/c/60/60/1*IThyiP47QpvVqEsWFclyCQ.png)](https://medium.com/@5argon?source=footer_card)

### [5argon](https://medium.com/@5argon)

เขียนไว้ให้ตัวเองคุ้ยอ่านทีหลัง ไม่ได้แคร์สื่อ 555 | Homepage : [http://www.5argon.info](http://www.5argon.info/) | Game development team Exceed7 Experiments : [http://www.exceed7.com](http://www.exceed7.com/)

Follow

[![อยากทำเกมต้องทรหดอดทน](https://cdn-images-1.medium.com/fit/c/60/60/1*lwYPVhKwFBvicoKoqM3Lrw.png)](https://medium.com/gametorrahod?source=footer_card)

### [อยากทำเกมต้องทรหดอดทน](https://medium.com/gametorrahod?source=footer_card)

แชร์ประสบการณ์และความรู้ใหม่จากชีวิตทรหดของคนทำเกม ฆ่าได้บัคไม่ได้





Related reads

How Voxels Became ‘The Next Big Thing’

[![Go to the profile of 80Level](https://cdn-images-1.medium.com/fit/c/36/36/0*sCIwjZ1-SQBYNnPI.png)](https://medium.com/@EightyLevel)

80Level

[May 27, 2018](https://medium.com/@EightyLevel/how-voxels-became-the-next-big-thing-4eb9665cd13a?source=placement_card_footer_grid---------0-60)



2K









Related reads

Using Cinemachine in 2D for Unity





Jared Halpern

[May 20, 2018](https://medium.com/@jaredehalpern/using-cinemachine-in-2d-for-unity-f35dd394326d?source=placement_card_footer_grid---------1-60)



293









Related reads

Making Games Vs. Making Experiences

[![Go to the profile of Orange&Juicy](https://cdn-images-1.medium.com/fit/c/36/36/1*kintUi4Sd96mWNh_FVNmpA.jpeg)](https://medium.com/@orangeandjuicy)

Orange&Juicy

[Mar 24](https://medium.com/juicy-rants/making-games-vs-making-experiences-b50fa6ae13bf?source=placement_card_footer_grid---------2-60)



268





Responses

![冉晨阳](https://cdn-images-1.medium.com/proxy/1*dmbNkD5D-u45r44go_cf0g.png)

Be the first to write a response…

冉晨阳







[อยากทำเกมต้องทรหดอดทน](https://medium.com/gametorrahod?source=logo-43ddad213cbb)

แชร์ประสบการณ์และความรู้ใหม่จากชีวิตทรหดของคนทำเกม ฆ่าได้บัคไม่ได้