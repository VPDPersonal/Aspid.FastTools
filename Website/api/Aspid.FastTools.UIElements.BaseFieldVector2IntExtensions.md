---
title: "Class BaseFieldVector2IntExtensions"
sidebar_label: "BaseFieldVector2IntExtensions"
description: "Class BaseFieldVector2IntExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldVector2IntExtensions {#Aspid_FastTools_UIElements_BaseFieldVector2IntExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldVector2IntExtensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldVector2IntExtensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Vector2Int`](https://docs.unity3d.com/ScriptReference/Vector2Int.html).

```csharp
public static class BaseFieldVector2IntExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldVector2IntExtensions](Aspid.FastTools.UIElements.BaseFieldVector2IntExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldVector2IntExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<Vector2Int>
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

