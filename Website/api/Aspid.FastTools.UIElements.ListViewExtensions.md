---
title: "Class ListViewExtensions"
sidebar_label: "ListViewExtensions"
description: "Class ListViewExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class ListViewExtensions {#Aspid_FastTools_UIElements_ListViewExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.Unity.dll  

```csharp
public static class ListViewExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[ListViewExtensions](Aspid.FastTools.UIElements.ListViewExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddBindItem\<T\>\(T, Action\<VisualElement, int\>\) {#Aspid_FastTools_UIElements_ListViewExtensions_AddBindItem__1___0_System_Action_UnityEngine_UIElements_VisualElement_System_Int32__}

Subscribes to the [`bindItem`](https://docs.unity3d.com/ScriptReference/UIElements-ListView-bindItem.html) callback.

```csharp
public static T AddBindItem<T>(this T element, Action<VisualElement, int> value) where T : ListView
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

#### Remarks

Callback for binding a data item to the visual element.

### AddDestroyItem\<T\>\(T, Action\<VisualElement\>\) {#Aspid_FastTools_UIElements_ListViewExtensions_AddDestroyItem__1___0_System_Action_UnityEngine_UIElements_VisualElement__}

Subscribes to the [`destroyItem`](https://docs.unity3d.com/ScriptReference/UIElements-ListView-destroyItem.html) callback.

```csharp
public static T AddDestroyItem<T>(this T element, Action<VisualElement> value) where T : ListView
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

#### Remarks

Callback invoked when a VisualElement created via makeItem is no longer needed and will be destroyed.

### AddMakeItem\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_ListViewExtensions_AddMakeItem__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Subscribes to the [`makeItem`](https://docs.unity3d.com/ScriptReference/UIElements-ListView-makeItem.html) callback.

```csharp
public static T AddMakeItem<T>(this T element, Func<VisualElement> value) where T : ListView
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-1)\<VisualElement\>

The callback to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Callback for constructing the VisualElement that is the template for each recycled and re-bound element in the element.

### AddUnbindItem\<T\>\(T, Action\<VisualElement, int\>\) {#Aspid_FastTools_UIElements_ListViewExtensions_AddUnbindItem__1___0_System_Action_UnityEngine_UIElements_VisualElement_System_Int32__}

