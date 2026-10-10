---
title: "Class TypeField.UxmlSerializedData"
sidebar_label: "TypeField.UxmlSerializedData"
description: "Class TypeField.UxmlSerializedData — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TypeField.UxmlSerializedData {#Aspid_FastTools_Types_Editors_TypeField_UxmlSerializedData}

Namespace: [Aspid.FastTools.Types.Editors](Aspid.FastTools.Types.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

```csharp
[Serializable]
public class TypeField.UxmlSerializedData : BaseField<Type>.UxmlSerializedData
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
UxmlSerializedData ← 
VisualElement.UxmlSerializedData ← 
BindableElement.UxmlSerializedData ← 
BaseField\<Type\>.UxmlSerializedData ← 
[TypeField.UxmlSerializedData](Aspid.FastTools.Types.Editors.TypeField.UxmlSerializedData.md)

#### Derived

[InspectorTypeField.UxmlSerializedData](Aspid.FastTools.Types.Editors.InspectorTypeField.UxmlSerializedData.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<TypeField.UxmlSerializedData\>\(TypeField.UxmlSerializedData, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Methods

### CreateInstance\(\) {#Aspid_FastTools_Types_Editors_TypeField_UxmlSerializedData_CreateInstance}

<p>Returns an instance of the declaring element.</p>

```csharp
public override object CreateInstance()
```

#### Returns

 [object](https://learn.microsoft.com/dotnet/api/system.object)

<p>The new instance of the declaring element.</p>

### Deserialize\(object\) {#Aspid_FastTools_Types_Editors_TypeField_UxmlSerializedData_Deserialize_System_Object_}

<p>Applies serialized field values to a compatible visual element.</p>

```csharp
public override void Deserialize(object obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)

The element to have the serialized data applied to.

### Register\(\) {#Aspid_FastTools_Types_Editors_TypeField_UxmlSerializedData_Register}

```csharp
[RegisterUxmlCache]
[Conditional("UNITY_EDITOR")]
public static void Register()
```

