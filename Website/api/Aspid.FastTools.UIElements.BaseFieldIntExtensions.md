---
title: "Class BaseFieldIntExtensions"
sidebar_label: "BaseFieldIntExtensions"
description: "Class BaseFieldIntExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldIntExtensions {#Aspid_FastTools_UIElements_BaseFieldIntExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldIntExtensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldIntExtensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Int32`](https://learn.microsoft.com/dotnet/api/system.int32).

```csharp
public static class BaseFieldIntExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldIntExtensions](Aspid.FastTools.UIElements.BaseFieldIntExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldIntExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<int>
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

