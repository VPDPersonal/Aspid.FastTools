---
title: "Class TypeField"
sidebar_label: "TypeField"
description: "Class TypeField — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TypeField {#Aspid_FastTools_Types_Editors_TypeField}

Namespace: [Aspid.FastTools.Types.Editors](Aspid.FastTools.Types.Editors.md)  
Assembly: Aspid.FastTools.Editor.dll  

[`BaseField<T>`](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.BaseField_1.html) of [`Type`](https://learn.microsoft.com/dotnet/api/system.type) for selecting a type and optionally storing its assembly-qualified name.

```csharp
[UxmlElement]
public class TypeField : BaseField<Type>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, ICustomStyle, IBindable, IMixedValueSupport, INotifyValueChanged<Type>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
CallbackEventHandler ← 
Focusable ← 
VisualElement ← 
BindableElement ← 
AbstractBaseField ← 
BaseField\<Type\> ← 
[TypeField](Aspid.FastTools.Types.Editors.TypeField.md)

#### Derived

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

[VisualElementExtensions.AddAspidTheme\<TypeField\>\(TypeField\)](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_AddAspidTheme__1___0_),
[VisualElementExtensions.AddOpenScriptCommand\<TypeField\>\(TypeField, Object\)](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_AddOpenScriptCommand__1___0_UnityEngine_Object_),
[INotifyValueChangedExtensions.AddValueChanged\<TypeField, TValue\>\(TypeField, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[IBindableExtensions.BindPropertyTo\<TypeField\>\(TypeField, SerializedProperty\)](Aspid.FastTools.UIElements.Editors.IBindableExtensions.md#Aspid_FastTools_UIElements_Editors_IBindableExtensions_BindPropertyTo__1___0_UnityEditor_SerializedProperty_),
[IBindableExtensions.BindTo\<TypeField\>\(TypeField, SerializedObject, string\)](Aspid.FastTools.UIElements.Editors.IBindableExtensions.md#Aspid_FastTools_UIElements_Editors_IBindableExtensions_BindTo__1___0_UnityEditor_SerializedObject_System_String_),
[VisualElementExtensions.BindTo\<TypeField\>\(TypeField, SerializedObject\)](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_BindTo__1___0_UnityEditor_SerializedObject_),
[VisualElementExtensions.GetOwnerWindow\(VisualElement\)](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_GetOwnerWindow_UnityEngine_UIElements_VisualElement_),
[ProfilerMarkerExtensionsForGenerator.Marker\<TypeField\>\(TypeField, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_),
[INotifyValueChangedExtensions.RemoveValueChanged\<TypeField, TValue\>\(TypeField, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[IBindableExtensions.SetBindingPath\<TypeField\>\(TypeField, string\)](Aspid.FastTools.UIElements.Editors.IBindableExtensions.md#Aspid_FastTools_UIElements_Editors_IBindableExtensions_SetBindingPath__1___0_System_String_),
[BaseFieldExtensions.SetLabel\<TypeField, TValue\>\(TypeField, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_),
[INotifyValueChangedExtensions.SetValue\<TypeField, TValue\>\(TypeField, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_),
[VisualElementExtensions.UnbindFrom\<TypeField\>\(TypeField\)](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_UnbindFrom__1___0_)

## Remarks

<p>UXML creates an unbound field: it ignores <code>binding-path</code>, and [`TypeField.Types`](Aspid.FastTools.Types.Editors.TypeField.md#Aspid_FastTools_Types_Editors_TypeField_Types) and [`TypeField.Predicate`](Aspid.FastTools.Types.Editors.TypeField.md#Aspid_FastTools_Types_Editors_TypeField_Predicate) have no UXML attributes. Create a bound or constrained field in C#.</p>
<p>Not designed for inheritance outside the package.</p>

## Constructors

### TypeField\(\) {#Aspid_FastTools_Types_Editors_TypeField__ctor}

Creates an unbound field without a label.

```csharp
public TypeField()
```

### TypeField\(SerializedProperty\) {#Aspid_FastTools_Types_Editors_TypeField__ctor_UnityEditor_SerializedProperty_}

Creates a field bound to <code class="paramref">property</code>, labeled with its display name.

```csharp
public TypeField(SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

A string property holding the assembly-qualified type name.

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

<code class="paramref">property</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

<code class="paramref">property</code> is not a string property, or its path no longer exists on its targets.

### TypeField\(string, SerializedProperty\) {#Aspid_FastTools_Types_Editors_TypeField__ctor_System_String_UnityEditor_SerializedProperty_}

Creates a bound field: the picked type's name is written to the property, and external edits are
reflected back into the field.

```csharp
public TypeField(string label, SerializedProperty property)
```

#### Parameters

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

The field label; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for none.

`property` SerializedProperty

A string property holding the assembly-qualified type name.

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

### TypeField\(string\) {#Aspid_FastTools_Types_Editors_TypeField__ctor_System_String_}

Creates an unbound field without an initial type; the picked type is reported only through the change event.

```csharp
public TypeField(string label)
```

#### Parameters

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

The field label; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for none.

### TypeField\(string, Type\) {#Aspid_FastTools_Types_Editors_TypeField__ctor_System_String_System_Type_}

Creates an unbound field; the picked type is reported only through the change event.

```csharp
public TypeField(string label, Type defaultValue)
```

#### Parameters

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

The field label; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for none.

`defaultValue` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type shown initially, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for <code>&lt;None&gt;</code>.

## Properties

### Allow {#Aspid_FastTools_Types_Editors_TypeField_Allow}

Gets or sets which kinds of types can be picked.

```csharp
[UxmlAttribute]
public TypeAllow Allow { get; set; }
```

#### Property Value

 TypeAllow

#### Remarks

[`TypeAllow.None`](Aspid.FastTools.Types.TypeAllow.md) by default, so only concrete types are offered, unlike
[`TypeSelectorAttribute.Allow`](Aspid.FastTools.Types.TypeSelectorAttribute.md#Aspid_FastTools_Types_TypeSelectorAttribute_Allow), which defaults to [`TypeAllow.All`](Aspid.FastTools.Types.TypeAllow.md).

### AssemblyQualifiedName {#Aspid_FastTools_Types_Editors_TypeField_AssemblyQualifiedName}

Gets the assembly-qualified name of the selected type, or the stored name of a missing type; an empty
string when no type is selected.

```csharp
public string AssemblyQualifiedName { get; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### ExcludeEditorOnlyTypes {#Aspid_FastTools_Types_Editors_TypeField_ExcludeEditorOnlyTypes}

Gets or sets a value indicating whether types from editor-only assemblies are left out, for a value that a
player build must resolve.

```csharp
[UxmlAttribute]
public bool ExcludeEditorOnlyTypes { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Remarks

A bound field turns it on when its property belongs to an object of a runtime assembly.

### HideNoneOption {#Aspid_FastTools_Types_Editors_TypeField_HideNoneOption}

Gets or sets a value indicating whether the <code>&lt;None&gt;</code> row is left out, for a field whose value
must never be cleared.

```csharp
[UxmlAttribute]
public bool HideNoneOption { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### IsReadOnly {#Aspid_FastTools_Types_Editors_TypeField_IsReadOnly}

Gets or sets a value indicating whether the dropdown is disabled, so the displayed type cannot be
changed.

```csharp
[UxmlAttribute]
public bool IsReadOnly { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Remarks

The open-in-script-editor button stays active.

### Predicate {#Aspid_FastTools_Types_Editors_TypeField_Predicate}

Gets or sets the predicate applied to each candidate after the [`TypeField.Types`](Aspid.FastTools.Types.Editors.TypeField.md#Aspid_FastTools_Types_Editors_TypeField_Types) and
[`TypeField.Allow`](Aspid.FastTools.Types.Editors.TypeField.md#Aspid_FastTools_Types_Editors_TypeField_Allow) checks. <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> keeps every matching type.

```csharp
public Func<Type, bool> Predicate { get; set; }
```

#### Property Value

 [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[Type](https://learn.microsoft.com/dotnet/api/system.type), [bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### Types {#Aspid_FastTools_Types_Editors_TypeField_Types}

Gets or sets the base types; the dropdown lists only types assignable to every one of them.

```csharp
public Type[] Types { get; set; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

#### Remarks

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> entries are dropped. For a closed generic type such as <code>IHandler&lt;int&gt;</code>,
the dropdown also offers the generic classes that close to it, such as <code>Handler&lt;int&gt;</code>.

### value {#Aspid_FastTools_Types_Editors_TypeField_value}

Gets or sets the selected type, clearing any unresolved name and notifying listeners when it is cleared.

```csharp
public override sealed Type value { get; set; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

## Methods

### SetValueFromAssemblyQualifiedNameWithoutNotify\(string\) {#Aspid_FastTools_Types_Editors_TypeField_SetValueFromAssemblyQualifiedNameWithoutNotify_System_String_}

Sets the value from an assembly-qualified type name without raising a change event.

```csharp
public void SetValueFromAssemblyQualifiedNameWithoutNotify(string assemblyQualifiedName)
```

#### Parameters

`assemblyQualifiedName` [string](https://learn.microsoft.com/dotnet/api/system.string)

The type name, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> or whitespace for an empty selection.

#### Remarks

A name that cannot be resolved is preserved, so the field renders a <code>&lt;Missing&gt;</code> caption
instead of silently clearing.

### SetValueWithoutNotify\(Type\) {#Aspid_FastTools_Types_Editors_TypeField_SetValueWithoutNotify_System_Type_}

Sets the displayed type and clears any unresolved name without raising a change event.

```csharp
public override sealed void SetValueWithoutNotify(Type newValue)
```

#### Parameters

`newValue` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type to show, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for an empty selection.

### UpdateMixedValueContent\(\) {#Aspid_FastTools_Types_Editors_TypeField_UpdateMixedValueContent}

Called when the mixed-value state changes to refresh the caption and script button.

```csharp
protected override sealed void UpdateMixedValueContent()
```

