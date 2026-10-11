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
Assembly: Aspid.FastTools.dll  

Serializable dictionary that maps each member of a chosen enum to a value of type
<code class="typeparamref">TValue</code>. Supports both regular and <code>[Flags]</code> enums.

```csharp
[Serializable]
public sealed class EnumValues<TValue> : IReadOnlyCollection<KeyValuePair<Enum, TValue?>>, IEnumerable<KeyValuePair<Enum, TValue?>>, IEnumerable, ISerializationCallbackReceiver
```

#### Type Parameters

`TValue` 

The type of the value associated with each enum member.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[EnumValues\<TValue\>](Aspid.FastTools.Enums.EnumValues-1.md)

#### Implements

[IReadOnlyCollection\<KeyValuePair\<Enum, TValue?\>\>](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlycollection-1),
[IEnumerable\<KeyValuePair\<Enum, TValue?\>\>](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1),
[IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.ienumerable),
ISerializationCallbackReceiver


#### Extension Methods

[TextInputBaseFieldTextSelectionExtensions.AddOnCursorIndexChange\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_AddOnCursorIndexChange__2___0_System_Action_),
[TextInputBaseFieldTextSelectionExtensions.AddOnSelectIndexChange\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_AddOnSelectIndexChange__2___0_System_Action_),
[INotifyValueChangedExtensions.AddValueChanged\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[ProfilerMarkerExtensionsForGenerator.Marker\<EnumValues\<TValue\>\>\(EnumValues\<TValue\>, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_),
[TextInputBaseFieldTextSelectionExtensions.RemoveOnCursorIndexChange\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_RemoveOnCursorIndexChange__2___0_System_Action_),
[TextInputBaseFieldTextSelectionExtensions.RemoveOnSelectIndexChange\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_RemoveOnSelectIndexChange__2___0_System_Action_),
[INotifyValueChangedExtensions.RemoveValueChanged\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[TextInputBaseFieldExtensions.SetAutoCorrection\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetAutoCorrection__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetCursorIndex\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetCursorIndex__2___0_System_Int32_),
[TextInputBaseFieldExtensions.SetDelayed\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetDelayed__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetDoubleClickSelectsWord\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetDoubleClickSelectsWord__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHideMobileInput\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHideMobileInput__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHidePlaceholderOnFocus\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHidePlaceholderOnFocus__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHideSoftKeyboard\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHideSoftKeyboard__2___0_System_Boolean_),
[SliderExtensions.SetHighValue\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_),
[TextInputBaseFieldExtensions.SetKeyboardType\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, TouchScreenKeyboardType\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetKeyboardType__2___0_UnityEngine_TouchScreenKeyboardType_),
[BaseFieldExtensions.SetLabel\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_),
[SliderExtensions.SetLowValue\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_),
[TextInputBaseFieldExtensions.SetMaskChar\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, char\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetMaskChar__2___0_System_Char_),
[TextInputBaseFieldExtensions.SetMaxLength\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetMaxLength__2___0_System_Int32_),
[TextInputBaseFieldExtensions.SetPassword\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetPassword__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetPlaceholder\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, string\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetPlaceholder__2___0_System_String_),
[TextInputBaseFieldExtensions.SetReadOnly\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetReadOnly__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectAllOnFocus\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectAllOnFocus__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectAllOnMouseUp\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectAllOnMouseUp__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectIndex\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectIndex__2___0_System_Int32_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectable\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectable__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetTripleClickSelectsLine\<EnumValues\<TValue\>, TValue\>\(EnumValues\<TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetTripleClickSelectsLine__2___0_System_Boolean_),
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
A player resolves the enum by the stored name only, which managed code stripping does not see: from
Managed Stripping Level Low up, an enum referenced only by this name can be removed from the build.
The table then logs an error and returns the default value. Keep such enums with <code>[Preserve]</code> or
<code>link.xml</code>.
</p>
<p>
For <code>[Flags]</code> enums [`EnumValues<T>.Equals`](Aspid.FastTools.Enums.EnumValues-1.md#Aspid_FastTools_Enums_EnumValues_1_Equals_System_Enum_System_Enum_) uses flag-containment semantics
with special handling for the zero (<code>None</code>) value — two values are considered equal
only when both are zero or both are non-zero and the first (the lookup value) has all bits
of the second (the stored key) set.
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
Lookups and iteration may run on any thread, including the first access that initializes the
entries, as long as Unity is not deserializing the table at the same time.
</p>
<p>
Internal hot paths are wrapped in profiler markers; define the
<code>ASPID_FAST_TOOLS_UNITY_PROFILER_DISABLED</code> scripting symbol to compile them out.
</p>

## Properties

### Count {#Aspid_FastTools_Enums_EnumValues_1_Count}

Gets the number of entries [`EnumValues<T>.GetEnumerator`](Aspid.FastTools.Enums.EnumValues-1.md#Aspid_FastTools_Enums_EnumValues_1_GetEnumerator) yields: the rows whose key resolved to an
enum member, duplicate keys included. The default value is not counted.

```csharp
public int Count { get; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

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
Values of a different enum type than the configured one are never equal,
and neither is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

### GetEnumerator\(\) {#Aspid_FastTools_Enums_EnumValues_1_GetEnumerator}

Returns a struct enumerator over the explicitly configured (key, value) pairs in
serialized order — <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/statements/iteration-statements#the-foreach-statement">foreach</a> binds to it directly and does not allocate.
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
public TValue? GetValue(Enum enumValue)
```

#### Parameters

`enumValue` [Enum](https://learn.microsoft.com/dotnet/api/system.enum)

The enum member to look up.

#### Returns

 TValue?

The mapped value, or the default value when no entry matches. A reference-type
<code class="typeparamref">TValue</code> left unassigned in the Inspector is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

### GetValue\<TEnum\>\(TEnum\) {#Aspid_FastTools_Enums_EnumValues_1_GetValue__1___0_}

Returns the value mapped to <code class="paramref">enumValue</code> like [`EnumValues<T>.GetValue`](Aspid.FastTools.Enums.EnumValues-1.md#Aspid_FastTools_Enums_EnumValues_1_GetValue_System_Enum_),
without boxing the key.

```csharp
public TValue? GetValue<TEnum>(TEnum enumValue) where TEnum : struct, Enum
```

#### Parameters

`enumValue` TEnum

The enum member to look up.

#### Returns

 TValue?

The mapped value, or the default value when no entry matches. A <code class="typeparamref">TEnum</code>
other than the configured enum type never matches.

#### Type Parameters

`TEnum` 

The enum type of <code class="paramref">enumValue</code>.

### TryGetValue\(Enum, out TValue?\) {#Aspid_FastTools_Enums_EnumValues_1_TryGetValue_System_Enum__0__}

Looks up the value mapped to <code class="paramref">enumValue</code> and reports whether an entry matched.

```csharp
public bool TryGetValue(Enum enumValue, out TValue? value)
```

#### Parameters

`enumValue` [Enum](https://learn.microsoft.com/dotnet/api/system.enum)

The enum member to look up.

`value` TValue?

The mapped value, or the default value when no entry matches, as [`EnumValues<T>.GetValue`](Aspid.FastTools.Enums.EnumValues-1.md#Aspid_FastTools_Enums_EnumValues_1_GetValue_System_Enum_) returns it.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if an entry matches; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.
A <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> key, or a key of a different enum type than the configured one, never matches.

### TryGetValue\<TEnum\>\(TEnum, out TValue?\) {#Aspid_FastTools_Enums_EnumValues_1_TryGetValue__1___0__0__}

Looks up the value mapped to <code class="paramref">enumValue</code> like [`EnumValues<T>.TryGetValue`](Aspid.FastTools.Enums.EnumValues-1.md),
without boxing the key.

```csharp
public bool TryGetValue<TEnum>(TEnum enumValue, out TValue? value) where TEnum : struct, Enum
```

#### Parameters

`enumValue` TEnum

The enum member to look up.

`value` TValue?

The mapped value, or the default value when no entry matches, as [`EnumValues<T>.GetValue<T>`](Aspid.FastTools.Enums.EnumValues-1.md#Aspid_FastTools_Enums_EnumValues_1_GetValue__1___0_) returns it.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if an entry matches; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.
A <code class="typeparamref">TEnum</code> other than the configured enum type never matches.

#### Type Parameters

`TEnum` 

The enum type of <code class="paramref">enumValue</code>.

