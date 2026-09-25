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
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`ITextSelection`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection.html).

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

The element type.

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

The element type.

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

The element type.

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

The element type.

### SetCursorIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetCursorIndex__1___0_System_Int32_}

Sets [`cursorIndex`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-cursorIndex.html).

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

The element type.

### SetDoubleClickSelectsWord\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetDoubleClickSelectsWord__1___0_System_Boolean_}

Sets [`doubleClickSelectsWord`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-doubleClickSelectsWord.html).

```csharp
public static T SetDoubleClickSelectsWord<T>(this T element, bool value) where T : ITextSelection
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, a double click selects the word under the pointer.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetSelectAllOnFocus\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetSelectAllOnFocus__1___0_System_Boolean_}

Sets [`selectAllOnFocus`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-selectAllOnFocus.html).

```csharp
public static T SetSelectAllOnFocus<T>(this T element, bool value) where T : ITextSelection
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the whole text is selected when the element receives focus.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetSelectAllOnMouseUp\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetSelectAllOnMouseUp__1___0_System_Boolean_}

Sets [`selectAllOnMouseUp`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-selectAllOnMouseUp.html).

```csharp
public static T SetSelectAllOnMouseUp<T>(this T element, bool value) where T : ITextSelection
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the whole text is selected on the first mouse up.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetSelectIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetSelectIndex__1___0_System_Int32_}

Sets [`selectIndex`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-selectIndex.html).

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

The element type.

### SetSelectable\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetSelectable__1___0_System_Boolean_}

Sets [`isSelectable`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-isSelectable.html).

```csharp
public static T SetSelectable<T>(this T element, bool value) where T : ITextSelection
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the text can be selected.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetTripleClickSelectsLine\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextSelectionExtensions_SetTripleClickSelectsLine__1___0_System_Boolean_}

Sets [`tripleClickSelectsLine`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-tripleClickSelectsLine.html).

```csharp
public static T SetTripleClickSelectsLine<T>(this T element, bool value) where T : ITextSelection
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, a triple click selects the line under the pointer.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

