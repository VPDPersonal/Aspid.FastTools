---
title: "Class BaseFieldToggleButtonGroupStateExtensions"
sidebar_label: "BaseFieldToggleButtonGroupStateExtensions"
description: "Class BaseFieldToggleButtonGroupStateExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldToggleButtonGroupStateExtensions {#Aspid_FastTools_UIElements_BaseFieldToggleButtonGroupStateExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldToggleButtonGroupStateExtensions.SetLabel<T>`](Aspid.FastTools.UIElements.BaseFieldToggleButtonGroupStateExtensions.md#Aspid_FastTools_UIElements_BaseFieldToggleButtonGroupStateExtensions_SetLabel__1___0_System_String_) for [`BaseField<T>`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.BaseField_1.html) of [`ToggleButtonGroupState`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ToggleButtonGroupState.html).

```csharp
public static class BaseFieldToggleButtonGroupStateExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldToggleButtonGroupStateExtensions](Aspid.FastTools.UIElements.BaseFieldToggleButtonGroupStateExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldToggleButtonGroupStateExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.BaseField_1-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<ToggleButtonGroupState>
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The label text to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The field type.

