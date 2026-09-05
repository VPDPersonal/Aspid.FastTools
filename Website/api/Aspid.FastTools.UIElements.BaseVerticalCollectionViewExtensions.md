---
title: "Class BaseVerticalCollectionViewExtensions"
sidebar_label: "BaseVerticalCollectionViewExtensions"
description: "Class BaseVerticalCollectionViewExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseVerticalCollectionViewExtensions {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.Unity.dll  

```csharp
public static class BaseVerticalCollectionViewExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseVerticalCollectionViewExtensions](Aspid.FastTools.UIElements.BaseVerticalCollectionViewExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddCanStartDrag\<T\>\(T, Func\<CanStartDragArgs, bool\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_AddCanStartDrag__1___0_System_Func_UnityEngine_UIElements_CanStartDragArgs_System_Boolean__}

Subscribes to the [`canStartDrag`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-canStartDrag.html) callback.

```csharp
public static T AddCanStartDrag<T>(this T element, Func<CanStartDragArgs, bool> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<CanStartDragArgs, [bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddDragAndDropUpdate\<T\>\(T, Func\<HandleDragAndDropArgs, DragVisualMode\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_AddDragAndDropUpdate__1___0_System_Func_UnityEngine_UIElements_HandleDragAndDropArgs_UnityEngine_UIElements_DragVisualMode__}

Subscribes to the [`dragAndDropUpdate`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-dragAndDropUpdate.html) callback.

```csharp
public static T AddDragAndDropUpdate<T>(this T element, Func<HandleDragAndDropArgs, DragVisualMode> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<HandleDragAndDropArgs, DragVisualMode\>

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddHandleDrop\<T\>\(T, Func\<HandleDragAndDropArgs, DragVisualMode\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_AddHandleDrop__1___0_System_Func_UnityEngine_UIElements_HandleDragAndDropArgs_UnityEngine_UIElements_DragVisualMode__}

Subscribes to the [`handleDrop`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-handleDrop.html) callback.

```csharp
public static T AddHandleDrop<T>(this T element, Func<HandleDragAndDropArgs, DragVisualMode> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<HandleDragAndDropArgs, DragVisualMode\>

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddItemIndexChanged\<T\>\(T, Action\<int, int\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_AddItemIndexChanged__1___0_System_Action_System_Int32_System_Int32__}

Subscribes to the [`itemIndexChanged`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-itemIndexChanged.html) event.

```csharp
public static T AddItemIndexChanged<T>(this T element, Action<int, int> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<[int](https://learn.microsoft.com/dotnet/api/system.int32), [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddItemsChosen\<T\>\(T, Action\<IEnumerable\<object\>\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_AddItemsChosen__1___0_System_Action_System_Collections_Generic_IEnumerable_System_Object___}

Subscribes to the [`itemsChosen`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-itemsChosen.html) event.

```csharp
public static T AddItemsChosen<T>(this T element, Action<IEnumerable<object>> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<[IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[object](https://learn.microsoft.com/dotnet/api/system.object)\>\>

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddItemsSourceChanged\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_AddItemsSourceChanged__1___0_System_Action_}

Subscribes to the [`itemsSourceChanged`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-itemsSourceChanged.html) event.

```csharp
public static T AddItemsSourceChanged<T>(this T element, Action value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddSelectedIndicesChanged\<T\>\(T, Action\<IEnumerable\<int\>\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_AddSelectedIndicesChanged__1___0_System_Action_System_Collections_Generic_IEnumerable_System_Int32___}

Subscribes to the [`selectedIndicesChanged`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-selectedIndicesChanged.html) event.

```csharp
public static T AddSelectedIndicesChanged<T>(this T element, Action<IEnumerable<int>> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<[IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[int](https://learn.microsoft.com/dotnet/api/system.int32)\>\>

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddSelectionChanged\<T\>\(T, Action\<IEnumerable\<object\>\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_AddSelectionChanged__1___0_System_Action_System_Collections_Generic_IEnumerable_System_Object___}

Subscribes to the [`selectionChanged`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-selectionChanged.html) event.

```csharp
public static T AddSelectionChanged<T>(this T element, Action<IEnumerable<object>> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<[IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[object](https://learn.microsoft.com/dotnet/api/system.object)\>\>

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### AddSetupDragAndDrop\<T\>\(T, Func\<SetupDragAndDropArgs, StartDragArgs\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_AddSetupDragAndDrop__1___0_System_Func_UnityEngine_UIElements_SetupDragAndDropArgs_UnityEngine_UIElements_StartDragArgs__}

Subscribes to the [`setupDragAndDrop`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-setupDragAndDrop.html) callback.

```csharp
public static T AddSetupDragAndDrop<T>(this T element, Func<SetupDragAndDropArgs, StartDragArgs> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<SetupDragAndDropArgs, StartDragArgs\>

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveCanStartDrag\<T\>\(T, Func\<CanStartDragArgs, bool\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_RemoveCanStartDrag__1___0_System_Func_UnityEngine_UIElements_CanStartDragArgs_System_Boolean__}

Unsubscribes from the [`canStartDrag`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-canStartDrag.html) callback.

```csharp
public static T RemoveCanStartDrag<T>(this T element, Func<CanStartDragArgs, bool> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<CanStartDragArgs, [bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveDragAndDropUpdate\<T\>\(T, Func\<HandleDragAndDropArgs, DragVisualMode\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_RemoveDragAndDropUpdate__1___0_System_Func_UnityEngine_UIElements_HandleDragAndDropArgs_UnityEngine_UIElements_DragVisualMode__}

Unsubscribes from the [`dragAndDropUpdate`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-dragAndDropUpdate.html) callback.

```csharp
public static T RemoveDragAndDropUpdate<T>(this T element, Func<HandleDragAndDropArgs, DragVisualMode> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<HandleDragAndDropArgs, DragVisualMode\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveHandleDrop\<T\>\(T, Func\<HandleDragAndDropArgs, DragVisualMode\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_RemoveHandleDrop__1___0_System_Func_UnityEngine_UIElements_HandleDragAndDropArgs_UnityEngine_UIElements_DragVisualMode__}

Unsubscribes from the [`handleDrop`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-handleDrop.html) callback.

```csharp
public static T RemoveHandleDrop<T>(this T element, Func<HandleDragAndDropArgs, DragVisualMode> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<HandleDragAndDropArgs, DragVisualMode\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveItemIndexChanged\<T\>\(T, Action\<int, int\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_RemoveItemIndexChanged__1___0_System_Action_System_Int32_System_Int32__}

Unsubscribes from the [`itemIndexChanged`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-itemIndexChanged.html) event.

```csharp
public static T RemoveItemIndexChanged<T>(this T element, Action<int, int> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<[int](https://learn.microsoft.com/dotnet/api/system.int32), [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveItemsChosen\<T\>\(T, Action\<IEnumerable\<object\>\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_RemoveItemsChosen__1___0_System_Action_System_Collections_Generic_IEnumerable_System_Object___}

Unsubscribes from the [`itemsChosen`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-itemsChosen.html) event.

```csharp
public static T RemoveItemsChosen<T>(this T element, Action<IEnumerable<object>> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<[IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[object](https://learn.microsoft.com/dotnet/api/system.object)\>\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveItemsSourceChanged\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_RemoveItemsSourceChanged__1___0_System_Action_}

Unsubscribes from the [`itemsSourceChanged`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-itemsSourceChanged.html) event.

```csharp
public static T RemoveItemsSourceChanged<T>(this T element, Action value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveSelectedIndicesChanged\<T\>\(T, Action\<IEnumerable\<int\>\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_RemoveSelectedIndicesChanged__1___0_System_Action_System_Collections_Generic_IEnumerable_System_Int32___}

Unsubscribes from the [`selectedIndicesChanged`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-selectedIndicesChanged.html) event.

```csharp
public static T RemoveSelectedIndicesChanged<T>(this T element, Action<IEnumerable<int>> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<[IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[int](https://learn.microsoft.com/dotnet/api/system.int32)\>\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveSelectionChanged\<T\>\(T, Action\<IEnumerable\<object\>\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_RemoveSelectionChanged__1___0_System_Action_System_Collections_Generic_IEnumerable_System_Object___}

Unsubscribes from the [`selectionChanged`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-selectionChanged.html) event.

```csharp
public static T RemoveSelectionChanged<T>(this T element, Action<IEnumerable<object>> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<[IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[object](https://learn.microsoft.com/dotnet/api/system.object)\>\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveSetupDragAndDrop\<T\>\(T, Func\<SetupDragAndDropArgs, StartDragArgs\>\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_RemoveSetupDragAndDrop__1___0_System_Func_UnityEngine_UIElements_SetupDragAndDropArgs_UnityEngine_UIElements_StartDragArgs__}

Unsubscribes from the [`setupDragAndDrop`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-setupDragAndDrop.html) callback.

```csharp
public static T RemoveSetupDragAndDrop<T>(this T element, Func<SetupDragAndDropArgs, StartDragArgs> value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<SetupDragAndDropArgs, StartDragArgs\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### SetFixedItemHeight\<T\>\(T, float\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_SetFixedItemHeight__1___0_System_Single_}

Sets [`fixedItemHeight`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-fixedItemHeight.html) and returns the element for chaining.

```csharp
public static T SetFixedItemHeight<T>(this T element, float value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The fixed item height in pixels.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The height of a single item in the list, in pixels.

### SetHorizontalScrollingEnabled\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_SetHorizontalScrollingEnabled__1___0_System_Boolean_}

Sets [`horizontalScrollingEnabled`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-horizontalScrollingEnabled.html) and returns the element for chaining.

```csharp
public static T SetHorizontalScrollingEnabled<T>(this T element, bool value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether horizontal scrolling is enabled.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

This property controls whether the collection view shows a horizontal scroll bar when its content does not fit in the visible area.

### SetItemsSource\<T\>\(T, IList\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_SetItemsSource__1___0_System_Collections_IList_}

Sets [`itemsSource`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-itemsSource.html) and returns the element for chaining.

```csharp
public static T SetItemsSource<T>(this T element, IList value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [IList](https://learn.microsoft.com/dotnet/api/system.collections.ilist)

The items source to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The data source for collection items.

### SetReorderable\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_SetReorderable__1___0_System_Boolean_}

Sets [`reorderable`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-reorderable.html) and returns the element for chaining.

```csharp
public static T SetReorderable<T>(this T element, bool value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether items can be reordered by dragging.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Gets or sets a value that indicates whether the user can drag list items to reorder them.

### SetSelectedIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_SetSelectedIndex__1___0_System_Int32_}

Sets [`selectedIndex`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-selectedIndex.html) and returns the element for chaining.

```csharp
public static T SetSelectedIndex<T>(this T element, int value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The selected index to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Returns or sets the selected item's index in the data source. If multiple items are selected, returns the first selected item's index. If multiple items are provided, sets them all as selected. If no item is selected, returns -1.

### SetSelectionType\<T\>\(T, SelectionType\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_SetSelectionType__1___0_UnityEngine_UIElements_SelectionType_}

Sets [`selectionType`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-selectionType.html) and returns the element for chaining.

```csharp
public static T SetSelectionType<T>(this T element, SelectionType value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` SelectionType

The selection type to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Controls the selection type.

### SetShowAlternatingRowBackgrounds\<T\>\(T, AlternatingRowBackground\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_SetShowAlternatingRowBackgrounds__1___0_UnityEngine_UIElements_AlternatingRowBackground_}

Sets [`showAlternatingRowBackgrounds`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-showAlternatingRowBackgrounds.html) and returns the element for chaining.

```csharp
public static T SetShowAlternatingRowBackgrounds<T>(this T element, AlternatingRowBackground value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` AlternatingRowBackground

The alternating row background mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

This property controls whether the background colors of collection view rows alternate. Takes a value from the AlternatingRowBackground enum.

### SetVirtualizationMethod\<T\>\(T, CollectionVirtualizationMethod\) {#Aspid_FastTools_UIElements_BaseVerticalCollectionViewExtensions_SetVirtualizationMethod__1___0_UnityEngine_UIElements_CollectionVirtualizationMethod_}

Sets [`virtualizationMethod`](https://docs.unity3d.com/ScriptReference/UIElements-BaseVerticalCollectionView-virtualizationMethod.html) and returns the element for chaining.

```csharp
public static T SetVirtualizationMethod<T>(this T element, CollectionVirtualizationMethod value) where T : BaseVerticalCollectionView
```

#### Parameters

`element` T

The element to modify.

`value` CollectionVirtualizationMethod

The virtualization method to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

The virtualization method to use for this collection when a scroll bar is visible. Takes a value from the CollectionVirtualizationMethod enum.

