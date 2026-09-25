---
title: "Class ITextEditionExtensions"
sidebar_label: "ITextEditionExtensions"
description: "Class ITextEditionExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class ITextEditionExtensions {#Aspid_FastTools_UIElements_ITextEditionExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`ITextEdition`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition.html).

```csharp
public static class ITextEditionExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[ITextEditionExtensions](Aspid.FastTools.UIElements.ITextEditionExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetAutoCorrection\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetAutoCorrection__1___0_System_Boolean_}

Sets [`autoCorrection`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-autoCorrection.html).

```csharp
public static T SetAutoCorrection<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the soft keyboard auto-corrects input.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetDelayed\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetDelayed__1___0_System_Boolean_}

Sets [`isDelayed`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-isDelayed.html).

```csharp
public static T SetDelayed<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the value is committed only on Enter or when the element loses focus.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetHideMobileInput\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetHideMobileInput__1___0_System_Boolean_}

Sets [`hideMobileInput`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-hideMobileInput.html).

```csharp
public static T SetHideMobileInput<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the mobile input field is hidden.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetHidePlaceholderOnFocus\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetHidePlaceholderOnFocus__1___0_System_Boolean_}

Sets [`hidePlaceholderOnFocus`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-hidePlaceholderOnFocus.html).

```csharp
public static T SetHidePlaceholderOnFocus<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the placeholder is hidden while the field has focus.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetHideSoftKeyboard\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetHideSoftKeyboard__1___0_System_Boolean_}

Sets [`hideSoftKeyboard`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-hideSoftKeyboard.html).

```csharp
public static T SetHideSoftKeyboard<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the soft keyboard is not shown.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetKeyboardType\<T\>\(T, TouchScreenKeyboardType\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetKeyboardType__1___0_UnityEngine_TouchScreenKeyboardType_}

Sets [`keyboardType`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-keyboardType.html).

```csharp
public static T SetKeyboardType<T>(this T element, TouchScreenKeyboardType value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` TouchScreenKeyboardType

The keyboard type to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetMaskChar\<T\>\(T, char\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetMaskChar__1___0_System_Char_}

Sets [`maskChar`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-maskChar.html).

```csharp
public static T SetMaskChar<T>(this T element, char value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [char](https://learn.microsoft.com/dotnet/api/system.char)

The mask character to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetMaxLength\<T\>\(T, int\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetMaxLength__1___0_System_Int32_}

Sets [`maxLength`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-maxLength.html).

```csharp
public static T SetMaxLength<T>(this T element, int value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The maximum character count to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetPassword\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetPassword__1___0_System_Boolean_}

Sets [`isPassword`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-isPassword.html).

```csharp
public static T SetPassword<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, input characters are masked.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetPlaceholder\<T\>\(T, string\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetPlaceholder__1___0_System_String_}

Sets [`placeholder`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-placeholder.html).

```csharp
public static T SetPlaceholder<T>(this T element, string value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The placeholder text to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetReadOnly\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetReadOnly__1___0_System_Boolean_}

Sets [`isReadOnly`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-isReadOnly.html).

```csharp
public static T SetReadOnly<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the element is read-only.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

