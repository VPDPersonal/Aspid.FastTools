---
title: "Class SerializableType<T>"
sidebar_label: "SerializableType<T>"
description: "Class SerializableType<T> — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializableType\<T\> {#Aspid_FastTools_Types_SerializableType_1}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

[`SerializableType`](Aspid.FastTools.Types.SerializableType.md) constrained to types assignable to <code class="typeparamref">T</code>.

```csharp
[Serializable]
public sealed class SerializableType<T> : SerializableType, ISerializableType, ISerializationCallbackReceiver
```

#### Type Parameters

`T` 

Base constraint type; the picker offers only types assignable to it.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md) ← 
[SerializableType](Aspid.FastTools.Types.SerializableType.md) ← 
[SerializableType\<T\>](Aspid.FastTools.Types.SerializableType-1.md)

#### Implements

[ISerializableType](Aspid.FastTools.Types.ISerializableType.md),
ISerializationCallbackReceiver


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<SerializableType\<T\>\>\(SerializableType\<T\>, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Examples


```csharp
public class MyComponent : MonoBehaviour
{
    [SerializeField] private SerializableType<MonoBehaviour> _behaviorType;

    private void Start()
    {
        Type type = _behaviorType;
        if (type != null)
            gameObject.AddComponent(type);
    }
}
```


## Remarks

Unity serializes a field by its declared type, so a [`SerializableType<T>`](Aspid.FastTools.Types.SerializableType-1.md) assigned from code to a
field declared as [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) is reloaded unconstrained: the type survives, the constraint
does not. Only the picker and the constructor check <code class="typeparamref">T</code>: a loaded name is not re-checked,
so after <code class="typeparamref">T</code> or the stored class's base changes, the type may no longer be assignable to it.

## Constructors

### SerializableType\(Type?\) {#Aspid_FastTools_Types_SerializableType_1__ctor_System_Type_}

Creates a wrapper holding <code class="paramref">type</code>.

```csharp
public SerializableType(Type? type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)?

The type to store, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for an empty wrapper.

#### Exceptions

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

<code class="paramref">type</code> is not assignable to <code class="typeparamref">T</code>.

## Properties

### BaseType {#Aspid_FastTools_Types_SerializableType_1_BaseType}

Gets the base type that types offered by the picker are assignable to; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public override Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

#### Remarks

A loaded type is not checked against it: a name stored before the constraint changed resolves as is.

