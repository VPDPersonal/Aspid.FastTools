---
title: "Class BaseFieldColor32Extensions"
sidebar_label: "BaseFieldColor32Extensions"
description: "Class BaseFieldColor32Extensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldColor32Extensions {#Aspid_FastTools_UIElements_BaseFieldColor32Extensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldColor32Extensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldColor32Extensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Color32`](https://docs.unity3d.com/ScriptReference/Color32.html).

```csharp
public static class BaseFieldColor32Extensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldColor32Extensions](Aspid.FastTools.UIElements.BaseFieldColor32Extensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldColor32Extensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<Color32>
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

