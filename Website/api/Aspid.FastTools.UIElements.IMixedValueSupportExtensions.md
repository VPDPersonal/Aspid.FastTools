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
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`IMixedValueSupport`](https://docs.unity3d.com/ScriptReference/UIElements-IMixedValueSupport.html).

```csharp
public static class IMixedValueSupportExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[IMixedValueSupportExtensions](Aspid.FastTools.UIElements.IMixedValueSupportExtensions.md)


## Methods

### SetShowMixedValue\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_IMixedValueSupportExtensions_SetShowMixedValue__1___0_System_Boolean_}

Sets [`showMixedValue`](https://docs.unity3d.com/ScriptReference/UIElements-IMixedValueSupport-showMixedValue.html).

```csharp
public static T SetShowMixedValue<T>(this T element, bool value) where T : IMixedValueSupport
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the mixed value state is shown.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

