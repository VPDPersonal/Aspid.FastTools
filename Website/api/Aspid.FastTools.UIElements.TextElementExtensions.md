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

Provides extension methods for [`TextElement`](https://docs.unity3d.com/ScriptReference/UIElements-TextElement.html).

```csharp
public static class TextElementExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TextElementExtensions](Aspid.FastTools.UIElements.TextElementExtensions.md)


## Methods

### SetDisplayTooltipWhenElided\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetDisplayTooltipWhenElided__1___0_System_Boolean_}

Sets [`displayTooltipWhenElided`](https://docs.unity3d.com/ScriptReference/UIElements-TextElement-displayTooltipWhenElided.html).

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

Sets [`emojiFallbackSupport`](https://docs.unity3d.com/ScriptReference/UIElements-TextElement-emojiFallbackSupport.html).

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

Sets [`enableRichText`](https://docs.unity3d.com/ScriptReference/UIElements-TextElement-enableRichText.html).

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

Sets [`parseEscapeSequences`](https://docs.unity3d.com/ScriptReference/UIElements-TextElement-parseEscapeSequences.html).

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

### SetText\<T\>\(T, string\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetText__1___0_System_String_}

Sets [`text`](https://docs.unity3d.com/ScriptReference/UIElements-TextElement-text.html).

```csharp
public static T SetText<T>(this T element, string value) where T : TextElement
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