Subscribes to the [`unbindItem`](https://docs.unity3d.com/ScriptReference/UIElements-ListView-unbindItem.html) callback.

```csharp
public static T AddUnbindItem<T>(this T element, Action<VisualElement, int> value) where T : ListView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<VisualElement, [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

The callback invoked to release bindings from a list item element.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Callback for unbinding a data item from the VisualElement.

### RemoveBindItem\<T\>\(T, Action\<VisualElement, int\>\) {#Aspid_FastTools_UIElements_ListViewExtensions_RemoveBindItem__1___0_System_Action_UnityEngine_UIElements_VisualElement_System_Int32__}

Unsubscribes from the [`bindItem`](https://docs.unity3d.com/ScriptReference/UIElements-ListView-bindItem.html) callback.

```csharp
public static T RemoveBindItem<T>(this T element, Action<VisualElement, int> value) where T : ListView
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

#### Remarks

Callback for binding a data item to the visual element.

### RemoveDestroyItem\<T\>\(T, Action\<VisualElement\>\) {#Aspid_FastTools_UIElements_ListViewExtensions_RemoveDestroyItem__1___0_System_Action_UnityEngine_UIElements_VisualElement__}

Unsubscribes from the [`destroyItem`](https://docs.unity3d.com/ScriptReference/UIElements-ListView-destroyItem.html) callback.

```csharp
public static T RemoveDestroyItem<T>(this T element, Action<VisualElement> value) where T : ListView
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

#### Remarks

Callback invoked when a VisualElement created via makeItem is no longer needed and will be destroyed.

### RemoveMakeItem\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_ListViewExtensions_RemoveMakeItem__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Unsubscribes from the [`makeItem`](https://docs.unity3d.com/ScriptReference/UIElements-ListView-makeItem.html) callback.

```csharp
public static T RemoveMakeItem<T>(this T element, Func<VisualElement> value) where T : ListView
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-1)\<VisualElement\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Callback for constructing the VisualElement that is the template for each recycled and re-bound element in the element.

### RemoveUnbindItem\<T\>\(T, Action\<VisualElement, int\>\) {#Aspid_FastTools_UIElements_ListViewExtensions_RemoveUnbindItem__1___0_System_Action_UnityEngine_UIElements_VisualElement_System_Int32__}

Unsubscribes from the [`unbindItem`](https://docs.unity3d.com/ScriptReference/UIElements-ListView-unbindItem.html) callback.

```csharp
public static T RemoveUnbindItem<T>(this T element, Action<VisualElement, int> value) where T : ListView
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

#### Remarks

Callback for unbinding a data item from the VisualElement.

### SetBindItem\<T\>\(T, Action\<VisualElement, int\>\) {#Aspid_FastTools_UIElements_ListViewExtensions_SetBindItem__1___0_System_Action_UnityEngine_UIElements_VisualElement_System_Int32__}

Sets [`bindItem`](https://docs.unity3d.com/ScriptReference/UIElements-ListView-bindItem.html), replacing any existing callback, and returns the element for chaining.

```csharp
public static T SetBindItem<T>(this T element, Action<VisualElement, int> value) where T : ListView
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

#### Remarks

Callback for binding a data item to the visual element.

### SetDestroyItem\<T\>\(T, Action\<VisualElement\>\) {#Aspid_FastTools_UIElements_ListViewExtensions_SetDestroyItem__1___0_System_Action_UnityEngine_UIElements_VisualElement__}

Sets [`destroyItem`](https://docs.unity3d.com/ScriptReference/UIElements-ListView-destroyItem.html), replacing any existing callback, and returns the element for chaining.

```csharp
public static T SetDestroyItem<T>(this T element, Action<VisualElement> value) where T : ListView
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

#### Remarks

Callback invoked when a VisualElement created via makeItem is no longer needed and will be destroyed.

### SetItemTemplate\<T\>\(T, VisualTreeAsset\) {#Aspid_FastTools_UIElements_ListViewExtensions_SetItemTemplate__1___0_UnityEngine_UIElements_VisualTreeAsset_}

Sets [`itemTemplate`](https://docs.unity3d.com/ScriptReference/UIElements-ListView-itemTemplate.html) and returns the element for chaining.

```csharp
public static T SetItemTemplate<T>(this T element, VisualTreeAsset value) where T : ListView
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

#### Remarks

A UXML template that constructs each recycled and rebound element within the element. This template is designed to replace the makeItem definition.

### SetMakeItem\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_ListViewExtensions_SetMakeItem__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Sets [`makeItem`](https://docs.unity3d.com/ScriptReference/UIElements-ListView-makeItem.html), replacing any existing callback, and returns the element for chaining.

```csharp
public static T SetMakeItem<T>(this T element, Func<VisualElement> value) where T : ListView
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

#### Remarks

Callback for constructing the VisualElement that is the template for each recycled and re-bound element in the element.

### SetUnbindItem\<T\>\(T, Action\<VisualElement, int\>\) {#Aspid_FastTools_UIElements_ListViewExtensions_SetUnbindItem__1___0_System_Action_UnityEngine_UIElements_VisualElement_System_Int32__}

Sets [`unbindItem`](https://docs.unity3d.com/ScriptReference/UIElements-ListView-unbindItem.html), replacing any existing callback, and returns the element for chaining.

```csharp
public static T SetUnbindItem<T>(this T element, Action<VisualElement, int> value) where T : ListView
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

#### Remarks

Callback for unbinding a data item from the VisualElement.

