---
title: "Class BaseFieldAnimationCurveExtensions"
sidebar_label: "BaseFieldAnimationCurveExtensions"
description: "Class BaseFieldAnimationCurveExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldAnimationCurveExtensions {#Aspid_FastTools_UIElements_BaseFieldAnimationCurveExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldAnimationCurveExtensions.SetLabel<T>`](Aspid.FastTools.UIElements.BaseFieldAnimationCurveExtensions.md#Aspid_FastTools_UIElements_BaseFieldAnimationCurveExtensions_SetLabel__1___0_System_String_) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`AnimationCurve`](https://docs.unity3d.com/ScriptReference/AnimationCurve.html).

```csharp
public static class BaseFieldAnimationCurveExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldAnimationCurveExtensions](Aspid.FastTools.UIElements.BaseFieldAnimationCurveExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldAnimationCurveExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<AnimationCurve>
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

