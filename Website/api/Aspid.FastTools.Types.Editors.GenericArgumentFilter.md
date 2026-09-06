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

Represents the method that decides whether <code class="paramref">argument</code> may close
<code class="paramref">parameter</code>.

```csharp
public delegate bool GenericArgumentFilter(Type openDefinition, Type parameter, Type argument)
```

#### Parameters

`openDefinition` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The generic definition being closed.

`parameter` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type parameter being closed.

`argument` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The concrete type proposed for it.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if <code class="paramref">argument</code> may close <code class="paramref">parameter</code>; otherwise,
<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<GenericArgumentFilter, TValue\>\(GenericArgumentFilter, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<GenericArgumentFilter, TValue\>\(GenericArgumentFilter, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<GenericArgumentFilter, TValue\>\(GenericArgumentFilter, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<GenericArgumentFilter, TValue\>\(GenericArgumentFilter, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<GenericArgumentFilter, TValue\>\(GenericArgumentFilter, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<GenericArgumentFilter, TValue\>\(GenericArgumentFilter, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Remarks

Unlike a per-type predicate this is asked about a position: what a closed type must store depends on where its
parameter lands, since a parameter reaching a by-value field constrains the argument and one reaching only a
<code>[SerializeReference]</code> field does not.

