---
title: "Class BaseFieldDecimalExtensions"
sidebar_label: "BaseFieldDecimalExtensions"
description: "Class BaseFieldDecimalExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldDecimalExtensions {#Aspid_FastTools_UIElements_BaseFieldDecimalExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldDecimalExtensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldDecimalExtensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Decimal`](https://learn.microsoft.com/dotnet/api/system.decimal).

```csharp
public static class BaseFieldDecimalExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldDecimalExtensions](Aspid.FastTools.UIElements.BaseFieldDecimalExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldDecimalExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<decimal>
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

