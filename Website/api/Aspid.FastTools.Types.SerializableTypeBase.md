---
title: "Class SerializableTypeBase"
sidebar_label: "SerializableTypeBase"
description: "Class SerializableTypeBase — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializableTypeBase {#Aspid_FastTools_Types_SerializableTypeBase}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Shared implementation of the serializable [`Type`](https://learn.microsoft.com/dotnet/api/system.type) wrappers: stores the type by its
assembly-qualified name and resolves it lazily on first access.

```csharp
[Serializable]
public abstract class SerializableTypeBase : ISerializableType, IEquatable<SerializableTypeBase>, ISerializationCallbackReceiver
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md)

#### Derived

[SerializableMonoScript](Aspid.FastTools.Types.SerializableMonoScript.md),
[SerializableType](Aspid.FastTools.Types.SerializableType.md)

#### Implements

[ISerializableType](Aspid.FastTools.Types.ISerializableType.md),
[IEquatable\<SerializableTypeBase\>](https://learn.microsoft.com/dotnet/api/system.iequatable-1),
ISerializationCallbackReceiver


#### Extension Methods

[TextInputBaseFieldTextSelectionExtensions.AddOnCursorIndexChange\<SerializableTypeBase, TValue\>\(SerializableTypeBase, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_AddOnCursorIndexChange__2___0_System_Action_),
[TextInputBaseFieldTextSelectionExtensions.AddOnSelectIndexChange\<SerializableTypeBase, TValue\>\(SerializableTypeBase, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_AddOnSelectIndexChange__2___0_System_Action_),
[INotifyValueChangedExtensions.AddValueChanged\<SerializableTypeBase, TValue\>\(SerializableTypeBase, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[ProfilerMarkerExtensionsForGenerator.Marker\<SerializableTypeBase\>\(SerializableTypeBase, int\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_System_Int32_),
[TextInputBaseFieldTextSelectionExtensions.RemoveOnCursorIndexChange\<SerializableTypeBase, TValue\>\(SerializableTypeBase, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_RemoveOnCursorIndexChange__2___0_System_Action_),
[TextInputBaseFieldTextSelectionExtensions.RemoveOnSelectIndexChange\<SerializableTypeBase, TValue\>\(SerializableTypeBase, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_RemoveOnSelectIndexChange__2___0_System_Action_),
[INotifyValueChangedExtensions.RemoveValueChanged\<SerializableTypeBase, TValue\>\(SerializableTypeBase, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[TextInputBaseFieldExtensions.SetAutoCorrection\<SerializableTypeBase, TValue\>\(SerializableTypeBase, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetAutoCorrection__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetCursorIndex\<SerializableTypeBase, TValue\>\(SerializableTypeBase, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetCursorIndex__2___0_System_Int32_),
[TextInputBaseFieldExtensions.SetDelayed\<SerializableTypeBase, TValue\>\(SerializableTypeBase, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetDelayed__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetDoubleClickSelectsWord\<SerializableTypeBase, TValue\>\(SerializableTypeBase, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetDoubleClickSelectsWord__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHideMobileInput\<SerializableTypeBase, TValue\>\(SerializableTypeBase, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHideMobileInput__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHidePlaceholderOnFocus\<SerializableTypeBase, TValue\>\(SerializableTypeBase, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHidePlaceholderOnFocus__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHideSoftKeyboard\<SerializableTypeBase, TValue\>\(SerializableTypeBase, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHideSoftKeyboard__2___0_System_Boolean_),
[SliderExtensions.SetHighValue\<SerializableTypeBase, TValue\>\(SerializableTypeBase, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_),
[TextInputBaseFieldExtensions.SetKeyboardType\<SerializableTypeBase, TValue\>\(SerializableTypeBase, TouchScreenKeyboardType\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetKeyboardType__2___0_UnityEngine_TouchScreenKeyboardType_),
[BaseFieldExtensions.SetLabel\<SerializableTypeBase, TValue\>\(SerializableTypeBase, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_),
[SliderExtensions.SetLowValue\<SerializableTypeBase, TValue\>\(SerializableTypeBase, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_),
[TextInputBaseFieldExtensions.SetMaskChar\<SerializableTypeBase, TValue\>\(SerializableTypeBase, char\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetMaskChar__2___0_System_Char_),
[TextInputBaseFieldExtensions.SetMaxLength\<SerializableTypeBase, TValue\>\(SerializableTypeBase, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetMaxLength__2___0_System_Int32_),
[TextInputBaseFieldExtensions.SetPassword\<SerializableTypeBase, TValue\>\(SerializableTypeBase, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetPassword__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetPlaceholder\<SerializableTypeBase, TValue\>\(SerializableTypeBase, string\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetPlaceholder__2___0_System_String_),
[TextInputBaseFieldExtensions.SetReadOnly\<SerializableTypeBase, TValue\>\(SerializableTypeBase, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetReadOnly__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectAllOnFocus\<SerializableTypeBase, TValue\>\(SerializableTypeBase, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectAllOnFocus__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectAllOnMouseUp\<SerializableTypeBase, TValue\>\(SerializableTypeBase, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectAllOnMouseUp__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectIndex\<SerializableTypeBase, TValue\>\(SerializableTypeBase, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectIndex__2___0_System_Int32_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectable\<SerializableTypeBase, TValue\>\(SerializableTypeBase, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectable__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetTripleClickSelectsLine\<SerializableTypeBase, TValue\>\(SerializableTypeBase, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetTripleClickSelectsLine__2___0_System_Boolean_),
[INotifyValueChangedExtensions.SetValue\<SerializableTypeBase, TValue\>\(SerializableTypeBase, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Remarks

<p>
Not meant to be derived from outside the package — use [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) or
[`SerializableMonoScript`](Aspid.FastTools.Types.SerializableMonoScript.md). Unity serializes the name under the same field for all of them,
so every wrapper shares one serialized layout.
</p>
<p>
A player resolves the type by the stored name only, which managed code stripping does not see: from
Managed Stripping Level Low up, a class referenced only by this name can be removed from the build and
[`SerializableTypeBase.Type`](Aspid.FastTools.Types.SerializableTypeBase.md#Aspid_FastTools_Types_SerializableTypeBase_Type) returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>. Keep such classes with <code>[Preserve]</code> or <code>link.xml</code>.
</p>
<p>
A failed lookup is cached until the stored name changes or the object is deserialized again, so an assembly
loaded later is not picked up before that.
</p>
<p>
Two wrappers are equal when they store the same assembly-qualified name, whatever their wrapper class or
constraint, so they work as dictionary keys. <code>==</code> still compares references. The hash code follows the
stored name: do not change a wrapper that is a key, which includes deserializing it again.
</p>

## Properties

### AssemblyQualifiedName {#Aspid_FastTools_Types_SerializableTypeBase_AssemblyQualifiedName}

Gets the stored assembly-qualified type name, or an empty string when no type is stored.

```csharp
public string AssemblyQualifiedName { get; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Remarks

Kept even when it no longer resolves, so the Inspector can show what the field used to point at.

### BaseType {#Aspid_FastTools_Types_SerializableTypeBase_BaseType}

Gets the base type that types offered by the picker are assignable to; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public abstract Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

#### Remarks

A loaded type is not checked against it: a name stored before the constraint changed resolves as is.

### Type {#Aspid_FastTools_Types_SerializableTypeBase_Type}

Gets the resolved type, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when no type is stored or its stored name cannot be resolved.

```csharp
public Type? Type { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)?

## Methods

### Equals\(SerializableTypeBase?\) {#Aspid_FastTools_Types_SerializableTypeBase_Equals_Aspid_FastTools_Types_SerializableTypeBase_}

Determines whether <code class="paramref">other</code> stores the same assembly-qualified name.

```csharp
public bool Equals(SerializableTypeBase? other)
```

#### Parameters

`other` [SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md)?

The wrapper to compare with.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if <code class="paramref">other</code> is not <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> and stores the same
name, also when neither stores a type; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

### Equals\(object?\) {#Aspid_FastTools_Types_SerializableTypeBase_Equals_System_Object_}

Determines whether <code class="paramref">obj</code> is a wrapper that stores the same assembly-qualified name.

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The object to compare with.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if [`SerializableTypeBase.Equals`](Aspid.FastTools.Types.SerializableTypeBase.md#Aspid_FastTools_Types_SerializableTypeBase_Equals_Aspid_FastTools_Types_SerializableTypeBase_) is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> for
<code class="paramref">obj</code>; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

### GetHashCode\(\) {#Aspid_FastTools_Types_SerializableTypeBase_GetHashCode}

Returns a hash code of the stored assembly-qualified name.

```csharp
public override int GetHashCode()
```

#### Returns

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString\(\) {#Aspid_FastTools_Types_SerializableTypeBase_ToString}

Returns the short name of the resolved type, the stored name when it cannot be resolved,
or an empty string when no type is stored.

```csharp
public override string ToString()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

