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

[`TypeField`](Aspid.FastTools.Types.Editors.TypeField.md) pre-styled as an Inspector property row, so its label aligns with sibling fields.

 [TypeExtensions](Aspid.FastTools.Types.Editors.TypeExtensions.md)

Provides editor-side extension methods for locating and opening the [`MonoScript`](https://docs.unity3d.com/ScriptReference/MonoScript.html) defining a
[`Type`](https://learn.microsoft.com/dotnet/api/system.type).

 [TypeField](Aspid.FastTools.Types.Editors.TypeField.md)

UIToolkit field showing a [`Type`](https://learn.microsoft.com/dotnet/api/system.type) as a dropdown backed by [`TypeSelectorWindow`](Aspid.FastTools.Types.Editors.TypeSelectorWindow.md),
optionally bound to a string property holding the type's assembly-qualified name.

 [TypeSelectorWindow](Aspid.FastTools.Types.Editors.TypeSelectorWindow.md)

Dropdown window for browsing and selecting a [`Type`](https://learn.microsoft.com/dotnet/api/system.type) from a filtered list.

 [TypeField.UxmlSerializedData](Aspid.FastTools.Types.Editors.TypeField.UxmlSerializedData.md)

 [InspectorTypeField.UxmlSerializedData](Aspid.FastTools.Types.Editors.InspectorTypeField.UxmlSerializedData.md)

### Structs

 [TypeSelectorFilter](Aspid.FastTools.Types.Editors.TypeSelectorFilter.md)

Represents the constraints deciding which types the selector offers.

### Delegates

 [GenericArgumentFilter](Aspid.FastTools.Types.Editors.GenericArgumentFilter.md)

Represents the method that decides whether <code class="paramref">argument</code> may close
<code class="paramref">parameter</code>.

