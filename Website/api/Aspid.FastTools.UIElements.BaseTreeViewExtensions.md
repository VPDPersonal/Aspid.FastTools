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

```csharp
public static class BaseTreeViewExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseTreeViewExtensions](Aspid.FastTools.UIElements.BaseTreeViewExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

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

### SetAutoExpand\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseTreeViewExtensions_SetAutoExpand__1___0_System_Boolean_}

Sets the [`autoExpand`](https://docs.unity3d.com/ScriptReference/UIElements-BaseTreeView-autoExpand.html) property and returns the element for chaining.

```csharp
public static T SetAutoExpand<T>(this T element, bool value) where T : BaseTreeView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to auto-expand tree items.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

