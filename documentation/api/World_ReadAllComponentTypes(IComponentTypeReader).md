#### [DefaultEcs](index.md 'index')
### [DefaultEcs](index.md#DefaultEcs 'DefaultEcs').[World](World.md 'DefaultEcs.World')
## World.ReadAllComponentTypes(IComponentTypeReader) Method
Calls on [reader](World_ReadAllComponentTypes(IComponentTypeReader).md#DefaultEcs_World_ReadAllComponentTypes(DefaultEcs_Serialization_IComponentTypeReader)_reader 'DefaultEcs.World.ReadAllComponentTypes(DefaultEcs.Serialization.IComponentTypeReader).reader') with all the maximum number of component of the current [World](World.md 'DefaultEcs.World').  
This method is primiraly used for serialization purpose and should not be called in game logic.  
```csharp
public void ReadAllComponentTypes(DefaultEcs.Serialization.IComponentTypeReader reader);
```
#### Parameters
<a name='DefaultEcs_World_ReadAllComponentTypes(DefaultEcs_Serialization_IComponentTypeReader)_reader'></a>
`reader` [IComponentTypeReader](IComponentTypeReader.md 'DefaultEcs.Serialization.IComponentTypeReader')  
The [IComponentTypeReader](IComponentTypeReader.md 'DefaultEcs.Serialization.IComponentTypeReader') instance to be used as callback with the current [World](World.md 'DefaultEcs.World') maximum number of component.
  