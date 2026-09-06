---
title: "Enum TypeAllow"
sidebar_label: "TypeAllow"
description: "Enum TypeAllow — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Enum TypeAllow {#Aspid_FastTools_Types_TypeAllow}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Specifies which special type categories the type picker offers in addition to concrete classes.

```csharp
[Flags]
public enum TypeAllow
```

#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<TypeAllow, TValue\>\(TypeAllow, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<TypeAllow, TValue\>\(TypeAllow, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<TypeAllow, TValue\>\(TypeAllow, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<TypeAllow, TValue\>\(TypeAllow, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<TypeAllow, TValue\>\(TypeAllow, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<TypeAllow, TValue\>\(TypeAllow, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Fields

`None = 0` 

Only concrete types are offered.



`Abstract = 1` 

Abstract classes are offered too. Static classes never are.



`Interface = 2` 

Interfaces are offered too.



`All = 3` 

Both abstract classes and interfaces are offered.



## See Also

[TypeSelectorAttribute](Aspid.FastTools.Types.TypeSelectorAttribute.md)

