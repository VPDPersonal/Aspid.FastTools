---
title: "Class TabViewExtensions"
sidebar_label: "TabViewExtensions"
description: "Class TabViewExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TabViewExtensions {#Aspid_FastTools_UIElements_TabViewExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`TabView`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TabView.html).

```csharp
public static class TabViewExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TabViewExtensions](Aspid.FastTools.UIElements.TabViewExtensions.md)


## Methods

### AddActiveTabChanged\<T\>\(T, Action\<Tab, Tab\>\) {#Aspid_FastTools_UIElements_TabViewExtensions_AddActiveTabChanged__1___0_System_Action_UnityEngine_UIElements_Tab_UnityEngine_UIElements_Tab__}

Subscribes to the [`activeTabChanged`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TabView-activeTabChanged.html) event.

```csharp
public static T AddActiveTabChanged<T>(this T element, Action<Tab, Tab> value) where T : TabView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<Tab, Tab\>

The callback to subscribe; it receives the previous and the new active tab.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### AddTabClosed\<T\>\(T, Action\<Tab, int\>\) {#Aspid_FastTools_UIElements_TabViewExtensions_AddTabClosed__1___0_System_Action_UnityEngine_UIElements_Tab_System_Int32__}

Subscribes to the [`tabClosed`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TabView-tabClosed.html) event.

```csharp
public static T AddTabClosed<T>(this T element, Action<Tab, int> value) where T : TabView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<Tab, [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

The callback to subscribe; it receives the closed tab and its former index.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### AddTabReordered\<T\>\(T, Action\<int, int\>\) {#Aspid_FastTools_UIElements_TabViewExtensions_AddTabReordered__1___0_System_Action_System_Int32_System_Int32__}

Subscribes to the [`tabReordered`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TabView-tabReordered.html) event.

```csharp
public static T AddTabReordered<T>(this T element, Action<int, int> value) where T : TabView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<[int](https://learn.microsoft.com/dotnet/api/system.int32), [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

The callback to subscribe; it receives the old and the new tab index.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveActiveTabChanged\<T\>\(T, Action\<Tab, Tab\>\) {#Aspid_FastTools_UIElements_TabViewExtensions_RemoveActiveTabChanged__1___0_System_Action_UnityEngine_UIElements_Tab_UnityEngine_UIElements_Tab__}

Unsubscribes from the [`activeTabChanged`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TabView-activeTabChanged.html) event.

```csharp
public static T RemoveActiveTabChanged<T>(this T element, Action<Tab, Tab> value) where T : TabView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<Tab, Tab\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveTabClosed\<T\>\(T, Action\<Tab, int\>\) {#Aspid_FastTools_UIElements_TabViewExtensions_RemoveTabClosed__1___0_System_Action_UnityEngine_UIElements_Tab_System_Int32__}

Unsubscribes from the [`tabClosed`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TabView-tabClosed.html) event.

```csharp
public static T RemoveTabClosed<T>(this T element, Action<Tab, int> value) where T : TabView
```

#### Parameters

`element` T

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action-2)\<Tab, [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### RemoveTabReordered\<T\>\(T, Action\<int, int\>\) {#Aspid_FastTools_UIElements_TabViewExtensions_RemoveTabReordered__1___0_System_Action_System_Int32_System_Int32__}

Unsubscribes from the [`tabReordered`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TabView-tabReordered.html) event.

```csharp
public static T RemoveTabReordered<T>(this T element, Action<int, int> value) where T : TabView
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

The element type.

### SetActiveTab\<T\>\(T, Tab\) {#Aspid_FastTools_UIElements_TabViewExtensions_SetActiveTab__1___0_UnityEngine_UIElements_Tab_}

Sets [`activeTab`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TabView-activeTab.html).

```csharp
public static T SetActiveTab<T>(this T element, Tab value) where T : TabView
```

#### Parameters

`element` T

The element to modify.

`value` Tab

The tab to activate.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Unity throws when <code class="paramref">value</code> is not a tab of this view, so add the tabs first.

### SetReorderable\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TabViewExtensions_SetReorderable__1___0_System_Boolean_}

Sets [`reorderable`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TabView-reorderable.html).

```csharp
public static T SetReorderable<T>(this T element, bool value) where T : TabView
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, tabs can be reordered by dragging their headers.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetSelectedTabIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_TabViewExtensions_SetSelectedTabIndex__1___0_System_Int32_}

Sets [`selectedTabIndex`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TabView-selectedTabIndex.html).

```csharp
public static T SetSelectedTabIndex<T>(this T element, int value) where T : TabView
```

#### Parameters

`element` T

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index of the tab to activate.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Unity ignores an index outside the tab range, so add the tabs first.

