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

 [SerializableMonoScript](Aspid.FastTools.Types.SerializableMonoScript.md)

Unity-serializable wrapper around a [`Type`](https://learn.microsoft.com/dotnet/api/system.type) referencing it through its <code>MonoScript</code>
asset, so renaming or moving the class does not break the field.

 [SerializableMonoScript\<T\>](Aspid.FastTools.Types.SerializableMonoScript-1.md)

[`SerializableMonoScript`](Aspid.FastTools.Types.SerializableMonoScript.md) constrained to types assignable to <code class="typeparamref">T</code>.

 [SerializableType](Aspid.FastTools.Types.SerializableType.md)

Unity-serializable wrapper around a [`Type`](https://learn.microsoft.com/dotnet/api/system.type), stored by its <code>AssemblyQualifiedName</code>
and resolved lazily on first access.

 [SerializableType\<T\>](Aspid.FastTools.Types.SerializableType-1.md)

[`SerializableType`](Aspid.FastTools.Types.SerializableType.md) constrained to types assignable to <code class="typeparamref">T</code>.

 [SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md)

Shared implementation of the serializable [`Type`](https://learn.microsoft.com/dotnet/api/system.type) wrappers: stores the type by its
assembly-qualified name and resolves it lazily on first access.

 [TypeSelectorAttribute](Aspid.FastTools.Types.TypeSelectorAttribute.md)

Draws the field with the type-selector window.

 [TypeSelectorDisplayAttribute](Aspid.FastTools.Types.TypeSelectorDisplayAttribute.md)

Supplies presentation metadata for a type in the type-selector window — display name, group, tooltip and
icon — or keeps the type out of the picker entirely with [`TypeSelectorDisplayAttribute.Hidden`](Aspid.FastTools.Types.TypeSelectorDisplayAttribute.md#Aspid_FastTools_Types_TypeSelectorDisplayAttribute_Hidden).

### Structs

 [ComponentTypeSelector](Aspid.FastTools.Types.ComponentTypeSelector.md)

Represents a marker field adding an Inspector dropdown that swaps the object's script to any subtype of the
field's declaring class.

### Interfaces

 [ISerializableType](Aspid.FastTools.Types.ISerializableType.md)

Defines the common contract of the serializable [`Type`](https://learn.microsoft.com/dotnet/api/system.type) wrappers.

### Enums

 [TypeAllow](Aspid.FastTools.Types.TypeAllow.md)

Specifies which special type categories the type picker offers in addition to concrete classes.

