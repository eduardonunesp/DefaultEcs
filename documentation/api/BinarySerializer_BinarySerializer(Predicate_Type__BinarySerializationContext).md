#### [DefaultEcs](index.md 'index')
### [DefaultEcs.Serialization](index.md#DefaultEcs_Serialization 'DefaultEcs.Serialization').[BinarySerializer](BinarySerializer.md 'DefaultEcs.Serialization.BinarySerializer')
## BinarySerializer.BinarySerializer(Predicate&lt;Type&gt;, BinarySerializationContext) Constructor
Initializes a new instance of the [BinarySerializer](BinarySerializer.md 'DefaultEcs.Serialization.BinarySerializer') class.  
```csharp
public BinarySerializer(System.Predicate<System.Type> componentFilter, DefaultEcs.Serialization.BinarySerializationContext context);
```
#### Parameters
<a name='DefaultEcs_Serialization_BinarySerializer_BinarySerializer(System_Predicate_System_Type__DefaultEcs_Serialization_BinarySerializationContext)_componentFilter'></a>
`componentFilter` [System.Predicate&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Predicate-1 'System.Predicate`1')[System.Type](https://docs.microsoft.com/en-us/dotnet/api/System.Type 'System.Type')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Predicate-1 'System.Predicate`1')  
A filter used to check wether a component type should be serialized/deserialized or not. A [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null') value means everything is taken.
  
<a name='DefaultEcs_Serialization_BinarySerializer_BinarySerializer(System_Predicate_System_Type__DefaultEcs_Serialization_BinarySerializationContext)_context'></a>
`context` [BinarySerializationContext](BinarySerializationContext.md 'DefaultEcs.Serialization.BinarySerializationContext')  
The [BinarySerializationContext](BinarySerializationContext.md 'DefaultEcs.Serialization.BinarySerializationContext') used to convert type during serialization/deserialization.
  
