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

Serializable table that maps members of <code class="typeparamref">TEnum</code> to <code class="typeparamref">TValue</code> values.

```csharp
[Serializable]
public sealed class EnumValues<TEnum, TValue> : IReadOnlyCollection<KeyValuePair<TEnum, TValue?>>, IEnumerable<KeyValuePair<TEnum, TValue?>>, IEnumerable, ISerializationCallbackReceiver where TEnum : struct, Enum
```

#### Type Parameters

`TEnum` 

The enum the entries are keyed by.

`TValue` 

The type of the mapped values.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[EnumValues\<TEnum, TValue\>](Aspid.FastTools.Enums.EnumValues-2.md)

#### Implements

[IReadOnlyCollection\<KeyValuePair\<TEnum, TValue?\>\>](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlycollection-1),
[IEnumerable\<KeyValuePair\<TEnum, TValue?\>\>](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1),
[IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.ienumerable),
ISerializationCallbackReceiver


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\<EnumValues\<TEnum, TValue\>\>\(EnumValues\<TEnum, TValue\>, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_)

## Examples


```csharp
[SerializeField] private EnumValues<DamageType, Color> _damageColors;
```


## Remarks

Lookup rules, thread safety and the serialized layout match [`EnumValues<T>`](Aspid.FastTools.Enums.EnumValues-1.md): a field can switch
between the two types without losing entries if the enum is the same.

## Properties

### Count {#Aspid_FastTools_Enums_EnumValues_2_Count}

Gets the number of entries with a resolved key, duplicates included; the default value is not counted.

```csharp
public int Count { get; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

## Methods

### Equals\(TEnum, TEnum\) {#Aspid_FastTools_Enums_EnumValues_2_Equals__0__0_}

Determines whether the lookup value <code class="paramref">enumValue1</code> matches the entry key
<code class="paramref">enumValue2</code>.

```csharp
public bool Equals(TEnum enumValue1, TEnum enumValue2)
```

#### Parameters

`enumValue1` TEnum

The lookup value.

`enumValue2` TEnum

The entry key.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if the values are equal or, for a <code>[Flags]</code> enum, <code class="paramref">enumValue1</code>
contains all bits of <code class="paramref">enumValue2</code> and zero matches only zero; otherwise,
<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

### GetEnumerator\(\) {#Aspid_FastTools_Enums_EnumValues_2_GetEnumerator}

Returns an enumerator over the entries with a resolved key in serialized order, without the default value.

```csharp
public EnumValuesEnumerator<TEnum, TValue> GetEnumerator()
```

#### Returns

 [EnumValuesEnumerator](Aspid.FastTools.Enums.EnumValuesEnumerator-2.md)\<TEnum, TValue\>

### GetValue\(TEnum\) {#Aspid_FastTools_Enums_EnumValues_2_GetValue__0_}

Returns the value mapped to <code class="paramref">enumValue</code>.

```csharp
public TValue? GetValue(TEnum enumValue)
```

#### Parameters

`enumValue` TEnum

The enum member to look up.

#### Returns

 TValue?

The mapped value; otherwise, the default value.

### TryGetValue\(TEnum, out TValue?\) {#Aspid_FastTools_Enums_EnumValues_2_TryGetValue__0__1__}

Looks up the value mapped to <code class="paramref">enumValue</code>.

```csharp
public bool TryGetValue(TEnum enumValue, out TValue? value)
```

#### Parameters

`enumValue` TEnum

The enum member to look up.

`value` TValue?

The mapped value; otherwise, the default value.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if an entry matches; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

