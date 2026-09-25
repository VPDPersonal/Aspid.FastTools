---
title: "Class BaseFieldUshortExtensions"
sidebar_label: "BaseFieldUshortExtensions"
description: "Class BaseFieldUshortExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldUshortExtensions {#Aspid_FastTools_UIElements_BaseFieldUshortExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldUshortExtensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldUshortExtensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`UInt16`](https://learn.microsoft.com/dotnet/api/system.uint16).

```csharp
public static class BaseFieldUshortExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldUshortExtensions](Aspid.FastTools.UIElements.BaseFieldUshortExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldUshortExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<ushort>
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

