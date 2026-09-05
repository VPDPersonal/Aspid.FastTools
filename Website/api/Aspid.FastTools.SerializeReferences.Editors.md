---
title: "Namespace Aspid.FastTools.SerializeReferences.Editors"
sidebar_label: "Aspid.FastTools.SerializeReferences.Editors"
description: "Namespace Aspid.FastTools.SerializeReferences.Editors — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Namespace Aspid.FastTools.SerializeReferences.Editors {#Aspid_FastTools_SerializeReferences_Editors}

### Classes

 [SerializeReferenceEditorGUI](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md)

The custom-editor entry point to the SerializeReference dropdown field: draws a <code>[SerializeReference]</code>
property with the package's type-dropdown UI from an editor's own code, no <code>[TypeSelector]</code> attribute
needed. This is how a custom editor offers the same fields Unity's own inspector would need
<code>[TypeSelector]</code> for: [`SerializeReferenceEditorGUI.CreateField`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md) / [`SerializeReferenceEditorGUI.CreateList`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md) from
<code>CreateInspectorGUI</code>, [`SerializeReferenceEditorGUI.DrawFieldLayout`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI.md) from an IMGUI <code>OnInspectorGUI</code> (lists there:
[`SerializeReferenceIMGUIList.Draw`](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList.md)).

 [SerializeReferenceIMGUIList](Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList.md)

IMGUI parity for the UIToolkit ListView's picker-backed "+": draws a <code>[SerializeReference]</code> list/array whose
add button opens the type picker and appends a fresh typed instance (or an empty <code>&lt;None&gt;</code> element),
mirroring [`Editors.SerializeReferenceListAddBehavior`](Aspid.FastTools.SerializeReferences.Editors.md).

