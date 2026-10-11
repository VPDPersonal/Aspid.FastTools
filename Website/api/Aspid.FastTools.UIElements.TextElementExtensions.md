---
title: "Class TextElementExtensions"
sidebar_label: "TextElementExtensions"
description: "Class TextElementExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TextElementExtensions {#Aspid_FastTools_UIElements_TextElementExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`TextElement`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.TextElement.html).

```csharp
public static class TextElementExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TextElementExtensions](Aspid.FastTools.UIElements.TextElementExtensions.md)


## Methods

### SetDisplayTooltipWhenElided\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetDisplayTooltipWhenElided__1___0_System_Boolean_}

Sets [`displayTooltipWhenElided`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.TextElement-displayTooltipWhenElided.html).

```csharp
public static T SetDisplayTooltipWhenElided<T>(this T element, bool value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, a tooltip shows the full text when it is elided.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetEmojiFallbackSupport\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetEmojiFallbackSupport__1___0_System_Boolean_}

Sets [`emojiFallbackSupport`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.TextElement-emojiFallbackSupport.html).

```csharp
public static T SetEmojiFallbackSupport<T>(this T element, bool value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the global emoji fallback list is searched first.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetEnableRichText\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetEnableRichText__1___0_System_Boolean_}

Sets [`enableRichText`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.TextElement-enableRichText.html).

```csharp
public static T SetEnableRichText<T>(this T element, bool value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, rich text tags are parsed.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetParseEscapeSequences\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetParseEscapeSequences__1___0_System_Boolean_}

Sets [`parseEscapeSequences`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.TextElement-parseEscapeSequences.html).

```csharp
public static T SetParseEscapeSequences<T>(this T element, bool value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, escape sequences such as <code>\n</code> are parsed; otherwise, they are shown as raw text.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetTextSelf\<T\>\(T, string\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetTextSelf__1___0_System_String_}

Sets [`text`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.TextElement-text.html).

```csharp
public static T SetTextSelf<T>(this T element, string value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The text to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetTextSelf\<T\>\(T, int\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetTextSelf__1___0_System_Int32_}

Sets the text to a number via [`SetText`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.TextElement.SetText.html), without allocating a string.

```csharp
public static T SetTextSelf<T>(this T element, int value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The number to show.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Allocates a string while the element is not attached to a panel.

### SetTextSelf\<T\>\(T, long\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetTextSelf__1___0_System_Int64_}

Sets the text to a number via [`SetText`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.TextElement.SetText.html), without allocating a string.

```csharp
public static T SetTextSelf<T>(this T element, long value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

The number to show.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Allocates a string while the element is not attached to a panel.

### SetTextSelf\<T\>\(T, ulong\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetTextSelf__1___0_System_UInt64_}

Sets the text to a number via [`SetText`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.TextElement.SetText.html), without allocating a string.

```csharp
public static T SetTextSelf<T>(this T element, ulong value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

The number to show.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Allocates a string while the element is not attached to a panel.

### SetTextSelf\<T\>\(T, float, string\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetTextSelf__1___0_System_Single_System_String_}

Sets the text to a number via [`SetText`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.TextElement.SetText.html), without allocating a string.

```csharp
public static T SetTextSelf<T>(this T element, float value, string format = null) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

The number to show.

`format` [string](https://learn.microsoft.com/dotnet/api/system.string)

A standard or custom numeric format string; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for the default format.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Allocates a string while the element is not attached to a panel.

### SetTextSelf\<T\>\(T, ReadOnlySpan\<char\>\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetTextSelf__1___0_System_ReadOnlySpan_System_Char__}

Sets the text from a character span via [`SetText`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.TextElement.SetText.html), without allocating a string.

```csharp
public static T SetTextSelf<T>(this T element, ReadOnlySpan<char> value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan-1)\<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The characters to copy.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Allocates a string while the element is not attached to a panel.

### SetTextSelf\<T\>\(T, StringBuilder\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetTextSelf__1___0_System_Text_StringBuilder_}

Sets the text from a [`StringBuilder`](https://learn.microsoft.com/dotnet/api/system.text.stringbuilder) via [`SetText`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.TextElement.SetText.html), without allocating a string.

```csharp
public static T SetTextSelf<T>(this T element, StringBuilder value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [StringBuilder](https://learn.microsoft.com/dotnet/api/system.text.stringbuilder)

The builder whose content to copy.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Allocates a string while the element is not attached to a panel.

### SetTextSelf\<T\>\(T, char\[\], int, int\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetTextSelf__1___0_System_Char___System_Int32_System_Int32_}

Sets the text from a slice of a character array via [`SetText`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.TextElement.SetText.html), without allocating a string.

```csharp
public static T SetTextSelf<T>(this T element, char[] value, int start, int length) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [char](https://learn.microsoft.com/dotnet/api/system.char)\[\]

The array to copy from.

`start` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The index of the first character to copy.

`length` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The number of characters to copy.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

#### Remarks

Allocates a string while the element is not attached to a panel.

