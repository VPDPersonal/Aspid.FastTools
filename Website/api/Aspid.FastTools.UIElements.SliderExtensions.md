---
title: "Class SliderExtensions"
sidebar_label: "SliderExtensions"
description: "Class SliderExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SliderExtensions {#Aspid_FastTools_UIElements_SliderExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`BaseSlider<T>`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1.html).

```csharp
public static class SliderExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SliderExtensions](Aspid.FastTools.UIElements.SliderExtensions.md)


## Remarks

A slider clamps its value to its range, which is 0 to 10 for a new [`Slider`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.Slider.html) or [`SliderInt`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.SliderInt.html).
Set the range with [`SliderExtensions.SetLowValue<T1, T2>`](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_) and [`SliderExtensions.SetHighValue<T1, T2>`](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_)
before <code>SetValue</code>:
<code>new Slider().SetValue(50f).SetHighValue(100f)</code> ends at 10,
while <code>new Slider().SetHighValue(100f).SetValue(50f)</code> ends at 50.

## Methods

### SetHighValue\<T\>\(T, byte\) {#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__1___0_System_Byte_}

Sets [`highValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-highValue.html).

```csharp
public static T SetHighValue<T>(this T element, byte value) where T : BaseSlider<byte>
```

#### Parameters

`element` T

The element to modify.

