---
title: "Class RadioButtonGroupExtensions"
sidebar_label: "RadioButtonGroupExtensions"
description: "Class RadioButtonGroupExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class RadioButtonGroupExtensions {#Aspid_FastTools_UIElements_RadioButtonGroupExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`RadioButtonGroup`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.RadioButtonGroup.html).

```csharp
public static class RadioButtonGroupExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[RadioButtonGroupExtensions](Aspid.FastTools.UIElements.RadioButtonGroupExtensions.md)


## Methods

### SetChoices\<T\>\(T, IEnumerable\<string\>\) {#Aspid_FastTools_UIElements_RadioButtonGroupExtensions_SetChoices__1___0_System_Collections_Generic_IEnumerable_System_String__}

Sets [`choices`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.RadioButtonGroup-choices.html).

```csharp
public static T SetChoices<T>(this T element, IEnumerable<string> value) where T : RadioButtonGroup
```

#### Parameters

`element` T

The element to modify.

`value` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)\>

The labels of the radio buttons to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

