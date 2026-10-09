---
title: "Class MinMaxSliderExtensions"
sidebar_label: "MinMaxSliderExtensions"
description: "Class MinMaxSliderExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class MinMaxSliderExtensions {#Aspid_FastTools_UIElements_MinMaxSliderExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`MinMaxSlider`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.MinMaxSlider.html).

```csharp
public static class MinMaxSliderExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[MinMaxSliderExtensions](Aspid.FastTools.UIElements.MinMaxSliderExtensions.md)


## Methods

### SetHighLimit\<T\>\(T, float\) {#Aspid_FastTools_UIElements_MinMaxSliderExtensions_SetHighLimit__1___0_System_Single_}

Sets [`highLimit`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.MinMaxSlider-highLimit.html).

```csharp
public static T SetHighLimit<T>(this T element, float value) where T : MinMaxSlider
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The highest value the selected range can reach.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Exceptions

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

<code class="paramref">value</code> is smaller than [`lowLimit`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.MinMaxSlider-lowLimit.html).

### SetLowLimit\<T\>\(T, float\) {#Aspid_FastTools_UIElements_MinMaxSliderExtensions_SetLowLimit__1___0_System_Single_}

Sets [`lowLimit`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.MinMaxSlider-lowLimit.html).

```csharp
public static T SetLowLimit<T>(this T element, float value) where T : MinMaxSlider
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The lowest value the selected range can reach.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Exceptions

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

<code class="paramref">value</code> is greater than [`highLimit`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.MinMaxSlider-highLimit.html).

### SetMaxValue\<T\>\(T, float\) {#Aspid_FastTools_UIElements_MinMaxSliderExtensions_SetMaxValue__1___0_System_Single_}

Sets [`maxValue`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.MinMaxSlider-maxValue.html).

```csharp
public static T SetMaxValue<T>(this T element, float value) where T : MinMaxSlider
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The upper end of the selected range to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Unity clamps <code class="paramref">value</code> to the current [`minValue`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.MinMaxSlider-minValue.html). To move the range
down, call [`MinMaxSliderExtensions.SetMinValue<T>`](Aspid.FastTools.UIElements.MinMaxSliderExtensions.md#Aspid_FastTools_UIElements_MinMaxSliderExtensions_SetMinValue__1___0_System_Single_) first or set both ends with
[`INotifyValueChangedExtensions.SetValue<T>`](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__1___0_UnityEngine_Vector2_System_Boolean_).

### SetMinValue\<T\>\(T, float\) {#Aspid_FastTools_UIElements_MinMaxSliderExtensions_SetMinValue__1___0_System_Single_}

Sets [`minValue`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.MinMaxSlider-minValue.html).

```csharp
public static T SetMinValue<T>(this T element, float value) where T : MinMaxSlider
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The lower end of the selected range to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Unity clamps <code class="paramref">value</code> to the current [`maxValue`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.MinMaxSlider-maxValue.html): on the default
range of 0 to 10, <code>SetMinValue(20).SetMaxValue(30)</code> gives 10 to 30. To move the range up, call
[`MinMaxSliderExtensions.SetMaxValue<T>`](Aspid.FastTools.UIElements.MinMaxSliderExtensions.md#Aspid_FastTools_UIElements_MinMaxSliderExtensions_SetMaxValue__1___0_System_Single_) first or set both ends with
[`INotifyValueChangedExtensions.SetValue<T>`](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__1___0_UnityEngine_Vector2_System_Boolean_).

