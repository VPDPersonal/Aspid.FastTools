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

Sets [`text`](https://docs.unity3d.com/ScriptReference/UIElements-Foldout-text.html) and returns the element for chaining.

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

#### Remarks

The label text for the toggle.

### SetToggleOnLabelClick\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_FoldoutExtensions_SetToggleOnLabelClick__1___0_System_Boolean_}

Sets [`toggleOnLabelClick`](https://docs.unity3d.com/ScriptReference/UIElements-Foldout-toggleOnLabelClick.html) and returns the element for chaining.

```csharp
public static T SetToggleOnLabelClick<T>(this T element, bool value) where T : Foldout
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether clicking the label toggles the foldout.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Whether to toggle the element state when the user clicks the label.

