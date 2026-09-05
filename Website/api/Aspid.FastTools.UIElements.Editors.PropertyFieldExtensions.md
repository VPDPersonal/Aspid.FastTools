---
title: "Class PropertyFieldExtensions"
sidebar_label: "PropertyFieldExtensions"
description: "Class PropertyFieldExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class PropertyFieldExtensions {#Aspid_FastTools_UIElements_Editors_PropertyFieldExtensions}

Namespace: [Aspid.FastTools.UIElements.Editors](Aspid.FastTools.UIElements.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

```csharp
public static class PropertyFieldExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[PropertyFieldExtensions](Aspid.FastTools.UIElements.Editors.PropertyFieldExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddValueChanged\<T\>\(T, EventCallback\<SerializedPropertyChangeEvent\>\) {#Aspid_FastTools_UIElements_Editors_PropertyFieldExtensions_AddValueChanged__1___0_UnityEngine_UIElements_EventCallback_UnityEditor_UIElements_SerializedPropertyChangeEvent__}

Subscribes to the value-changed event of the element.

```csharp
public static T AddValueChanged<T>(this T element, EventCallback<SerializedPropertyChangeEvent> value) where T : PropertyField
```

#### Parameters

`element` T

The element to modify.

`value` EventCallback\<SerializedPropertyChangeEvent\>

The callback to subscribe.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### RemoveValueChanged\<T\>\(T, EventCallback\<SerializedPropertyChangeEvent\>\) {#Aspid_FastTools_UIElements_Editors_PropertyFieldExtensions_RemoveValueChanged__1___0_UnityEngine_UIElements_EventCallback_UnityEditor_UIElements_SerializedPropertyChangeEvent__}

Unsubscribes from the value-changed event of the element.

```csharp
public static T RemoveValueChanged<T>(this T element, EventCallback<SerializedPropertyChangeEvent> value) where T : PropertyField
```

#### Parameters

`element` T

The element to modify.

`value` EventCallback\<SerializedPropertyChangeEvent\>

The callback to remove.

#### Returns

 T

The element, for chaining.

#### Type Parameters

`T` 

### SetLabel\<T\>\(T, string\) {#Aspid_FastTools_UIElements_Editors_PropertyFieldExtensions_SetLabel__1___0_System_String_}

Sets the label of the property field.

```csharp
public static T SetLabel<T>(this T element, string value) where T : PropertyField
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

The element type.

