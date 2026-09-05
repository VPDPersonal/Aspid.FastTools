---
title: "Delegate GenericArgumentFilter"
sidebar_label: "GenericArgumentFilter"
description: "Delegate GenericArgumentFilter — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Delegate GenericArgumentFilter {#Aspid_FastTools_Types_Editors_GenericArgumentFilter}

Namespace: [Aspid.FastTools.Types.Editors](Aspid.FastTools.Types.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

Decides whether <code class="paramref">argument</code> may close <code class="paramref">parameter</code> of
<code class="paramref">openDefinition</code>. Unlike a plain per-type predicate this is asked <i>about a position</i>,
because what a closed type has to store depends on where its parameter lands: a parameter reaching a field
the engine writes by value constrains the argument, one reaching only a <code>[SerializeReference]</code> field
does not.

```csharp
public delegate bool GenericArgumentFilter(Type openDefinition, Type parameter, Type argument)
```

#### Parameters

`openDefinition` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The generic definition being closed.

`parameter` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type parameter of <code class="paramref">openDefinition</code> being closed.

`argument` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The concrete type proposed for it.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<GenericArgumentFilter, TValue\>\(GenericArgumentFilter, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<GenericArgumentFilter, TValue\>\(GenericArgumentFilter, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<GenericArgumentFilter, TValue\>\(GenericArgumentFilter, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<GenericArgumentFilter, TValue\>\(GenericArgumentFilter, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<GenericArgumentFilter, TValue\>\(GenericArgumentFilter, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<GenericArgumentFilter, TValue\>\(GenericArgumentFilter, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

