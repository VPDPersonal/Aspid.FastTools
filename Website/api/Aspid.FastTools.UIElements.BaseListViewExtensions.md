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

### AddMakeFooter\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_AddMakeFooter__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Subscribes to the [`makeFooter`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-makeFooter.html) callback.

```csharp
public static T AddMakeFooter<T>(this T element, Func<VisualElement> value) where T : BaseListView
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

This callback allows the user to make their own footer for this control.

### AddMakeHeader\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_AddMakeHeader__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Subscribes to the [`makeHeader`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-makeHeader.html) callback.

```csharp
public static T AddMakeHeader<T>(this T element, Func<VisualElement> value) where T : BaseListView
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

This callback allows the user to make their own header for this control.

### AddMakeNoneElement\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_AddMakeNoneElement__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Subscribes to the [`makeNoneElement`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-makeNoneElement.html) callback.

```csharp
public static T AddMakeNoneElement<T>(this T element, Func<VisualElement> value) where T : BaseListView
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

This callback allows the user to set a Visual Element to replace the "List is empty" Label shown when the ListView is empty.

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

#### Remarks

This callback allows the user to implement their own code to be executed when the Add Button is clicked.

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

#### Remarks

This callback allows the user to implement their own code to be executed when the Remove Button is clicked.

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

#### Remarks

This callback allows the user to implement a DropdownMenu when the Add Button is clicked.

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

### RemoveMakeFooter\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_RemoveMakeFooter__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Unsubscribes from the [`makeFooter`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-makeFooter.html) callback.

```csharp
public static T RemoveMakeFooter<T>(this T element, Func<VisualElement> value) where T : BaseListView
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

This callback allows the user to make their own footer for this control.

### RemoveMakeHeader\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_RemoveMakeHeader__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Unsubscribes from the [`makeHeader`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-makeHeader.html) callback.

```csharp
public static T RemoveMakeHeader<T>(this T element, Func<VisualElement> value) where T : BaseListView
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

This callback allows the user to make their own header for this control.

### RemoveMakeNoneElement\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_RemoveMakeNoneElement__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Unsubscribes from the [`makeNoneElement`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-makeNoneElement.html) callback.

```csharp
public static T RemoveMakeNoneElement<T>(this T element, Func<VisualElement> value) where T : BaseListView
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

This callback allows the user to set a Visual Element to replace the "List is empty" Label shown when the ListView is empty.

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

#### Remarks

This callback allows the user to implement their own code to be executed when the Add Button is clicked.

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

#### Remarks

This callback allows the user to implement their own code to be executed when the Remove Button is clicked.

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

#### Remarks

This callback allows the user to implement a DropdownMenu when the Add Button is clicked.

### SetAllowAdd\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetAllowAdd__1___0_System_Boolean_}

Sets [`allowAdd`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-allowAdd.html) and returns the element for chaining.

```csharp
public static T SetAllowAdd<T>(this T element, bool value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether item addition is allowed.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

This property allows the user to allow or block the addition of an item when clicking on the Add Button. It must return true or false.

### SetAllowRemove\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetAllowRemove__1___0_System_Boolean_}

Sets [`allowRemove`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-allowRemove.html) and returns the element for chaining.

```csharp
public static T SetAllowRemove<T>(this T element, bool value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether item removal is allowed.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

This property allows the user to allow or block the removal of an item when clicking on the Remove Button. It must return true or false.

### SetBindingSourceSelectionMode\<T\>\(T, BindingSourceSelectionMode\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetBindingSourceSelectionMode__1___0_UnityEngine_UIElements_BindingSourceSelectionMode_}

Sets [`bindingSourceSelectionMode`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-bindingSourceSelectionMode.html) and returns the element for chaining.

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

#### Remarks

This property controls whether every element in the element will get its data source setup automatically to the correct item in the collection's source.

### SetHeaderTitle\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetHeaderTitle__1___0_System_String_}

Sets [`headerTitle`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-headerTitle.html) and returns the element for chaining.

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

#### Remarks

This property controls the text of the foldout header when using showFoldoutHeader.

### SetMakeFooter\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetMakeFooter__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Sets [`makeFooter`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-makeFooter.html), replacing any existing callback, and returns the element for chaining.

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

#### Remarks

This callback allows the user to make their own footer for this control.

### SetMakeHeader\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetMakeHeader__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Sets [`makeHeader`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-makeHeader.html), replacing any existing callback, and returns the element for chaining.

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

#### Remarks

This callback allows the user to make their own header for this control.

### SetMakeNoneElement\<T\>\(T, Func\<VisualElement\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetMakeNoneElement__1___0_System_Func_UnityEngine_UIElements_VisualElement__}

Sets [`makeNoneElement`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-makeNoneElement.html), replacing any existing callback, and returns the element for chaining.

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

#### Remarks

This callback allows the user to set a Visual Element to replace the "List is empty" Label shown when the ListView is empty.

### SetOnAdd\<T\>\(T, Action\<BaseListView\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetOnAdd__1___0_System_Action_UnityEngine_UIElements_BaseListView__}

Sets [`onAdd`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-onAdd.html), replacing any existing callback, and returns the element for chaining.

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

#### Remarks

This callback allows the user to implement their own code to be executed when the Add Button is clicked.

### SetOnRemove\<T\>\(T, Action\<BaseListView\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetOnRemove__1___0_System_Action_UnityEngine_UIElements_BaseListView__}

Sets [`onRemove`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-onRemove.html), replacing any existing callback, and returns the element for chaining.

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

#### Remarks

This callback allows the user to implement their own code to be executed when the Remove Button is clicked.

### SetOverridingAddButtonBehavior\<T\>\(T, Action\<BaseListView, Button\>\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetOverridingAddButtonBehavior__1___0_System_Action_UnityEngine_UIElements_BaseListView_UnityEngine_UIElements_Button__}

Sets [`overridingAddButtonBehavior`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-overridingAddButtonBehavior.html), replacing any existing callback, and returns the element for chaining.

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

#### Remarks

This callback allows the user to implement a DropdownMenu when the Add Button is clicked.

### SetReorderMode\<T\>\(T, ListViewReorderMode\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetReorderMode__1___0_UnityEngine_UIElements_ListViewReorderMode_}

Sets [`reorderMode`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-reorderMode.html) and returns the element for chaining.

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

#### Remarks

This property controls the drag and drop mode for the element view.

### SetShowAddRemoveFooter\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetShowAddRemoveFooter__1___0_System_Boolean_}

Sets [`showAddRemoveFooter`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-showAddRemoveFooter.html) and returns the element for chaining.

```csharp
public static T SetShowAddRemoveFooter<T>(this T element, bool value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to show the add/remove footer.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

This property controls whether a footer will be added to the list view.

### SetShowBoundCollectionSize\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetShowBoundCollectionSize__1___0_System_Boolean_}

Sets [`showBoundCollectionSize`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-showBoundCollectionSize.html) and returns the element for chaining.

```csharp
public static T SetShowBoundCollectionSize<T>(this T element, bool value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to show the bound collection size.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

This property controls whether the element view displays the collection size (number of items).

### SetShowFoldoutHeader\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_BaseListViewExtensions_SetShowFoldoutHeader__1___0_System_Boolean_}

Sets [`showFoldoutHeader`](https://docs.unity3d.com/ScriptReference/UIElements-BaseListView-showFoldoutHeader.html) and returns the element for chaining.

```csharp
public static T SetShowFoldoutHeader<T>(this T element, bool value) where T : BaseListView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to show the foldout header.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

This property controls whether the element view displays a header, in the form of a foldout that can be expanded or collapsed.

