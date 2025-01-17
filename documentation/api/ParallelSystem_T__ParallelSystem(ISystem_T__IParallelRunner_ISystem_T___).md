#### [DefaultEcs](index.md 'index')
### [DefaultEcs.System](index.md#DefaultEcs_System 'DefaultEcs.System').[ParallelSystem&lt;T&gt;](ParallelSystem_T_.md 'DefaultEcs.System.ParallelSystem&lt;T&gt;')
## ParallelSystem&lt;T&gt;.ParallelSystem(ISystem&lt;T&gt;, IParallelRunner, ISystem&lt;T&gt;[]) Constructor
Initialises a new instance of the [ParallelSystem&lt;T&gt;](ParallelSystem_T_.md 'DefaultEcs.System.ParallelSystem&lt;T&gt;') class.  
```csharp
public ParallelSystem(DefaultEcs.System.ISystem<T> mainSystem, DefaultEcs.Threading.IParallelRunner runner, params DefaultEcs.System.ISystem<T>[] systems);
```
#### Parameters
<a name='DefaultEcs_System_ParallelSystem_T__ParallelSystem(DefaultEcs_System_ISystem_T__DefaultEcs_Threading_IParallelRunner_DefaultEcs_System_ISystem_T___)_mainSystem'></a>
`mainSystem` [DefaultEcs.System.ISystem&lt;](ISystem_T_.md 'DefaultEcs.System.ISystem&lt;T&gt;')[T](ParallelSystem_T_.md#DefaultEcs_System_ParallelSystem_T__T 'DefaultEcs.System.ParallelSystem&lt;T&gt;.T')[&gt;](ISystem_T_.md 'DefaultEcs.System.ISystem&lt;T&gt;')  
The [ISystem&lt;T&gt;](ISystem_T_.md 'DefaultEcs.System.ISystem&lt;T&gt;') instance to be updated on the calling thread.
  
<a name='DefaultEcs_System_ParallelSystem_T__ParallelSystem(DefaultEcs_System_ISystem_T__DefaultEcs_Threading_IParallelRunner_DefaultEcs_System_ISystem_T___)_runner'></a>
`runner` [IParallelRunner](IParallelRunner.md 'DefaultEcs.Threading.IParallelRunner')  
The [IParallelRunner](IParallelRunner.md 'DefaultEcs.Threading.IParallelRunner') used to process the update in parallel if not null.
  
<a name='DefaultEcs_System_ParallelSystem_T__ParallelSystem(DefaultEcs_System_ISystem_T__DefaultEcs_Threading_IParallelRunner_DefaultEcs_System_ISystem_T___)_systems'></a>
`systems` [DefaultEcs.System.ISystem&lt;](ISystem_T_.md 'DefaultEcs.System.ISystem&lt;T&gt;')[T](ParallelSystem_T_.md#DefaultEcs_System_ParallelSystem_T__T 'DefaultEcs.System.ParallelSystem&lt;T&gt;.T')[&gt;](ISystem_T_.md 'DefaultEcs.System.ISystem&lt;T&gt;')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')  
The [ISystem&lt;T&gt;](ISystem_T_.md 'DefaultEcs.System.ISystem&lt;T&gt;') instances.
  
#### Exceptions
[System.ArgumentNullException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentNullException 'System.ArgumentNullException')  
[runner](ParallelSystem_T__ParallelSystem(ISystem_T__IParallelRunner_ISystem_T___).md#DefaultEcs_System_ParallelSystem_T__ParallelSystem(DefaultEcs_System_ISystem_T__DefaultEcs_Threading_IParallelRunner_DefaultEcs_System_ISystem_T___)_runner 'DefaultEcs.System.ParallelSystem&lt;T&gt;.ParallelSystem(DefaultEcs.System.ISystem&lt;T&gt;, DefaultEcs.Threading.IParallelRunner, DefaultEcs.System.ISystem&lt;T&gt;[]).runner') is null.
[System.ArgumentNullException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentNullException 'System.ArgumentNullException')  
[systems](ParallelSystem_T__ParallelSystem(ISystem_T__IParallelRunner_ISystem_T___).md#DefaultEcs_System_ParallelSystem_T__ParallelSystem(DefaultEcs_System_ISystem_T__DefaultEcs_Threading_IParallelRunner_DefaultEcs_System_ISystem_T___)_systems 'DefaultEcs.System.ParallelSystem&lt;T&gt;.ParallelSystem(DefaultEcs.System.ISystem&lt;T&gt;, DefaultEcs.Threading.IParallelRunner, DefaultEcs.System.ISystem&lt;T&gt;[]).systems') is null.
