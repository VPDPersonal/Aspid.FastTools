---
title: "Class BaseFieldSbyteExtensions"
sidebar_label: "BaseFieldSbyteExtensions"
description: "Class BaseFieldSbyteExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldSbyteExtensions {#Aspid_FastTools_UIElements_BaseFieldSbyteExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldSbyteExtensions.SetLabel<T>`](Aspid.FastTools.UIElements.BaseFieldSbyteExtensions.md#Aspid_FastTools_UIElements_BaseFieldSbyteExtensions_SetLabel__1___0_System_String_) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`SByte`](https://learn.microsoft.com/dotnet/api/system.sbyte).

```csharp
public static class BaseFieldSbyteExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldSbyteExtensions](Aspid.FastTools.UIElements.BaseFieldSbyteExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldSbyteExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<sbyte>
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

