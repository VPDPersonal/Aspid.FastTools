---
title: "Class BaseFieldVector3IntExtensions"
sidebar_label: "BaseFieldVector3IntExtensions"
description: "Class BaseFieldVector3IntExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldVector3IntExtensions {#Aspid_FastTools_UIElements_BaseFieldVector3IntExtensions}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.dll  

Provides [`BaseFieldVector3IntExtensions.SetLabel%60<T>`](Aspid.FastTools.UIElements.BaseFieldVector3IntExtensions.md) for [`BaseField<T>`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField.html) of [`Vector3Int`](https://docs.unity3d.com/ScriptReference/Vector3Int.html).

```csharp
public static class BaseFieldVector3IntExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldVector3IntExtensions](Aspid.FastTools.UIElements.BaseFieldVector3IntExtensions.md)


## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldVector3IntExtensions_SetLabel__1___0_System_String_}

Sets the label of the field via [`label`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-label.html).

```csharp
public static T SetLabel<T>(this T element, string value) where T : BaseField<Vector3Int>
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

