---
title: "Class BaseFieldUintExtensions"
sidebar_label: "BaseFieldUintExtensions"
description: "Class BaseFieldUintExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldUintExtensions {#Aspid_FastTools_UIElements_BaseFieldUintExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldUintExtensions.SetLabel<T>`](Aspid.FastTools.UIElements.BaseFieldUintExtensions.md#Aspid_FastTools_UIElements_BaseFieldUintExtensions_SetLabel__1___0_System_String_) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`UInt32`](https://learn.microsoft.com/dotnet/api/system.uint32).

```csharp
public static class BaseFieldUintExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldUintExtensions](Aspid.FastTools.UIElements.BaseFieldUintExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldUintExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<uint>
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

