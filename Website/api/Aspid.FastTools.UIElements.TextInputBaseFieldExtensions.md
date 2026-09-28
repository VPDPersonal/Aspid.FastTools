---
title: "Class TextInputBaseFieldExtensions"
sidebar_label: "TextInputBaseFieldExtensions"
description: "Class TextInputBaseFieldExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TextInputBaseFieldExtensions {#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`TextInputBaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField.html).

```csharp
public static class TextInputBaseFieldExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TextInputBaseFieldExtensions](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md)


## Methods

### SetAutoCorrection\<TField, TValue\>\(TField, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetAutoCorrection__2___0_System_Boolean_}

Sets [`autoCorrection`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-autoCorrection.html).

```csharp
public static TField SetAutoCorrection<TField, TValue>(this TField element, bool value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the soft keyboard auto-corrects input.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetDelayed\<TField, TValue\>\(TField, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetDelayed__2___0_System_Boolean_}

Sets [`isDelayed`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-isDelayed.html).

```csharp
public static TField SetDelayed<TField, TValue>(this TField element, bool value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the value is committed only on Enter or when the field loses focus.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetHideMobileInput\<TField, TValue\>\(TField, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHideMobileInput__2___0_System_Boolean_}

Sets [`hideMobileInput`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-hideMobileInput.html).

```csharp
public static TField SetHideMobileInput<TField, TValue>(this TField element, bool value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the mobile input field is hidden.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetHidePlaceholderOnFocus\<TField, TValue\>\(TField, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHidePlaceholderOnFocus__2___0_System_Boolean_}

Sets [`hidePlaceholderOnFocus`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-hidePlaceholderOnFocus.html) of [`textEdition`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-textEdition.html).

```csharp
public static TField SetHidePlaceholderOnFocus<TField, TValue>(this TField element, bool value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the placeholder is hidden while the field has focus.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetHideSoftKeyboard\<TField, TValue\>\(TField, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHideSoftKeyboard__2___0_System_Boolean_}

Sets [`hideSoftKeyboard`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-hideSoftKeyboard.html).

```csharp
public static TField SetHideSoftKeyboard<TField, TValue>(this TField element, bool value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the soft keyboard is not shown.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetKeyboardType\<TField, TValue\>\(TField, TouchScreenKeyboardType\) {#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetKeyboardType__2___0_UnityEngine_TouchScreenKeyboardType_}

Sets [`keyboardType`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-keyboardType.html).

```csharp
public static TField SetKeyboardType<TField, TValue>(this TField element, TouchScreenKeyboardType value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` TouchScreenKeyboardType

The keyboard type to set.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetMaskChar\<TField, TValue\>\(TField, char\) {#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetMaskChar__2___0_System_Char_}

Sets [`maskChar`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-maskChar.html).

```csharp
public static TField SetMaskChar<TField, TValue>(this TField element, char value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [char](https://learn.microsoft.com/dotnet/api/system.char)

The mask character to set.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetMaxLength\<TField, TValue\>\(TField, int\) {#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetMaxLength__2___0_System_Int32_}

Sets [`maxLength`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-maxLength.html).

```csharp
public static TField SetMaxLength<TField, TValue>(this TField element, int value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The maximum character count to set.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetPassword\<TField, TValue\>\(TField, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetPassword__2___0_System_Boolean_}

Sets [`isPasswordField`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-isPasswordField.html).

```csharp
public static TField SetPassword<TField, TValue>(this TField element, bool value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, input characters are masked.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetPlaceholder\<TField, TValue\>\(TField, string\) {#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetPlaceholder__2___0_System_String_}

Sets [`placeholder`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-placeholder.html) of [`textEdition`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-textEdition.html).

```csharp
public static TField SetPlaceholder<TField, TValue>(this TField element, string value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The placeholder text to set.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

### SetReadOnly\<TField, TValue\>\(TField, bool\) {#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetReadOnly__2___0_System_Boolean_}

Sets [`isReadOnly`](https://docs.unity3d.com/ScriptReference/UIElements-TextInputBaseField-isReadOnly.html).

```csharp
public static TField SetReadOnly<TField, TValue>(this TField element, bool value) where TField : TextInputBaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the field is read-only.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

