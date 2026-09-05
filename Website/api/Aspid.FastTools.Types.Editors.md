---
title: "Namespace Aspid.FastTools.Types.Editors"
sidebar_label: "Aspid.FastTools.Types.Editors"
description: "Namespace Aspid.FastTools.Types.Editors — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Namespace Aspid.FastTools.Types.Editors {#Aspid_FastTools_Types_Editors}

### Classes

 [InspectorTypeField](Aspid.FastTools.Types.Editors.InspectorTypeField.md)

[`TypeField`](Aspid.FastTools.Types.Editors.TypeField.md) variant pre-styled to match a Unity Inspector property row,
so its label aligns with sibling property fields.

 [TypeExtensions](Aspid.FastTools.Types.Editors.TypeExtensions.md)

Editor-side extension methods for [`Type`](https://learn.microsoft.com/dotnet/api/system.type): locate the [`MonoScript`](https://docs.unity3d.com/ScriptReference/MonoScript.html)
asset that defines a type and open it in the external script editor.

 [TypeField](Aspid.FastTools.Types.Editors.TypeField.md)

UIToolkit field that displays a [`Type`](https://learn.microsoft.com/dotnet/api/system.type) as a dropdown backed by
[`TypeSelectorWindow`](Aspid.FastTools.Types.Editors.TypeSelectorWindow.md), optionally bound to a string-typed
[`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) that stores the type's assembly-qualified name.

 [TypeSelectorWindow](Aspid.FastTools.Types.Editors.TypeSelectorWindow.md)

Editor window that displays a hierarchical type selector dropdown, allowing the user to browse and select a [`Type`](https://learn.microsoft.com/dotnet/api/system.type) from a filtered list.

 [InspectorTypeField.UxmlSerializedData](Aspid.FastTools.Types.Editors.InspectorTypeField.UxmlSerializedData.md)

 [TypeField.UxmlSerializedData](Aspid.FastTools.Types.Editors.TypeField.UxmlSerializedData.md)

### Structs

 [TypeSelectorFilter](Aspid.FastTools.Types.Editors.TypeSelectorFilter.md)

Describes which types the selector offers: the base-type and kind constraints, an optional per-type
predicate, any verbatim extra entries, and the argument predicate for open generics. Bundles the
candidate-defining inputs of [`TypeSelectorWindow.Show`](Aspid.FastTools.Types.Editors.TypeSelectorWindow.md) and the [`Editors.TypeSelectorView`](Aspid.FastTools.Types.Editors.md)
constructor into a single value so they travel together.

### Delegates

 [GenericArgumentFilter](Aspid.FastTools.Types.Editors.GenericArgumentFilter.md)

Decides whether <code class="paramref">argument</code> may close <code class="paramref">parameter</code> of
<code class="paramref">openDefinition</code>. Unlike a plain per-type predicate this is asked <i>about a position</i>,
because what a closed type has to store depends on where its parameter lands: a parameter reaching a field
the engine writes by value constrains the argument, one reaching only a <code>[SerializeReference]</code> field
does not.

