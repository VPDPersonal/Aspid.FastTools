---
title: "Class ICustomStyleExtensions"
sidebar_label: "ICustomStyleExtensions"
description: "Class ICustomStyleExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class ICustomStyleExtensions {#Aspid_FastTools_UIElements_ICustomStyleExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.Unity.dll  

Extension methods for [`ICustomStyle`](https://docs.unity3d.com/ScriptReference/UIElements-ICustomStyle.html) that bridge USS string-typed custom
properties to strongly-typed C# values.

```csharp
public static class ICustomStyleExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[ICustomStyleExtensions](Aspid.FastTools.UIElements.ICustomStyleExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### TryGetByEnum\<T\>\(ICustomStyle, CustomStyleProperty\<string\>, out T\) {#Aspid_FastTools_UIElements_ICustomStyleExtensions_TryGetByEnum__1_UnityEngine_UIElements_ICustomStyle_UnityEngine_UIElements_CustomStyleProperty_System_String____0__}

Resolves a [`CustomStyleProperty<T>`](https://docs.unity3d.com/ScriptReference/UIElements-CustomStyleProperty.html) whose USS value is a string and parses
it as the enum <code class="typeparamref">T</code>. Parsing is case-insensitive.

```csharp
public static bool TryGetByEnum<T>(this ICustomStyle style, CustomStyleProperty<string> property, out T value) where T : struct, Enum
```

#### Parameters

`style` ICustomStyle

The resolved custom-style container, typically obtained from
    [`customStyle`](https://docs.unity3d.com/ScriptReference/UIElements-CustomStyleResolvedEvent-customStyle.html).

`property` CustomStyleProperty\<[string](https://learn.microsoft.com/dotnet/api/system.string)\>

The custom style property whose string value should be parsed.

`value` T

When this method returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the parsed enum
    value; otherwise <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/default">default</a>.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if the property was resolved and successfully parsed
    as <code class="typeparamref">T</code>; otherwise <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

#### Type Parameters

`T` 

The enum type to parse the USS value as.

