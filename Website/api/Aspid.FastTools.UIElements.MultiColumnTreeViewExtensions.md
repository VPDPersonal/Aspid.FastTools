---
title: "Class MultiColumnTreeViewExtensions"
sidebar_label: "MultiColumnTreeViewExtensions"
description: "Class MultiColumnTreeViewExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class MultiColumnTreeViewExtensions {#Aspid_FastTools_UIElements_MultiColumnTreeViewExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.Unity.dll  

```csharp
public static class MultiColumnTreeViewExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[MultiColumnTreeViewExtensions](Aspid.FastTools.UIElements.MultiColumnTreeViewExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddColumnSortingChanged\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_MultiColumnTreeViewExtensions_AddColumnSortingChanged__1___0_System_Action_}

Subscribes to the [`columnSortingChanged`](https://docs.unity3d.com/ScriptReference/UIElements-MultiColumnTreeView-columnSortingChanged.html) event and returns the element for chaining.

```csharp
public static T AddColumnSortingChanged<T>(this T element, Action callback) where T : MultiColumnTreeView
```

#### Parameters

`element` T

The element to modify.

`callback` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveColumnSortingChanged\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_MultiColumnTreeViewExtensions_RemoveColumnSortingChanged__1___0_System_Action_}

Unsubscribes from the [`columnSortingChanged`](https://docs.unity3d.com/ScriptReference/UIElements-MultiColumnTreeView-columnSortingChanged.html) event and returns the element for chaining.

```csharp
public static T RemoveColumnSortingChanged<T>(this T element, Action callback) where T : MultiColumnTreeView
```

#### Parameters

`element` T

The element to modify.

`callback` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetSortingMode\<T\>\(T, ColumnSortingMode\) {#Aspid_FastTools_UIElements_MultiColumnTreeViewExtensions_SetSortingMode__1___0_UnityEngine_UIElements_ColumnSortingMode_}

Sets [`sortingMode`](https://docs.unity3d.com/ScriptReference/UIElements-MultiColumnTreeView-sortingMode.html) and returns the element for chaining.

```csharp
public static T SetSortingMode<T>(this T element, ColumnSortingMode value) where T : MultiColumnTreeView
```

#### Parameters

`element` T

The element to modify.

`value` ColumnSortingMode

The sorting mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Indicates how to sort columns. To enable sorting, set it to ColumnSortingMode.Default or ColumnSortingMode.Custom.
The Default mode uses the sorting algorithm provided by MultiColumnController, acting on indices. You can also implement your own sorting with the Custom mode, by responding to the columnSortingChanged event.

