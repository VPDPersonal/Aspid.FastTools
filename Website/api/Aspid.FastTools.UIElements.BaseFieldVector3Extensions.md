---
title: "Class BaseFieldVector3Extensions"
sidebar_label: "BaseFieldVector3Extensions"
description: "Class BaseFieldVector3Extensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldVector3Extensions {#Aspid_FastTools_UIElements_BaseFieldVector3Extensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldVector3Extensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldVector3Extensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Vector3`](https://docs.unity3d.com/ScriptReference/Vector3.html).

```csharp
public static class BaseFieldVector3Extensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldVector3Extensions](Aspid.FastTools.UIElements.BaseFieldVector3Extensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldVector3Extensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<Vector3>
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

