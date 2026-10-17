---
title: "Class SerializableMonoScript"
sidebar_label: "SerializableMonoScript"
description: "Class SerializableMonoScript — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializableMonoScript {#Aspid_FastTools_Types_SerializableMonoScript}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Unity-serializable wrapper around a [`Type`](https://learn.microsoft.com/dotnet/api/system.type) referencing it through its <code>MonoScript</code>
asset, so renaming or moving the class does not break the field.

```csharp
[Serializable]
public class SerializableMonoScript : SerializableTypeBase, ISerializableType, IEquatable<SerializableTypeBase>, ISerializationCallbackReceiver
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md) ← 
[SerializableMonoScript](Aspid.FastTools.Types.SerializableMonoScript.md)

#### Derived

[SerializableMonoScript\<T\>](Aspid.FastTools.Types.SerializableMonoScript-1.md)

#### Implements

[ISerializableType](Aspid.FastTools.Types.ISerializableType.md),
[IEquatable\<SerializableTypeBase\>](https://learn.microsoft.com/dotnet/api/system.iequatable-1),
ISerializationCallbackReceiver


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<SerializableMonoScript\>\(SerializableMonoScript, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Examples


```csharp
[SerializeField] private SerializableMonoScript _componentType;

Type type = _componentType; // implicit conversion
```


## Remarks

A script asset exists only for a top-level, non-generic class in a file of the same name, or for a
[`MonoBehaviour`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.html) or [`ScriptableObject`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/ScriptableObject.html) in a DLL, matched by
namespace and name; use [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) for other types. A player keeps only the name and
resolves it as [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) does.

## Properties

### BaseType {#Aspid_FastTools_Types_SerializableMonoScript_BaseType}

Gets the base type that types offered by the picker are assignable to; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public override Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

#### Remarks

A loaded type is not checked against it: a name stored before the constraint changed resolves as is.

## Operators

### implicit operator Type?\(SerializableMonoScript?\) {#Aspid_FastTools_Types_SerializableMonoScript_op_Implicit_Aspid_FastTools_Types_SerializableMonoScript__System_Type}

Converts the wrapper to the type it holds.

```csharp
public static implicit operator Type?(SerializableMonoScript? type)
```

#### Parameters

`type` [SerializableMonoScript](Aspid.FastTools.Types.SerializableMonoScript.md)?

The wrapper to convert.

#### Returns

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

The wrapped type, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when the wrapper is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or holds no
resolvable type.

