---
title: "Class IMixedValueSupportExtensions"
sidebar_label: "IMixedValueSupportExtensions"
description: "Class IMixedValueSupportExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class IMixedValueSupportExtensions {#Aspid_FastTools_UIElements_IMixedValueSupportExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.Unity.dll  

```csharp
public static class IMixedValueSupportExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[IMixedValueSupportExtensions](Aspid.FastTools.UIElements.IMixedValueSupportExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetShowMixedValue\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_IMixedValueSupportExtensions_SetShowMixedValue__1___0_System_Boolean_}

Sets [`showMixedValue`](https://docs.unity3d.com/ScriptReference/UIElements-IMixedValueSupport-showMixedValue.html) and returns the element for chaining.

```csharp
public static T SetShowMixedValue<T>(this T element, bool value = true) where T : IMixedValueSupport
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to show the mixed value state.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Indicates whether to enable the mixed value state on the value field.

