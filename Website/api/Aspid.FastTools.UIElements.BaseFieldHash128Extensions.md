---
title: "Class BaseFieldHash128Extensions"
sidebar_label: "BaseFieldHash128Extensions"
description: "Class BaseFieldHash128Extensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldHash128Extensions {#Aspid_FastTools_UIElements_BaseFieldHash128Extensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldHash128Extensions.SetLabel<T>`](Aspid.FastTools.UIElements.BaseFieldHash128Extensions.md#Aspid_FastTools_UIElements_BaseFieldHash128Extensions_SetLabel__1___0_System_String_) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Hash128`](https://docs.unity3d.com/ScriptReference/Hash128.html).

```csharp
public static class BaseFieldHash128Extensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldHash128Extensions](Aspid.FastTools.UIElements.BaseFieldHash128Extensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldHash128Extensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<Hash128>
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

