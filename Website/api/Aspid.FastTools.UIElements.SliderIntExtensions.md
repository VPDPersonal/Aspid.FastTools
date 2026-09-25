---
title: "Class SliderIntExtensions"
sidebar_label: "SliderIntExtensions"
description: "Class SliderIntExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SliderIntExtensions {#Aspid_FastTools_UIElements_SliderIntExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`BaseSlider<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseSlider.html) of [`Int32`](https://learn.microsoft.com/dotnet/api/system.int32).

```csharp
public static class SliderIntExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SliderIntExtensions](Aspid.FastTools.UIElements.SliderIntExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Remarks

Kept apart from [`SliderExtensions`](Aspid.FastTools.UIElements.SliderExtensions.md): overloads that differ only by the constraint on the element type
cannot share one class.

## Methods

### SetDirection\<T\>\(T, SliderDirection\) {#Aspid_FastTools_UIElements_SliderIntExtensions_SetDirection__1___0_UnityEngine_UIElements_SliderDirection_}

Sets [`direction`](https://docs.unity3d.com/ScriptReference/UIElements-BaseSlider-direction.html) controlling the orientation of the element.

```csharp
public static T SetDirection<T>(this T element, SliderDirection value) where T : BaseSlider<int>
```

#### Parameters

`element` T

The element to modify.

`value` SliderDirection

The slider direction to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetFill\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_SliderIntExtensions_SetFill__1___0_System_Boolean_}

Sets [`fill`](https://docs.unity3d.com/ScriptReference/UIElements-BaseSlider-fill.html) controlling whether the track is filled up to the current value.

```csharp
public static T SetFill<T>(this T element, bool value) where T : BaseSlider<int>
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the track is filled up to the current value.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetInverted\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_SliderIntExtensions_SetInverted__1___0_System_Boolean_}

Sets [`inverted`](https://docs.unity3d.com/ScriptReference/UIElements-BaseSlider-inverted.html) reversing the direction of the element.

```csharp
public static T SetInverted<T>(this T element, bool value) where T : BaseSlider<int>
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the slider direction is reversed.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetPageSize\<T\>\(T, float\) {#Aspid_FastTools_UIElements_SliderIntExtensions_SetPageSize__1___0_System_Single_}

Sets [`pageSize`](https://docs.unity3d.com/ScriptReference/UIElements-BaseSlider-pageSize.html) controlling how much the value changes per page step.

```csharp
public static T SetPageSize<T>(this T element, float value) where T : BaseSlider<int>
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The page size to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetShowInputField\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_SliderIntExtensions_SetShowInputField__1___0_System_Boolean_}

Sets [`showInputField`](https://docs.unity3d.com/ScriptReference/UIElements-BaseSlider-showInputField.html) controlling whether a numeric input field is shown alongside the element.

```csharp
public static T SetShowInputField<T>(this T element, bool value) where T : BaseSlider<int>
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, a numeric input field is shown next to the slider.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

