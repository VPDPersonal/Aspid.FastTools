---
title: "Struct EnumValuesEnumerator<TKey, TValue>"
sidebar_label: "EnumValuesEnumerator<TKey, TValue>"
description: "Struct EnumValuesEnumerator<TKey, TValue> — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Struct EnumValuesEnumerator\<TKey, TValue\> {#Aspid_FastTools_Enums_EnumValuesEnumerator_2}

Namespace: [Aspid.FastTools.Enums](Aspid.FastTools.Enums.md)  
Assembly: Aspid.FastTools.dll  

Allocation-free enumerator over the resolved entries of an [`EnumValues<T>`](Aspid.FastTools.Enums.EnumValues-1.md)
(<code class="typeparamref">TKey</code> = [`Enum`](https://learn.microsoft.com/dotnet/api/system.enum)) or an [`EnumValues<T1, T2>`](Aspid.FastTools.Enums.EnumValues-2.md)
(<code class="typeparamref">TKey</code> = the enum type). Boxed only when consumed through the
[`IEnumerable<T>`](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1) interface (e.g. LINQ).

```csharp
public struct EnumValuesEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue?>>, IEnumerator, IDisposable
```

#### Type Parameters

`TKey` 

The key type the entries are yielded as.

`TValue` 

The type of the value associated with each enum member.

#### Implements

[IEnumerator\<KeyValuePair\<TKey, TValue?\>\>](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerator-1), 
[IEnumerator](https://learn.microsoft.com/dotnet/api/system.collections.ienumerator), 
[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<EnumValuesEnumerator\<TKey, TValue\>, TValue\>\(EnumValuesEnumerator\<TKey, TValue\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<EnumValuesEnumerator\<TKey, TValue\>, TValue\>\(EnumValuesEnumerator\<TKey, TValue\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<EnumValuesEnumerator\<TKey, TValue\>, TValue\>\(EnumValuesEnumerator\<TKey, TValue\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<EnumValuesEnumerator\<TKey, TValue\>, TValue\>\(EnumValuesEnumerator\<TKey, TValue\>, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<EnumValuesEnumerator\<TKey, TValue\>, TValue\>\(EnumValuesEnumerator\<TKey, TValue\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<EnumValuesEnumerator\<TKey, TValue\>, TValue\>\(EnumValuesEnumerator\<TKey, TValue\>, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Properties

### Current {#Aspid_FastTools_Enums_EnumValuesEnumerator_2_Current}

The entry at the current position; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/default">default</a> before the first
[`EnumValuesEnumerator<T1, T2>.MoveNext`](Aspid.FastTools.Enums.EnumValuesEnumerator-2.md#Aspid_FastTools_Enums_EnumValuesEnumerator_2_MoveNext) and after the last one.

```csharp
public readonly KeyValuePair<TKey, TValue?> Current { get; }
```

#### Property Value

 [KeyValuePair](https://learn.microsoft.com/dotnet/api/system.collections.generic.keyvaluepair-2)\<TKey, TValue?\>

## Methods

### MoveNext\(\) {#Aspid_FastTools_Enums_EnumValuesEnumerator_2_MoveNext}

Advances to the next resolved entry, skipping entries whose key could not be parsed.

```csharp
public bool MoveNext()
```

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if an entry was found; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> at the end.

