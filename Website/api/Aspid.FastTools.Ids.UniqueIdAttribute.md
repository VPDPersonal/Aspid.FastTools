---
title: "Class UniqueIdAttribute"
sidebar_label: "UniqueIdAttribute"
description: "Class UniqueIdAttribute — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class UniqueIdAttribute {#Aspid_FastTools_Ids_UniqueIdAttribute}

Namespace: [Aspid.FastTools.Ids](Aspid.FastTools.Ids.md)  
Assembly: Aspid.FastTools.Unity.dll  

Marks an integer field as a project-wide unique id. The editor drawer enforces uniqueness across all
fields decorated with this attribute and offers a registry-aware id picker.
The attribute is editor-only — its [`ConditionalAttribute`](https://learn.microsoft.com/dotnet/api/system.diagnostics.conditionalattribute) ensures usages are stripped from player builds.

```csharp
[Conditional("UNITY_EDITOR")]
public sealed class UniqueIdAttribute : PropertyAttribute
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[Attribute](https://learn.microsoft.com/dotnet/api/system.attribute) ← 
PropertyAttribute ← 
[UniqueIdAttribute](Aspid.FastTools.Ids.UniqueIdAttribute.md)


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<UniqueIdAttribute, TValue\>\(UniqueIdAttribute, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<UniqueIdAttribute, TValue\>\(UniqueIdAttribute, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<UniqueIdAttribute, TValue\>\(UniqueIdAttribute, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<UniqueIdAttribute, TValue\>\(UniqueIdAttribute, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<UniqueIdAttribute, TValue\>\(UniqueIdAttribute, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<UniqueIdAttribute, TValue\>\(UniqueIdAttribute, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

