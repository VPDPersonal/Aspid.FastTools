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
Assembly: Aspid.FastTools.Unity.Editor.dll  

Describes which types the selector offers: the base-type and kind constraints, an optional per-type
predicate, any verbatim extra entries, and the argument predicate for open generics. Bundles the
candidate-defining inputs of [`TypeSelectorWindow.Show`](Aspid.FastTools.Types.Editors.TypeSelectorWindow.md) and the [`Editors.TypeSelectorView`](Aspid.FastTools.Types.Editors.md)
constructor into a single value so they travel together.

```csharp
public struct TypeSelectorFilter
```


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<TypeSelectorFilter, TValue\>\(TypeSelectorFilter, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<TypeSelectorFilter, TValue\>\(TypeSelectorFilter, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<TypeSelectorFilter, TValue\>\(TypeSelectorFilter, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<TypeSelectorFilter, TValue\>\(TypeSelectorFilter, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<TypeSelectorFilter, TValue\>\(TypeSelectorFilter, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<TypeSelectorFilter, TValue\>\(TypeSelectorFilter, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Properties

### AdditionalTypes {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_AdditionalTypes}

Optional extra types appended to the list verbatim, bypassing the base-type and [`TypeSelectorFilter.Allow`](Aspid.FastTools.Types.Editors.TypeSelectorFilter.md#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Allow) checks —
used to inject entries the assignability scan cannot match, such as open generic definitions.

```csharp
public IEnumerable<Type> AdditionalTypes { readonly get; set; }
```

#### Property Value

 [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[Type](https://learn.microsoft.com/dotnet/api/system.type)\>

### Allow {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Allow}

Which type kinds are included in the list.

```csharp
public TypeAllow Allow { readonly get; set; }
```

#### Property Value

 TypeAllow

### ArgumentFilter {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_ArgumentFilter}

Optional predicate applied to candidate types offered for an open generic's type arguments (in addition to
the parameter's own constraints). Used to restrict arguments to, e.g., Unity-serializable types. Leave
<code>null</code> to accept any constraint-satisfying type.

```csharp
public Func<Type, bool> ArgumentFilter { readonly get; set; }
```

#### Property Value

 [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[Type](https://learn.microsoft.com/dotnet/api/system.type), [bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### IncludeHidden {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_IncludeHidden}

Includes types marked <code>[TypeSelectorDisplay(Hidden = true)]</code>, which the picker leaves out by default.
Set it only on a picker that <b>repairs</b> a reference rather than authors one: hiding a type means "do
not offer this for new work", not "make existing data holding it unfixable".

```csharp
public bool IncludeHidden { readonly get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### InferredArgumentFilter {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_InferredArgumentFilter}

Optional predicate applied to an argument the selector <b>infers</b> from the field instead of asking for
it. Leave <code>null</code> to accept whatever the field determines.

```csharp
public GenericArgumentFilter InferredArgumentFilter { readonly get; set; }
```

#### Property Value

 [GenericArgumentFilter](Aspid.FastTools.Types.Editors.GenericArgumentFilter.md)

#### Remarks

Separate from [`TypeSelectorFilter.ArgumentFilter`](Aspid.FastTools.Types.Editors.TypeSelectorFilter.md#Aspid_FastTools_Types_Editors_TypeSelectorFilter_ArgumentFilter) because the two decide different things. That one curates a
page a human reads and has to stay a finite, sensible list; this one judges a single argument the field
has already fixed, which no one is going to browse — so it can afford to ask the exact question, per
parameter, and admit an argument the page would not have bothered to offer.

### Predicate {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Predicate}

Optional predicate applied to each candidate type after the base-type and [`TypeSelectorFilter.Allow`](Aspid.FastTools.Types.Editors.TypeSelectorFilter.md#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Allow) checks.
Return <code>false</code> to hide a type. Leave <code>null</code> to keep every matching type.

```csharp
public Func<Type, bool> Predicate { readonly get; set; }
```

#### Property Value

 [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[Type](https://learn.microsoft.com/dotnet/api/system.type), [bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### Types {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Types}

Base types used to filter which concrete types are shown. Only types assignable to all entries are listed.
Defaults to [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when left <code>null</code>.

```csharp
public Type[] Types { readonly get; set; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

