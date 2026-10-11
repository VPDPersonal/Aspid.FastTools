---
title: "Class TextInputBaseFieldFloatTextSelectionExtensions"
sidebar_label: "TextInputBaseFieldFloatTextSelectionExtensions"
description: "Class TextInputBaseFieldFloatTextSelectionExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TextInputBaseFieldFloatTextSelectionExtensions {#Aspid_FastTools_UIElements_TextInputBaseFieldFloatTextSelectionExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides text-selection extension methods for [`TextInputBaseField<T>`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextInputBaseField_1.html) of [`Single`](https://learn.microsoft.com/dotnet/api/system.single).

```csharp
public static class TextInputBaseFieldFloatTextSelectionExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TextInputBaseFieldFloatTextSelectionExtensions](Aspid.FastTools.UIElements.TextInputBaseFieldFloatTextSelectionExtensions.md)


## Methods

### AddOnCursorIndexChange\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_TextInputBaseFieldFloatTextSelectionExtensions_AddOnCursorIndexChange__1___0_System_Action_}

Subscribes to the [`OnCursorIndexChange`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ITextSelection.OnCursorIndexChange.html) event of [`textSelection`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextInputBaseField_1-textSelection.html).

```csharp
public static T AddOnCursorIndexChange<T>(this T element, Action value) where T : TextInputBaseField<float>
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

#### Remarks

Available in Unity 6000.3 and newer.

### AddOnSelectIndexChange\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_TextInputBaseFieldFloatTextSelectionExtensions_AddOnSelectIndexChange__1___0_System_Action_}

Subscribes to the [`OnSelectIndexChange`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ITextSelection.OnSelectIndexChange.html) event of [`textSelection`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextInputBaseField_1-textSelection.html).

```csharp
public static T AddOnSelectIndexChange<T>(this T element, Action value) where T : TextInputBaseField<float>
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

#### Remarks

Available in Unity 6000.3 and newer.

### RemoveOnCursorIndexChange\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_TextInputBaseFieldFloatTextSelectionExtensions_RemoveOnCursorIndexChange__1___0_System_Action_}

Unsubscribes from the [`OnCursorIndexChange`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ITextSelection.OnCursorIndexChange.html) event of [`textSelection`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextInputBaseField_1-textSelection.html).

```csharp
public static T RemoveOnCursorIndexChange<T>(this T element, Action value) where T : TextInputBaseField<float>
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

#### Remarks

Available in Unity 6000.3 and newer.

### RemoveOnSelectIndexChange\<T\>\(T, Action\) {#Aspid_FastTools_UIElements_TextInputBaseFieldFloatTextSelectionExtensions_RemoveOnSelectIndexChange__1___0_System_Action_}

Unsubscribes from the [`OnSelectIndexChange`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ITextSelection.OnSelectIndexChange.html) event of [`textSelection`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextInputBaseField_1-textSelection.html).

```csharp
public static T RemoveOnSelectIndexChange<T>(this T element, Action value) where T : TextInputBaseField<float>
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

#### Remarks

Available in Unity 6000.3 and newer.

### SetCursorIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_TextInputBaseFieldFloatTextSelectionExtensions_SetCursorIndex__1___0_System_Int32_}

Sets [`cursorIndex`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextInputBaseField_1-cursorIndex.html).

```csharp
public static T SetCursorIndex<T>(this T element, int value) where T : TextInputBaseField<float>
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

### SetDoubleClickSelectsWord\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldFloatTextSelectionExtensions_SetDoubleClickSelectsWord__1___0_System_Boolean_}

Sets [`doubleClickSelectsWord`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextInputBaseField_1-doubleClickSelectsWord.html).

```csharp
public static T SetDoubleClickSelectsWord<T>(this T element, bool value) where T : TextInputBaseField<float>
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

### SetSelectAllOnFocus\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldFloatTextSelectionExtensions_SetSelectAllOnFocus__1___0_System_Boolean_}

Sets [`selectAllOnFocus`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextInputBaseField_1-selectAllOnFocus.html).

```csharp
public static T SetSelectAllOnFocus<T>(this T element, bool value) where T : TextInputBaseField<float>
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

### SetSelectAllOnMouseUp\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldFloatTextSelectionExtensions_SetSelectAllOnMouseUp__1___0_System_Boolean_}

Sets [`selectAllOnMouseUp`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextInputBaseField_1-selectAllOnMouseUp.html).

```csharp
public static T SetSelectAllOnMouseUp<T>(this T element, bool value) where T : TextInputBaseField<float>
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

### SetSelectIndex\<T\>\(T, int\) {#Aspid_FastTools_UIElements_TextInputBaseFieldFloatTextSelectionExtensions_SetSelectIndex__1___0_System_Int32_}

Sets [`selectIndex`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextInputBaseField_1-selectIndex.html).

```csharp
public static T SetSelectIndex<T>(this T element, int value) where T : TextInputBaseField<float>
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

### SetSelectable\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldFloatTextSelectionExtensions_SetSelectable__1___0_System_Boolean_}

Sets [`isSelectable`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.ITextSelection-isSelectable.html) of [`textSelection`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextInputBaseField_1-textSelection.html).

```csharp
public static T SetSelectable<T>(this T element, bool value) where T : TextInputBaseField<float>
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

### SetTripleClickSelectsLine\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldFloatTextSelectionExtensions_SetTripleClickSelectsLine__1___0_System_Boolean_}

Sets [`tripleClickSelectsLine`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextInputBaseField_1-tripleClickSelectsLine.html).

```csharp
public static T SetTripleClickSelectsLine<T>(this T element, bool value) where T : TextInputBaseField<float>
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

