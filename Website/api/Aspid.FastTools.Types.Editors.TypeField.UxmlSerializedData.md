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
Assembly: Aspid.FastTools.Unity.Editor.dll  

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

[INotifyValueChangedExtensions.AddValueChanged\<TypeField.UxmlSerializedData, TValue\>\(TypeField.UxmlSerializedData, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<TypeField.UxmlSerializedData, TValue\>\(TypeField.UxmlSerializedData, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<TypeField.UxmlSerializedData, TValue\>\(TypeField.UxmlSerializedData, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<TypeField.UxmlSerializedData, TValue\>\(TypeField.UxmlSerializedData, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<TypeField.UxmlSerializedData, TValue\>\(TypeField.UxmlSerializedData, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<TypeField.UxmlSerializedData, TValue\>\(TypeField.UxmlSerializedData, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Methods

### CreateInstance\(\) {#Aspid_FastTools_Types_Editors_TypeField_UxmlSerializedData_CreateInstance}

<p>
Returns an instance of the declaring element.
</p>

```csharp
public override object CreateInstance()
```

#### Returns

 [object](https://learn.microsoft.com/dotnet/api/system.object)

<p>The new instance of the declaring element.</p>

### Deserialize\(object\) {#Aspid_FastTools_Types_Editors_TypeField_UxmlSerializedData_Deserialize_System_Object_}

<p>
Applies serialized field values to a compatible visual element.
</p>

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

