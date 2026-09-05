---
title: "Namespace Aspid.FastTools.Enums"
sidebar_label: "Aspid.FastTools.Enums"
description: "Namespace Aspid.FastTools.Enums — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Namespace Aspid.FastTools.Enums {#Aspid_FastTools_Enums}

### Classes

 [EnumValues\<TValue\>](Aspid.FastTools.Enums.EnumValues-1.md)

A serializable dictionary that maps each member of a chosen enum to a value of type
<code class="typeparamref">TValue</code>. Supports both regular and <code>[Flags]</code> enums.

 [EnumValues\<TEnum, TValue\>](Aspid.FastTools.Enums.EnumValues-2.md)

A serializable dictionary that maps members of <code class="typeparamref">TEnum</code> to values of
type <code class="typeparamref">TValue</code>. The typed counterpart of [`EnumValues<T>`](Aspid.FastTools.Enums.EnumValues-1.md)
for the common case where the enum type is known at compile time — the Inspector type-picker
is read-only, and lookups are compile-time safe.

### Structs

 [EnumValuesEnumerator\<TKey, TValue\>](Aspid.FastTools.Enums.EnumValuesEnumerator-2.md)

Allocation-free enumerator over the resolved entries of an [`EnumValues<T>`](Aspid.FastTools.Enums.EnumValues-1.md)
(<code class="typeparamref">TKey</code> = [`Enum`](https://learn.microsoft.com/dotnet/api/system.enum)) or an [`EnumValues<T1, T2>`](Aspid.FastTools.Enums.EnumValues-2.md)
(<code class="typeparamref">TKey</code> = the enum type). Boxed only when consumed through the
[`IEnumerable<T>`](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1) interface (e.g. LINQ).

