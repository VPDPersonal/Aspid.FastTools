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

Serializable table that maps members of an enum chosen in the Inspector to <code class="typeparamref">TValue</code> values.

```csharp
[Serializable]
public sealed class EnumValues<TValue> : IReadOnlyCollection<KeyValuePair<Enum, TValue?>>, IEnumerable<KeyValuePair<Enum, TValue?>>, IEnumerable, ISerializationCallbackReceiver
```

#### Type Parameters

`TValue` 

The type of the mapped values.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[EnumValues\<TValue\>](Aspid.FastTools.Enums.EnumValues-1.md)

#### Implements

[IReadOnlyCollection\<KeyValuePair\<Enum, TValue?\>\>](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlycollection-1),
[IEnumerable\<KeyValuePair\<Enum, TValue?\>\>](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1),
[IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.ienumerable),
ISerializationCallbackReceiver


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<EnumValues\<TValue\>\>\(EnumValues\<TValue\>, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Examples


```csharp
[SerializeField] private EnumValues<Color> _damageColors;
```


## Remarks

<p>For a <code>[Flags]</code> enum, an entry matches when the lookup value contains all its bits, and zero matches only zero. An exact key wins; otherwise, the first match in serialized order.</p>
<p>A player finds the enum by its stored assembly-qualified name. From Managed Stripping Level Medium up, keep an enum used only through this table with <code>[Preserve]</code> or <code>link.xml</code>, or every lookup returns the default value.</p>
<p>Lookups and iteration are thread-safe, the first access included, unless Unity deserializes the table at the same time.</p>

## Properties

### Count {#Aspid_FastTools_Enums_EnumValues_1_Count}

Gets the number of entries with a resolved key, duplicates included; the default value is not counted.

```csharp
public int Count { get; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

## Methods

### Equals\(Enum, Enum\) {#Aspid_FastTools_Enums_EnumValues_1_Equals_System_Enum_System_Enum_}

Determines whether the lookup value <code class="paramref">enumValue1</code> matches the entry key
<code class="paramref">enumValue2</code>.

```csharp
public bool Equals(Enum enumValue1, Enum enumValue2)
```

#### Parameters

`enumValue1` [Enum](https://learn.microsoft.com/dotnet/api/system.enum)

The lookup value.

`enumValue2` [Enum](https://learn.microsoft.com/dotnet/api/system.enum)

The entry key.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if the values are equal or, for a <code>[Flags]</code> enum, <code class="paramref">enumValue1</code>
contains all bits of <code class="paramref">enumValue2</code> and zero matches only zero; otherwise, including for
<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or another enum, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

### GetEnumerator\(\) {#Aspid_FastTools_Enums_EnumValues_1_GetEnumerator}

Returns an enumerator over the entries with a resolved key in serialized order, without the default value.

```csharp
public EnumValuesEnumerator<Enum, TValue> GetEnumerator()
```

#### Returns

 [EnumValuesEnumerator](Aspid.FastTools.Enums.EnumValuesEnumerator-2.md)\<[Enum](https://learn.microsoft.com/dotnet/api/system.enum), TValue\>

### GetValue\(Enum\) {#Aspid_FastTools_Enums_EnumValues_1_GetValue_System_Enum_}

Returns the value mapped to <code class="paramref">enumValue</code>.

```csharp
public TValue? GetValue(Enum enumValue)
```

#### Parameters

`enumValue` [Enum](https://learn.microsoft.com/dotnet/api/system.enum)

The enum member to look up; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or a member of another enum never matches.

#### Returns

 TValue?

The mapped value; otherwise, the default value.

### GetValue\<TEnum\>\(TEnum\) {#Aspid_FastTools_Enums_EnumValues_1_GetValue__1___0_}

Returns the value mapped to <code class="paramref">enumValue</code> without boxing it.

```csharp
public TValue? GetValue<TEnum>(TEnum enumValue) where TEnum : struct, Enum
```

#### Parameters

`enumValue` TEnum

The enum member to look up.

#### Returns

 TValue?

The mapped value; otherwise, the default value.

#### Type Parameters

`TEnum` 

The configured enum; another enum never matches.

### TryGetValue\(Enum, out TValue?\) {#Aspid_FastTools_Enums_EnumValues_1_TryGetValue_System_Enum__0__}

Looks up the value mapped to <code class="paramref">enumValue</code>.

```csharp
public bool TryGetValue(Enum enumValue, out TValue? value)
```

#### Parameters

`enumValue` [Enum](https://learn.microsoft.com/dotnet/api/system.enum)

The enum member to look up; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or a member of another enum never matches.

`value` TValue?

The mapped value; otherwise, the default value.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if an entry matches; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

### TryGetValue\<TEnum\>\(TEnum, out TValue?\) {#Aspid_FastTools_Enums_EnumValues_1_TryGetValue__1___0__0__}

Looks up the value mapped to <code class="paramref">enumValue</code> without boxing it.

```csharp
public bool TryGetValue<TEnum>(TEnum enumValue, out TValue? value) where TEnum : struct, Enum
```

#### Parameters

`enumValue` TEnum

The enum member to look up.

`value` TValue?

The mapped value; otherwise, the default value.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if an entry matches; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

#### Type Parameters

`TEnum` 

The configured enum; another enum never matches.

## See Also

[EnumValues](Aspid.FastTools.Enums.EnumValues-2.md)\<TEnum, TValue\>

