---
title: "Class BaseFieldVector2Extensions"
sidebar_label: "BaseFieldVector2Extensions"
description: "Class BaseFieldVector2Extensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldVector2Extensions {#Aspid_FastTools_UIElements_BaseFieldVector2Extensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldVector2Extensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldVector2Extensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Vector2`](https://docs.unity3d.com/ScriptReference/Vector2.html).

```csharp
public static class BaseFieldVector2Extensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldVector2Extensions](Aspid.FastTools.UIElements.BaseFieldVector2Extensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldVector2Extensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<Vector2>
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

