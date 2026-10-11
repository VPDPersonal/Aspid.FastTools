---
title: "Interface ISerializableType"
sidebar_label: "ISerializableType"
description: "Interface ISerializableType — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Interface ISerializableType {#Aspid_FastTools_Types_ISerializableType}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Defines the common contract of the serializable [`Type`](https://learn.microsoft.com/dotnet/api/system.type) wrappers.

```csharp
public interface ISerializableType
```

#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<ISerializableType\>\(ISerializableType, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Properties

### BaseType {#Aspid_FastTools_Types_ISerializableType_BaseType}

Gets the base type that types offered by the picker are assignable to; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

#### Remarks

A loaded type is not checked against it: a name stored before the constraint changed resolves as is.

### Type {#Aspid_FastTools_Types_ISerializableType_Type}

Gets the resolved type, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when no type is stored or its stored name cannot be resolved.

```csharp
Type? Type { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

