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

[INotifyValueChangedExtensions.AddValueChanged\<TypeSelectorFilter, TValue\>\(TypeSelectorFilter, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<TypeSelectorFilter, TValue\>\(TypeSelectorFilter, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<TypeSelectorFilter, TValue\>\(TypeSelectorFilter, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<TypeSelectorFilter, TValue\>\(TypeSelectorFilter, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<TypeSelectorFilter, TValue\>\(TypeSelectorFilter, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<TypeSelectorFilter, TValue\>\(TypeSelectorFilter, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Properties

### AdditionalTypes {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_AdditionalTypes}

Gets or sets extra types appended verbatim, bypassing the base-type and [`TypeSelectorFilter.Allow`](Aspid.FastTools.Types.Editors.TypeSelectorFilter.md#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Allow) checks — for
entries the assignability scan cannot match, such as open generic definitions.

```csharp
public IEnumerable<Type> AdditionalTypes { readonly get; set; }
```

#### Property Value

 [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[Type](https://learn.microsoft.com/dotnet/api/system.type)\>

### Allow {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Allow}

Gets or sets which type kinds the list includes.

```csharp
public TypeAllow Allow { readonly get; set; }
```

#### Property Value

 TypeAllow

### ArgumentFilter {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_ArgumentFilter}

Gets or sets the predicate applied to the types offered for an open generic's arguments, on top of the
parameter's own constraints. <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> accepts any constraint-satisfying type.

```csharp
public Func<Type, bool> ArgumentFilter { readonly get; set; }
```

#### Property Value

 [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[Type](https://learn.microsoft.com/dotnet/api/system.type), [bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### HideNoneOption {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_HideNoneOption}

Gets or sets a value indicating whether the <code>&lt;None&gt;</code> row is left out of the root page.

```csharp
public bool HideNoneOption { readonly get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Remarks

Set it on a picker whose target must always hold a type, such as one swapping a component's script. By
default the row is offered and reports <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when selected.

### IncludeHidden {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_IncludeHidden}

Gets or sets a value indicating whether types marked <code>[TypeSelectorDisplay(Hidden = true)]</code> are
offered.

```csharp
public bool IncludeHidden { readonly get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Remarks

Set it only on a picker that repairs a reference: hiding a type means "do not offer this for new work",
not "make existing data holding it unfixable".

### InferredArgumentFilter {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_InferredArgumentFilter}

Gets or sets the filter applied to an argument the selector infers from the field instead of asking for
it. <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> accepts whatever the field determines.

```csharp
public GenericArgumentFilter InferredArgumentFilter { readonly get; set; }
```

#### Property Value

 [GenericArgumentFilter](Aspid.FastTools.Types.Editors.GenericArgumentFilter.md)

#### Remarks

Separate from [`TypeSelectorFilter.ArgumentFilter`](Aspid.FastTools.Types.Editors.TypeSelectorFilter.md#Aspid_FastTools_Types_Editors_TypeSelectorFilter_ArgumentFilter), which curates a page a human reads and must stay a finite
list. This one judges a single argument the field has already fixed, so it can ask the exact question per
parameter and admit an argument the page would not have offered.

### Predicate {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Predicate}

Gets or sets the predicate applied to each candidate after the base-type and [`TypeSelectorFilter.Allow`](Aspid.FastTools.Types.Editors.TypeSelectorFilter.md#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Allow) checks,
returning <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> to hide a type. <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> keeps every matching type.

```csharp
public Func<Type, bool> Predicate { readonly get; set; }
```

#### Property Value

 [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[Type](https://learn.microsoft.com/dotnet/api/system.type), [bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### Types {#Aspid_FastTools_Types_Editors_TypeSelectorFilter_Types}

Gets or sets the base types the candidates must all be assignable to.
<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> stands for [`Object`](https://learn.microsoft.com/dotnet/api/system.object).

```csharp
public Type[] Types { readonly get; set; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

