---
title: "Class StringExtensions"
sidebar_label: "StringExtensions"
description: "Class StringExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class StringExtensions {#Aspid_FastTools_StringExtensions}

Namespace: [Aspid.FastTools](Aspid.FastTools.md)  
Assembly: Aspid.FastTools.dll  

```csharp
public static class StringExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[StringExtensions](Aspid.FastTools.StringExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### ToKebabCase\(string\) {#Aspid_FastTools_StringExtensions_ToKebabCase_System_String_}

Converts a PascalCase, camelCase, snake_case or space-separated string to kebab-case.
Leading underscores are dropped and consecutive uppercase letters (acronyms) are kept
together, e.g. "_damageColors" → "damage-colors", "HTTPServer" → "http-server".

```csharp
public static string ToKebabCase(this string value)
```

#### Parameters

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The string to convert.

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

The kebab-case representation of <code class="paramref">value</code>.

