---
title: "Class InspectorTypeField"
sidebar_label: "InspectorTypeField"
description: "Class InspectorTypeField — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class InspectorTypeField {#Aspid_FastTools_Types_Editors_InspectorTypeField}

Namespace: [Aspid.FastTools.Types.Editors](Aspid.FastTools.Types.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

[`TypeField`](Aspid.FastTools.Types.Editors.TypeField.md) pre-styled as an Inspector property row, so its label aligns with sibling fields.

```csharp
[UxmlElement]
public sealed class InspectorTypeField : TypeField, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, ICustomStyle, IBindable, IMixedValueSupport, INotifyValueChanged<Type>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
CallbackEventHandler ← 
Focusable ← 
VisualElement ← 
BindableElement ← 
AbstractBaseField ← 
BaseField\<Type\> ← 
[TypeField](Aspid.FastTools.Types.Editors.TypeField.md) ← 
[InspectorTypeField](Aspid.FastTools.Types.Editors.InspectorTypeField.md)

#### Implements

IEventHandler,
IResolvedStyle,
ITransform,
ITransitionAnimations,
IExperimentalFeatures,
IVisualElementScheduler,
ICustomStyle,
IBindable,
IMixedValueSupport,
INotifyValueChanged\<Type\>


#### Extension Methods

[VisualElementExtensions.AddOpenScriptCommand\<InspectorTypeField\>\(InspectorTypeField, Object\)](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_AddOpenScriptCommand__1___0_UnityEngine_Object_),
[INotifyValueChangedExtensions.AddValueChanged\<InspectorTypeField, TValue\>\(InspectorTypeField, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[IBindableExtensions.BindPropertyTo\<InspectorTypeField\>\(InspectorTypeField, SerializedProperty\)](Aspid.FastTools.UIElements.Editors.IBindableExtensions.md#Aspid_FastTools_UIElements_Editors_IBindableExtensions_BindPropertyTo__1___0_UnityEditor_SerializedProperty_),
[IBindableExtensions.BindTo\<InspectorTypeField\>\(InspectorTypeField, SerializedObject, string\)](Aspid.FastTools.UIElements.Editors.IBindableExtensions.md#Aspid_FastTools_UIElements_Editors_IBindableExtensions_BindTo__1___0_UnityEditor_SerializedObject_System_String_),
[VisualElementExtensions.BindTo\<InspectorTypeField\>\(InspectorTypeField, SerializedObject\)](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_BindTo__1___0_UnityEditor_SerializedObject_),
[VisualElementExtensions.GetOwnerWindow\(VisualElement\)](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_GetOwnerWindow_UnityEngine_UIElements_VisualElement_),
[ProfilerMarkerExtensionsForGenerator.Marker\<InspectorTypeField\>\(InspectorTypeField, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_),
[INotifyValueChangedExtensions.RemoveValueChanged\<InspectorTypeField, TValue\>\(InspectorTypeField, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[IBindableExtensions.SetBindingPath\<InspectorTypeField\>\(InspectorTypeField, string\)](Aspid.FastTools.UIElements.Editors.IBindableExtensions.md#Aspid_FastTools_UIElements_Editors_IBindableExtensions_SetBindingPath__1___0_System_String_),
[BaseFieldExtensions.SetLabel\<InspectorTypeField, TValue\>\(InspectorTypeField, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_),
[INotifyValueChangedExtensions.SetValue\<InspectorTypeField, TValue\>\(InspectorTypeField, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_),
[VisualElementExtensions.UnbindFrom\<InspectorTypeField\>\(InspectorTypeField\)](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_UnbindFrom__1___0_)

## Constructors

### InspectorTypeField\(\) {#Aspid_FastTools_Types_Editors_InspectorTypeField__ctor}

Creates an unbound field with Inspector label alignment.

```csharp
public InspectorTypeField()
```

### InspectorTypeField\(SerializedProperty\) {#Aspid_FastTools_Types_Editors_InspectorTypeField__ctor_UnityEditor_SerializedProperty_}

Creates an Inspector-aligned field labeled with the bound property's display name.

```csharp
public InspectorTypeField(SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

The string property storing the assembly-qualified type name.

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

<code class="paramref">property</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

<code class="paramref">property</code> is not a string property, or its path no longer exists on its targets.

### InspectorTypeField\(string, SerializedProperty\) {#Aspid_FastTools_Types_Editors_InspectorTypeField__ctor_System_String_UnityEditor_SerializedProperty_}

Creates an Inspector-aligned field bound to a type-name property.

```csharp
public InspectorTypeField(string label, SerializedProperty property)
```

#### Parameters

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

The field label, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for no label.

`property` SerializedProperty

The string property storing the assembly-qualified type name.

#### Remarks

Before [`ChangeEvent<T>`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.ChangeEvent_1.html) is sent, a pick writes the property and calls
[`Update`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/SerializedObject.Update.html) on the [`SerializedObject`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/SerializedObject.html) of <code class="paramref">property</code>:
a change handler reads the picked name from <code class="paramref">property</code>, and edits it applies through that
object keep the pick. Changes not yet applied to that object are lost.

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

<code class="paramref">property</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

<code class="paramref">property</code> is not a string property, or its path no longer exists on its targets.

### InspectorTypeField\(string\) {#Aspid_FastTools_Types_Editors_InspectorTypeField__ctor_System_String_}

Creates an unbound Inspector-aligned field without an initial type.

```csharp
public InspectorTypeField(string label)
```

#### Parameters

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

The field label, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for no label.

### InspectorTypeField\(string, Type\) {#Aspid_FastTools_Types_Editors_InspectorTypeField__ctor_System_String_System_Type_}

Creates an unbound Inspector-aligned field with an initial type.

```csharp
public InspectorTypeField(string label, Type defaultValue)
```

#### Parameters

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

The field label, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for no label.

`defaultValue` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The initial type, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for an empty selection.

