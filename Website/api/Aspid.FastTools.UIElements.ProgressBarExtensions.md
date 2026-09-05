---
title: "Class ProgressBarExtensions"
sidebar_label: "ProgressBarExtensions"
description: "Class ProgressBarExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class ProgressBarExtensions {#Aspid_FastTools_UIElements_ProgressBarExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

```csharp
public static class ProgressBarExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[ProgressBarExtensions](Aspid.FastTools.UIElements.ProgressBarExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetHighValue\<T\>\(T, float\) {#Aspid_FastTools_UIElements_ProgressBarExtensions_SetHighValue__1___0_System_Single_}

Sets the maximum value of the ProgressBar and returns the element for chaining.

```csharp
public static T SetHighValue<T>(this T element, float value) where T : AbstractProgressBar
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The maximum value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### SetLowValue\<T\>\(T, float\) {#Aspid_FastTools_UIElements_ProgressBarExtensions_SetLowValue__1___0_System_Single_}

Sets the minimum value of the ProgressBar and returns the element for chaining.

```csharp
public static T SetLowValue<T>(this T element, float value) where T : AbstractProgressBar
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The minimum value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### SetTitle\<T\>\(T, string\) {#Aspid_FastTools_UIElements_ProgressBarExtensions_SetTitle__1___0_System_String_}

Sets the title of the ProgressBar that displays in the center of the control and returns the element for chaining.

```csharp
public static T SetTitle<T>(this T element, string value) where T : AbstractProgressBar
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The title text to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### SetValue\<T\>\(T, float\) {#Aspid_FastTools_UIElements_ProgressBarExtensions_SetValue__1___0_System_Single_}

Sets the current value of the progress bar via [`value`](https://docs.unity3d.com/ScriptReference/UIElements-AbstractProgressBar-value.html).

```csharp
public static T SetValue<T>(this T element, float value) where T : AbstractProgressBar
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The value to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

