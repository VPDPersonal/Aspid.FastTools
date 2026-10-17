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

Shared base of the serializable [`Type`](https://learn.microsoft.com/dotnet/api/system.type) wrappers: stores the type by its assembly-qualified
name and resolves it lazily.

```csharp
[Serializable]
public abstract class SerializableTypeBase : ISerializableType, IEquatable<SerializableTypeBase>, ISerializationCallbackReceiver
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md)

#### Derived

[SerializableMonoScript](Aspid.FastTools.Types.SerializableMonoScript.md),
[SerializableType](Aspid.FastTools.Types.SerializableType.md)

#### Implements

[ISerializableType](Aspid.FastTools.Types.ISerializableType.md),
[IEquatable\<SerializableTypeBase\>](https://learn.microsoft.com/dotnet/api/system.iequatable-1),
ISerializationCallbackReceiver


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<SerializableTypeBase\>\(SerializableTypeBase, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Remarks

A player resolves only the stored name, which managed code stripping does not see: from Managed Stripping
Level Medium up, keep a class referenced only here with <code>[Preserve]</code> or <code>link.xml</code>. A failed lookup
is cached until the name changes or the object is deserialized again, so an assembly loaded later is missed.

## Properties

### AssemblyQualifiedName {#Aspid_FastTools_Types_SerializableTypeBase_AssemblyQualifiedName}

Gets the stored assembly-qualified type name, or an empty string when no type is stored.

```csharp
public string AssemblyQualifiedName { get; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### BaseType {#Aspid_FastTools_Types_SerializableTypeBase_BaseType}

Gets the base type that types offered by the picker are assignable to; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public abstract Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

#### Remarks

A loaded type is not checked against it: a name stored before the constraint changed resolves as is.

### IsEmpty {#Aspid_FastTools_Types_SerializableTypeBase_IsEmpty}

Gets a value indicating whether no type is stored.

```csharp
public bool IsEmpty { get; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Remarks

On an object created in code the field itself stays <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> until Unity serializes it:
test an unset field with <code>field is null || field.IsEmpty</code>.

### IsMissing {#Aspid_FastTools_Types_SerializableTypeBase_IsMissing}

Gets a value indicating whether a type name is stored but does not resolve, for example after the class
was renamed, moved or stripped from a player build.

```csharp
public bool IsMissing { get; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Type {#Aspid_FastTools_Types_SerializableTypeBase_Type}

Gets the resolved type, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when no type is stored or its stored name cannot be resolved.

```csharp
public Type? Type { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

## Methods

### Equals\(SerializableTypeBase?\) {#Aspid_FastTools_Types_SerializableTypeBase_Equals_Aspid_FastTools_Types_SerializableTypeBase_}

Determines whether <code class="paramref">other</code> stores the same assembly-qualified name, whatever its wrapper
class or constraint.

```csharp
public bool Equals(SerializableTypeBase? other)
```

#### Parameters

`other` [SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md)?

The wrapper to compare with.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if <code class="paramref">other</code> is not <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> and stores the same
name, also when neither stores a type; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

#### Remarks

<code>==</code> still compares references.

### Equals\(object?\) {#Aspid_FastTools_Types_SerializableTypeBase_Equals_System_Object_}

Determines whether <code class="paramref">obj</code> is a wrapper that stores the same assembly-qualified name.

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The object to compare with.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if <code class="paramref">obj</code> is such a wrapper; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

### GetHashCode\(\) {#Aspid_FastTools_Types_SerializableTypeBase_GetHashCode}

Returns a hash code of the stored assembly-qualified name.

```csharp
public override int GetHashCode()
```

#### Returns

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

#### Remarks

Follows the stored name: do not change a wrapper used as a key, also through Unity serialization.

### ToString\(\) {#Aspid_FastTools_Types_SerializableTypeBase_ToString}

Returns the short name of the resolved type, the stored name when it cannot be resolved,
or an empty string when no type is stored.

```csharp
public override string ToString()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

