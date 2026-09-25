---
title: "Class BaseTreeViewExtensions"
sidebar_label: "BaseTreeViewExtensions"
description: "Class BaseTreeViewExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseTreeViewExtensions {#Aspid_FastTools_UIElements_BaseTreeViewExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`BaseTreeView`](https://docs.unity3d.com/ScriptReference/UIElements-BaseTreeView.html).

```csharp
public static class BaseTreeViewExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseTreeViewExtensions](Aspid.FastTools.UIElements.BaseTreeViewExtensions.md)


## Methods

### AddItemExpandedChanged\<T\>\(T, Action\<TreeViewExpansionChangedArgs\>\) {#Aspid_FastTools_UIElements_BaseTreeViewExtensions_AddItemExpandedChanged__1___0_System_Action_UnityEngine_UIElements_TreeViewExpansionChangedArgs__}

Subscribes to the [`itemExpandedChanged`](https://docs.unity3d.com/ScriptReference/UIElements-BaseTreeView-itemExpandedChanged.html) event.

```csharp
public static T AddItemExpandedChanged<T>(this T element, Action<TreeViewExpansionChangedArgs> value) where T : BaseTreeView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<TreeViewExpansionChangedArgs\>

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveItemExpandedChanged\<T\>\(T, Action\<TreeViewExpansionChangedArgs\>\) {#Aspid_FastTools_UIElements_BaseTreeViewExtensions_RemoveItemExpandedChanged__1___0_System_Action_UnityEngine_UIElements_TreeViewExpansionChangedArgs__}

Unsubscribes from the [`itemExpandedChanged`](https://docs.unity3d.com/ScriptReference/UIElements-BaseTreeView-itemExpandedChanged.html) event.

```csharp
public static T RemoveItemExpandedChanged<T>(this T element, Action<TreeViewExpansionChangedArgs> value) where T : BaseTreeView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<TreeViewExpansionChangedArgs\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetAutoExpand\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseTreeViewExtensions_SetAutoExpand__1___0_System_Boolean_}

Sets [`autoExpand`](https://docs.unity3d.com/ScriptReference/UIElements-BaseTreeView-autoExpand.html).

```csharp
public static T SetAutoExpand<T>(this T element, bool value) where T : BaseTreeView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, items are expanded automatically.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

