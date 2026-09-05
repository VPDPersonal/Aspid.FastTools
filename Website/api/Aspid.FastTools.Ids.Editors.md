---
title: "Namespace Aspid.FastTools.Ids.Editors"
sidebar_label: "Aspid.FastTools.Ids.Editors"
description: "Namespace Aspid.FastTools.Ids.Editors — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Namespace Aspid.FastTools.Ids.Editors {#Aspid_FastTools_Ids_Editors}

### Classes

 [IdField](Aspid.FastTools.Ids.Editors.IdField.md)

UIToolkit field that displays an [`IId`](Aspid.FastTools.Ids.IId.md)-style integer id as an EnumField-style
dropdown backed by [`Editors.IdSelectorWindow`](Aspid.FastTools.Ids.Editors.md). Optionally bound to an
[`IId`](Aspid.FastTools.Ids.IId.md) struct [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) whose generated children
(<code>_id</code> and <code>__stringId</code>) are updated together; when no [`IdRegistry`](Aspid.FastTools.Ids.IdRegistry.md)
is bound to [`IdField.IdType`](Aspid.FastTools.Ids.Editors.IdField.md#Aspid_FastTools_Ids_Editors_IdField_IdType) or the id cannot be resolved to a name, the field renders
a <code>&lt;Missing&gt;</code> caption instead of silently clearing.

 [InspectorIdField](Aspid.FastTools.Ids.Editors.InspectorIdField.md)

[`IdField`](Aspid.FastTools.Ids.Editors.IdField.md) variant pre-styled to match a Unity Inspector property row:
applies [`alignedFieldUssClassName`](https://docs.unity3d.com/ScriptReference/UIElements-BaseField-alignedFieldUssClassName.html) and the
[`PropertyField`](https://docs.unity3d.com/ScriptReference/UIElements-PropertyField.html) USS classes so the label aligns with sibling property fields.

 [InspectorIdField.UxmlSerializedData](Aspid.FastTools.Ids.Editors.InspectorIdField.UxmlSerializedData.md)

 [IdField.UxmlSerializedData](Aspid.FastTools.Ids.Editors.IdField.UxmlSerializedData.md)

