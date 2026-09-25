---
title: "Class BaseFieldObjectExtensions"
sidebar_label: "BaseFieldObjectExtensions"
description: "Class BaseFieldObjectExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldObjectExtensions {#Aspid_FastTools_UIElements_BaseFieldObjectExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldObjectExtensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldObjectExtensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Object`](https://docs.unity3d.com/ScriptReference/Object.html).

```csharp
public static class BaseFieldObjectExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldObjectExtensions](Aspid.FastTools.UIElements.BaseFieldObjectExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldObjectExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<Object>
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

