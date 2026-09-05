---
title: "Namespace Aspid.FastTools.Types"
sidebar_label: "Aspid.FastTools.Types"
description: "Namespace Aspid.FastTools.Types — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Namespace Aspid.FastTools.Types {#Aspid_FastTools_Types}

### Namespaces

 [Aspid.FastTools.Types.Editors](Aspid.FastTools.Types.Editors.md)

### Classes

 [SerializableType](Aspid.FastTools.Types.SerializableType.md)

A wrapper around [`Type`](https://learn.microsoft.com/dotnet/api/system.type) that supports Unity Inspector serialization.
The type is stored by its <code>AssemblyQualifiedName</code> and resolved lazily on first access.

 [SerializableType\<T\>](Aspid.FastTools.Types.SerializableType-1.md)

A wrapper around [`Type`](https://learn.microsoft.com/dotnet/api/system.type) that supports Unity Inspector serialization,
constrained to types assignable to <code class="typeparamref">T</code>.

 [TypeSelectorAttribute](Aspid.FastTools.Types.TypeSelectorAttribute.md)

Instructs the Unity Editor to use the type-selector window.

 [TypeSelectorDisplayAttribute](Aspid.FastTools.Types.TypeSelectorDisplayAttribute.md)

Supplies presentation metadata for a type shown in the type-selector window: a display name,
a picker group, a tooltip and an icon — or, with [`TypeSelectorDisplayAttribute.Hidden`](Aspid.FastTools.Types.TypeSelectorDisplayAttribute.md#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Hidden), keeps the type out of the
picker altogether.

### Structs

 [ComponentTypeSelector](Aspid.FastTools.Types.ComponentTypeSelector.md)

Adds an Inspector dropdown that lets you swap the object's script
to any subtype of the field's declaring class.

### Interfaces

 [ISerializableType](Aspid.FastTools.Types.ISerializableType.md)

Common contract of the serializable [`Type`](https://learn.microsoft.com/dotnet/api/system.type) wrappers
([`SerializableType`](Aspid.FastTools.Types.SerializableType.md) and [`SerializableType<T>`](Aspid.FastTools.Types.SerializableType-1.md)).

### Enums

 [TypeAllow](Aspid.FastTools.Types.TypeAllow.md)

Flags describing which special type categories the type picker should include
in addition to plain concrete classes.

