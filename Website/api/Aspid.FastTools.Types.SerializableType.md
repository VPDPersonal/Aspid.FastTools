---
title: "Class SerializableType"
sidebar_label: "SerializableType"
description: "Class SerializableType — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializableType {#Aspid_FastTools_Types_SerializableType}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Unity-serializable wrapper around a [`Type`](https://learn.microsoft.com/dotnet/api/system.type), stored by its <code>AssemblyQualifiedName</code>
and resolved lazily on first access.

```csharp
[Serializable]
public class SerializableType : SerializableTypeBase, ISerializableType, ISerializationCallbackReceiver
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md) ← 
[SerializableType](Aspid.FastTools.Types.SerializableType.md)

#### Derived

[SerializableType\<T\>](Aspid.FastTools.Types.SerializableType-1.md)

#### Implements

[ISerializableType](Aspid.FastTools.Types.ISerializableType.md),
ISerializationCallbackReceiver


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<SerializableType\>\(SerializableType, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Examples


```csharp
public class MyComponent : MonoBehaviour
{
    [SerializeField] private SerializableType _targetType;

    private void Start()
    {
        Type type = _targetType;  // implicit conversion
        if (type != null)
            Debug.Log(type.FullName);
    }
}
```


## Constructors

### SerializableType\(Type?\) {#Aspid_FastTools_Types_SerializableType__ctor_System_Type_}

Creates a wrapper holding <code class="paramref">type</code>.

```csharp
public SerializableType(Type? type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)?

The type to store, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for an empty wrapper.

## Properties

### BaseType {#Aspid_FastTools_Types_SerializableType_BaseType}

Gets the base type that types offered by the picker are assignable to; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public override Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

#### Remarks

A loaded type is not checked against it: a name stored before the constraint changed resolves as is.

## Operators

### implicit operator Type?\(SerializableType?\) {#Aspid_FastTools_Types_SerializableType_op_Implicit_Aspid_FastTools_Types_SerializableType__System_Type}

Converts the wrapper to the type it holds.

```csharp
public static implicit operator Type?(SerializableType? type)
```

#### Parameters

`type` [SerializableType](Aspid.FastTools.Types.SerializableType.md)?

The wrapper to convert.

#### Returns

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

The wrapped type, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when the wrapper is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or holds no
resolvable type.

