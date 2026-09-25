---
title: "Class BaseFieldFloatExtensions"
sidebar_label: "BaseFieldFloatExtensions"
description: "Class BaseFieldFloatExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldFloatExtensions {#Aspid_FastTools_UIElements_BaseFieldFloatExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldFloatExtensions.SetLabel<T>`](Aspid.FastTools.UIElements.BaseFieldFloatExtensions.md#Aspid_FastTools_UIElements_BaseFieldFloatExtensions_SetLabel__1___0_System_String_) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Single`](https://learn.microsoft.com/dotnet/api/system.single).

```csharp
public static class BaseFieldFloatExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldFloatExtensions](Aspid.FastTools.UIElements.BaseFieldFloatExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldFloatExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<float>
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

