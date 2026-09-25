---
title: "Class EnumFieldExtensions"
sidebar_label: "EnumFieldExtensions"
description: "Class EnumFieldExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class EnumFieldExtensions {#Aspid_FastTools_UIElements_EnumFieldExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides extension methods for [`EnumField`](https://docs.unity3d.com/ScriptReference/UIElements-EnumField.html).

```csharp
public static class EnumFieldExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[EnumFieldExtensions](Aspid.FastTools.UIElements.EnumFieldExtensions.md)


## Methods

### Initialize\<T\>\(T, Enum, bool\) {#Aspid_FastTools_UIElements_EnumFieldExtensions_Initialize__1___0_System_Enum_System_Boolean_}

Initializes the field with a default enum value via [`Init`](https://docs.unity3d.com/ScriptReference/UIElements-EnumField-Init.html).

```csharp
public static T Initialize<T>(this T element, Enum defaultValue, bool includeObsoleteValues = false) where T : EnumField
```

#### Parameters

`element` T

The element to modify.

`defaultValue` [Enum](https://learn.microsoft.com/dotnet/api/system.enum)

The default enum value to display.

`includeObsoleteValues` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, obsolete enum values are included in the choices.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

