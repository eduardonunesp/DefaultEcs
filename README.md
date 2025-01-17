﻿![DefaultEcs](https://github.com/Doraku/DefaultEcs/blob/master/image/DefaultEcsLogo.png)
DefaultEcs is an [Entity Component System](https://en.wikipedia.org/wiki/Entity_component_system) framework which aims to be accessible with little constraints while retaining as much performance as possible for game development.

[![NuGet](https://buildstats.info/nuget/DefaultEcs)](https://www.nuget.org/packages/DefaultEcs)
[![Coverage Status](https://coveralls.io/repos/github/Doraku/DefaultEcs/badge.svg?branch=master)](https://coveralls.io/github/Doraku/DefaultEcs?branch=master)
![continuous integration status](https://github.com/doraku/defaultecs/workflows/continuous%20integration/badge.svg)
[![preview package](https://img.shields.io/badge/preview-package-blue?style=flat&logo=github)](https://github.com/Doraku/DefaultEcs/packages/26448)
[![Join the chat at https://gitter.im/Doraku/DefaultEcs](https://badges.gitter.im/Doraku/DefaultEcs.svg)](https://gitter.im/Doraku/DefaultEcs?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

- [Api documentation](./documentation/api/index.md 'Api documentation')
- [FAQ](./documentation/FAQ.md 'Frequently Asked Questions')
<a/>

- [Requirement](#Requirement)
- [Versioning](#Versioning)
- [Analyzer](#Analyzer)
- [Overview](#Overview)
  - [World](#Overview_World)
  - [Entity](#Overview_Entity)
  - [Component](#Overview_Component)
  - [Resource](#Overview_Resource)
  - [System](#Overview_System)
    - [ISystem](#Overview_System_ISystem)
    - [ActionSystem](#Overview_System_ActionSystem)
    - [SequentialSystem](#Overview_System_SequentialSystem)
    - [EntitySetSystem](#Overview_System_AEntitySystem)
    - [AComponentSystem](#Overview_System_AComponentSystem)
  - [Threading](#Overview_Threading)
    - [IParallelRunnable](#Overview_Threading_IParallelRunnable)
    - [IParallelRunner](#Overview_Threading_IParallelRunner)
    - [DefaultParallelRunner](#Overview_Threading_DefaultParallelRunner)
  - [Command](#Overview_Command)
  - [Message](#Overview_Message)
  - [Serialization](#Overview_Serialization)
    - [TextSerializer](#Overview_Serialization_TextSerializer)
    - [BinarySerializer](#Overview_Serialization_BinarySerializer)
- [Extension](#Extension)
- [Sample](#Sample)
- [Projects using DefaultEcs](#Projects)
- [Performance](#Performance)

<a name='Requirement'></a>
# Requirement
DefaultEcs heavily uses features from C#7.0 and Span from the System.Memory package, compatible from .NETStandard 1.1.  
For development, a C#9.0 compatible environment, net framework 4.8, netcore 3.1 and net5 are required to build and run all tests (it is possible to disable some target in the test project if needed).  
It is possible to use DefaultEcs in Unity (check [FAQ](./documentation/FAQ.md#unity)).

<a name='Versioning'></a>
# Versioning
This is the current strategy used to version DefaultEcs: v0.major.minor
- 0: DefaultEcs is still in heavy development and although a lot of care is given to not break the current api, it can still happen
- major: incremented when there is a breaking change (reset minor number)
- minor: incremented when there is a new feature or a bug fix

<a name='Analyzer'></a>
# Analyzer
To help development with DefaultEcs, there is a roslyn analyzer which provides some warnings against potential bad usages. It can be found [here](https://github.com/Doraku/DefaultEcs.Analyzer).

<a name='Overview'></a>
# Overview
<a name='Overview_World'></a>
## World
The World class act as a manager to create entity, get a selection of specific entities, get a family of component or publish and subscribe to messages that can be used to communicate in a decoupled way between the different elements.  
Multiple World objects can be used in parallel, each instance being thread-safe from one another but operations performed on a single instance and all of its created items should be thought as non thread-safe. Depending on what is done, it is still possible to process operations concurrently to optimise performance.

Worlds are created as such
```csharp
World world = new World();
```

It should be noted that the World class also implement the IDisposable interface.

<a name='Overview_Entity'></a>
## Entity
Entities are simple struct wraping above two Int32, acting as a key to manage components.

Entities are created as such
```csharp
Entity entity = world.CreateEntity();
```

To clear an entity, simply call its Dispose method
```csharp
entity.Dispose();
```

<a name='Overview_Component'></a>
## Component
Components are not restricted by any heritage hierarchy. It is recommanded that component objects only hold data and to be struct to generate as little as possible garbage and to have them contiguous in memory.
```csharp
public struct Example
{
    public float Value;
}
```

To reduce memory, it is possible to set a maximum count for a given component type. If nothing is set, then the maximum entity count of the world will be used.
```csharp
int componentMaxCapacity = 42;
world.SetMaxCapacity<Example>(componentMaxCapacity);
```

It is then possible to add the component to the entity
```csharp
entity.Set(new Example { Value = 42 });
```

It is also possible to share a component between entities without creating a new object
```csharp
entity.SetSameAs<Example>(referenceEntity);
```
If the component is removed from the entity used as reference, it will not remove the component from the other entities using the same component.

To get a component from an entity, simply do the following
```csharp
entity.Get<Example>();
```
Note that the Get method return the component as a ref so you can directly update its value without using the Set method again (but it still need to be set at least once).

<a name='Overview_Resource'></a>
## Resource
Not all components can easily be serialized to be loaded from data file (texture, sound, ...). To help with the handling of those cases, helper types are provided to give a way to load managed resources, shared across entities and even worlds, and automatically dispose them once no entity using them exist anymore.  
To setup a managed resource on an entity, the type `ManagedResource<TInfo, TResource>` need to be set as a component where TInfo is a type used as a single identifier for a single resource and information needed to load it, and TResource is the type of the resource.  
Should multiple resource of the same type be needed on a single entity, it is also possible to set the type `ManagedResource<TInfo[], TResource>` as component.  
If the `ManagedResource` component is removed from the entity or the entity holding it disposed, the internal reference count on the resource will decrease and it will be disposed if zero is reached.  
To actually load the resource, an implementation of the class `AResourceManager<TInfo, TResource>` is need as shown in the next exemple:
```csharp
// TInfo is string, the name of the texture and TResource is Texture2D
public sealed class TextureResourceManager : AResourceManager<string, Texture2D>
{
    private readonly GraphicsDevice _device;
    private readonly ITextureLoader _loader;

    // ITextureLoader is the actual loader, not shown here
    public TextureResourceManager(GraphicsDevice device, ITextureLoader loader)
    {
        _device = device;
        _loader = loader;
    }

    // this will only be called if the texture with the key info has never been loaded yet or it has previously been disposed because it was not used anymore
    protected override Texture2D Load(string info) => _loader.Load(_device, info);

    // this is the callback method where the entity with the ManagedResource<string, Texture2D> component is set, the TInfo and the resource are given do act as needed
    protected override void OnResourceLoaded(in Entity entity, string info, Texture2D resource)
    {
        // here we just set the texture to a field of an other component of the entity which contains all the information needed to draw it (position, size, origin, rotation, texture, ...)
        entity.Get<DrawInfo>().Texture = resource;
    }
}

// we simply set the special component like any other one
entity.Set(new ManagedResource<string, Texture2D>("square.png"));

// or we could set multiple resources like this
entity.Set(new ManagedResource<string[], Texture2D>(new [] { "square.png", "circle.png" }));

// you can also use the helper class
entity.Set(ManagedResource<Texture2D>.Create("square.png")); // set up a ManagedResource<string, Texture2D>
entity.Set(ManagedResource<Texture2D>.Create("square.png", "circle.png")); // set up a ManagedResource<string[], Texture2D>

// this is how to set up a resource manager on a world, it will process all curently existing entities with the special component type, and react to all futur entities also
textureResourceManager.Manage(_world);
```

<a name='Overview_System'></a>
## System
To perform operation, systems should get EntitySet from the World instance. EntitySet are updated as components are added/removed from entities and are used to get a subset of entities with the required component.  
EntitySet are created from EntitySetBuilder and it is possible to apply rules for required components or excluded components
```csharp
// this set when enumerated will give all the entities with an Example component
EntitySet set = world.GetEntities().With<Example>().AsSet();

// this set when enumerated will give all the entities without an Example component
EntitySet set = world.GetEntities().Without<Example>().AsSet();

// this set when enumerated will give all the entities with both an Example and an int component
EntitySet set = world.GetEntities().With<Example>().With<int>().AsSet();

// this set when enumerated will give all the entities with either an Example or an int component
EntitySet set = world.GetEntities().WithEither<Example>().Or<int>().AsSet();

// this gives all the component of type Example currently used in the world
Span<Example> components = world.Get<Example>();
```

There is also some special rules which will make the EntitySet react to some events
```csharp
// this set when enumerated will give all the entities on which an Example component has been added for the first time
EntitySet set = world.GetEntities().WhenAdded<Example>().AsSet();

// this set when enumerated will give all the entities on which the Example component has been explicitly changed with Entity.Set<Example> method
EntitySet set = world.GetEntities().WhenChanged<Example>().AsSet();

// this set when enumerated will give all the entities on which the Example component has been removed
EntitySet set = world.GetEntities().WhenRemoved<Example>().AsSet();

// this set when enumerated will give all the entities on which the Example component has been added or changed
EntitySet set = world.GetEntities().WhenAdded<Example>().WhenChanged<Example>().AsSet();

// this set when enumerated will give all the entities with an int component on which the Example component has been changed, the order is important
EntitySet set = world.GetEntities().With<int>().WhenChanged<Example>().AsSet();
```

Note that if such a rule is used, the method `Complete` of the EntitySet needs to be called once every Entity has been processed to clear the EntitySet of its content.  
Calling this method on an EntitySet created with only static filtering will do nothing.

Although there is no obligation, a set of base classes are provided to help the creation of systems:
<a name='Overview_System_ISystem'></a>
### ISystem<T>
This is a base interface for all the systems. it exposes an `Update` method and an `IsEnabled` property. In all derived types provided in DefaultEcs, the responsibility to check this property is handled by the callee, not the caller. It is set to true by default.

<a name='Overview_System_ActionSystem'></a>
### ActionSystem<T>
This class is used to quickly make a system with a given custom action to be called on every update.
```csharp
private void Exit(float elaspedTime)
{
    if (EscapedIsPressed)
    {
        // escape code
    }
}

...

ISystem<float> system = new ActionSystem<float>(Exit);

...

// this will call the Exit method as a system
system.Update(elapsedTime);
```

<a name='Overview_System_SequentialSystem'></a>
### SequentialSystem
This class is used to easily create a list of system to be updated in a sequential order.
```csharp
ISystem<float> system = new SequentialSystem<float>(
        new InputSystem(),
        new AISystem(),
        new PositionSystem(),
        new DrawSystem()
    );
...

// this will call in order InputSystem, AISystem, PositionSystem and DrawSystem
system.Update(elaspedTime);
```

<a name='Overview_System_AEntitySetSystem'></a>
### AEntitySetSystem<T>
This is a base class to create system to update a given EntitySet.
```csharp
public sealed class VelocitySystem : AEntitySetSystem<float>
{
    public VelocitySystem(World world, IParallelRunner runner)
        : base(world.GetEntities().With<Velocity>().With<Position>().AsSet(), runner)
    {
    }

    protected override void Update(float elaspedTime, in Entity entity)
    {
        ref Velocity velocity = ref entity.Get<Velocity>();
        ref Position position = ref entity.Get<Position>();

        Vector2 offset = velocity.Value * elaspedTime;

        position.Value.X += offset.X;
        position.Value.Y += offset.Y;
    }
}
```

It is also possible to declare the needed component by using the WithAttribute and WithoutAttribute on the system type.
```csharp
[With(typeof(Velocity)]
[With(typeof(Position)]
public sealed class VelocitySystem : AEntitySetSystem<float>
{
    public VelocitySystem(World world, IParallelRunner runner)
        : base(world, runner)
    {
    }

    protected override void Update(float elaspedTime, in Entity entity)
    {
        ref Velocity velocity = ref entity.Get<Velocity>();
        ref Position position = ref entity.Get<Position>();

        Vector2 offset = velocity.Value * elaspedTime;

        position.Value.X += offset.X;
        position.Value.Y += offset.Y;
    }
}
```

<a name='Overview_System_AComponentSystem'></a>
### AComponentSystem<TState, TComponent>
This is a base class to create system to update a specific component type from a given World.
```csharp
public class DrawSystem : AComponentSystem<float, DrawInfo>
{
    private readonly SpriteBatch _batch;
    private readonly Texture2D _square;

    public DrawSystem(SpriteBatch batch, Texture2D square, World world)
        : base(world)
    {
        _batch = batch;
        _square = square;
    }

    protected override void PreUpdate()
    {
        _batch.Begin();
    }

    protected override void Update(float elaspedTime, ref DrawInfo component)
    {
        _batch.Draw(_square, component.Destination, component.Color);
    }

    protected override void PostUpdate()
    {
        _batch.End();
    }
}
```

<a name='Overview_Threading'></a>
## Threading
Some systems are compatible with multithreading execution: ParallelSystem, AEntitySetSystem and AComponentSystem. This is done by passing a IParallelRunner to their respective constructor.
```csharp
IParallelRunner runner = new DefaultParallelRunner(Environment.ProcessorCount);

ISystem<float> system = new VelocitySystem(world, runner);

// this will process the update on Environment.ProcessorCount threads
system.Update(elaspedTime);
```
It is safe to run a system with multithreading when:
* for an AEntitySetSystem
  * each entity can be safely updated separately with no dependency to an other entity
  * there is no new Set, Remove or Dispose action on entity (only read or update)
* for an AComponentSystem
  * each component can be safely updated separately with no dependency to an other component

<a name='Overview_Threading_IParallelRunnable'></a>
### IParallelRunnable
This interface allow the creation of custom parallelisable process by an IParallelRunner.
```csharp
IParallelRunner runner = new DefaultParallelRunner(Environment.ProcessorCount);

public class CustomRunnable : IParallelRunnable
{
    public void Run(int index, int maxIndex)
    {
        // a runnable is separated in (maxIndex + 1) part to run in parallel, index gives you the part running
    }
}

runner.Run(new CustomRunnable());
```

<a name='Overview_Threading_IParallelRunner'></a>
### IParallelRunner
This interface allow the creation of custom parallel execution.
```csharp
public class TaskRunner : IParallelRunner
{
    int DegreeOfParallelism { get; }

    public void Run(IParallelRunnable runnable) Enumerable.Range(0, DegreeOfParallelism).AsParallel().ForAll(i => runnable.Run(i, DegreeOfParallelism));
}
```

<a name='Overview_Threading_DefaultParallelRunner'></a>
### DefaultParallelRunner
This is the default implementation of IParallelRunner. it uses exclusive Task to run an IParallelRunnable and only return when the full runnable has been processed. When `index == maxIndex` this is the calling thread.  
It is safe to reuse the same DefaultParallelRunner in multiple system but it should not be used in parallel itself.
```csharp
IParallelRunner runner = new DefaultParallelRunner(Environment.ProcessorCount);

// wrong
ISystem<float> system = new ParallelSystem<float>(runner,
    new ParallelSystem1(runner),
    new ParallelSystem2(runner));
    
// ok
ISystem<float> system = new SequentialSystem<float>(
    new ParallelSystem1(runner),
    new ParallelSystem2(runner));
```

<a name='Overview_Command'></a>
## Command
Since it is not possible to make structural modification on an Entity in a multithreading  context, the EntityCommandRecorder type is provided to adress this short-coming.  
It is possible de record command on entities in a thread-safe way to later execute them when those structural modifications are safe to do.
```csharp
// This creates an expandable recorder with a default capacity of 1Ko
EntityCommandRecorder recorder = new EntityCommandRecorder();

// This creates a fixed capacity recorder of .5Ko
EntityCommandRecorder recorder = new EntityCommandRecorder(512);

// This creates an expandable recorder with a default capacity of .5Ko which can have a maximum capacity of 2Ko
EntityCommandRecorder recorder = new EntityCommandRecorder(512, 2048);
```

Note that a fixed capacity EntityCommandRecorder (or one which has expanded to its max capacity) has better performance.  
When needed, an expandable EntityCommandRecorder will double its capacity so it is preferred to use a power of 2 as default capacity.

```csharp
// Create a new Entity defered and give an EntityRecord to record commands on it
EntityRecord newRecord = recorder.CreateEntity();

// Register an Entity and give an EntityRecord to record commands on it
EntityRecord record = recorder.Record(entity);

// EntityRecord has the same API as Entity so all action expected are available to record as command this way
newRecord.Set<bool>(true);

// To execute all recorded commands
recorder.Execute(world);
```

<a name='Overview_Message'></a>
## Message
It is possible to send and receive message transiting in a World.
```csharp
void On(in bool message) { }

// the method On will be called back every time a bool object is published
// it is possible to use any type
world.Subscribe<bool>(On);

world.Publish(true);
```

It is also possible to subscribe to multiple method of an instance by using the SubscribeAttribute:
```csharp
public class Dummy
{
    [Subscribe]
    void On(in bool message) { }
	
    [Subscribe]
    void On(in int message) { }
	
    void On(in string message) { }
}

Dummy dummy = new Dummy();

// this will subscribe the decorated methods only
world.Subscribe(dummy);

// the dummy bool method will be called
world.Publish(true);

// but not the string one as it dit not have the SubscribeAttribute
world.Publish(string.Empty);
```

Note that the Subscribe method return an IDisposable object acting as a subscription. To unsubscribe, simply dispose this object.

<a name='Overview_Serialization'></a>
## Serialization
DefaultEcs support serialization to save and load a World state. Two implementations are provided which are equals in feature and it is possible to create a custom serialization engine using the framework of your choice by implementing a set of interfaces.

- ISerializer is the base interface
- IComponentTypeReader is used to get the settings of the serialized World in case a component max capacity has been set for a specific type different from the world max capacity
- IComponentReader is used to get all the components of an Entity

The provided implementation TextSerializer and BinarySerializer are highly permissive and will serialize every fields and properties even if the are private or readonly and do not require any attribute decoration to work.  
This was a target from the get go as graphic and framework libraries do not always have well decorated type which would be used as component.  
Although the lowest target is netstandard1.1, please be aware that the capability of both implementation to handle type with no default constructor maybe not work if the version of your .NET plateform is too low. Other known limitations are:
- do not handle multidimensional arrays
- do not handle cyclic object graph (all objects are copied, thus creating an infinit loop)
- not compatible with Xamarin.iOS, AOT platforms (use System.Reflection.Emit namespace)


```csharp
ISerializer serializer = new TextSerializer();

using (Stream stream = File.Create(filePath))
{
    serializer.Serialize(stream, world);
}

using (Stream stream = File.OpenRead(filePath))
{
    World worldCopy = serializer.Deserialize(stream);
}
```
Each implementation has its own serialization context which can be used to transform a given type to something else or just change the value at serialization and deserialization time.
```csharp
using BinarySerializationContext context = new BinarySerializationContext()
    .Marshal<string, string>(_ => null) // set every string as null during serialization
    .Marshal<NonSerializableData, SerializableData>(d => new SerializableData(d)) // transform non serializable data to a serializable type during serialization
    .Unmarshal<SerializableData, NonSerializableData>(d => Load(d)); // reload non serializable data from serializable data during deserialization

BinarySerializer serializer = new BinarySerializer(context);
```

<a name='Overview_Serialization_TextSerializer'></a>
### TextSerializer
The purpose of this serializer is to provide a readable save format which can be edited by hand.
```
// declare the maximum number of entity in the World, this must be before any Entity or ComponentMaxCapacity line
WorldMaxCapacity 42

// this line is used to define an alias for a type used as component inside the world and must be declared before being used
ComponentType Int32 System.Int32, System.Private.CoreLib

// this line is used to set the max capacity for the given type, in case it is different from the world max capacity
ComponentMaxCapacity Int32 13

// this create a new entity with the id "Foo"
Entity Foo

// this line set the component of the type with the alias Int32 on the previously created Entity to the value 13
Component Int32 13

// let's say we have the type defined as such already declared with the alias Test
// struct Test
// {
//     int Hello
//     int World
// }
ComponentType Test MyNamespace.Text, MyLib

// composite objects are setted like this
Component Test {
	Hello 666
	// this line is ignored since the type does not have a member with the name Wow
	// also the World member will have its default value since not present
	Wow 42
}

// this create a new entity with no id
Entity

// this sets the component of the type with the alias Test of the previously created Entity as the same as the one of the Entity with the id Foo
ComponentSameAs Test Foo
```
<a name='Overview_Serialization_BinarySerializer'></a>
### BinarySerializer
This serializer is optimized for speed and file space.

<a name='Extension'></a>
# Extension
A DefaultEcs.Extension project is present to show how other features can be built upon the base framework. Those features are just provided as example and are not part of DefaultEcs because the implementation is not generic nor satisfactory enough.

<a name='Sample'></a>
# Sample
Some sample projects are available to give a better picture on how to use DefaultEcs. Those exemples were done relatively fast so they are probably not the best representation of the Entity Component System framework application.

[DefaultBoids](https://github.com/Doraku/DefaultEcs/tree/master/source/Sample/DefaultBoids)

[![DefaultBoids](https://img.youtube.com/vi/yEdcqOTCteY/0.jpg)](https://youtu.be/yEdcqOTCteY)

A really simple implementation of a [boids simulation](https://en.wikipedia.org/wiki/Boids), here displaying 30k boids with an old Intel Core i5-3570K CPU 3.40GHz at ~70fps.

[DefaultBrick](https://github.com/Doraku/DefaultEcs/tree/master/source/Sample/DefaultBrick)
[win10-x64](https://github.com/Doraku/DefaultEcs/releases/download/v0.9.0/DefaultBrick_win10-x64.zip)

![](https://github.com/Doraku/DefaultEcs/raw/master/image/defaultbrick.gif)

Basic breakout clone. The collision is buggy! As said not much time was spent debuging those. Ball moves faster as the more bricks you destroy and reset to default speed if lost. The stage reload once completed.

[DefaultSlap](https://github.com/Doraku/DefaultEcs/tree/master/source/Sample/DefaultSlap)
[win10-x64](https://github.com/Doraku/DefaultEcs/releases/download/v0.9.0/DefaultSlap_win10-x64.zip)

![](https://github.com/Doraku/DefaultEcs/raw/master/image/defaultslap.gif)

Basic fly swatter clone. Every five seconds, flies (blue square) will damage the player (up to 3 times until the "game" resets) and new ones will spawn.

<a name='Projects'></a>
# Projects using DefaultEcs
Your game uses DefaultEcs? Don't hesitate to contact me.  

[![Chambers of Anubis](https://img.itch.zone/aW1nLzQ2MDYzODcucG5n/original/IALw4S.png)](https://github.com/PodeCaradox/HellowIInJam)

<a name='Performance'></a>
# Performance
Feel free to correct my use of the compared ecs libraries as I looked only for basic uses which may not be the most performant way.

[Entitas](https://github.com/sschmid/Entitas-CSharp)  
[MonoGameExtendedEntities](https://github.com/craftworkgames/MonoGame.Extended)  
[Leo](https://github.com/Leopotam/ecs)  
[Svelto](https://github.com/sebas77/Svelto.ECS)  

```
BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i5-3570K CPU 3.40GHz (Ivy Bridge), 1 CPU, 4 logical and 4 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4075.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4075.0), X64 RyuJIT
```

SingleComponentEntityEnumeration: add one to the basic component (containing one int) of 100000 entities

|                                Method |        Mean |     Error |    StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------------------------- |------------:|----------:|----------:|------:|------:|------:|----------:|
|                  DefaultEcs_EntitySet |   291.62 μs |  0.059 μs |  0.049 μs |     - |     - |     - |         - |
|                     DefaultEcs_System |   291.14 μs |  0.148 μs |  0.124 μs |     - |     - |     - |         - |
|                DefaultEcs_MultiSystem |    79.77 μs |  0.578 μs |  0.512 μs |     - |     - |     - |         - |
|      DefaultEcs_EntityComponentSystem |   190.19 μs |  0.207 μs |  0.194 μs |     - |     - |     - |         - |
| DefaultEcs_MultiEntityComponentSystem |    53.95 μs |  0.640 μs |  0.534 μs |     - |     - |     - |         - |
|                  DefaultEcs_Component |    95.17 μs |  0.055 μs |  0.046 μs |     - |     - |     - |         - |
|            DefaultEcs_ComponentSystem |    83.96 μs |  0.062 μs |  0.055 μs |     - |     - |     - |         - |
|       DefaultEcs_ComponentMultiSystem |    27.40 μs |  0.153 μs |  0.119 μs |     - |     - |     - |         - |
|            DefaultEcs_GeneratorSystem |   189.30 μs |  0.038 μs |  0.030 μs |     - |     - |     - |         - |
|       DefaultEcs_GeneratorMultiSystem |    54.03 μs |  0.618 μs |  0.516 μs |     - |     - |     - |         - |
|                        Entitas_System | 6,718.45 μs | 27.601 μs | 24.467 μs |     - |     - |     - |     128 B |
|                   Entitas_MultiSystem | 3,729.49 μs | 39.063 μs | 34.628 μs |     - |     - |     - |     478 B |
|       MonoGameExtendedEntities_System | 2,206.19 μs | 21.454 μs | 19.018 μs |     - |     - |     - |     192 B |
|                            Leo_System |   145.16 μs |  0.012 μs |  0.010 μs |     - |     - |     - |         - |
|                       Leo_MultiSystem |    73.94 μs |  0.217 μs |  0.203 μs |     - |     - |     - |         - |
|                         Svelto_System |    83.93 μs |  0.036 μs |  0.030 μs |     - |     - |     - |         - |

DoubleComponentEntityEnumeration: do basic movement with two components (position, speed) on 100000 entities

|                          Method |       Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------------------- |-----------:|---------:|---------:|------:|------:|------:|----------:|
|            DefaultEcs_EntitySet |   608.2 μs |  0.98 μs |  0.92 μs |     - |     - |     - |         - |
|               DefaultEcs_System |   594.7 μs |  0.38 μs |  0.31 μs |     - |     - |     - |         - |
|          DefaultEcs_MultiSystem |   161.8 μs |  2.45 μs |  2.04 μs |     - |     - |     - |         - |
|      DefaultEcs_ComponentSystem |   402.5 μs |  0.55 μs |  0.52 μs |     - |     - |     - |         - |
| DefaultEcs_ComponentMultiSystem |   109.7 μs |  0.35 μs |  0.31 μs |     - |     - |     - |         - |
|      DefaultEcs_GeneratorSystem |   402.5 μs |  0.40 μs |  0.37 μs |     - |     - |     - |         - |
| DefaultEcs_GeneratorMultiSystem |   109.7 μs |  0.32 μs |  0.27 μs |     - |     - |     - |         - |
|                  Entitas_System | 6,390.6 μs | 13.91 μs | 12.33 μs |     - |     - |     - |     128 B |
|             Entitas_MultiSystem | 3,972.4 μs | 22.66 μs | 20.09 μs |     - |     - |     - |     474 B |
| MonoGameExtendedEntities_System | 3,203.4 μs | 11.93 μs | 11.16 μs |     - |     - |     - |     192 B |
|                      Leo_System |   481.3 μs |  0.86 μs |  0.80 μs |     - |     - |     - |         - |
|                 Leo_MultiSystem |   152.8 μs |  0.18 μs |  0.16 μs |     - |     - |     - |         - |
|                   Svelto_System |   155.3 μs |  0.03 μs |  0.03 μs |     - |     - |     - |         - |

TripleComponentEntityEnumeration: do basic operation with three components on 100000 entities

|                          Method |       Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------------------- |-----------:|---------:|---------:|------:|------:|------:|----------:|
|            DefaultEcs_EntitySet |   661.0 μs |  0.30 μs |  0.23 μs |     - |     - |     - |         - |
|               DefaultEcs_System |   643.7 μs |  1.20 μs |  1.12 μs |     - |     - |     - |         - |
|          DefaultEcs_MultiSystem |   173.5 μs |  0.80 μs |  0.75 μs |     - |     - |     - |         - |
|      DefaultEcs_ComponentSystem |   356.8 μs |  1.19 μs |  1.06 μs |     - |     - |     - |         - |
| DefaultEcs_ComponentMultiSystem |   106.6 μs |  0.58 μs |  0.54 μs |     - |     - |     - |         - |
|      DefaultEcs_GeneratorSystem |   384.5 μs |  0.42 μs |  0.40 μs |     - |     - |     - |         - |
| DefaultEcs_GeneratorMultiSystem |   119.1 μs |  1.43 μs |  1.34 μs |     - |     - |     - |         - |
|                  Entitas_System | 6,171.5 μs | 13.61 μs | 12.73 μs |     - |     - |     - |     128 B |
|             Entitas_MultiSystem | 3,995.7 μs | 36.20 μs | 33.86 μs |     - |     - |     - |     487 B |
| MonoGameExtendedEntities_System | 3,569.5 μs |  6.88 μs |  6.44 μs |     - |     - |     - |     186 B |
|                      Leo_System |   838.2 μs |  0.48 μs |  0.45 μs |     - |     - |     - |         - |
|                 Leo_MultiSystem |   289.9 μs |  0.67 μs |  0.60 μs |     - |     - |     - |         - |
|                   Svelto_System |   130.1 μs |  0.13 μs |  0.12 μs |     - |     - |     - |         - |