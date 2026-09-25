---
title: "Class BaseFieldQuaternionExtensions"
sidebar_label: "BaseFieldQuaternionExtensions"
description: "Class BaseFieldQuaternionExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldQuaternionExtensions {#Aspid_FastTools_UIElements_BaseFieldQuaternionExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldQuaternionExtensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldQuaternionExtensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Quaternion`](https://docs.unity3d.com/ScriptReference/Quaternion.html).

```csharp
public static class BaseFieldQuaternionExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldQuaternionExtensions](Aspid.FastTools.UIElements.BaseFieldQuaternionExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldQuaternionExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<Quaternion>
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

