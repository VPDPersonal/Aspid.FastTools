---
title: "Class TreeViewExtensions"
sidebar_label: "TreeViewExtensions"
description: "Class TreeViewExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TreeViewExtensions {#Aspid_FastTools_UIElements_TreeViewExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`TreeView`](https://docs.unity3d.com/ScriptReference/UIElements-TreeView.html).

```csharp
public static class TreeViewExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TreeViewExtensions](Aspid.FastTools.UIElements.TreeViewExtensions.md)


## Methods

### AddBindItem\<T\>\(T, Action\<VisualElement, int\>\) {#Aspid_FastTools_UIElements_TreeViewExtensions_AddBindItem__1___0_System_Action_UnityEngine_UIElements_VisualElement_System_Int32__}

Subscribes to the [`bindItem`](https://docs.unity3d.com/ScriptReference/UIElements-TreeView-bindItem.html) callback.

```csharp
public static T AddBindItem<T>(this T element, Action<VisualElement, int> value) where T : TreeView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<VisualElement, [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

The callback to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### AddDestroyItem\<T\>\(T, Action\<VisualElement\>\) {#Aspid_FastTools_UIElements_TreeViewExtensions_AddDestroyItem__1___0_System_Action_UnityEngine_UIElements_VisualElement__}

Subscribes to the [`destroyItem`](https://docs.unity3d.com/ScriptReference/UIElements-TreeView-destroyItem.html) callback.

```csharp
public static T AddDestroyItem<T>(this T element, Action<VisualElement> value) where T : TreeView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<VisualElement\>

The callback to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### AddUnbindItem\<T\>\(T, Action\<VisualElement, int\>\) {#Aspid_FastTools_UIElements_TreeViewExtensions_AddUnbindItem__1___0_System_Action_UnityEngine_UIElements_VisualElement_System_Int32__}

Subscribes to the [`unbindItem`](https://docs.unity3d.com/ScriptReference/UIElements-TreeView-unbindItem.html) callback.

```csharp
public static T AddUnbindItem<T>(this T element, Action<VisualElement, int> value) where T : TreeView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<VisualElement, [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

The callback invoked to release bindings from a tree item element.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveBindItem\<T\>\(T, Action\<VisualElement, int\>\) {#Aspid_FastTools_UIElements_TreeViewExtensions_RemoveBindItem__1___0_System_Action_UnityEngine_UIElements_VisualElement_System_Int32__}

Unsubscribes from the [`bindItem`](https://docs.unity3d.com/ScriptReference/UIElements-TreeView-bindItem.html) callback.

```csharp
public static T RemoveBindItem<T>(this T element, Action<VisualElement, int> value) where T : TreeView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<VisualElement, [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveDestroyItem\<T\>\(T, Action\<VisualElement\>\) {#Aspid_FastTools_UIElements_TreeViewExtensions_RemoveDestroyItem__1___0_System_Action_UnityEngine_UIElements_VisualElement__}

Unsubscribes from the [`destroyItem`](https://docs.unity3d.com/ScriptReference/UIElements-TreeView-destroyItem.html) callback.

```csharp
public static T RemoveDestroyItem<T>(this T element, Action<VisualElement> value) where T : TreeView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<VisualElement\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveUnbindItem\<T\>\(T, Action\<VisualElement, int\>\) {#Aspid_FastTools_UIElements_TreeViewExtensions_RemoveUnbindItem__1___0_System_Action_UnityEngine_UIElements_VisualElement_System_Int32__}

Unsubscribes from the [`unbindItem`](https://docs.unity3d.com/ScriptReference/UIElements-TreeView-unbindItem.html) callback.

```csharp
public static T RemoveUnbindItem<T>(this T element, Action<VisualElement, int> value) where T : TreeView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<VisualElement, [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetBindItem\<T\>\(T, Action\<VisualElement, int\>\) {#Aspid_FastTools_UIElements_TreeViewExtensions_SetBindItem__1___0_System_Action_UnityEngine_UIElements_VisualElement_System_Int32__}

Sets [`bindItem`](https://docs.unity3d.com/ScriptReference/UIElements-TreeView-bindItem.html), replacing any existing callback.

```csharp
public static T SetBindItem<T>(this T element, Action<VisualElement, int> value) where T : TreeView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<VisualElement, [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

The callback to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetDestroyItem\<T\>\(T, Action\<VisualElement\>\) {#Aspid_FastTools_UIElements_TreeViewExtensions_SetDestroyItem__1___0_System_Action_UnityEngine_UIElements_VisualElement__}

Sets [`destroyItem`](https://docs.unity3d.com/ScriptReference/UIElements-TreeView-destroyItem.html), replacing any existing callback.

```csharp
public static T SetDestroyItem<T>(this T element, Action<VisualElement> value) where T : TreeView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<VisualElement\>

The callback to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetItemTemplate\<T\>\(T, VisualTreeAsset\) {#Aspid_FastTools_UIElements_TreeViewExtensions_SetItemTemplate__1___0_UnityEngine_UIElements_VisualTreeAsset_}

Sets [`itemTemplate`](https://docs.unity3d.com/ScriptReference/UIElements-TreeView-itemTemplate.html).

```csharp
public static T SetItemTemplate<T>(this T element, VisualTreeAsset value) where T : TreeView
```

#### Parameters

`element` T

The element to modify.

`value` VisualTreeAsset

The UXML template to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetMakeItem\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_TreeViewExtensions_SetMakeItem__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Sets [`makeItem`](https://docs.unity3d.com/ScriptReference/UIElements-TreeView-makeItem.html), replacing any existing callback.

```csharp
public static T SetMakeItem<T>(this T element, Func<VisualElement> value) where T : TreeView
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-1)\<VisualElement\>

The callback to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetUnbindItem\<T\>\(T, Action\<VisualElement, int\>\) {#Aspid_FastTools_UIElements_TreeViewExtensions_SetUnbindItem__1___0_System_Action_UnityEngine_UIElements_VisualElement_System_Int32__}

Sets [`unbindItem`](https://docs.unity3d.com/ScriptReference/UIElements-TreeView-unbindItem.html), replacing any existing callback.

```csharp
public static T SetUnbindItem<T>(this T element, Action<VisualElement, int> value) where T : TreeView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<VisualElement, [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

The callback to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

