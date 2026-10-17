---
title: "Class TypeSelectorAttribute"
sidebar_label: "TypeSelectorAttribute"
description: "Class TypeSelectorAttribute — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TypeSelectorAttribute {#Aspid_FastTools_Types_TypeSelectorAttribute}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Draws the field with the type-selector window.

```csharp
[Conditional("UNITY_EDITOR")]
public sealed class TypeSelectorAttribute : PropertyAttribute
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[Attribute](https://learn.microsoft.com/dotnet/api/system.attribute) ← 
PropertyAttribute ← 
[TypeSelectorAttribute](Aspid.FastTools.Types.TypeSelectorAttribute.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<TypeSelectorAttribute\>\(TypeSelectorAttribute, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Examples


```csharp
[TypeSelector(typeof(MonoBehaviour))]
[SerializeField] private string _behaviorType;
```


```csharp
[TypeSelector(typeof(IDisposable), typeof(ScriptableObject))]
[SerializeField] private string _type;
```


```csharp
[SerializeField] private SerializableType _category;
[TypeSelector(nameof(_category))]
[SerializeField] private string _subType;
```


```csharp
[TypeSelector(typeof(IWeapon), Required = true)]
[SerializeReference] private IWeapon _weapon;
```


## Remarks

On an array or [`List<T>`](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1) field each element gets a picker, and the add
button of a <code>[SerializeReference]</code> list opens one.

## Constructors

### TypeSelectorAttribute\(\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor}

Creates an unconstrained attribute: any type is offered.

```csharp
public TypeSelectorAttribute()
```

### TypeSelectorAttribute\(Type\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor_System_Type_}

Creates an attribute constrained to a single base type.

```csharp
public TypeSelectorAttribute(Type type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The base type.

### TypeSelectorAttribute\(params Type\[\]?\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor_System_Type___}

Creates an attribute constrained to one or more base types.

```csharp
public TypeSelectorAttribute(params Type[]? types)
```

#### Parameters

`types` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]?

The base types; the picker offers only types assignable to all of them.

### TypeSelectorAttribute\(string\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor_System_String_}

Creates an attribute constrained by a type name or by a member that supplies the base types.

```csharp
public TypeSelectorAttribute(string assemblyQualifiedName)
```

#### Parameters

`assemblyQualifiedName` [string](https://learn.microsoft.com/dotnet/api/system.string)

An assembly-qualified type name (<code>"MyGame.IWeapon, MyGame"</code>) or a member name
(<code>nameof(_category)</code>).

#### Remarks

An instance field or property of that name supplies the types from its current value: a
[`Type`](https://learn.microsoft.com/dotnet/api/system.type), a [`String`](https://learn.microsoft.com/dotnet/api/system.string), an [`ISerializableType`](Aspid.FastTools.Types.ISerializableType.md) or an array of these.
Any other string is an assembly-qualified type name.

### TypeSelectorAttribute\(params string\[\]?\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor_System_String___}

Creates an attribute constrained by one or more type or member names.

```csharp
public TypeSelectorAttribute(params string[]? assemblyQualifiedNames)
```

#### Parameters

`assemblyQualifiedNames` [string](https://learn.microsoft.com/dotnet/api/system.string)\[\]?

Type or member names, each resolved as in [`TypeSelectorAttribute.%23ctor`](Aspid.FastTools.Types.TypeSelectorAttribute.md).

## Properties

### Allow {#Aspid_FastTools_Types_TypeSelectorAttribute_Allow}

Gets or sets which special type categories the picker offers besides concrete classes;
[`TypeAllow.All`](Aspid.FastTools.Types.TypeAllow.md) by default.

```csharp
public TypeAllow Allow { get; set; }
```

#### Property Value

 [TypeAllow](Aspid.FastTools.Types.TypeAllow.md)

#### Remarks

Ignored on a <code>[SerializeReference]</code> field, which always lists only instantiable types.

### AssemblyQualifiedNames {#Aspid_FastTools_Types_TypeSelectorAttribute_AssemblyQualifiedNames}

Gets the raw constraint arguments: assembly-qualified names of base types, or names of members supplying
them (see [`TypeSelectorAttribute.%23ctor`](Aspid.FastTools.Types.TypeSelectorAttribute.md)). Empty for an unconstrained selector.

```csharp
public string[] AssemblyQualifiedNames { get; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)\[\]

### Required {#Aspid_FastTools_Types_TypeSelectorAttribute_Required}

Gets or sets a value indicating whether an unset field shows a "required" warning and is reported as a
violation.

```csharp
public bool Required { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Remarks

Applies to <code>[SerializeReference]</code>, [`String`](https://learn.microsoft.com/dotnet/api/system.string) and [`SerializableTypeBase`](Aspid.FastTools.Types.SerializableTypeBase.md) fields;
a type that no longer resolves is reported by the missing-type check instead.

