---
title: "Class BaseFieldExtensions"
sidebar_label: "BaseFieldExtensions"
description: "Class BaseFieldExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldExtensions {#Aspid_FastTools_UIElements_BaseFieldExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

```csharp
public static class BaseFieldExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldExtensions](Aspid.FastTools.UIElements.BaseFieldExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetLabel\<TField, TValue\>\(TField, string\) {#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_}

Sets the [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html) property displayed next to the field and returns the element for chaining.

```csharp
public static TField SetLabel<TField, TValue>(this TField element, string value) where TField : BaseField<TValue>
```

#### Parameters

`element` TField

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The label text to set.

#### Returns

 TField

The element, for chaining.

#### Type Parameters

`TField` 

The field type.

`TValue` 

The value type held by the field.

