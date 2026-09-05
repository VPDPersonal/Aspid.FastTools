---
title: "Class IStyleExtensions"
sidebar_label: "IStyleExtensions"
description: "Class IStyleExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class IStyleExtensions {#Aspid_FastTools_UIElements_IStyleExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.Unity.dll  

```csharp
public static class IStyleExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[IStyleExtensions](Aspid.FastTools.UIElements.IStyleExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddBoldUnityFontStyleAndWeight\<T\>\(T\) {#Aspid_FastTools_UIElements_IStyleExtensions_AddBoldUnityFontStyleAndWeight__1___0_}

Adds bold to [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html), preserving any inline italic style.

```csharp
public static T AddBoldUnityFontStyleAndWeight<T>(this T style) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Transitions: [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html) → [`Bold`](https://docs.unity3d.com/ScriptReference/FontStyle-Bold.html),
[`Italic`](https://docs.unity3d.com/ScriptReference/FontStyle-Italic.html) → [`BoldAndItalic`](https://docs.unity3d.com/ScriptReference/FontStyle-BoldAndItalic.html).
Other values are left unchanged.
Only the inline [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html) value is read; a bold or italic
style resolved from USS (with no inline value set) reports as [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html)
and is therefore not preserved.

### AddItalicUnityFontStyleAndWeight\<T\>\(T\) {#Aspid_FastTools_UIElements_IStyleExtensions_AddItalicUnityFontStyleAndWeight__1___0_}

Adds italic to [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html), preserving any inline bold style.

```csharp
public static T AddItalicUnityFontStyleAndWeight<T>(this T style) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Transitions: [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html) → [`Italic`](https://docs.unity3d.com/ScriptReference/FontStyle-Italic.html),
[`Bold`](https://docs.unity3d.com/ScriptReference/FontStyle-Bold.html) → [`BoldAndItalic`](https://docs.unity3d.com/ScriptReference/FontStyle-BoldAndItalic.html).
Other values are left unchanged.
Only the inline [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html) value is read; a bold or italic
style resolved from USS (with no inline value set) reports as [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html)
and is therefore not preserved.

### RemoveBoldUnityFontStyleAndWeight\<T\>\(T\) {#Aspid_FastTools_UIElements_IStyleExtensions_RemoveBoldUnityFontStyleAndWeight__1___0_}

Removes bold from [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html), preserving any inline italic style.

```csharp
public static T RemoveBoldUnityFontStyleAndWeight<T>(this T style) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Transitions: [`Bold`](https://docs.unity3d.com/ScriptReference/FontStyle-Bold.html) → [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html),
[`BoldAndItalic`](https://docs.unity3d.com/ScriptReference/FontStyle-BoldAndItalic.html) → [`Italic`](https://docs.unity3d.com/ScriptReference/FontStyle-Italic.html).
Other values are left unchanged.
Only the inline [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html) value is read; a bold or italic
style resolved from USS (with no inline value set) reports as [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html)
and is therefore not preserved.

### RemoveItalicUnityFontStyleAndWeight\<T\>\(T\) {#Aspid_FastTools_UIElements_IStyleExtensions_RemoveItalicUnityFontStyleAndWeight__1___0_}

Removes italic from [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html), preserving any inline bold style.

```csharp
public static T RemoveItalicUnityFontStyleAndWeight<T>(this T style) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Transitions: [`Italic`](https://docs.unity3d.com/ScriptReference/FontStyle-Italic.html) → [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html),
[`BoldAndItalic`](https://docs.unity3d.com/ScriptReference/FontStyle-BoldAndItalic.html) → [`Bold`](https://docs.unity3d.com/ScriptReference/FontStyle-Bold.html).
Other values are left unchanged.
Only the inline [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html) value is read; a bold or italic
style resolved from USS (with no inline value set) reports as [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html)
and is therefore not preserved.

### SetAlignContent\<T\>\(T, StyleEnum\<Align\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetAlignContent__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Align__}

Sets [`alignContent`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-alignContent.html) and returns the style for chaining.

```csharp
public static T SetAlignContent<T>(this T style, StyleEnum<Align> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<Align\>

The content alignment to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Alignment of the whole area of children on the cross axis if they span over multiple lines in this container.

### SetAlignContent\<T\>\(T, Align\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetAlignContent__1___0_UnityEngine_UIElements_Align_}

Sets [`alignContent`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-alignContent.html) and returns the style for chaining.

```csharp
public static T SetAlignContent<T>(this T style, Align value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` Align

The content alignment to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Alignment of the whole area of children on the cross axis if they span over multiple lines in this container.

### SetAlignItems\<T\>\(T, StyleEnum\<Align\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetAlignItems__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Align__}

Sets [`alignItems`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-alignItems.html) and returns the style for chaining.

```csharp
public static T SetAlignItems<T>(this T style, StyleEnum<Align> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<Align\>

The children alignment to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Alignment of children on the cross axis of this container.

### SetAlignItems\<T\>\(T, Align\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetAlignItems__1___0_UnityEngine_UIElements_Align_}

Sets [`alignItems`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-alignItems.html) and returns the style for chaining.

```csharp
public static T SetAlignItems<T>(this T style, Align value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` Align

The children alignment to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Alignment of children on the cross axis of this container.

### SetAlignSelf\<T\>\(T, StyleEnum\<Align\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetAlignSelf__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Align__}

Sets [`alignSelf`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-alignSelf.html) and returns the style for chaining.

```csharp
public static T SetAlignSelf<T>(this T style, StyleEnum<Align> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<Align\>

The alignment to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Similar to align-items, but only for this specific element.

### SetAlignSelf\<T\>\(T, Align\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetAlignSelf__1___0_UnityEngine_UIElements_Align_}

Sets [`alignSelf`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-alignSelf.html) and returns the style for chaining.

```csharp
public static T SetAlignSelf<T>(this T style, Align value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` Align

The alignment to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Similar to align-items, but only for this specific element.

### SetAspectRatio\<T\>\(T, StyleRatio\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetAspectRatio__1___0_UnityEngine_UIElements_StyleRatio_}

Sets [`aspectRatio`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-aspectRatio.html) and returns the style for chaining.

```csharp
public static T SetAspectRatio<T>(this T style, StyleRatio value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleRatio

The aspect ratio to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Sets a preferred aspect ratio for the box, which will be used in the calculation of auto sizes and some other layout functions.

### SetBackgroundColor\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBackgroundColor__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`backgroundColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundColor.html) and returns the style for chaining.

```csharp
public static T SetBackgroundColor<T>(this T style, StyleColor value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleColor

The background color to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Background color to paint in the element's box.

### SetBackgroundColor\<T\>\(T, string\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBackgroundColor__1___0_System_String_}

Parses an HTML color string and sets [`backgroundColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundColor.html), returning the style for chaining.

```csharp
public static T SetBackgroundColor<T>(this T style, string value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string to parse (e.g. "#RRGGBB" or a named color).

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Background color to paint in the element's box.

### SetBackgroundImage\<T\>\(T, StyleBackground\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBackgroundImage__1___0_UnityEngine_UIElements_StyleBackground_}

Sets [`backgroundImage`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundImage.html) and returns the style for chaining.

```csharp
public static T SetBackgroundImage<T>(this T style, StyleBackground value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleBackground

The background image to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Background image to paint in the element's box.

### SetBackgroundImageFromResource\<T\>\(T, string\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBackgroundImageFromResource__1___0_System_String_}

Loads a [`Texture2D`](https://docs.unity3d.com/ScriptReference/Texture2D.html) from Resources and sets the [`backgroundImage`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundImage.html) property.

```csharp
public static T SetBackgroundImageFromResource<T>(this T style, string path) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`path` [string](https://learn.microsoft.com/dotnet/api/system.string)

The Resources path of the texture to load.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

### SetBackgroundPosition\<T\>\(T, StyleBackgroundPosition\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBackgroundPosition__1___0_UnityEngine_UIElements_StyleBackgroundPosition_}

Sets [`backgroundPositionX`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundPositionX.html), [`backgroundPositionY`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundPositionY.html) and returns the style for chaining.

```csharp
public static T SetBackgroundPosition<T>(this T style, StyleBackgroundPosition value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleBackgroundPosition

The background position to apply to both axes.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>backgroundPositionX</code> –– Background image x position value.</p>
<p><code>backgroundPositionY</code> –– Background image y position value.</p>

### SetBackgroundPosition\<T\>\(T, StyleBackgroundPosition?, StyleBackgroundPosition?\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBackgroundPosition__1___0_System_Nullable_UnityEngine_UIElements_StyleBackgroundPosition__System_Nullable_UnityEngine_UIElements_StyleBackgroundPosition__}

Sets [`backgroundPositionX`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundPositionX.html), [`backgroundPositionY`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundPositionY.html) and returns the style for chaining.

```csharp
public static T SetBackgroundPosition<T>(this T style, StyleBackgroundPosition? x = null, StyleBackgroundPosition? y = null) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`x` StyleBackgroundPosition?

The horizontal background position, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`y` StyleBackgroundPosition?

The vertical background position, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>backgroundPositionX</code> –– Background image x position value.</p>
<p><code>backgroundPositionY</code> –– Background image y position value.</p>

### SetBackgroundPositionX\<T\>\(T, StyleBackgroundPosition\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBackgroundPositionX__1___0_UnityEngine_UIElements_StyleBackgroundPosition_}

Sets [`backgroundPositionX`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundPositionX.html) and returns the style for chaining.

```csharp
public static T SetBackgroundPositionX<T>(this T style, StyleBackgroundPosition value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleBackgroundPosition

The horizontal background position to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>backgroundPositionX</code> –– Background image x position value.</p>

### SetBackgroundPositionY\<T\>\(T, StyleBackgroundPosition\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBackgroundPositionY__1___0_UnityEngine_UIElements_StyleBackgroundPosition_}

Sets [`backgroundPositionY`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundPositionY.html) and returns the style for chaining.

```csharp
public static T SetBackgroundPositionY<T>(this T style, StyleBackgroundPosition value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleBackgroundPosition

The vertical background position to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>backgroundPositionY</code> –– Background image y position value.</p>

### SetBackgroundRepeat\<T\>\(T, StyleBackgroundRepeat\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBackgroundRepeat__1___0_UnityEngine_UIElements_StyleBackgroundRepeat_}

Sets [`backgroundRepeat`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundRepeat.html) and returns the style for chaining.

```csharp
public static T SetBackgroundRepeat<T>(this T style, StyleBackgroundRepeat value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleBackgroundRepeat

The background repeat mode to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Background image repeat value.

### SetBackgroundSize\<T\>\(T, StyleBackgroundSize\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBackgroundSize__1___0_UnityEngine_UIElements_StyleBackgroundSize_}

Sets [`backgroundSize`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-backgroundSize.html) and returns the style for chaining.

```csharp
public static T SetBackgroundSize<T>(this T style, StyleBackgroundSize value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleBackgroundSize

The background size to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Background image size value. Transitions are fully supported only when using size in pixels or percentages, such as pixel-to-pixel or percentage-to-percentage transitions.

### SetBorderColor\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColor__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderTopColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopColor.html), [`borderRightColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightColor.html),
[`borderBottomColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomColor.html), [`borderLeftColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftColor.html) and returns the style for chaining.

```csharp
public static T SetBorderColor<T>(this T style, StyleColor value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleColor

The border color to apply to all sides.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopColor</code> –– Color of the element's top border.</p>
<p><code>borderRightColor</code> –– Color of the element's right border.</p>
<p><code>borderBottomColor</code> –– Color of the element's bottom border.</p>
<p><code>borderLeftColor</code> –– Color of the element's left border.</p>

### SetBorderColor\<T\>\(T, string\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColor__1___0_System_String_}

Sets the border color on all sides by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColor<T>(this T style, string value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

The style type.

### SetBorderColor\<T\>\(T, StyleColor?, StyleColor?, StyleColor?, StyleColor?\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColor__1___0_System_Nullable_UnityEngine_UIElements_StyleColor__System_Nullable_UnityEngine_UIElements_StyleColor__System_Nullable_UnityEngine_UIElements_StyleColor__System_Nullable_UnityEngine_UIElements_StyleColor__}

Sets [`borderTopColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopColor.html), [`borderRightColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightColor.html),
[`borderBottomColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomColor.html), [`borderLeftColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftColor.html) and returns the style for chaining.

```csharp
public static T SetBorderColor<T>(this T style, StyleColor? top = null, StyleColor? right = null, StyleColor? bottom = null, StyleColor? left = null) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`top` StyleColor?

The top border color, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`right` StyleColor?

The right border color, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottom` StyleColor?

The bottom border color, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`left` StyleColor?

The left border color, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopColor</code> –– Color of the element's top border.</p>
<p><code>borderRightColor</code> –– Color of the element's right border.</p>
<p><code>borderBottomColor</code> –– Color of the element's bottom border.</p>
<p><code>borderLeftColor</code> –– Color of the element's left border.</p>

### SetBorderColorBottom\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColorBottom__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderBottomColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomColor.html) and returns the style for chaining.

```csharp
public static T SetBorderColorBottom<T>(this T style, StyleColor value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleColor

The bottom border color to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderBottomColor</code> –– Color of the element's bottom border.</p>

### SetBorderColorBottom\<T\>\(T, string\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColorBottom__1___0_System_String_}

Sets the bottom border color by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColorBottom<T>(this T style, string value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

The style type.

### SetBorderColorLeft\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColorLeft__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderLeftColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftColor.html) and returns the style for chaining.

```csharp
public static T SetBorderColorLeft<T>(this T style, StyleColor value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleColor

The left border color to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderLeftColor</code> –– Color of the element's left border.</p>

### SetBorderColorLeft\<T\>\(T, string\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColorLeft__1___0_System_String_}

Sets the left border color by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColorLeft<T>(this T style, string value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

The style type.

### SetBorderColorRight\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColorRight__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderRightColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightColor.html) and returns the style for chaining.

```csharp
public static T SetBorderColorRight<T>(this T style, StyleColor value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleColor

The right border color to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderRightColor</code> –– Color of the element's right border.</p>

### SetBorderColorRight\<T\>\(T, string\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColorRight__1___0_System_String_}

Sets the right border color by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColorRight<T>(this T style, string value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

The style type.

### SetBorderColorTop\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColorTop__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderTopColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopColor.html) and returns the style for chaining.

```csharp
public static T SetBorderColorTop<T>(this T style, StyleColor value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleColor

The top border color to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopColor</code> –– Color of the element's top border.</p>

### SetBorderColorTop\<T\>\(T, string\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColorTop__1___0_System_String_}

Sets the top border color by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColorTop<T>(this T style, string value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

The style type.

### SetBorderColorX\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColorX__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderRightColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightColor.html), [`borderLeftColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftColor.html) and returns the style for chaining.

```csharp
public static T SetBorderColorX<T>(this T style, StyleColor value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleColor

The border color to apply to the left and right sides.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderRightColor</code> –– Color of the element's right border.</p>
<p><code>borderLeftColor</code> –– Color of the element's left border.</p>

### SetBorderColorX\<T\>\(T, string\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColorX__1___0_System_String_}

Sets the left and right border colors by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColorX<T>(this T style, string value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

The style type.

### SetBorderColorY\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColorY__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`borderTopColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopColor.html) and [`borderBottomColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomColor.html) and returns the style for chaining.

```csharp
public static T SetBorderColorY<T>(this T style, StyleColor value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleColor

The border color to apply to the top and bottom sides.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopColor</code> –– Color of the element's top border.</p>
<p><code>borderBottomColor</code> –– Color of the element's bottom border.</p>

### SetBorderColorY\<T\>\(T, string\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderColorY__1___0_System_String_}

Sets the top and bottom border colors by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetBorderColorY<T>(this T style, string value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

The style type.

### SetBorderRadius\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderRadius__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderTopLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopLeftRadius.html), [`borderTopRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopRightRadius.html),
[`borderBottomRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomRightRadius.html), [`borderBottomLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomLeftRadius.html) and returns the style for chaining.

```csharp
public static T SetBorderRadius<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The border radius to apply to all corners.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopLeftRadius</code> –– The radius of the top-left corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderTopRightRadius</code> –– The radius of the top-right corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomRightRadius</code> –– The radius of the bottom-right corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomLeftRadius</code> –– The radius of the bottom-left corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadius\<T\>\(T, StyleLength?, StyleLength?, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderRadius__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`borderTopLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopLeftRadius.html), [`borderTopRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopRightRadius.html),
[`borderBottomRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomRightRadius.html), [`borderBottomLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomLeftRadius.html) and returns the style for chaining.

```csharp
public static T SetBorderRadius<T>(this T style, StyleLength? topLeft = null, StyleLength? topRight = null, StyleLength? bottomRight = null, StyleLength? bottomLeft = null) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`topLeft` StyleLength?

The top-left radius, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`topRight` StyleLength?

The top-right radius, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottomRight` StyleLength?

The bottom-right radius, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottomLeft` StyleLength?

The bottom-left radius, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopLeftRadius</code> –– The radius of the top-left corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderTopRightRadius</code> –– The radius of the top-right corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomRightRadius</code> –– The radius of the bottom-right corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomLeftRadius</code> –– The radius of the bottom-left corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusBottom\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderRadiusBottom__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderBottomRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomRightRadius.html), [`borderBottomLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomLeftRadius.html) and returns the style for chaining.

```csharp
public static T SetBorderRadiusBottom<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The radius to apply to both bottom corners.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderBottomRightRadius</code> –– The radius of the bottom-right corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomLeftRadius</code> –– The radius of the bottom-left corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusBottomLeft\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderRadiusBottomLeft__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderBottomLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomLeftRadius.html) and returns the style for chaining.

```csharp
public static T SetBorderRadiusBottomLeft<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The bottom-left corner radius to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderBottomLeftRadius</code> –– The radius of the bottom-left corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusBottomRight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderRadiusBottomRight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderBottomRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomRightRadius.html) and returns the style for chaining.

```csharp
public static T SetBorderRadiusBottomRight<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The bottom-right corner radius to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderBottomRightRadius</code> –– The radius of the bottom-right corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusLeft\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderRadiusLeft__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderTopLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopLeftRadius.html), [`borderBottomLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomLeftRadius.html) and returns the style for chaining.

```csharp
public static T SetBorderRadiusLeft<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The radius to apply to both left corners.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

The style type.

#### Remarks

<p><code>borderTopLeftRadius</code> –– The radius of the top-left corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomLeftRadius</code> –– The radius of the bottom-left corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusRight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderRadiusRight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderTopRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopRightRadius.html), [`borderBottomRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomRightRadius.html) and returns the style for chaining.

```csharp
public static T SetBorderRadiusRight<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The radius to apply to both right corners.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

The style type.

#### Remarks

<p><code>borderTopRightRadius</code> –– The radius of the top-right corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderBottomRightRadius</code> –– The radius of the bottom-right corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusTop\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderRadiusTop__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderTopLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopLeftRadius.html), [`borderTopRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopRightRadius.html) and returns the style for chaining.

```csharp
public static T SetBorderRadiusTop<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The radius to apply to both top corners.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopLeftRadius</code> –– The radius of the top-left corner when a rounded rectangle is drawn in the element's box.</p>
<p><code>borderTopRightRadius</code> –– The radius of the top-right corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusTopLeft\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderRadiusTopLeft__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderTopLeftRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopLeftRadius.html) and returns the style for chaining.

```csharp
public static T SetBorderRadiusTopLeft<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The top-left corner radius to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopLeftRadius</code> –– The radius of the top-left corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderRadiusTopRight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderRadiusTopRight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`borderTopRightRadius`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopRightRadius.html) and returns the style for chaining.

```csharp
public static T SetBorderRadiusTopRight<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The top-right corner radius to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopRightRadius</code> –– The radius of the top-right corner when a rounded rectangle is drawn in the element's box.</p>

### SetBorderWidth\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderWidth__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderTopWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopWidth.html), [`borderRightWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightWidth.html),
[`borderBottomWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomWidth.html), [`borderLeftWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftWidth.html) and returns the style for chaining.

```csharp
public static T SetBorderWidth<T>(this T style, StyleFloat value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFloat

The border width to apply to all sides.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopWidth</code> –– Space reserved for the top edge of the border during the layout phase.</p>
<p><code>borderRightWidth</code> –– Space reserved for the right edge of the border during the layout phase.</p>
<p><code>borderBottomWidth</code> –– Space reserved for the bottom edge of the border during the layout phase.</p>
<p><code>borderLeftWidth</code> –– Space reserved for the left edge of the border during the layout phase.</p>

### SetBorderWidth\<T\>\(T, StyleFloat?, StyleFloat?, StyleFloat?, StyleFloat?\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderWidth__1___0_System_Nullable_UnityEngine_UIElements_StyleFloat__System_Nullable_UnityEngine_UIElements_StyleFloat__System_Nullable_UnityEngine_UIElements_StyleFloat__System_Nullable_UnityEngine_UIElements_StyleFloat__}

Sets [`borderTopWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopWidth.html), [`borderRightWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightWidth.html),
[`borderBottomWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomWidth.html), [`borderLeftWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftWidth.html) and returns the style for chaining.

```csharp
public static T SetBorderWidth<T>(this T style, StyleFloat? top = null, StyleFloat? right = null, StyleFloat? bottom = null, StyleFloat? left = null) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`top` StyleFloat?

The top border width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`right` StyleFloat?

The right border width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottom` StyleFloat?

The bottom border width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`left` StyleFloat?

The left border width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopWidth</code> –– Space reserved for the top edge of the border during the layout phase.</p>
<p><code>borderRightWidth</code> –– Space reserved for the right edge of the border during the layout phase.</p>
<p><code>borderBottomWidth</code> –– Space reserved for the bottom edge of the border during the layout phase.</p>
<p><code>borderLeftWidth</code> –– Space reserved for the left edge of the border during the layout phase.</p>

### SetBorderWidthBottom\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderWidthBottom__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderBottomWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomWidth.html) and returns the style for chaining.

```csharp
public static T SetBorderWidthBottom<T>(this T style, StyleFloat value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFloat

The bottom border width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderBottomWidth</code> –– Space reserved for the bottom edge of the border during the layout phase.</p>

### SetBorderWidthLeft\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderWidthLeft__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderLeftWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftWidth.html) and returns the style for chaining.

```csharp
public static T SetBorderWidthLeft<T>(this T style, StyleFloat value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFloat

The left border width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderLeftWidth</code> –– Space reserved for the left edge of the border during the layout phase.</p>

### SetBorderWidthRight\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderWidthRight__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderRightWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightWidth.html) and returns the style for chaining.

```csharp
public static T SetBorderWidthRight<T>(this T style, StyleFloat value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFloat

The right border width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderRightWidth</code> –– Space reserved for the right edge of the border during the layout phase.</p>

### SetBorderWidthTop\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderWidthTop__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderTopWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopWidth.html) and returns the style for chaining.

```csharp
public static T SetBorderWidthTop<T>(this T style, StyleFloat value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFloat

The top border width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopWidth</code> –– Space reserved for the top edge of the border during the layout phase.</p>

### SetBorderWidthX\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderWidthX__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderLeftWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderLeftWidth.html) and [`borderRightWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderRightWidth.html) and returns the style for chaining.

```csharp
public static T SetBorderWidthX<T>(this T style, StyleFloat value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFloat

The border width to apply to the left and right sides.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderRightWidth</code> –– Space reserved for the right edge of the border during the layout phase.</p>
<p><code>borderLeftWidth</code> –– Space reserved for the left edge of the border during the layout phase.</p>

### SetBorderWidthY\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBorderWidthY__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`borderTopWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderTopWidth.html) and [`borderBottomWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-borderBottomWidth.html) and returns the style for chaining.

```csharp
public static T SetBorderWidthY<T>(this T style, StyleFloat value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFloat

The border width to apply to the top and bottom sides.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>borderTopWidth</code> –– Space reserved for the top edge of the border during the layout phase.</p>
<p><code>borderBottomWidth</code> –– Space reserved for the bottom edge of the border during the layout phase.</p>

### SetBottom\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetBottom__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`bottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-bottom.html) and returns the style for chaining.

```csharp
public static T SetBottom<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The bottom offset to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>bottom</code> –– Bottom distance from the element's box during layout.</p>

### SetColor\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetColor__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`color`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-color.html) and returns the style for chaining.

```csharp
public static T SetColor<T>(this T style, StyleColor value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleColor

The text color to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Color to use when drawing the text of an element.

### SetColor\<T\>\(T, string\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetColor__1___0_System_String_}

Parses an HTML color string and sets [`color`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-color.html), returning the style for chaining.

```csharp
public static T SetColor<T>(this T style, string value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string to parse (e.g. "#RRGGBB" or a named color).

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Color to use when drawing the text of an element.

### SetCursor\<T\>\(T, StyleCursor\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetCursor__1___0_UnityEngine_UIElements_StyleCursor_}

Sets [`cursor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-cursor.html) and returns the style for chaining.

```csharp
public static T SetCursor<T>(this T style, StyleCursor value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleCursor

The cursor style to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Mouse cursor to display when the mouse pointer is over an element.

### SetDisplay\<T\>\(T, StyleEnum\<DisplayStyle\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetDisplay__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_DisplayStyle__}

Sets [`display`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-display.html) and returns the style for chaining.

```csharp
public static T SetDisplay<T>(this T style, StyleEnum<DisplayStyle> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<DisplayStyle\>

The display mode to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Defines how an element is displayed in the layout.

### SetDisplay\<T\>\(T, DisplayStyle\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetDisplay__1___0_UnityEngine_UIElements_DisplayStyle_}

Sets [`display`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-display.html) and returns the style for chaining.

```csharp
public static T SetDisplay<T>(this T style, DisplayStyle value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` DisplayStyle

The display mode to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Defines how an element is displayed in the layout.

### SetDistance\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetDistance__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`top`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-top.html), [`right`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-right.html),
[`bottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-bottom.html), [`left`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-left.html) and returns the style for chaining.

```csharp
public static T SetDistance<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The distance to apply to all sides.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>top</code> –– Top distance from the element's box during layout.</p>
<p><code>right</code> –– Right distance from the element's box during layout.</p>
<p><code>bottom</code> –– Bottom distance from the element's box during layout.</p>
<p><code>left</code> –– Left distance from the element's box during layout.</p>

### SetDistance\<T\>\(T, StyleLength?, StyleLength?, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetDistance__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`top`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-top.html), [`right`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-right.html),
[`bottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-bottom.html), [`left`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-left.html) and returns the style for chaining.

```csharp
public static T SetDistance<T>(this T style, StyleLength? top = null, StyleLength? right = null, StyleLength? bottom = null, StyleLength? left = null) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`top` StyleLength?

The top offset, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`right` StyleLength?

The right offset, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottom` StyleLength?

The bottom offset, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`left` StyleLength?

The left offset, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>top</code> –– Top distance from the element's box during layout.</p>
<p><code>right</code> –– Right distance from the element's box during layout.</p>
<p><code>bottom</code> –– Bottom distance from the element's box during layout.</p>
<p><code>left</code> –– Left distance from the element's box during layout.</p>

### SetDistanceX\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetDistanceX__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`right`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-right.html), [`left`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-left.html) and returns the style for chaining.

```csharp
public static T SetDistanceX<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The horizontal offset to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>right</code> –– Right distance from the element's box during layout.</p>
<p><code>left</code> –– Left distance from the element's box during layout.</p>

### SetDistanceY\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetDistanceY__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`top`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-top.html), [`bottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-bottom.html) and returns the style for chaining.

```csharp
public static T SetDistanceY<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The vertical offset to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>top</code> –– Top distance from the element's box during layout.</p>
<p><code>bottom</code> –– Bottom distance from the element's box during layout.</p>

### SetFilter\<T\>\(T, StyleList\<FilterFunction\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetFilter__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_FilterFunction__}

Sets [`filter`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-filter.html) and returns the style for chaining.

```csharp
public static T SetFilter<T>(this T style, StyleList<FilterFunction> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleList\<FilterFunction\>

The filter effects to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Filter effects to apply to the element.

### SetFlexBasis\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetFlexBasis__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`flexBasis`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexBasis.html) and returns the style for chaining.

```csharp
public static T SetFlexBasis<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The flex basis to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Initial main size of a flex item, on the main flex axis. The final layout might be smaller or larger, according to the flex shrinking and growing determined by the other flex properties.

### SetFlexDirection\<T\>\(T, StyleEnum\<FlexDirection\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetFlexDirection__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_FlexDirection__}

Sets [`flexDirection`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexDirection.html) and returns the style for chaining.

```csharp
public static T SetFlexDirection<T>(this T style, StyleEnum<FlexDirection> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<FlexDirection\>

The flex direction to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Direction of the main axis to layout children in a container.

### SetFlexDirection\<T\>\(T, FlexDirection\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetFlexDirection__1___0_UnityEngine_UIElements_FlexDirection_}

Sets [`flexDirection`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexDirection.html) and returns the style for chaining.

```csharp
public static T SetFlexDirection<T>(this T style, FlexDirection value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` FlexDirection

The flex direction to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Direction of the main axis to layout children in a container.

### SetFlexGrow\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetFlexGrow__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`flexGrow`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexGrow.html) and returns the style for chaining.

```csharp
public static T SetFlexGrow<T>(this T style, StyleFloat value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFloat

The flex grow factor to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies how the item will grow relative to the rest of the flexible items inside the same container.

### SetFlexShrink\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetFlexShrink__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`flexShrink`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexShrink.html) and returns the style for chaining.

```csharp
public static T SetFlexShrink<T>(this T style, StyleFloat value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFloat

The flex shrink factor to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies how the item will shrink relative to the rest of the flexible items inside the same container.

### SetFlexWrap\<T\>\(T, StyleEnum\<Wrap\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetFlexWrap__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Wrap__}

Sets [`flexWrap`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexWrap.html) and returns the style for chaining.

```csharp
public static T SetFlexWrap<T>(this T style, StyleEnum<Wrap> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<Wrap\>

The flex wrap mode to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Placement of children over multiple lines if not enough space is available in this container.

### SetFlexWrap\<T\>\(T, Wrap\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetFlexWrap__1___0_UnityEngine_UIElements_Wrap_}

Sets [`flexWrap`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-flexWrap.html) and returns the style for chaining.

```csharp
public static T SetFlexWrap<T>(this T style, Wrap value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` Wrap

The flex wrap mode to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Placement of children over multiple lines if not enough space is available in this container.

### SetFontSize\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetFontSize__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`fontSize`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-fontSize.html) and returns the style for chaining.

```csharp
public static T SetFontSize<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The font size to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Font size to draw the element's text, specified in point size.

### SetHeight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetHeight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`height`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-height.html) and returns the style for chaining.

```csharp
public static T SetHeight<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The height to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>height</code> –– Fixed height of an element for the layout.</p>

### SetJustifyContent\<T\>\(T, StyleEnum\<Justify\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetJustifyContent__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Justify__}

Sets [`justifyContent`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-justifyContent.html) and returns the style for chaining.

```csharp
public static T SetJustifyContent<T>(this T style, StyleEnum<Justify> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<Justify\>

The justify content mode to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Justification of children on the main axis of this container.

### SetJustifyContent\<T\>\(T, Justify\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetJustifyContent__1___0_UnityEngine_UIElements_Justify_}

Sets [`justifyContent`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-justifyContent.html) and returns the style for chaining.

```csharp
public static T SetJustifyContent<T>(this T style, Justify value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` Justify

The justify content mode to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Justification of children on the main axis of this container.

### SetLeft\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetLeft__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`left`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-left.html) and returns the style for chaining.

```csharp
public static T SetLeft<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The left offset to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>left</code> –– Left distance from the element's box during layout.</p>

### SetLetterSpacing\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetLetterSpacing__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`letterSpacing`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-letterSpacing.html) and returns the style for chaining.

```csharp
public static T SetLetterSpacing<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The letter spacing to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Increases or decreases the space between characters.

### SetMargin\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMargin__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginTop.html), [`marginRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginRight.html),
[`marginBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginBottom.html), [`marginLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginLeft.html) and returns the style for chaining.

```csharp
public static T SetMargin<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The margin to apply to all sides.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginTop</code> –– Space reserved for the top edge of the margin during the layout phase.</p>
<p><code>marginRight</code> –– Space reserved for the right edge of the margin during the layout phase.</p>
<p><code>marginBottom</code> –– Space reserved for the bottom edge of the margin during the layout phase.</p>
<p><code>marginLeft</code> –– Space reserved for the left edge of the margin during the layout phase.</p>

### SetMargin\<T\>\(T, StyleLength?, StyleLength?, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMargin__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`marginTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginTop.html), [`marginRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginRight.html),
[`marginBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginBottom.html), [`marginLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginLeft.html) and returns the style for chaining.

```csharp
public static T SetMargin<T>(this T style, StyleLength? top = null, StyleLength? right = null, StyleLength? bottom = null, StyleLength? left = null) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`top` StyleLength?

The top margin, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`right` StyleLength?

The right margin, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottom` StyleLength?

The bottom margin, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`left` StyleLength?

The left margin, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginTop</code> –– Space reserved for the top edge of the margin during the layout phase.</p>
<p><code>marginRight</code> –– Space reserved for the right edge of the margin during the layout phase.</p>
<p><code>marginBottom</code> –– Space reserved for the bottom edge of the margin during the layout phase.</p>
<p><code>marginLeft</code> –– Space reserved for the left edge of the margin during the layout phase.</p>

### SetMarginBottom\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMarginBottom__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginBottom.html) and returns the style for chaining.

```csharp
public static T SetMarginBottom<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The bottom margin to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginBottom</code> –– Space reserved for the bottom edge of the margin during the layout phase.</p>

### SetMarginLeft\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMarginLeft__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginLeft.html) and returns the style for chaining.

```csharp
public static T SetMarginLeft<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The left margin to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginLeft</code> –– Space reserved for the left edge of the margin during the layout phase.</p>

### SetMarginRight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMarginRight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginRight.html) and returns the style for chaining.

```csharp
public static T SetMarginRight<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The right margin to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginRight</code> –– Space reserved for the right edge of the margin during the layout phase.</p>

### SetMarginTop\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMarginTop__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginTop.html) and returns the style for chaining.

```csharp
public static T SetMarginTop<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The top margin to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginTop</code> –– Space reserved for the top edge of the margin during the layout phase.</p>

### SetMarginX\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMarginX__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginRight.html), [`marginLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginLeft.html) and returns the style for chaining.

```csharp
public static T SetMarginX<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The horizontal margin to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginRight</code> –– Space reserved for the right edge of the margin during the layout phase.</p>
<p><code>marginLeft</code> –– Space reserved for the left edge of the margin during the layout phase.</p>

### SetMarginY\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMarginY__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`marginTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginTop.html), [`marginBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-marginBottom.html) and returns the style for chaining.

```csharp
public static T SetMarginY<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The vertical margin to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>marginTop</code> –– Space reserved for the top edge of the margin during the layout phase.</p>
<p><code>marginBottom</code> –– Space reserved for the bottom edge of the margin during the layout phase.</p>

### SetMaxHeight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMaxHeight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`maxHeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-maxHeight.html) and returns the style for chaining.

```csharp
public static T SetMaxHeight<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The maximum height to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>maxHeight</code> –– Maximum height for an element, when it is flexible or measures its own size.</p>

### SetMaxSize\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMaxSize__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`maxWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-maxWidth.html), [`maxHeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-maxHeight.html) and returns the style for chaining.

```csharp
public static T SetMaxSize<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The maximum size to apply to both width and height.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>maxWidth</code> –– Maximum width for an element, when it is flexible or measures its own size.</p>
<p><code>maxHeight</code> –– Maximum height for an element, when it is flexible or measures its own size.</p>

### SetMaxSize\<T\>\(T, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMaxSize__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`maxWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-maxWidth.html), [`maxHeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-maxHeight.html) and returns the style for chaining.

```csharp
public static T SetMaxSize<T>(this T style, StyleLength? maxWidth = null, StyleLength? maxHeight = null) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`maxWidth` StyleLength?

The maximum width to set, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`maxHeight` StyleLength?

The maximum height to set, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>maxWidth</code> –– Maximum width for an element, when it is flexible or measures its own size.</p>
<p><code>maxHeight</code> –– Maximum height for an element, when it is flexible or measures its own size.</p>

### SetMaxWidth\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMaxWidth__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`maxWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-maxWidth.html) and returns the style for chaining.

```csharp
public static T SetMaxWidth<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The maximum width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>maxWidth</code> –– Maximum width for an element, when it is flexible or measures its own size.</p>

### SetMinHeight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMinHeight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`minHeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-minHeight.html) and returns the style for chaining.

```csharp
public static T SetMinHeight<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The minimum height to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>minHeight</code> –– Minimum height for an element, when it is flexible or measures its own size.</p>

### SetMinSize\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMinSize__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`minWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-minWidth.html), [`minHeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-minHeight.html) and returns the style for chaining.

```csharp
public static T SetMinSize<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The minimum size to apply to both width and height.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>minWidth</code> –– Minimum width for an element, when it is flexible or measures its own size.</p>
<p><code>minHeight</code> –– Minimum height for an element, when it is flexible or measures its own size.</p>

### SetMinSize\<T\>\(T, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMinSize__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`minWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-minWidth.html), [`minHeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-minHeight.html) and returns the style for chaining.

```csharp
public static T SetMinSize<T>(this T style, StyleLength? minWidth = null, StyleLength? minHeight = null) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`minWidth` StyleLength?

The minimum width to set, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`minHeight` StyleLength?

The minimum height to set, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>minWidth</code> –– Minimum width for an element, when it is flexible or measures its own size.</p>
<p><code>minHeight</code> –– Minimum height for an element, when it is flexible or measures its own size.</p>

### SetMinWidth\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetMinWidth__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`minWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-minWidth.html) and returns the style for chaining.

```csharp
public static T SetMinWidth<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The minimum width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>minWidth</code> –– Minimum width for an element, when it is flexible or measures its own size.</p>

### SetNormalUnityFontStyleAndWeight\<T\>\(T\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetNormalUnityFontStyleAndWeight__1___0_}

Sets [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html) to [`Normal`](https://docs.unity3d.com/ScriptReference/FontStyle-Normal.html), removing bold and italic.

```csharp
public static T SetNormalUnityFontStyleAndWeight<T>(this T style) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Sets the value unconditionally, regardless of any current bold or italic style.

### SetOpacity\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetOpacity__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`opacity`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-opacity.html) and returns the style for chaining.

```csharp
public static T SetOpacity<T>(this T style, StyleFloat value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFloat

The opacity to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies the transparency of an element and of its children.

### SetOverflow\<T\>\(T, StyleEnum\<Overflow\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetOverflow__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Overflow__}

Sets [`overflow`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-overflow.html) and returns the style for chaining.

```csharp
public static T SetOverflow<T>(this T style, StyleEnum<Overflow> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<Overflow\>

The overflow behavior to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

How a container behaves if its content overflows its own box.

### SetOverflow\<T\>\(T, Overflow\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetOverflow__1___0_UnityEngine_UIElements_Overflow_}

Sets [`overflow`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-overflow.html) and returns the style for chaining.

```csharp
public static T SetOverflow<T>(this T style, Overflow value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` Overflow

The overflow behavior to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

How a container behaves if its content overflows its own box.

### SetPadding\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetPadding__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingTop.html), [`paddingRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingRight.html),
[`paddingBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingBottom.html), [`paddingLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingLeft.html) and returns the style for chaining.

```csharp
public static T SetPadding<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The padding to apply to all sides.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingTop</code> –– Space reserved for the top edge of the padding during the layout phase.</p>
<p><code>paddingRight</code> –– Space reserved for the right edge of the padding during the layout phase.</p>
<p><code>paddingBottom</code> –– Space reserved for the bottom edge of the padding during the layout phase.</p>
<p><code>paddingLeft</code> –– Space reserved for the left edge of the padding during the layout phase.</p>

### SetPadding\<T\>\(T, StyleLength?, StyleLength?, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetPadding__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`paddingTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingTop.html), [`paddingRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingRight.html),
[`paddingBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingBottom.html), [`paddingLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingLeft.html) and returns the style for chaining.

```csharp
public static T SetPadding<T>(this T style, StyleLength? top = null, StyleLength? right = null, StyleLength? bottom = null, StyleLength? left = null) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`top` StyleLength?

The top padding, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`right` StyleLength?

The right padding, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottom` StyleLength?

The bottom padding, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`left` StyleLength?

The left padding, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingTop</code> –– Space reserved for the top edge of the padding during the layout phase.</p>
<p><code>paddingRight</code> –– Space reserved for the right edge of the padding during the layout phase.</p>
<p><code>paddingBottom</code> –– Space reserved for the bottom edge of the padding during the layout phase.</p>
<p><code>paddingLeft</code> –– Space reserved for the left edge of the padding during the layout phase.</p>

### SetPaddingBottom\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetPaddingBottom__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingBottom.html) and returns the style for chaining.

```csharp
public static T SetPaddingBottom<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The bottom padding to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingBottom</code> –– Space reserved for the bottom edge of the padding during the layout phase.</p>

### SetPaddingLeft\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetPaddingLeft__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingLeft.html) and returns the style for chaining.

```csharp
public static T SetPaddingLeft<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The left padding to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingLeft</code> –– Space reserved for the left edge of the padding during the layout phase.</p>

### SetPaddingRight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetPaddingRight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingRight.html) and returns the style for chaining.

```csharp
public static T SetPaddingRight<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The right padding to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingRight</code> –– Space reserved for the right edge of the padding during the layout phase.</p>

### SetPaddingTop\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetPaddingTop__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingTop.html) and returns the style for chaining.

```csharp
public static T SetPaddingTop<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The top padding to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingTop</code> –– Space reserved for the top edge of the padding during the layout phase.</p>

### SetPaddingX\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetPaddingX__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingRight.html), [`paddingLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingLeft.html) and returns the style for chaining.

```csharp
public static T SetPaddingX<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The horizontal padding to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingRight</code> –– Space reserved for the right edge of the padding during the layout phase.</p>
<p><code>paddingLeft</code> –– Space reserved for the left edge of the padding during the layout phase.</p>

### SetPaddingY\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetPaddingY__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`paddingTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingTop.html), [`paddingBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-paddingBottom.html) and returns the style for chaining.

```csharp
public static T SetPaddingY<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The vertical padding to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>paddingTop</code> –– Space reserved for the top edge of the padding during the layout phase.</p>
<p><code>paddingBottom</code> –– Space reserved for the bottom edge of the padding during the layout phase.</p>

### SetPosition\<T\>\(T, StyleEnum\<Position\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetPosition__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Position__}

Sets [`position`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-position.html) and returns the style for chaining.

```csharp
public static T SetPosition<T>(this T style, StyleEnum<Position> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<Position\>

The position type to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Element's positioning in its parent container.

### SetPosition\<T\>\(T, Position\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetPosition__1___0_UnityEngine_UIElements_Position_}

Sets [`position`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-position.html) and returns the style for chaining.

```csharp
public static T SetPosition<T>(this T style, Position value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` Position

The position type to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Element's positioning in its parent container.

### SetRight\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetRight__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`right`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-right.html) and returns the style for chaining.

```csharp
public static T SetRight<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The right offset to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>right</code> –– Right distance from the element's box during layout.</p>

### SetRotate\<T\>\(T, StyleRotate\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetRotate__1___0_UnityEngine_UIElements_StyleRotate_}

Sets [`rotate`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-rotate.html) and returns the style for chaining.

```csharp
public static T SetRotate<T>(this T style, StyleRotate value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleRotate

The rotation to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

A rotation transformation.

### SetScale\<T\>\(T, StyleScale\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetScale__1___0_UnityEngine_UIElements_StyleScale_}

Sets [`scale`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-scale.html) and returns the style for chaining.

```csharp
public static T SetScale<T>(this T style, StyleScale value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleScale

The scale transformation to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

A scaling transformation.

### SetSize\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetSize__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`width`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-width.html), [`height`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-height.html) and returns the style for chaining.

```csharp
public static T SetSize<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The size to apply to both width and height.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>width</code> –– Fixed width of an element for the layout.</p>
<p><code>height</code> –– Fixed height of an element for the layout.</p>

### SetSize\<T\>\(T, StyleLength?, StyleLength?\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetSize__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__}

Sets [`width`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-width.html), [`height`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-height.html) and returns the style for chaining.

```csharp
public static T SetSize<T>(this T style, StyleLength? width = null, StyleLength? height = null) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`width` StyleLength?

The width to set, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`height` StyleLength?

The height to set, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>width</code> –– Fixed width of an element for the layout.</p>
<p><code>height</code> –– Fixed height of an element for the layout.</p>

### SetTextOverflow\<T\>\(T, StyleEnum\<TextOverflow\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetTextOverflow__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_TextOverflow__}

Sets [`textOverflow`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-textOverflow.html) and returns the style for chaining.

```csharp
public static T SetTextOverflow<T>(this T style, StyleEnum<TextOverflow> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<TextOverflow\>

The text overflow mode to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

The element's text overflow mode.

### SetTextOverflow\<T\>\(T, TextOverflow\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetTextOverflow__1___0_UnityEngine_UIElements_TextOverflow_}

Sets [`textOverflow`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-textOverflow.html) and returns the style for chaining.

```csharp
public static T SetTextOverflow<T>(this T style, TextOverflow value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` TextOverflow

The text overflow mode to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

The element's text overflow mode.

### SetTextShadow\<T\>\(T, StyleTextShadow\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetTextShadow__1___0_UnityEngine_UIElements_StyleTextShadow_}

Sets [`textShadow`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-textShadow.html) and returns the style for chaining.

```csharp
public static T SetTextShadow<T>(this T style, StyleTextShadow value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleTextShadow

The text shadow to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Drop shadow of the text.

### SetTop\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetTop__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`top`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-top.html) and returns the style for chaining.

```csharp
public static T SetTop<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The top offset to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>top</code> –– Top distance from the element's box during layout.</p>

### SetTransformOrigin\<T\>\(T, StyleTransformOrigin\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetTransformOrigin__1___0_UnityEngine_UIElements_StyleTransformOrigin_}

Sets [`transformOrigin`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-transformOrigin.html) and returns the style for chaining.

```csharp
public static T SetTransformOrigin<T>(this T style, StyleTransformOrigin value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleTransformOrigin

The transform origin to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

The transformation origin is the point around which a transformation is applied.

### SetTransitionDelay\<T\>\(T, StyleList\<TimeValue\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetTransitionDelay__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_TimeValue__}

Sets [`transitionDelay`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-transitionDelay.html) and returns the style for chaining.

```csharp
public static T SetTransitionDelay<T>(this T style, StyleList<TimeValue> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleList\<TimeValue\>

The transition delays to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Duration to wait before starting a property's transition effect when its value changes.

### SetTransitionDuration\<T\>\(T, StyleList\<TimeValue\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetTransitionDuration__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_TimeValue__}

Sets [`transitionDuration`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-transitionDuration.html) and returns the style for chaining.

```csharp
public static T SetTransitionDuration<T>(this T style, StyleList<TimeValue> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleList\<TimeValue\>

The transition durations to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Time a transition animation should take to complete.

### SetTransitionProperty\<T\>\(T, StyleList\<StylePropertyName\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetTransitionProperty__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_StylePropertyName__}

Sets [`transitionProperty`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-transitionProperty.html) and returns the style for chaining.

```csharp
public static T SetTransitionProperty<T>(this T style, StyleList<StylePropertyName> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleList\<StylePropertyName\>

The transition properties to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Properties to which a transition effect should be applied.

### SetTransitionTimingFunction\<T\>\(T, StyleList\<EasingFunction\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetTransitionTimingFunction__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_EasingFunction__}

Sets [`transitionTimingFunction`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-transitionTimingFunction.html) and returns the style for chaining.

```csharp
public static T SetTransitionTimingFunction<T>(this T style, StyleList<EasingFunction> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleList\<EasingFunction\>

The transition timing functions to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Determines how intermediate values are calculated for properties modified by a transition effect.

### SetTranslate\<T\>\(T, StyleTranslate\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetTranslate__1___0_UnityEngine_UIElements_StyleTranslate_}

Sets [`translate`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-translate.html) and returns the style for chaining.

```csharp
public static T SetTranslate<T>(this T style, StyleTranslate value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleTranslate

The translation to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

A translate transformation.

### SetUnityBackgroundImageTintColor\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityBackgroundImageTintColor__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`unityBackgroundImageTintColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityBackgroundImageTintColor.html) and returns the style for chaining.

```csharp
public static T SetUnityBackgroundImageTintColor<T>(this T style, StyleColor value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleColor

The background image tint color to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Tinting color for the element's backgroundImage.

### SetUnityBackgroundImageTintColor\<T\>\(T, string\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityBackgroundImageTintColor__1___0_System_String_}

Sets the background image tint color by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetUnityBackgroundImageTintColor<T>(this T style, string value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

The style type.

### SetUnityEditorTextRenderingMode\<T\>\(T, StyleEnum\<EditorTextRenderingMode\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityEditorTextRenderingMode__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_EditorTextRenderingMode__}

Sets [`unityEditorTextRenderingMode`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityEditorTextRenderingMode.html) and returns the style for chaining.

```csharp
public static T SetUnityEditorTextRenderingMode<T>(this T style, StyleEnum<EditorTextRenderingMode> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<EditorTextRenderingMode\>

The editor text rendering mode to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

TextElement editor rendering mode.

### SetUnityEditorTextRenderingMode\<T\>\(T, EditorTextRenderingMode\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityEditorTextRenderingMode__1___0_UnityEngine_UIElements_EditorTextRenderingMode_}

Sets [`unityEditorTextRenderingMode`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityEditorTextRenderingMode.html) and returns the style for chaining.

```csharp
public static T SetUnityEditorTextRenderingMode<T>(this T style, EditorTextRenderingMode value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` EditorTextRenderingMode

The editor text rendering mode to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

TextElement editor rendering mode.

### SetUnityFont\<T\>\(T, StyleFont\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityFont__1___0_UnityEngine_UIElements_StyleFont_}

Sets [`unityFont`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFont.html) and returns the style for chaining.

```csharp
public static T SetUnityFont<T>(this T style, StyleFont value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFont

The font to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Font to draw the element's text, defined as a Font object.

### SetUnityFontDefinition\<T\>\(T, StyleFontDefinition\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityFontDefinition__1___0_UnityEngine_UIElements_StyleFontDefinition_}

Sets [`unityFontDefinition`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontDefinition.html) and returns the style for chaining.

```csharp
public static T SetUnityFontDefinition<T>(this T style, StyleFontDefinition value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFontDefinition

The font definition to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Font to draw the element's text, defined as a FontDefinition structure. It takes precedence over -unity-font.

### SetUnityFontStyleAndWeight\<T\>\(T, StyleEnum\<FontStyle\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityFontStyleAndWeight__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_FontStyle__}

Sets [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html) and returns the style for chaining.

```csharp
public static T SetUnityFontStyleAndWeight<T>(this T style, StyleEnum<FontStyle> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<FontStyle\>

The font style and weight to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Font style and weight (normal, bold, italic) to draw the element's text.

### SetUnityFontStyleAndWeight\<T\>\(T, FontStyle\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityFontStyleAndWeight__1___0_UnityEngine_FontStyle_}

Sets [`unityFontStyleAndWeight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityFontStyleAndWeight.html) and returns the style for chaining.

```csharp
public static T SetUnityFontStyleAndWeight<T>(this T style, FontStyle value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` FontStyle

The font style and weight to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Font style and weight (normal, bold, italic) to draw the element's text.

### SetUnityMaterial\<T\>\(T, StyleMaterialDefinition\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityMaterial__1___0_UnityEngine_UIElements_StyleMaterialDefinition_}

Sets [`unityMaterial`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityMaterial.html) and returns the style for chaining.

```csharp
public static T SetUnityMaterial<T>(this T style, StyleMaterialDefinition value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleMaterialDefinition

The material to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Custom material to use on the element.

### SetUnityOverflowClipBox\<T\>\(T, StyleEnum\<OverflowClipBox\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityOverflowClipBox__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_OverflowClipBox__}

Sets [`unityOverflowClipBox`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityOverflowClipBox.html) and returns the style for chaining.

```csharp
public static T SetUnityOverflowClipBox<T>(this T style, StyleEnum<OverflowClipBox> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<OverflowClipBox\>

The overflow clip box to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies which box the element content is clipped against.

### SetUnityOverflowClipBox\<T\>\(T, OverflowClipBox\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityOverflowClipBox__1___0_UnityEngine_UIElements_OverflowClipBox_}

Sets [`unityOverflowClipBox`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityOverflowClipBox.html) and returns the style for chaining.

```csharp
public static T SetUnityOverflowClipBox<T>(this T style, OverflowClipBox value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` OverflowClipBox

The overflow clip box to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies which box the element content is clipped against.

### SetUnityParagraphSpacing\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityParagraphSpacing__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`unityParagraphSpacing`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityParagraphSpacing.html) and returns the style for chaining.

```csharp
public static T SetUnityParagraphSpacing<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The paragraph spacing to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Increases or decreases the space between paragraphs.

### SetUnitySlice\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnitySlice__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceTop.html), [`unitySliceRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceRight.html),
[`unitySliceBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceBottom.html), [`unitySliceLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceLeft.html) and returns the style for chaining.

```csharp
public static T SetUnitySlice<T>(this T style, StyleInt value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleInt

The slice width to apply to all sides.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceTop</code> –– Size of the 9-slice's top edge when painting an element's background image.</p>
<p><code>unitySliceRight</code> –– Size of the 9-slice's right edge when painting an element's background image.</p>
<p><code>unitySliceBottom</code> –– Size of the 9-slice's bottom edge when painting an element's background image.</p>
<p><code>unitySliceLeft</code> –– Size of the 9-slice's left edge when painting an element's background image.</p>

### SetUnitySlice\<T\>\(T, StyleInt?, StyleInt?, StyleInt?, StyleInt?\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnitySlice__1___0_System_Nullable_UnityEngine_UIElements_StyleInt__System_Nullable_UnityEngine_UIElements_StyleInt__System_Nullable_UnityEngine_UIElements_StyleInt__System_Nullable_UnityEngine_UIElements_StyleInt__}

Sets [`unitySliceTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceTop.html), [`unitySliceRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceRight.html),
[`unitySliceBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceBottom.html), [`unitySliceLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceLeft.html) and returns the style for chaining.

```csharp
public static T SetUnitySlice<T>(this T style, StyleInt? top = null, StyleInt? right = null, StyleInt? bottom = null, StyleInt? left = null) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`top` StyleInt?

The top slice width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`right` StyleInt?

The right slice width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`bottom` StyleInt?

The bottom slice width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

`left` StyleInt?

The left slice width, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to leave unchanged.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceTop</code> –– Size of the 9-slice's top edge when painting an element's background image.</p>
<p><code>unitySliceRight</code> –– Size of the 9-slice's right edge when painting an element's background image.</p>
<p><code>unitySliceBottom</code> –– Size of the 9-slice's bottom edge when painting an element's background image.</p>
<p><code>unitySliceLeft</code> –– Size of the 9-slice's left edge when painting an element's background image.</p>

### SetUnitySliceBottom\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnitySliceBottom__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceBottom.html) and returns the style for chaining.

```csharp
public static T SetUnitySliceBottom<T>(this T style, StyleInt value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleInt

The bottom slice width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceBottom</code> –– Size of the 9-slice's bottom edge when painting an element's background image.</p>

### SetUnitySliceLeft\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnitySliceLeft__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceLeft.html) and returns the style for chaining.

```csharp
public static T SetUnitySliceLeft<T>(this T style, StyleInt value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleInt

The left slice width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceLeft</code> –– Size of the 9-slice's left edge when painting an element's background image.</p>

### SetUnitySliceRight\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnitySliceRight__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceRight.html) and returns the style for chaining.

```csharp
public static T SetUnitySliceRight<T>(this T style, StyleInt value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleInt

The right slice width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceRight</code> –– Size of the 9-slice's right edge when painting an element's background image.</p>

### SetUnitySliceScale\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnitySliceScale__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`unitySliceScale`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceScale.html) and returns the style for chaining.

```csharp
public static T SetUnitySliceScale<T>(this T style, StyleFloat value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFloat

The slice scale to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Scale applied to an element's slices.

### SetUnitySliceTop\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnitySliceTop__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceTop.html) and returns the style for chaining.

```csharp
public static T SetUnitySliceTop<T>(this T style, StyleInt value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleInt

The top slice width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceTop</code> –– Size of the 9-slice's top edge when painting an element's background image.</p>

### SetUnitySliceType\<T\>\(T, StyleEnum\<SliceType\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnitySliceType__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_SliceType__}

Sets [`unitySliceType`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceType.html) and returns the style for chaining.

```csharp
public static T SetUnitySliceType<T>(this T style, StyleEnum<SliceType> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<SliceType\>

The slice type to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies the type of slicing.

### SetUnitySliceType\<T\>\(T, SliceType\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnitySliceType__1___0_UnityEngine_UIElements_SliceType_}

Sets [`unitySliceType`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceType.html) and returns the style for chaining.

```csharp
public static T SetUnitySliceType<T>(this T style, SliceType value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` SliceType

The slice type to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies the type of slicing.

### SetUnitySliceX\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnitySliceX__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceRight`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceRight.html), [`unitySliceLeft`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceLeft.html) and returns the style for chaining.

```csharp
public static T SetUnitySliceX<T>(this T style, StyleInt value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleInt

The horizontal slice width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceRight</code> –– Size of the 9-slice's right edge when painting an element's background image.</p>
<p><code>unitySliceLeft</code> –– Size of the 9-slice's left edge when painting an element's background image.</p>

### SetUnitySliceY\<T\>\(T, StyleInt\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnitySliceY__1___0_UnityEngine_UIElements_StyleInt_}

Sets [`unitySliceTop`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceTop.html), [`unitySliceBottom`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unitySliceBottom.html) and returns the style for chaining.

```csharp
public static T SetUnitySliceY<T>(this T style, StyleInt value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleInt

The vertical slice width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>unitySliceTop</code> –– Size of the 9-slice's top edge when painting an element's background image.</p>
<p><code>unitySliceBottom</code> –– Size of the 9-slice's bottom edge when painting an element's background image.</p>

### SetUnityTextAlign\<T\>\(T, StyleEnum\<TextAnchor\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityTextAlign__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_TextAnchor__}

Sets [`unityTextAlign`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextAlign.html) and returns the style for chaining.

```csharp
public static T SetUnityTextAlign<T>(this T style, StyleEnum<TextAnchor> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<TextAnchor\>

The text alignment to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Horizontal and vertical text alignment in the element's box.

### SetUnityTextAlign\<T\>\(T, TextAnchor\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityTextAlign__1___0_UnityEngine_TextAnchor_}

Sets [`unityTextAlign`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextAlign.html) and returns the style for chaining.

```csharp
public static T SetUnityTextAlign<T>(this T style, TextAnchor value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` TextAnchor

The text alignment to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Horizontal and vertical text alignment in the element's box.

### SetUnityTextAutoSize\<T\>\(T, StyleTextAutoSize\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityTextAutoSize__1___0_UnityEngine_UIElements_StyleTextAutoSize_}

Sets [`unityTextAutoSize`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextAutoSize.html) and returns the style for chaining.

```csharp
public static T SetUnityTextAutoSize<T>(this T style, StyleTextAutoSize value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleTextAutoSize

The text auto size settings to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Overrides any explicit font-size to scale text within the defined minimum and maximum bounds, recalculating as needed to fit its container.

### SetUnityTextGenerator\<T\>\(T, StyleEnum\<TextGeneratorType\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityTextGenerator__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_TextGeneratorType__}

Sets [`unityTextGenerator`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextGenerator.html) and returns the style for chaining.

```csharp
public static T SetUnityTextGenerator<T>(this T style, StyleEnum<TextGeneratorType> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<TextGeneratorType\>

The text generator type to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Switches between Unity's standard and advanced text generator.

### SetUnityTextGenerator\<T\>\(T, TextGeneratorType\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityTextGenerator__1___0_UnityEngine_TextGeneratorType_}

Sets [`unityTextGenerator`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextGenerator.html) and returns the style for chaining.

```csharp
public static T SetUnityTextGenerator<T>(this T style, TextGeneratorType value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` TextGeneratorType

The text generator type to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Switches between Unity's standard and advanced text generator.

### SetUnityTextOutlineColor\<T\>\(T, StyleColor\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityTextOutlineColor__1___0_UnityEngine_UIElements_StyleColor_}

Sets [`unityTextOutlineColor`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextOutlineColor.html) and returns the style for chaining.

```csharp
public static T SetUnityTextOutlineColor<T>(this T style, StyleColor value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleColor

The text outline color to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Outline color of the text.

### SetUnityTextOutlineColor\<T\>\(T, string\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityTextOutlineColor__1___0_System_String_}

Sets the text outline color by parsing an HTML color string via [`TryParseHtmlString`](https://docs.unity3d.com/ScriptReference/ColorUtility-TryParseHtmlString.html).

```csharp
public static T SetUnityTextOutlineColor<T>(this T style, string value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The HTML color string (e.g. "#FF0000", "red").

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

The style type.

### SetUnityTextOutlineWidth\<T\>\(T, StyleFloat\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityTextOutlineWidth__1___0_UnityEngine_UIElements_StyleFloat_}

Sets [`unityTextOutlineWidth`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextOutlineWidth.html) and returns the style for chaining.

```csharp
public static T SetUnityTextOutlineWidth<T>(this T style, StyleFloat value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleFloat

The text outline width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Outline width of the text.

### SetUnityTextOverflowPosition\<T\>\(T, StyleEnum\<TextOverflowPosition\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityTextOverflowPosition__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_TextOverflowPosition__}

Sets [`unityTextOverflowPosition`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextOverflowPosition.html) and returns the style for chaining.

```csharp
public static T SetUnityTextOverflowPosition<T>(this T style, StyleEnum<TextOverflowPosition> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<TextOverflowPosition\>

The text overflow position to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

The element's text overflow position.

### SetUnityTextOverflowPosition\<T\>\(T, TextOverflowPosition\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetUnityTextOverflowPosition__1___0_UnityEngine_UIElements_TextOverflowPosition_}

Sets [`unityTextOverflowPosition`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-unityTextOverflowPosition.html) and returns the style for chaining.

```csharp
public static T SetUnityTextOverflowPosition<T>(this T style, TextOverflowPosition value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` TextOverflowPosition

The text overflow position to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

The element's text overflow position.

### SetVisibility\<T\>\(T, StyleEnum\<Visibility\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetVisibility__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Visibility__}

Sets [`visibility`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-visibility.html) and returns the style for chaining.

```csharp
public static T SetVisibility<T>(this T style, StyleEnum<Visibility> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<Visibility\>

The visibility to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies whether an element is visible.

### SetVisibility\<T\>\(T, Visibility\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetVisibility__1___0_UnityEngine_UIElements_Visibility_}

Sets [`visibility`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-visibility.html) and returns the style for chaining.

```csharp
public static T SetVisibility<T>(this T style, Visibility value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` Visibility

The visibility to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Specifies whether an element is visible.

### SetWhiteSpace\<T\>\(T, StyleEnum\<WhiteSpace\>\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetWhiteSpace__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_WhiteSpace__}

Sets [`whiteSpace`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-whiteSpace.html) and returns the style for chaining.

```csharp
public static T SetWhiteSpace<T>(this T style, StyleEnum<WhiteSpace> value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleEnum\<WhiteSpace\>

The white-space mode to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Word wrap over multiple lines if not enough space is available to draw the text of an element.

### SetWhiteSpace\<T\>\(T, WhiteSpace\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetWhiteSpace__1___0_UnityEngine_UIElements_WhiteSpace_}

Sets [`whiteSpace`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-whiteSpace.html) and returns the style for chaining.

```csharp
public static T SetWhiteSpace<T>(this T style, WhiteSpace value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` WhiteSpace

The white-space mode to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Word wrap over multiple lines if not enough space is available to draw the text of an element.

### SetWidth\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetWidth__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`width`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-width.html) and returns the style for chaining.

```csharp
public static T SetWidth<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The width to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

<p><code>width</code> –– Fixed width of an element for the layout.</p>

### SetWordSpacing\<T\>\(T, StyleLength\) {#Aspid_FastTools_UIElements_IStyleExtensions_SetWordSpacing__1___0_UnityEngine_UIElements_StyleLength_}

Sets [`wordSpacing`](https://docs.unity3d.com/ScriptReference/UIElements-IStyle-wordSpacing.html) and returns the style for chaining.

```csharp
public static T SetWordSpacing<T>(this T style, StyleLength value) where T : IStyle
```

#### Parameters

`style` T

The style to modify.

`value` StyleLength

The word spacing to set.

#### Returns

 T

The style, for chaining.

#### Type Parameters

`T` 

#### Remarks

Increases or decreases the space between words.

