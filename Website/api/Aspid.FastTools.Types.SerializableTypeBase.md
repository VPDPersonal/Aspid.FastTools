---
title: "Class SerializableTypeBase"
sidebar_label: "SerializableTypeBase"
description: "Class SerializableTypeBase — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializableTypeBase {#Aspid_FastTools_Types_SerializableTypeBase}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Shared implementation of the serializable [`Type`](https://learn.microsoft.com/dotnet/api/system.type) wrappers: stores the type by its
assembly-qualified name and resolves it lazily on first access.

```csharp
[Serializable]
public abstract class SerializableTypeBase : ISerializableType, ISerializationCallbackReceiver
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md)

#### Derived

[SerializableMonoScript](Aspid.FastTools.Types.SerializableMonoScript.md),
[SerializableType](Aspid.FastTools.Types.SerializableType.md)

#### Implements

[ISerializableType](Aspid.FastTools.Types.ISerializableType.md),
ISerializationCallbackReceiver


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<SerializableTypeBase\>\(SerializableTypeBase, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Remarks

<p>Not meant to be derived from outside the package — use [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) or [`SerializableMonoScript`](Aspid.FastTools.Types.SerializableMonoScript.md). Unity serializes the name under the same field for all of them, so every wrapper shares one serialized layout.</p>
<p>A player resolves the type by the stored name only, which managed code stripping does not see: from Managed Stripping Level Low up, a class referenced only by this name can be removed from the build and [`SerializableTypeBase.Type`](Aspid.FastTools.Types.SerializableTypeBase.md#Aspid_FastTools_Types_SerializableTypeBase_Type) returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>. Keep such classes with <code>[Preserve]</code> or <code>link.xml</code>.</p>
<p>A failed lookup is cached until the stored name changes or the object is deserialized again, so an assembly loaded later is not picked up before that.</p>

## Properties

### AssemblyQualifiedName {#Aspid_FastTools_Types_SerializableTypeBase_AssemblyQualifiedName}

Gets the stored assembly-qualified type name, or an empty string when no type is stored.

```csharp
public string AssemblyQualifiedName { get; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Remarks

Kept even when it no longer resolves, so the Inspector can show what the field used to point at.

### BaseType {#Aspid_FastTools_Types_SerializableTypeBase_BaseType}

Gets the base type that types offered by the picker are assignable to; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public abstract Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

#### Remarks

A loaded type is not checked against it: a name stored before the constraint changed resolves as is.

### Type {#Aspid_FastTools_Types_SerializableTypeBase_Type}

Gets the resolved type, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when no type is stored or its stored name cannot be resolved.

```csharp
public Type? Type { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

## Methods

### ToString\(\) {#Aspid_FastTools_Types_SerializableTypeBase_ToString}

Returns the short name of the resolved type, the stored name when it cannot be resolved,
or an empty string when no type is stored.

```csharp
public override string ToString()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

