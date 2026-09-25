---
title: "Class BaseFieldVector4Extensions"
sidebar_label: "BaseFieldVector4Extensions"
description: "Class BaseFieldVector4Extensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldVector4Extensions {#Aspid_FastTools_UIElements_BaseFieldVector4Extensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldVector4Extensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldVector4Extensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Vector4`](https://docs.unity3d.com/ScriptReference/Vector4.html).

```csharp
public static class BaseFieldVector4Extensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldVector4Extensions](Aspid.FastTools.UIElements.BaseFieldVector4Extensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldVector4Extensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<Vector4>
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

