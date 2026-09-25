---
title: "Class TextInputBaseFieldTextSelectionExtensions"
sidebar_label: "TextInputBaseFieldTextSelectionExtensions"
description: "Class TextInputBaseFieldTextSelectionExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TextInputBaseFieldTextSelectionExtensions {#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides text-selection extension methods for [`TextInputBaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField.html).

```csharp
public static class TextInputBaseFieldTextSelectionExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TextInputBaseFieldTextSelectionExtensions](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddOnCursorIndexChange\<TField, TValue\>\(TField, Action\) {#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_AddOnCursorIndexChange__2___0_System_Action_}

Subscribes to the [`OnCursorIndexChange`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-OnCursorIndexChange.html) event of [`textSelection`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-textSelection.html).

```csharp
public static TField AddOnCursorIndexChange<TField, TValue>(this TField element, Action value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The callback to subscribe.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### AddOnSelectIndexChange\<TField, TValue\>\(TField, Action\) {#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_AddOnSelectIndexChange__2___0_System_Action_}

Subscribes to the [`OnSelectIndexChange`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-OnSelectIndexChange.html) event of [`textSelection`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-textSelection.html).

```csharp
public static TField AddOnSelectIndexChange<TField, TValue>(this TField element, Action value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The callback to subscribe.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### RemoveOnCursorIndexChange\<TField, TValue\>\(TField, Action\) {#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_RemoveOnCursorIndexChange__2___0_System_Action_}

Unsubscribes from the [`OnCursorIndexChange`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-OnCursorIndexChange.html) event of [`textSelection`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-textSelection.html).

```csharp
public static TField RemoveOnCursorIndexChange<TField, TValue>(this TField element, Action value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The callback to remove.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### RemoveOnSelectIndexChange\<TField, TValue\>\(TField, Action\) {#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_RemoveOnSelectIndexChange__2___0_System_Action_}

Unsubscribes from the [`OnSelectIndexChange`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-OnSelectIndexChange.html) event of [`textSelection`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-textSelection.html).

```csharp
public static TField RemoveOnSelectIndexChange<TField, TValue>(this TField element, Action value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The callback to remove.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetCursorIndex\<TField, TValue\>\(TField, int\) {#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetCursorIndex__2___0_System_Int32_}

Sets [`cursorIndex`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-cursorIndex.html).

```csharp
public static TField SetCursorIndex<TField, TValue>(this TField element, int value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The cursor index to set.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetDoubleClickSelectsWord\<TField, TValue\>\(TField, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetDoubleClickSelectsWord__2___0_System_Boolean_}

Sets [`doubleClickSelectsWord`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-doubleClickSelectsWord.html).

```csharp
public static TField SetDoubleClickSelectsWord<TField, TValue>(this TField element, bool value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, a double click selects the word under the pointer.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetSelectAllOnFocus\<TField, TValue\>\(TField, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectAllOnFocus__2___0_System_Boolean_}

Sets [`selectAllOnFocus`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-selectAllOnFocus.html).

```csharp
public static TField SetSelectAllOnFocus<TField, TValue>(this TField element, bool value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the whole text is selected when the field receives focus.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetSelectAllOnMouseUp\<TField, TValue\>\(TField, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectAllOnMouseUp__2___0_System_Boolean_}

Sets [`selectAllOnMouseUp`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-selectAllOnMouseUp.html).

```csharp
public static TField SetSelectAllOnMouseUp<TField, TValue>(this TField element, bool value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the whole text is selected on the first mouse up.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetSelectIndex\<TField, TValue\>\(TField, int\) {#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectIndex__2___0_System_Int32_}

Sets [`selectIndex`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-selectIndex.html).

```csharp
public static TField SetSelectIndex<TField, TValue>(this TField element, int value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The selection index to set.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetSelectable\<TField, TValue\>\(TField, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectable__2___0_System_Boolean_}

Sets [`isSelectable`](https://docs.unity3d.com/ScriptReference/UIElements-ITextSelection-isSelectable.html) of [`textSelection`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-textSelection.html).

```csharp
public static TField SetSelectable<TField, TValue>(this TField element, bool value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the text can be selected.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetTripleClickSelectsLine\<TField, TValue\>\(TField, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetTripleClickSelectsLine__2___0_System_Boolean_}

Sets [`tripleClickSelectsLine`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-tripleClickSelectsLine.html).

```csharp
public static TField SetTripleClickSelectsLine<TField, TValue>(this TField element, bool value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, a triple click selects the line under the pointer.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

