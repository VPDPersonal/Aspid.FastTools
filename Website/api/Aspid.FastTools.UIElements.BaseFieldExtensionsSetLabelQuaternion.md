---
title: "Class BaseFieldExtensionsSetLabelQuaternion"
sidebar_label: "BaseFieldExtensionsSetLabelQuaternion"
description: "Class BaseFieldExtensionsSetLabelQuaternion — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class BaseFieldExtensionsSetLabelQuaternion {#Aspid_FastTools_UIElements_BaseFieldExtensionsSetLabelQuaternion}

Namespace: [Aspid.FastTools.UIElements](Aspid.FastTools.UIElements.md)  
Assembly: Aspid.FastTools.Unity.dll  

```csharp
public static class BaseFieldExtensionsSetLabelQuaternion
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[BaseFieldExtensionsSetLabelQuaternion](Aspid.FastTools.UIElements.BaseFieldExtensionsSetLabelQuaternion.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_BaseFieldExtensionsSetLabelQuaternion_SetLabel__1___0_System_String_}

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

