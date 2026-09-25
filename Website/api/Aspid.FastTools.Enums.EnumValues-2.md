---
title: "Class EnumValues<TEnum, TValue>"
sidebar_label: "EnumValues<TEnum, TValue>"
description: "Class EnumValues<TEnum, TValue> — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class EnumValues\<TEnum, TValue\> {#Aspid_FastTools_Enums_EnumValues_2}

Namespace: [Aspid.FastTools.Enums](Aspid.FastTools.Enums.md)  
Assembly: Aspid.FastTools.dll  

A serializable dictionary that maps members of <code class="typeparamref">TEnum</code> to values of
type <code class="typeparamref">TValue</code>. The typed counterpart of [`EnumValues<T>`](Aspid.FastTools.Enums.EnumValues-1.md)
for the common case where the enum type is known at compile time — the Inspector type-picker
is read-only, and lookups are compile-time safe.

```csharp
[Serializable]
public sealed class EnumValues<TEnum, TValue> : IEnumerable<KeyValuePair<TEnum, TValue?>>, IEnumerable, ISerializationCallbackReceiver where TEnum : struct, Enum
```

#### Type Parameters

`TEnum` 

The enum type the entries are keyed by.

`TValue` 

The type of the value associated with each enum member.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[EnumValues\<TEnum, TValue\>](Aspid.FastTools.Enums.EnumValues-2.md)

#### Implements

[IEnumerable\<KeyValuePair\<TEnum, TValue?\>\>](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1),
[IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.ienumerable),
ISerializationCallbackReceiver


#### Extension Methods

