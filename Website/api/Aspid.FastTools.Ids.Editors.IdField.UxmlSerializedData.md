---
title: "Class IdField.UxmlSerializedData"
sidebar_label: "IdField.UxmlSerializedData"
description: "Class IdField.UxmlSerializedData — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class IdField.UxmlSerializedData {#Aspid_FastTools_Ids_Editors_IdField_UxmlSerializedData}

Namespace: [Aspid.FastTools.Ids.Editors](Aspid.FastTools.Ids.Editors.md)  
Assembly: Aspid.FastTools.Unity.Editor.dll  

```csharp
[Serializable]
public class IdField.UxmlSerializedData : BaseField<int>.UxmlSerializedData
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
UxmlSerializedData ← 
VisualElement.UxmlSerializedData ← 
BindableElement.UxmlSerializedData ← 
BaseField\<int\>.UxmlSerializedData ← 
[IdField.UxmlSerializedData](Aspid.FastTools.Ids.Editors.IdField.UxmlSerializedData.md)

#### Derived

[InspectorIdField.UxmlSerializedData](Aspid.FastTools.Ids.Editors.InspectorIdField.UxmlSerializedData.md)


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<IdField.UxmlSerializedData, TValue\>\(IdField.UxmlSerializedData, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<IdField.UxmlSerializedData, TValue\>\(IdField.UxmlSerializedData, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<IdField.UxmlSerializedData, TValue\>\(IdField.UxmlSerializedData, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<IdField.UxmlSerializedData, TValue\>\(IdField.UxmlSerializedData, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<IdField.UxmlSerializedData, TValue\>\(IdField.UxmlSerializedData, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<IdField.UxmlSerializedData, TValue\>\(IdField.UxmlSerializedData, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Methods

### CreateInstance\(\) {#Aspid_FastTools_Ids_Editors_IdField_UxmlSerializedData_CreateInstance}

<p>
Returns an instance of the declaring element.
</p>

```csharp
public override object CreateInstance()
```

#### Returns

 [object](https://learn.microsoft.com/dotnet/api/system.object)

<p>The new instance of the declaring element.</p>

### Register\(\) {#Aspid_FastTools_Ids_Editors_IdField_UxmlSerializedData_Register}

```csharp
[RegisterUxmlCache]
[Conditional("UNITY_EDITOR")]
public static void Register()
```

