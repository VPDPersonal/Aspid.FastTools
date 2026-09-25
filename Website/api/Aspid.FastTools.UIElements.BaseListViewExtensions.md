---
title: "Class BaseListViewExtensions"
sidebar_label: "BaseListViewExtensions"
description: "Class BaseListViewExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseListViewExtensions {#Aspid_FastTools_UIElements_BaseListViewExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`BaseListView`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView.html).

```csharp
public static class BaseListViewExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseListViewExtensions](Aspid.FastTools.UIElements.BaseListViewExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddItemsAdded\<T\>\(T, Action\<IEnumerable\<int\>\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_AddItemsAdded__1___0_System_Action_System_Collections_Generic_IEnumerable_System_Int32___}

Subscribes to the [`itemsAdded`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-itemsAdded.html) event.

```csharp
public static T AddItemsAdded<T>(this T element, Action<IEnumerable<int>> value) where T : BaseListView
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

The element type.

### AddItemsRemoved\<T\>\(T, Action\<IEnumerable\<int\>\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_AddItemsRemoved__1___0_System_Action_System_Collections_Generic_IEnumerable_System_Int32___}

Subscribes to the [`itemsRemoved`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-itemsRemoved.html) event.

```csharp
public static T AddItemsRemoved<T>(this T element, Action<IEnumerable<int>> value) where T : BaseListView
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

The element type.

### AddOnAdd\<T\>\(T, Action\<BaseListView\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_AddOnAdd__1___0_System_Action_UnityEngine_UIElements_BaseListView__}

Subscribes to the [`onAdd`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-onAdd.html) callback.

```csharp
public static T AddOnAdd<T>(this T element, Action<BaseListView> value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<BaseListView\>

The callback to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### AddOnRemove\<T\>\(T, Action\<BaseListView\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_AddOnRemove__1___0_System_Action_UnityEngine_UIElements_BaseListView__}

Subscribes to the [`onRemove`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-onRemove.html) callback.

```csharp
public static T AddOnRemove<T>(this T element, Action<BaseListView> value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<BaseListView\>

The callback to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### AddOverridingAddButtonBehavior\<T\>\(T, Action\<BaseListView, Button\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_AddOverridingAddButtonBehavior__1___0_System_Action_UnityEngine_UIElements_BaseListView_UnityEngine_UIElements_Button__}

Subscribes to the [`overridingAddButtonBehavior`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-overridingAddButtonBehavior.html) callback.

```csharp
public static T AddOverridingAddButtonBehavior<T>(this T element, Action<BaseListView, Button> value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<BaseListView, Button\>

The callback to add.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveItemsAdded\<T\>\(T, Action\<IEnumerable\<int\>\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_RemoveItemsAdded__1___0_System_Action_System_Collections_Generic_IEnumerable_System_Int32___}

Unsubscribes from the [`itemsAdded`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-itemsAdded.html) event.

```csharp
public static T RemoveItemsAdded<T>(this T element, Action<IEnumerable<int>> value) where T : BaseListView
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

The element type.

### RemoveItemsRemoved\<T\>\(T, Action\<IEnumerable\<int\>\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_RemoveItemsRemoved__1___0_System_Action_System_Collections_Generic_IEnumerable_System_Int32___}

Unsubscribes from the [`itemsRemoved`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-itemsRemoved.html) event.

```csharp
public static T RemoveItemsRemoved<T>(this T element, Action<IEnumerable<int>> value) where T : BaseListView
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

The element type.

### RemoveOnAdd\<T\>\(T, Action\<BaseListView\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_RemoveOnAdd__1___0_System_Action_UnityEngine_UIElements_BaseListView__}

Unsubscribes from the [`onAdd`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-onAdd.html) callback.

```csharp
public static T RemoveOnAdd<T>(this T element, Action<BaseListView> value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<BaseListView\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveOnRemove\<T\>\(T, Action\<BaseListView\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_RemoveOnRemove__1___0_System_Action_UnityEngine_UIElements_BaseListView__}

Unsubscribes from the [`onRemove`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-onRemove.html) callback.

```csharp
public static T RemoveOnRemove<T>(this T element, Action<BaseListView> value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<BaseListView\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveOverridingAddButtonBehavior\<T\>\(T, Action\<BaseListView, Button\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_RemoveOverridingAddButtonBehavior__1___0_System_Action_UnityEngine_UIElements_BaseListView_UnityEngine_UIElements_Button__}

Unsubscribes from the [`overridingAddButtonBehavior`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-overridingAddButtonBehavior.html) callback.

```csharp
public static T RemoveOverridingAddButtonBehavior<T>(this T element, Action<BaseListView, Button> value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<BaseListView, Button\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetAllowAdd\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetAllowAdd__1___0_System_Boolean_}

Sets [`allowAdd`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-allowAdd.html).

```csharp
public static T SetAllowAdd<T>(this T element, bool value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the Add button adds an item.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetAllowRemove\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetAllowRemove__1___0_System_Boolean_}

Sets [`allowRemove`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-allowRemove.html).

```csharp
public static T SetAllowRemove<T>(this T element, bool value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the Remove button removes an item.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetBindingSourceSelectionMode\<T\>\(T, BindingSourceSelectionMode\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetBindingSourceSelectionMode__1___0_UnityEngine_UIElements_BindingSourceSelectionMode_}

Sets [`bindingSourceSelectionMode`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-bindingSourceSelectionMode.html).

```csharp
public static T SetBindingSourceSelectionMode<T>(this T element, BindingSourceSelectionMode value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` BindingSourceSelectionMode

The binding source selection mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetHeaderTitle\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetHeaderTitle__1___0_System_String_}

Sets [`headerTitle`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-headerTitle.html).

```csharp
public static T SetHeaderTitle<T>(this T element, string value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The header title to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetMakeFooter\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetMakeFooter__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Sets [`makeFooter`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-makeFooter.html), replacing any existing callback.

```csharp
public static T SetMakeFooter<T>(this T element, Func<VisualElement> value) where T : BaseListView
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

### SetMakeHeader\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetMakeHeader__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Sets [`makeHeader`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-makeHeader.html), replacing any existing callback.

```csharp
public static T SetMakeHeader<T>(this T element, Func<VisualElement> value) where T : BaseListView
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

### SetMakeNoneElement\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetMakeNoneElement__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Sets [`makeNoneElement`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-makeNoneElement.html), replacing any existing callback.

```csharp
public static T SetMakeNoneElement<T>(this T element, Func<VisualElement> value) where T : BaseListView
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

### SetOnAdd\<T\>\(T, Action\<BaseListView\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetOnAdd__1___0_System_Action_UnityEngine_UIElements_BaseListView__}

Sets [`onAdd`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-onAdd.html), replacing any existing callback.

```csharp
public static T SetOnAdd<T>(this T element, Action<BaseListView> value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<BaseListView\>

The callback to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetOnRemove\<T\>\(T, Action\<BaseListView\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetOnRemove__1___0_System_Action_UnityEngine_UIElements_BaseListView__}

Sets [`onRemove`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-onRemove.html), replacing any existing callback.

```csharp
public static T SetOnRemove<T>(this T element, Action<BaseListView> value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<BaseListView\>

The callback to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetOverridingAddButtonBehavior\<T\>\(T, Action\<BaseListView, Button\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetOverridingAddButtonBehavior__1___0_System_Action_UnityEngine_UIElements_BaseListView_UnityEngine_UIElements_Button__}

Sets [`overridingAddButtonBehavior`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-overridingAddButtonBehavior.html), replacing any existing callback.

```csharp
public static T SetOverridingAddButtonBehavior<T>(this T element, Action<BaseListView, Button> value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<BaseListView, Button\>

The callback to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetReorderMode\<T\>\(T, ListViewReorderMode\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetReorderMode__1___0_UnityEngine_UIElements_ListViewReorderMode_}

Sets [`reorderMode`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-reorderMode.html).

```csharp
public static T SetReorderMode<T>(this T element, ListViewReorderMode value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` ListViewReorderMode

The reorder mode to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetShowAddRemoveFooter\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetShowAddRemoveFooter__1___0_System_Boolean_}

Sets [`showAddRemoveFooter`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-showAddRemoveFooter.html).

```csharp
public static T SetShowAddRemoveFooter<T>(this T element, bool value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the footer with the Add and Remove buttons is shown.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetShowBoundCollectionSize\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetShowBoundCollectionSize__1___0_System_Boolean_}

Sets [`showBoundCollectionSize`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-showBoundCollectionSize.html).

```csharp
public static T SetShowBoundCollectionSize<T>(this T element, bool value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the size of the bound collection is shown.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetShowFoldoutHeader\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetShowFoldoutHeader__1___0_System_Boolean_}

Sets [`showFoldoutHeader`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-showFoldoutHeader.html).

```csharp
public static T SetShowFoldoutHeader<T>(this T element, bool value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the list is wrapped in a foldout header.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

