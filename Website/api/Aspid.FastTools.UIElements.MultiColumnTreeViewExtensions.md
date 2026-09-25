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
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`MultiColumnTreeView`](https://docs.unity3d.com/ScriptReference/UIElements-MultiColumnTreeView.html).

```csharp
public static class MultiColumnTreeViewExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[MultiColumnTreeViewExtensions](Aspid.FastTools.UIElements.MultiColumnTreeViewExtensions.md)


## Methods

### AddColumnSortingChanged\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_MultiColumnTreeViewExtensions_AddColumnSortingChanged__1___0_System_Action_}

Subscribes to the [`columnSortingChanged`](https://docs.unity3d.com/ScriptReference/UIElements-MultiColumnTreeView-columnSortingChanged.html) event.

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

Unsubscribes from the [`columnSortingChanged`](https://docs.unity3d.com/ScriptReference/UIElements-MultiColumnTreeView-columnSortingChanged.html) event.

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

Sets [`sortingMode`](https://docs.unity3d.com/ScriptReference/UIElements-MultiColumnTreeView-sortingMode.html).

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

The element type.

