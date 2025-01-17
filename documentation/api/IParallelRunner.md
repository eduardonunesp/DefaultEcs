#### [DefaultEcs](index.md 'index')
### [DefaultEcs.Threading](index.md#DefaultEcs_Threading 'DefaultEcs.Threading')
## IParallelRunner Interface
Exposes a method to run in parallel a [IParallelRunnable](IParallelRunnable.md 'DefaultEcs.Threading.IParallelRunnable').  
```csharp
public interface IParallelRunner :
System.IDisposable
```

Derived  
&#8627; [DefaultParallelRunner](DefaultParallelRunner.md 'DefaultEcs.Threading.DefaultParallelRunner')  

Implements [System.IDisposable](https://docs.microsoft.com/en-us/dotnet/api/System.IDisposable 'System.IDisposable')  
### Properties

***
[DegreeOfParallelism](IParallelRunner_DegreeOfParallelism.md 'DefaultEcs.Threading.IParallelRunner.DegreeOfParallelism')

Gets the degree of parallelism used to run an [IParallelRunnable](IParallelRunnable.md 'DefaultEcs.Threading.IParallelRunnable').  
### Methods

***
[Run(IParallelRunnable)](IParallelRunner_Run(IParallelRunnable).md 'DefaultEcs.Threading.IParallelRunner.Run(DefaultEcs.Threading.IParallelRunnable)')

Runs the provided [IParallelRunnable](IParallelRunnable.md 'DefaultEcs.Threading.IParallelRunnable').  
