---
title: "Class IBindableExtensions"
sidebar_label: "IBindableExtensions"
description: "Class IBindableExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class IBindableExtensions {#Aspid_FastTools_UIElements_Editors_IBindableExtensions}

Namespace: [Aspid.FastTools.UIElements.Editors](Aspid.FastTools.UIElements.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

```csharp
public static class IBindableExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[IBindableExtensions](Aspid.FastTools.UIElements.Editors.IBindableExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### BindPropertyTo\<T\>\(T, SerializedProperty\) {#Aspid_FastTools_UIElements_Editors_IBindableExtensions_BindPropertyTo__1___0_UnityEditor_SerializedProperty_}

Binds the element to the specified [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html).

```csharp
public static T BindPropertyTo<T>(this T element, SerializedProperty property) where T : VisualElement, IBindable
```

#### Parameters

`element` T

The element to bind.

`property` SerializedProperty

The serialized property to bind to.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### BindTo\<T\>\(T, SerializedObject, string\) {#Aspid_FastTools_UIElements_Editors_IBindableExtensions_BindTo__1___0_UnityEditor_SerializedObject_System_String_}

Sets the binding path and binds the element to the specified [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html).

```csharp
public static T BindTo<T>(this T element, SerializedObject serializedObject, string propertyPath) where T : VisualElement, IBindable
```

#### Parameters

`element` T

The element to bind.

`serializedObject` SerializedObject

The serialized object to bind to.

`propertyPath` [string](https://learn.microsoft.com/dotnet/api/system.string)

The serialized property path to bind to.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### SetBindingPath\<T\>\(T, string\) {#Aspid_FastTools_UIElements_Editors_IBindableExtensions_SetBindingPath__1___0_System_String_}

Sets the binding path of the element.

```csharp
public static T SetBindingPath<T>(this T element, string value) where T : VisualElement, IBindable
```

#### Parameters

`element` T

The element to modify.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The binding path to set.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