[TextInputBaseFieldTextSelectionExtensions.AddOnCursorIndexChange\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_AddOnCursorIndexChange__2___0_System_Action_),
[TextInputBaseFieldTextSelectionExtensions.AddOnSelectIndexChange\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_AddOnSelectIndexChange__2___0_System_Action_),
[INotifyValueChangedExtensions.AddValueChanged\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_),
[TextInputBaseFieldTextSelectionExtensions.RemoveOnCursorIndexChange\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_RemoveOnCursorIndexChange__2___0_System_Action_),
[TextInputBaseFieldTextSelectionExtensions.RemoveOnSelectIndexChange\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_RemoveOnSelectIndexChange__2___0_System_Action_),
[INotifyValueChangedExtensions.RemoveValueChanged\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[TextInputBaseFieldExtensions.SetAutoCorrection\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetAutoCorrection__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetCursorIndex\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetCursorIndex__2___0_System_Int32_),
[TextInputBaseFieldExtensions.SetDelayed\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetDelayed__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetDoubleClickSelectsWord\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetDoubleClickSelectsWord__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHideMobileInput\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHideMobileInput__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHidePlaceholderOnFocus\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHidePlaceholderOnFocus__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHideSoftKeyboard\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHideSoftKeyboard__2___0_System_Boolean_),
[SliderExtensions.SetHighValue\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_),
[TextInputBaseFieldExtensions.SetKeyboardType\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, TouchScreenKeyboardType\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetKeyboardType__2___0_UnityEngine_TouchScreenKeyboardType_),
[BaseFieldExtensions.SetLabel\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_),
[SliderExtensions.SetLowValue\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_),
[TextInputBaseFieldExtensions.SetMaskChar\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, char\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetMaskChar__2___0_System_Char_),
[TextInputBaseFieldExtensions.SetMaxLength\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetMaxLength__2___0_System_Int32_),
[TextInputBaseFieldExtensions.SetPassword\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetPassword__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetPlaceholder\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, string\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetPlaceholder__2___0_System_String_),
[TextInputBaseFieldExtensions.SetReadOnly\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetReadOnly__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectAllOnFocus\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectAllOnFocus__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectAllOnMouseUp\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectAllOnMouseUp__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectIndex\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectIndex__2___0_System_Int32_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectable\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectable__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetTripleClickSelectsLine\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetTripleClickSelectsLine__2___0_System_Boolean_),
[INotifyValueChangedExtensions.SetValue\<EnumValues\<TEnum, TValue\>, TValue\>\(EnumValues\<TEnum, TValue\>, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Examples

Map a damage type to a color, with the enum fixed at compile time:


```csharp
public class HitEffect : MonoBehaviour
{
    [SerializeField] private EnumValues<DamageType, Color> _damageColors;

    public Color GetColor(DamageType type) =>
        _damageColors.GetValue(type);
}
```


## Remarks

<p>
Lookup semantics (including <code>[Flags]</code> handling) are identical to
[`EnumValues<T>`](Aspid.FastTools.Enums.EnumValues-1.md) — see its remarks for details. Steady-state
[`EnumValues<T1, T2>.GetValue`](Aspid.FastTools.Enums.EnumValues-2.md#Aspid_FastTools_Enums_EnumValues_2_GetValue__0_), [`EnumValues<T1, T2>.Equals`](Aspid.FastTools.Enums.EnumValues-2.md) and <code>foreach</code> (which binds to the struct
[`EnumValuesEnumerator<T1, T2>`](Aspid.FastTools.Enums.EnumValuesEnumerator-2.md)) never allocate.
</p>
<p>
In the editor the serialized layout is compatible with [`EnumValues<T>`](Aspid.FastTools.Enums.EnumValues-1.md):
the enum type is still stored in a hidden editor-only <code>_enumType</code> field, auto-filled
with <code class="typeparamref">TEnum</code>'s assembly-qualified name on serialization. Switching a
field between the two variants therefore migrates existing data, as long as the configured
enum type matches <code class="typeparamref">TEnum</code>. Player builds strip the field — at runtime
the enum type comes from the generic argument alone.
</p>
<p>
Internal hot paths are wrapped in profiler markers; define the
<code>ASPID_FAST_TOOLS_UNITY_PROFILER_DISABLED</code> scripting symbol to compile them out.
</p>

## Methods

### Equals\(TEnum, TEnum\) {#Aspid_FastTools_Enums_EnumValues_2_Equals__0__0_}

Determines whether two enum values should be considered equal for lookup purposes.
The first argument is the value being looked up; the second is the entry's stored key.

```csharp
public bool Equals(TEnum enumValue1, TEnum enumValue2)
```

#### Parameters

`enumValue1` TEnum

The lookup value (must contain the entry's bits to match).

`enumValue2` TEnum

The stored entry key.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

For regular enums: <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> when both values are identical.<br />
For <code>[Flags]</code> enums: <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> when <code class="paramref">enumValue1</code>
has all bits of <code class="paramref">enumValue2</code> set, with the additional rule that
the zero (<code>None</code>) value is only equal to another zero value.

### GetEnumerator\(\) {#Aspid_FastTools_Enums_EnumValues_2_GetEnumerator}

Returns a struct enumerator over the explicitly configured (key, value) pairs in
serialized order — <code>foreach</code> binds to it directly and does not allocate.
Does <b>not</b> include the default value or entries with an unresolved key.

```csharp
public EnumValuesEnumerator<TEnum, TValue> GetEnumerator()
```

#### Returns

 [EnumValuesEnumerator](Aspid.FastTools.Enums.EnumValuesEnumerator-2.md)\<TEnum, TValue\>

### GetValue\(TEnum\) {#Aspid_FastTools_Enums_EnumValues_2_GetValue__0_}

Returns the value mapped to <code class="paramref">enumValue</code>,
or the configured default value if no mapping exists.

```csharp
public TValue? GetValue(TEnum enumValue)
```

#### Parameters

`enumValue` TEnum

The enum member to look up.

#### Returns

 TValue?

The mapped value, or the default value when no entry matches. A reference-type
<code class="typeparamref">TValue</code> left unassigned in the Inspector is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

