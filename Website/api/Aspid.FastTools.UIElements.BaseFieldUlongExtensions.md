---
title: "Class BaseFieldUlongExtensions"
sidebar_label: "BaseFieldUlongExtensions"
description: "Class BaseFieldUlongExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldUlongExtensions {#Aspid_FastTools_UIElements_BaseFieldUlongExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldUlongExtensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldUlongExtensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`UInt64`](https://learn.microsoft.com/dotnet/api/system.uint64).

```csharp
public static class BaseFieldUlongExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldUlongExtensions](Aspid.FastTools.UIElements.BaseFieldUlongExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldUlongExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<ulong>
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

