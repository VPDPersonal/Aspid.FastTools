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
Assembly: Aspid.FastTools.Unity.dll  

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

Sets [`autoCorrection`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-autoCorrection.html) and returns the element for chaining.

```csharp
public static T SetAutoCorrection<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether auto correction is enabled.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Determines if the soft keyboard auto correction is turned on or off.

### SetHideMobileInput\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetHideMobileInput__1___0_System_Boolean_}

Sets [`hideMobileInput`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-hideMobileInput.html) and returns the element for chaining.

```csharp
public static T SetHideMobileInput<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to hide the mobile input field.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Hides or shows the mobile input field.

### SetHidePlaceholderOnFocus\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetHidePlaceholderOnFocus__1___0_System_Boolean_}

Sets [`hidePlaceholderOnFocus`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-hidePlaceholderOnFocus.html) and returns the element for chaining.

```csharp
public static T SetHidePlaceholderOnFocus<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to hide the placeholder when the field is focused.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Hides the placeholder on focus.

### SetHideSoftKeyboard\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetHideSoftKeyboard__1___0_System_Boolean_}

Sets [`hideSoftKeyboard`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-hideSoftKeyboard.html) and returns the element for chaining.

```csharp
public static T SetHideSoftKeyboard<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to hide the soft keyboard.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Should hide soft / virtual keyboard.

### SetIsDelayed\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetIsDelayed__1___0_System_Boolean_}

Sets [`isDelayed`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-isDelayed.html) and returns the element for chaining.

```csharp
public static T SetIsDelayed<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether the element update is delayed.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

If set to true, the value property isn't updated until either the user presses Enter or the element loses focus.

### SetIsPassword\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetIsPassword__1___0_System_Boolean_}

Sets [`isPassword`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-isPassword.html) and returns the element for chaining.

```csharp
public static T SetIsPassword<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether the field is in password mode.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

When set to true, the field is used to edit a password and masks input characters.

### SetIsReadOnly\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetIsReadOnly__1___0_System_Boolean_}

Sets [`isReadOnly`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-isReadOnly.html) and returns the element for chaining.

```csharp
public static T SetIsReadOnly<T>(this T element, bool value) where T : ITextEdition
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether the element is read-only.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

When set to true, the element becomes read-only.

### SetKeyboardType\<T\>\(T, TouchScreenKeyboardType\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetKeyboardType__1___0_UnityEngine_TouchScreenKeyboardType_}

Sets [`keyboardType`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-keyboardType.html) and returns the element for chaining.

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

#### Remarks

The type of mobile keyboard that will be used.

### SetMaskChar\<T\>\(T, char\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetMaskChar__1___0_System_Char_}

Sets [`maskChar`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-maskChar.html) and returns the element for chaining.

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

#### Remarks

The character used for masking when in password mode.

### SetMaxLength\<T\>\(T, int\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetMaxLength__1___0_System_Int32_}

Sets [`maxLength`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-maxLength.html) and returns the element for chaining.

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

#### Remarks

Maximum number of characters for that element.

### SetPlaceholder\<T\>\(T, string\) {#Aspid_FastTools_UIElements_ITextEditionExtensions_SetPlaceholder__1___0_System_String_}

Sets [`placeholder`](https://docs.unity3d.com/ScriptReference/UIElements-ITextEdition-placeholder.html) and returns the element for chaining.

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

#### Remarks

A short hint to help users understand what to enter in the field.

