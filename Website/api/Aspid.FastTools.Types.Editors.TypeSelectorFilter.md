---
title: "Struct TypeSelectorFilter"
sidebar_label: "TypeSelectorFilter"
description: "Struct TypeSelectorFilter — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Struct TypeSelectorFilter {#Aspid_FastTools_Types_Editors_TypeSelectorFilter}

Namespace: [Aspid.FastTools.Types.Editors](Aspid.FastTools.Types.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

Represents the constraints deciding which types the selector offers.

```csharp
public struct TypeSelectorFilter
```


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<TypeSelectorFilter\>\(TypeSelectorFilter, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Properties

### AdditionalTypes {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_AdditionalTypes}

Gets or sets extra candidates that bypass the base-type and kind checks.

```csharp
public IEnumerable<Type> AdditionalTypes { readonly get; set; }
```

#### Property Value

 [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[Type](https://learn.microsoft.com/dotnet/api/system.type)\>

#### Remarks

These candidates also bypass [`TypeSelectorFilter.Predicate`](Aspid.FastTools.Types.Editors.TypeSelectorFilter.md#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Predicate); the hidden-type filter still applies.

### Allow {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Allow}

Gets or sets which type kinds the list includes.

```csharp
public TypeAllow Allow { readonly get; set; }
```

#### Property Value

 TypeAllow

### ArgumentFilter {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_ArgumentFilter}

Gets or sets the additional predicate for manually selected generic arguments.

```csharp
public Func<Type, bool> ArgumentFilter { readonly get; set; }
```

#### Property Value

 [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[Type](https://learn.microsoft.com/dotnet/api/system.type), [bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

#### Remarks

A <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> predicate accepts every argument satisfying its parameter constraints.

### HideNoneOption {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_HideNoneOption}

Gets or sets a value indicating whether the empty selection is hidden on the root page.

```csharp
public bool HideNoneOption { readonly get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### IncludeHidden {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_IncludeHidden}

Gets or sets a value indicating whether types with [`TypeSelectorDisplayAttribute.Hidden`](Aspid.FastTools.Types.TypeSelectorDisplayAttribute.md#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Hidden) are offered.

```csharp
public bool IncludeHidden { readonly get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### InferredArgumentFilter {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_InferredArgumentFilter}

Gets or sets the predicate for generic arguments inferred from the field type.

```csharp
public GenericArgumentFilter InferredArgumentFilter { readonly get; set; }
```

#### Property Value

 [GenericArgumentFilter](Aspid.FastTools.Types.Editors.GenericArgumentFilter.md)

#### Remarks

A <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> predicate accepts every inferred argument satisfying its parameter constraints.

### Predicate {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Predicate}

Gets or sets the predicate that rejects candidates after the base-type and kind checks.

```csharp
public Func<Type, bool> Predicate { readonly get; set; }
```

#### Property Value

 [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[Type](https://learn.microsoft.com/dotnet/api/system.type), [bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

#### Remarks

A <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> predicate accepts every matching type.

### Types {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Types}

Gets or sets the base types that every candidate must be assignable to.

```csharp
public Type[] Types { readonly get; set; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

#### Remarks

A <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> array applies no base-type constraint.

