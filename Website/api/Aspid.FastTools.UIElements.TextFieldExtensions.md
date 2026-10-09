---
title: "Class TextFieldExtensions"
sidebar_label: "TextFieldExtensions"
description: "Class TextFieldExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TextFieldExtensions {#Aspid_FastTools_UIElements_TextFieldExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`TextField`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextField.html).

```csharp
public static class TextFieldExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[TextFieldExtensions](Aspid.FastTools.UIElements.TextFieldExtensions.md)


## Methods

### SetMultiline\<T\>\(T, bool\) {#Aspid_FastTools_UIElements_TextFieldExtensions_SetMultiline__1___0_System_Boolean_}

Sets [`multiline`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextField-multiline.html).

```csharp
public static T SetMultiline<T>(this T element, bool value) where T : TextField
```

#### Parameters

`element` T

The element to modify.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the field accepts several lines of text.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The field type.

### SetVerticalScrollerVisibilitySelf\<T\>\(T, ScrollerVisibility\) {#Aspid_FastTools_UIElements_TextFieldExtensions_SetVerticalScrollerVisibilitySelf__1___0_UnityEngine_UIElements_ScrollerVisibility_}

Sets [`verticalScrollerVisibility`](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.TextInputBaseField_1-verticalScrollerVisibility.html).

```csharp
public static T SetVerticalScrollerVisibilitySelf<T>(this T element, ScrollerVisibility value) where T : TextField
```

#### Parameters

`element` T

The element to modify.

`value` ScrollerVisibility

The visibility of the vertical scroller to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The field type.

#### Remarks

Named with <code>Self</code>: the obsolete instance method <code>SetVerticalScrollerVisibility</code> of Unity returns
[`Boolean`](https://learn.microsoft.com/dotnet/api/system.boolean) and hides an extension method of that name.

