---
title: "Class SerializableMonoScript<T>"
sidebar_label: "SerializableMonoScript<T>"
description: "Class SerializableMonoScript<T> — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializableMonoScript\<T\> {#Aspid_FastTools_Types_SerializableMonoScript_1}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

[`SerializableMonoScript`](Aspid.FastTools.Types.SerializableMonoScript.md) constrained to types assignable to <code class="typeparamref">T</code>.

```csharp
[Serializable]
public sealed class SerializableMonoScript<T> : SerializableMonoScript, ISerializableType, IEquatable<SerializableTypeBase>, ISerializationCallbackReceiver
```

#### Type Parameters

`T` 

The base type that the Inspector requires when a type is picked or dropped.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md) ← 
[SerializableMonoScript](Aspid.FastTools.Types.SerializableMonoScript.md) ← 
[SerializableMonoScript\<T\>](Aspid.FastTools.Types.SerializableMonoScript-1.md)

#### Implements

[ISerializableType](Aspid.FastTools.Types.ISerializableType.md),
[IEquatable\<SerializableTypeBase\>](https://learn.microsoft.com/dotnet/api/system.iequatable-1),
ISerializationCallbackReceiver


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<SerializableMonoScript\<T\>\>\(SerializableMonoScript\<T\>, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Examples


```csharp
[SerializeField] private SerializableMonoScript<Enemy> _enemyType;
```


## Remarks

Assigned to a field declared as [`SerializableMonoScript`](Aspid.FastTools.Types.SerializableMonoScript.md), it is reloaded without the constraint.

## Properties

### BaseType {#Aspid_FastTools_Types_SerializableMonoScript_1_BaseType}

Gets the base type that types offered by the picker are assignable to; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public override Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

#### Remarks

A loaded type is not checked against it: a name stored before the constraint changed resolves as is.

