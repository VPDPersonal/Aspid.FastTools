---
title: "Class EnumValues<TValue>"
sidebar_label: "EnumValues<TValue>"
description: "Class EnumValues<TValue> — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class EnumValues\<TValue\> {#Aspid_FastTools_Enums_EnumValues_1}

Namespace: [Aspid.FastTools.Enums](Aspid.FastTools.Enums.md)  
Assembly: Aspid.FastTools.Unity.dll  

A serializable dictionary that maps each member of a chosen enum to a value of type
<code class="typeparamref">TValue</code>. Supports both regular and <code>[Flags]</code> enums.

```csharp
[Serializable]
public sealed class EnumValues<TValue> : IEnumerable<KeyValuePair<Enum, TValue>>, IEnumerable, ISerializationCallbackReceiver
```

#### Type Parameters

`TValue` 

The type of the value associated with each enum member.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[EnumValues\<TValue\>](Aspid.FastTools.Enums.EnumValues-1.md)

#### Implements

[IEnumerable\<KeyValuePair\<Enum, TValue\>\>](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1), 
[IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.ienumerable), 
ISerializationCallbackReceiver


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Examples

Map a damage type to a color:


```csharp
public class HitEffect : MonoBehaviour
{
    [SerializeField] private EnumValues<Color> _damageColors;

    public Color GetColor(DamageType type) =>
        _damageColors.GetValue(type);
}
```


## Remarks

<p>
The enum type is selected in the Inspector via a [`TypeSelectorAttribute`](Aspid.FastTools.Types.TypeSelectorAttribute.md)
and stored as an assembly-qualified name. All entries are initialized lazily on first access.
When the enum type is already known at compile time, prefer
[`EnumValues<T1, T2>`](Aspid.FastTools.Enums.EnumValues-2.md) — its Inspector type-picker is read-only.
</p>
<p>
For <code>[Flags]</code> enums [`EnumValues<T>.Equals`](Aspid.FastTools.Enums.EnumValues-1.md) uses flag-containment semantics
with special handling for the zero (<code>None</code>) value — two values are considered equal
only when both are zero or both are non-zero and one has all bits of the other set.
</p>
<p>
[`EnumValues<T>.GetValue`](Aspid.FastTools.Enums.EnumValues-1.md#Aspid_FastTools_Enums_EnumValues_1_GetValue_System_Enum_) returns the configured default value when no entry matches the lookup key.
For <code>[Flags]</code> enums multiple entries may match a single lookup value; an exact-key entry
always wins first, and only if none exists does the first entry (in serialized order) whose
bits are all contained in the lookup value win.
</p>
<p>
Iteration via [`EnumValues<T>.GetEnumerator`](Aspid.FastTools.Enums.EnumValues-1.md#Aspid_FastTools_Enums_EnumValues_1_GetEnumerator) yields only the explicitly configured entries and
does <b>not</b> include the default value.
</p>
<p>
Internal hot paths are wrapped in profiler markers; define the
<code>ASPID_FAST_TOOLS_UNITY_PROFILER_DISABLED</code> scripting symbol to compile them out.
</p>

## Methods

### Equals\(Enum, Enum\) {#Aspid_FastTools_Enums_EnumValues_1_Equals_System_Enum_System_Enum_}

Determines whether two enum values should be considered equal for lookup purposes.
The first argument is the value being looked up; the second is the entry's stored key.

```csharp
public bool Equals(Enum enumValue1, Enum enumValue2)
```

#### Parameters

`enumValue1` [Enum](https://learn.microsoft.com/dotnet/api/system.enum)

The lookup value (must contain the entry's bits to match).

`enumValue2` [Enum](https://learn.microsoft.com/dotnet/api/system.enum)

The stored entry key.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

For regular enums: <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> when both values are identical.<br />
For <code>[Flags]</code> enums: <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> when <code class="paramref">enumValue1</code>
has all bits of <code class="paramref">enumValue2</code> set, with the additional rule that
the zero (<code>None</code>) value is only equal to another zero value.<br />
Values of different enum types are never equal.

### GetEnumerator\(\) {#Aspid_FastTools_Enums_EnumValues_1_GetEnumerator}

Returns a struct enumerator over the explicitly configured (key, value) pairs in
serialized order — <code>foreach</code> binds to it directly and does not allocate.
Does <b>not</b> include the default value or entries with an unresolved key.

```csharp
public EnumValuesEnumerator<Enum, TValue> GetEnumerator()
```

#### Returns

 [EnumValuesEnumerator](Aspid.FastTools.Enums.EnumValuesEnumerator-2.md)\<[Enum](https://learn.microsoft.com/dotnet/api/system.enum), TValue\>

### GetValue\(Enum\) {#Aspid_FastTools_Enums_EnumValues_1_GetValue_System_Enum_}

Returns the value mapped to <code class="paramref">enumValue</code>,
or the configured default value if no mapping exists.
A value of a different enum type than the configured one never matches.

```csharp
public TValue GetValue(Enum enumValue)
```

#### Parameters

`enumValue` [Enum](https://learn.microsoft.com/dotnet/api/system.enum)

The enum member to look up.

#### Returns

 TValue

The mapped value, or the default value when no entry matches.

