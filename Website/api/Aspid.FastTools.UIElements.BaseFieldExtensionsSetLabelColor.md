---
title: "Class BaseFieldExtensionsSetLabelColor"
sidebar_label: "BaseFieldExtensionsSetLabelColor"
description: "Class BaseFieldExtensionsSetLabelColor — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldExtensionsSetLabelColor {#Aspid_FastTools_UIElements_BaseFieldExtensionsSetLabelColor}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.Unity.dll  

```csharp
public static class BaseFieldExtensionsSetLabelColor
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldExtensionsSetLabelColor](Aspid.FastTools.UIElements.BaseFieldExtensionsSetLabelColor.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldExtensionsSetLabelColor_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<Color>
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

