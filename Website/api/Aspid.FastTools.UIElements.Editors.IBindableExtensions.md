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

Provides binding extension methods for [`IBindable`](https://docs.unity3d.com/ScriptReference/UIElements-IBindable.html) elements.

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

Binds the element to <code class="paramref">property</code>.

```csharp
public static T BindPropertyTo<T>(this T element, SerializedProperty property) where T : VisualElement, IBindable
```

#### Parameters

`element` T

The element to bind.

`property` SerializedProperty

The property to bind to.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### BindTo\<T\>\(T, SerializedObject, string\) {#Aspid_FastTools_UIElements_Editors_IBindableExtensions_BindTo__1___0_UnityEditor_SerializedObject_System_String_}

Sets the binding path and binds the element to <code class="paramref">serializedObject</code>.

```csharp
public static T BindTo<T>(this T element, SerializedObject serializedObject, string propertyPath) where T : VisualElement, IBindable
```

#### Parameters

`element` T

The element to bind.

`serializedObject` SerializedObject

The serialized object to bind to.

`propertyPath` [string](https://learn.microsoft.com/dotnet/api/system.string)

The property path to bind to.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

The element type.

### SetBindingPath\<T\>\(T, string\) {#Aspid_FastTools_UIElements_Editors_IBindableExtensions_SetBindingPath__1___0_System_String_}

Sets the element's binding path.

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

