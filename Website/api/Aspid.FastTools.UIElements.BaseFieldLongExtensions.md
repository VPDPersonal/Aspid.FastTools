---
title: "Class BaseFieldLongExtensions"
sidebar_label: "BaseFieldLongExtensions"
description: "Class BaseFieldLongExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldLongExtensions {#Aspid_FastTools_UIElements_BaseFieldLongExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldLongExtensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldLongExtensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Int64`](https://learn.microsoft.com/dotnet/api/system.int64).

```csharp
public static class BaseFieldLongExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldLongExtensions](Aspid.FastTools.UIElements.BaseFieldLongExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldLongExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<long>
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The label text to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The field type.

