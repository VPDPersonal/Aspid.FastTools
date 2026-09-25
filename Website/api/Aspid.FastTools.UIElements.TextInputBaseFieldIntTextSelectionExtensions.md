---
title: "Class TextInputBaseFieldIntTextSelectionExtensions"
sidebar_label: "TextInputBaseFieldIntTextSelectionExtensions"
description: "Class TextInputBaseFieldIntTextSelectionExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TextInputBaseFieldIntTextSelectionExtensions {#Aspid_FastTools_UIElements_TextInputBaseFieldIntTextSelectionExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides text-selection extension methods for [`TextInputBaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField.html) of [`Int32`](https://learn.microsoft.com/dotnet/api/system.int32).

```csharp
public static class TextInputBaseFieldIntTextSelectionExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TextInputBaseFieldIntTextSelectionExtensions](Aspid.FastTools.UIElements.TextInputBaseFieldIntTextSelectionExtensions.md)


## Methods

### AddOnCursorIndexChange\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_TextInputBaseFieldIntTextSelectionExtensions_AddOnCursorIndexChange__1___0_System_Action_}

Subscribes to the [`OnCursorIndexChange`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-OnCursorIndexChange.html) event of [`textSelection`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-textSelection.html).

```csharp
public static T AddOnCursorIndexChange<T>(this T element, Action value) where T : TextInputBaseField<int>
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

The field type.

### AddOnSelectIndexChange\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_TextInputBaseFieldIntTextSelectionExtensions_AddOnSelectIndexChange__1___0_System_Action_}

Subscribes to the [`OnSelectIndexChange`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-OnSelectIndexChange.html) event of [`textSelection`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-textSelection.html).

```csharp
public static T AddOnSelectIndexChange<T>(this T element, Action value) where T : TextInputBaseField<int>
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

The field type.

### RemoveOnCursorIndexChange\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_TextInputBaseFieldIntTextSelectionExtensions_RemoveOnCursorIndexChange__1___0_System_Action_}

Unsubscribes from the [`OnCursorIndexChange`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-OnCursorIndexChange.html) event of [`textSelection`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-textSelection.html).

```csharp
public static T RemoveOnCursorIndexChange<T>(this T element, Action value) where T : TextInputBaseField<int>
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

The field type.

### RemoveOnSelectIndexChange\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_TextInputBaseFieldIntTextSelectionExtensions_RemoveOnSelectIndexChange__1___0_System_Action_}

Unsubscribes from the [`OnSelectIndexChange`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-OnSelectIndexChange.html) event of [`textSelection`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-textSelection.html).

```csharp
public static T RemoveOnSelectIndexChange<T>(this T element, Action value) where T : TextInputBaseField<int>
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

The field type.

### SetCursorIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_TextInputBaseFieldIntTextSelectionExtensions_SetCursorIndex__1___0_System_Int32_}

Sets [`cursorIndex`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-cursorIndex.html).

```csharp
public static T SetCursorIndex<T>(this T element, int value) where T : TextInputBaseField<int>
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

The field type.

### SetDoubleClickSelectsWord\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldIntTextSelectionExtensions_SetDoubleClickSelectsWord__1___0_System_Boolean_}

Sets [`doubleClickSelectsWord`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-doubleClickSelectsWord.html).

```csharp
public static T SetDoubleClickSelectsWord<T>(this T element, bool value) where T : TextInputBaseField<int>
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

The field type.

### SetSelectAllOnFocus\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldIntTextSelectionExtensions_SetSelectAllOnFocus__1___0_System_Boolean_}

Sets [`selectAllOnFocus`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-selectAllOnFocus.html).

```csharp
public static T SetSelectAllOnFocus<T>(this T element, bool value) where T : TextInputBaseField<int>
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the whole text is selected when the field receives focus.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The field type.

### SetSelectAllOnMouseUp\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldIntTextSelectionExtensions_SetSelectAllOnMouseUp__1___0_System_Boolean_}

Sets [`selectAllOnMouseUp`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-selectAllOnMouseUp.html).

```csharp
public static T SetSelectAllOnMouseUp<T>(this T element, bool value) where T : TextInputBaseField<int>
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

The field type.

### SetSelectIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_TextInputBaseFieldIntTextSelectionExtensions_SetSelectIndex__1___0_System_Int32_}

Sets [`selectIndex`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-selectIndex.html).

```csharp
public static T SetSelectIndex<T>(this T element, int value) where T : TextInputBaseField<int>
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

The field type.

### SetSelectable\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldIntTextSelectionExtensions_SetSelectable__1___0_System_Boolean_}

Sets [`isSelectable`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-isSelectable.html) of [`textSelection`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-textSelection.html).

```csharp
public static T SetSelectable<T>(this T element, bool value) where T : TextInputBaseField<int>
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

The field type.

### SetTripleClickSelectsLine\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldIntTextSelectionExtensions_SetTripleClickSelectsLine__1___0_System_Boolean_}

Sets [`tripleClickSelectsLine`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-tripleClickSelectsLine.html).

```csharp
public static T SetTripleClickSelectsLine<T>(this T element, bool value) where T : TextInputBaseField<int>
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

The field type.

