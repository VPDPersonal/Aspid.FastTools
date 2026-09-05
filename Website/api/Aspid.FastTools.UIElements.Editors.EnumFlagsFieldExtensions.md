---
title: "Class EnumFlagsFieldExtensions"
sidebar_label: "EnumFlagsFieldExtensions"
description: "Class EnumFlagsFieldExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class EnumFlagsFieldExtensions {#Aspid_FastTools_UIElements_Editors_EnumFlagsFieldExtensions}

Namespace: [Aspid.FastTools.UIElements.Editors](Aspid.FastTools.UIElements.Editors.md)  
Assembly: Aspid.FastTools.Unity.Editor.dll  

```csharp
public static class EnumFlagsFieldExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[EnumFlagsFieldExtensions](Aspid.FastTools.UIElements.Editors.EnumFlagsFieldExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### Initialize\<T\>\(T, Enum, bool\) {#Aspid_FastTools_UIElements_Editors_EnumFlagsFieldExtensions_Initialize__1___0_System_Enum_System_Boolean_}

Initializes the field with a default enum flags value via [`Init`](https://docs.unity3d.com/ScriptReference/UIElements-EnumFlagsField-Init.html).

```csharp
public static T Initialize<T>(this T element, Enum defaultValue, bool includeObsoleteValues = false) where T : EnumFlagsField
```

#### Parameters

`element` T

The element to modify.

`defaultValue` [Enum](https://learn.microsoft.com/dotnet/api/system.enum)

The default enum flags value to display.

`includeObsoleteValues` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to include obsolete enum values in the choices.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

