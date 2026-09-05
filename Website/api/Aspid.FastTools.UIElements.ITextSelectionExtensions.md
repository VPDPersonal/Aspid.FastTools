---
title: "Class ITextSelectionExtensions"
sidebar_label: "ITextSelectionExtensions"
description: "Class ITextSelectionExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class ITextSelectionExtensions {#Aspid_FastTools_UIElements_ITextSelectionExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.Unity.dll  

```csharp
public static class ITextSelectionExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[ITextSelectionExtensions](Aspid.FastTools.UIElements.ITextSelectionExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddOnCursorIndexChange\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_AddOnCursorIndexChange__1___0_System_Action_}

Subscribes to the [`OnCursorIndexChange`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-OnCursorIndexChange.html) event.

```csharp
public static T AddOnCursorIndexChange<T>(this T element, Action value) where T : ITextSelection
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

### AddOnSelectIndexChange\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_AddOnSelectIndexChange__1___0_System_Action_}

Subscribes to the [`OnSelectIndexChange`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-OnSelectIndexChange.html) event.

```csharp
public static T AddOnSelectIndexChange<T>(this T element, Action value) where T : ITextSelection
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

### RemoveOnCursorIndexChange\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_RemoveOnCursorIndexChange__1___0_System_Action_}

Unsubscribes from the [`OnCursorIndexChange`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-OnCursorIndexChange.html) event.

```csharp
public static T RemoveOnCursorIndexChange<T>(this T element, Action value) where T : ITextSelection
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

### RemoveOnSelectIndexChange\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_RemoveOnSelectIndexChange__1___0_System_Action_}

Unsubscribes from the [`OnSelectIndexChange`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-OnSelectIndexChange.html) event.

```csharp
public static T RemoveOnSelectIndexChange<T>(this T element, Action value) where T : ITextSelection
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

### SetCursorIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetCursorIndex__1___0_System_Int32_}

Sets [`cursorIndex`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-cursorIndex.html) and returns the element for chaining.

```csharp
public static T SetCursorIndex<T>(this T element, int value) where T : ITextSelection
```

#### Parameters

`element` T

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The cursor index to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

This is the cursor index in the text presented.

### SetDoubleClickSelectsWord\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetDoubleClickSelectsWord__1___0_System_Boolean_}

Sets [`doubleClickSelectsWord`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-doubleClickSelectsWord.html) and returns the element for chaining.

```csharp
public static T SetDoubleClickSelectsWord<T>(this T element, bool value) where T : ITextSelection
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether double-clicking selects a word.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Controls whether double-clicking selects the word under the mouse pointer.

### SetIsSelectable\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetIsSelectable__1___0_System_Boolean_}

Sets [`isSelectable`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-isSelectable.html) and returns the element for chaining.

```csharp
public static T SetIsSelectable<T>(this T element, bool value) where T : ITextSelection
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether the field is selectable.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

When set to true, the field becomes selectable.

### SetSelectAllOnFocus\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetSelectAllOnFocus__1___0_System_Boolean_}

Sets [`selectAllOnFocus`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-selectAllOnFocus.html) and returns the element for chaining.

```csharp
public static T SetSelectAllOnFocus<T>(this T element, bool value) where T : ITextSelection
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to select all content on focus.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Controls whether the element's content is selected upon receiving focus.

### SetSelectAllOnMouseUp\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetSelectAllOnMouseUp__1___0_System_Boolean_}

Sets [`selectAllOnMouseUp`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-selectAllOnMouseUp.html) and returns the element for chaining.

```csharp
public static T SetSelectAllOnMouseUp<T>(this T element, bool value) where T : ITextSelection
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to select all content on the first mouse up.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Controls whether the element's content is selected when you mouse up for the first time.

### SetSelectIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetSelectIndex__1___0_System_Int32_}

Sets [`selectIndex`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-selectIndex.html) and returns the element for chaining.

```csharp
public static T SetSelectIndex<T>(this T element, int value) where T : ITextSelection
```

#### Parameters

`element` T

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The selection index to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

This is the selection index in the text presented.

### SetTripleClickSelectsLine\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetTripleClickSelectsLine__1___0_System_Boolean_}

Sets [`tripleClickSelectsLine`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-tripleClickSelectsLine.html) and returns the element for chaining.

```csharp
public static T SetTripleClickSelectsLine<T>(this T element, bool value) where T : ITextSelection
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether triple-clicking selects a line.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Controls whether triple-clicking selects the entire line under the mouse pointer.

