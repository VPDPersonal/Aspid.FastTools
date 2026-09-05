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

```csharp
public static class TextElementExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TextElementExtensions](Aspid.FastTools.UIElements.TextElementExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetDisplayTooltipWhenElided\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetDisplayTooltipWhenElided__1___0_System_Boolean_}

Sets [`displayTooltipWhenElided`](https://docs.unity3d.com/ScriptReference/UIElements-TextElement-displayTooltipWhenElided.html) and returns the element for chaining.

```csharp
public static T SetDisplayTooltipWhenElided<T>(this T element, bool value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to display a tooltip when text is elided.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

When true, a tooltip displays the full version of elided text, and also if a tooltip had been previously provided, it will be overwritten.

### SetEmojiFallbackSupport\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetEmojiFallbackSupport__1___0_System_Boolean_}

Sets [`emojiFallbackSupport`](https://docs.unity3d.com/ScriptReference/UIElements-TextElement-emojiFallbackSupport.html) and returns the element for chaining.

```csharp
public static T SetEmojiFallbackSupport<T>(this T element, bool value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether emoji fallback support is enabled.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies the order in which the system should look for Emoji characters when rendering text.
If this setting is enabled, the global Emoji Fallback list will be searched first for characters defined as Emoji in the Unicode 14.0 standard.

### SetEnableRichText\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetEnableRichText__1___0_System_Boolean_}

Sets [`enableRichText`](https://docs.unity3d.com/ScriptReference/UIElements-TextElement-enableRichText.html) and returns the element for chaining.

```csharp
public static T SetEnableRichText<T>(this T element, bool value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether rich text parsing is enabled.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

When false, rich text tags will not be parsed.

### SetParseEscapeSequences\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetParseEscapeSequences__1___0_System_Boolean_}

Sets [`parseEscapeSequences`](https://docs.unity3d.com/ScriptReference/UIElements-TextElement-parseEscapeSequences.html) and returns the element for chaining.

```csharp
public static T SetParseEscapeSequences<T>(this T element, bool value) where T : TextElement
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether escape sequences are parsed.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

#### Remarks

Determines how escape sequences are displayed. When set to true, escape sequences (such as \n, \t) are parsed and transformed into their corresponding characters.
For example, '\n' will insert a new line. When set to false, escape sequences are displayed as raw text (for example, \n is shown as the characters '\' followed by 'n').

### SetText\<T\>\(T, string\) {#Aspid_FastTools_UIElements_TextElementExtensions_SetText__1___0_System_String_}

Sets [`text`](https://docs.unity3d.com/ScriptReference/UIElements-TextElement-text.html) and returns the element for chaining.

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

#### Remarks

The text to be displayed.