`value` [byte](https://learn.microsoft.com/dotnet/api/system.byte)

The high value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetHighValue\<T\>\(T, sbyte\) {#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__1___0_System_SByte_}

Sets [`highValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-highValue.html).

```csharp
public static T SetHighValue<T>(this T element, sbyte value) where T : BaseSlider<sbyte>
```

#### Parameters

`element` T

The element to modify.

`value` [sbyte](https://learn.microsoft.com/dotnet/api/system.sbyte)

The high value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetHighValue\<T\>\(T, double\) {#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__1___0_System_Double_}

Sets [`highValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-highValue.html).

```csharp
public static T SetHighValue<T>(this T element, double value) where T : BaseSlider<double>
```

#### Parameters

`element` T

The element to modify.

`value` [double](https://learn.microsoft.com/dotnet/api/system.double)

The high value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetHighValue\<T\>\(T, float\) {#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__1___0_System_Single_}

Sets [`highValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-highValue.html).

```csharp
public static T SetHighValue<T>(this T element, float value) where T : BaseSlider<float>
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The high value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetHighValue\<T\>\(T, int\) {#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__1___0_System_Int32_}

Sets [`highValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-highValue.html).

```csharp
public static T SetHighValue<T>(this T element, int value) where T : BaseSlider<int>
```

#### Parameters

`element` T

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The high value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetHighValue\<T\>\(T, uint\) {#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__1___0_System_UInt32_}

Sets [`highValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-highValue.html).

```csharp
public static T SetHighValue<T>(this T element, uint value) where T : BaseSlider<uint>
```

#### Parameters

`element` T

The element to modify.

`value` [uint](https://learn.microsoft.com/dotnet/api/system.uint32)

The high value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetHighValue\<T\>\(T, long\) {#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__1___0_System_Int64_}

Sets [`highValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-highValue.html).

```csharp
public static T SetHighValue<T>(this T element, long value) where T : BaseSlider<long>
```

#### Parameters

`element` T

The element to modify.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

The high value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetHighValue\<T\>\(T, ulong\) {#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__1___0_System_UInt64_}

Sets [`highValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-highValue.html).

```csharp
public static T SetHighValue<T>(this T element, ulong value) where T : BaseSlider<ulong>
```

#### Parameters

`element` T

The element to modify.

`value` [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

The high value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetHighValue\<T\>\(T, short\) {#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__1___0_System_Int16_}

Sets [`highValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-highValue.html).

```csharp
public static T SetHighValue<T>(this T element, short value) where T : BaseSlider<short>
```

#### Parameters

`element` T

The element to modify.

`value` [short](https://learn.microsoft.com/dotnet/api/system.int16)

The high value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetHighValue\<T\>\(T, ushort\) {#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__1___0_System_UInt16_}

Sets [`highValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-highValue.html).

```csharp
public static T SetHighValue<T>(this T element, ushort value) where T : BaseSlider<ushort>
```

#### Parameters

`element` T

The element to modify.

`value` [ushort](https://learn.microsoft.com/dotnet/api/system.uint16)

The high value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetHighValue\<T, TValue\>\(T, TValue\) {#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_}

Sets [`highValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-highValue.html).

```csharp
public static T SetHighValue<T, TValue>(this T element, TValue value) where T : BaseSlider<TValue> where TValue : IComparable<TValue>
```

#### Parameters

`element` T

The element to modify.

`value` TValue

The high value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

`TValue` 

The value type of the element.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetLowValue\<T\>\(T, byte\) {#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__1___0_System_Byte_}

Sets [`lowValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-lowValue.html).

```csharp
public static T SetLowValue<T>(this T element, byte value) where T : BaseSlider<byte>
```

#### Parameters

`element` T

The element to modify.

`value` [byte](https://learn.microsoft.com/dotnet/api/system.byte)

The low value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetLowValue\<T\>\(T, sbyte\) {#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__1___0_System_SByte_}

Sets [`lowValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-lowValue.html).

```csharp
public static T SetLowValue<T>(this T element, sbyte value) where T : BaseSlider<sbyte>
```

#### Parameters

`element` T

The element to modify.

`value` [sbyte](https://learn.microsoft.com/dotnet/api/system.sbyte)

The low value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetLowValue\<T\>\(T, double\) {#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__1___0_System_Double_}

Sets [`lowValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-lowValue.html).

```csharp
public static T SetLowValue<T>(this T element, double value) where T : BaseSlider<double>
```

#### Parameters

`element` T

The element to modify.

`value` [double](https://learn.microsoft.com/dotnet/api/system.double)

The low value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetLowValue\<T\>\(T, float\) {#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__1___0_System_Single_}

Sets [`lowValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-lowValue.html).

```csharp
public static T SetLowValue<T>(this T element, float value) where T : BaseSlider<float>
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The low value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetLowValue\<T\>\(T, int\) {#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__1___0_System_Int32_}

Sets [`lowValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-lowValue.html).

```csharp
public static T SetLowValue<T>(this T element, int value) where T : BaseSlider<int>
```

#### Parameters

`element` T

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The low value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetLowValue\<T\>\(T, uint\) {#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__1___0_System_UInt32_}

Sets [`lowValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-lowValue.html).

```csharp
public static T SetLowValue<T>(this T element, uint value) where T : BaseSlider<uint>
```

#### Parameters

`element` T

The element to modify.

`value` [uint](https://learn.microsoft.com/dotnet/api/system.uint32)

The low value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetLowValue\<T\>\(T, long\) {#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__1___0_System_Int64_}

Sets [`lowValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-lowValue.html).

```csharp
public static T SetLowValue<T>(this T element, long value) where T : BaseSlider<long>
```

#### Parameters

`element` T

The element to modify.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

The low value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetLowValue\<T\>\(T, ulong\) {#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__1___0_System_UInt64_}

Sets [`lowValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-lowValue.html).

```csharp
public static T SetLowValue<T>(this T element, ulong value) where T : BaseSlider<ulong>
```

#### Parameters

`element` T

The element to modify.

`value` [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

The low value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetLowValue\<T\>\(T, short\) {#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__1___0_System_Int16_}

Sets [`lowValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-lowValue.html).

```csharp
public static T SetLowValue<T>(this T element, short value) where T : BaseSlider<short>
```

#### Parameters

`element` T

The element to modify.

`value` [short](https://learn.microsoft.com/dotnet/api/system.int16)

The low value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetLowValue\<T\>\(T, ushort\) {#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__1___0_System_UInt16_}

Sets [`lowValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-lowValue.html).

```csharp
public static T SetLowValue<T>(this T element, ushort value) where T : BaseSlider<ushort>
```

#### Parameters

`element` T

The element to modify.

`value` [ushort](https://learn.microsoft.com/dotnet/api/system.uint16)

The low value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

### SetLowValue\<T, TValue\>\(T, TValue\) {#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_}

Sets [`lowValue`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseSlider_1-lowValue.html).

```csharp
public static T SetLowValue<T, TValue>(this T element, TValue value) where T : BaseSlider<TValue> where TValue : IComparable<TValue>
```

#### Parameters

`element` T

The element to modify.

`value` TValue

The low value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

`TValue` 

The value type of the element.

#### Remarks

Call before <code>SetValue</code>: a slider clamps its value to the range.

