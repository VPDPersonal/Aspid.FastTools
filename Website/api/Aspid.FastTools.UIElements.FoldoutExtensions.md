---
title: "Class FoldoutExtensions"
sidebar_label: "FoldoutExtensions"
description: "Class FoldoutExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class FoldoutExtensions {#Aspid_FastTools_UIElements_FoldoutExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`Foldout`](https://docs.unity3d.com/ScriptReference/UIElements-Foldout.html).

```csharp
public static class FoldoutExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[FoldoutExtensions](Aspid.FastTools.UIElements.FoldoutExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetText\<T\>\(T, string\) {#Aspid_FastTools_UIElements_FoldoutExtensions_SetText__1___0_System_String_}

Sets [`text`](https://docs.unity3d.com/ScriptReference/UIElements-Foldout-text.html).

```csharp
public static T SetText<T>(this T element, string value) where T : Foldout
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The text to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetToggleOnLabelClick\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_FoldoutExtensions_SetToggleOnLabelClick__1___0_System_Boolean_}

Sets [`toggleOnLabelClick`](https://docs.unity3d.com/ScriptReference/UIElements-Foldout-toggleOnLabelClick.html).

```csharp
public static T SetToggleOnLabelClick<T>(this T element, bool value) where T : Foldout
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, clicking the label toggles the foldout.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

