---
title: "Class AbstractProgressBarExtensions"
sidebar_label: "AbstractProgressBarExtensions"
description: "Class AbstractProgressBarExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class AbstractProgressBarExtensions {#Aspid_FastTools_UIElements_AbstractProgressBarExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`AbstractProgressBar`](https://docs.unity3d.com/ScriptReference/UIElements-AbstractProgressBar.html).

```csharp
public static class AbstractProgressBarExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[AbstractProgressBarExtensions](Aspid.FastTools.UIElements.AbstractProgressBarExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetHighValue\<T\>\(T, float\) {#Aspid_FastTools_UIElements_AbstractProgressBarExtensions_SetHighValue__1___0_System_Single_}

Sets the maximum value of the ProgressBar.

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

The element type.

### SetLowValue\<T\>\(T, float\) {#Aspid_FastTools_UIElements_AbstractProgressBarExtensions_SetLowValue__1___0_System_Single_}

Sets the minimum value of the ProgressBar.

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

The element type.

### SetTitle\<T\>\(T, string\) {#Aspid_FastTools_UIElements_AbstractProgressBarExtensions_SetTitle__1___0_System_String_}

Sets the title of the ProgressBar that displays in the center of the control.

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

The element type.

### SetValue\<T\>\(T, float\) {#Aspid_FastTools_UIElements_AbstractProgressBarExtensions_SetValue__1___0_System_Single_}

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

