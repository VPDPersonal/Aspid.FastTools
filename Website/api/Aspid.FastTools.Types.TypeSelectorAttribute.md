---
title: "Class TypeSelectorAttribute"
sidebar_label: "TypeSelectorAttribute"
description: "Class TypeSelectorAttribute — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class TypeSelectorAttribute {#Aspid_FastTools_Types_TypeSelectorAttribute}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

Instructs the Unity Editor to use the type-selector window.

```csharp
[Conditional("UNITY_EDITOR")]
public sealed class TypeSelectorAttribute : PropertyAttribute
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[Attribute](https://learn.microsoft.com/dotnet/api/system.attribute) ← 
PropertyAttribute ← 
[TypeSelectorAttribute](Aspid.FastTools.Types.TypeSelectorAttribute.md)


#### Extension Methods

[INotifyValueChangedExtensions.AddValueChanged\<TypeSelectorAttribute, TValue\>\(TypeSelectorAttribute, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<TypeSelectorAttribute, TValue\>\(TypeSelectorAttribute, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[SliderExtensions.SetHighValue\<TypeSelectorAttribute, TValue\>\(TypeSelectorAttribute, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[BaseFieldExtensions.SetLabel\<TypeSelectorAttribute, TValue\>\(TypeSelectorAttribute, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[SliderExtensions.SetLowValue\<TypeSelectorAttribute, TValue\>\(TypeSelectorAttribute, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[INotifyValueChangedExtensions.SetValue\<TypeSelectorAttribute, TValue\>\(TypeSelectorAttribute, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Examples

Constrain to a single base type:


```csharp
[TypeSelector(typeof(MonoBehaviour))]
[SerializeField] private string _behaviourType;
```


Accept any type (unconstrained):


```csharp
[TypeSelector]
[SerializeField] private string _anyType;
```


Constrain to the intersection of several base types:


```csharp
[TypeSelector(typeof(IDisposable), typeof(ScriptableObject))]
[SerializeField] private string _type;
```


## Remarks

One or more base types can be supplied; the picker shows only types
assignable to <b>all</b> of them.

## Constructors

### TypeSelectorAttribute\(\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor}

Creates an unconstrained attribute (base type is [`Object`](https://learn.microsoft.com/dotnet/api/system.object)).

```csharp
public TypeSelectorAttribute()
```

### TypeSelectorAttribute\(Type\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor_System_Type_}

Creates an attribute constrained to a single base type.

```csharp
public TypeSelectorAttribute(Type type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The base constraint type.

### TypeSelectorAttribute\(params Type\[\]\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor_System_Type___}

Creates an attribute constrained to one or more base types.

```csharp
public TypeSelectorAttribute(params Type[] types)
```

#### Parameters

`types` [Type](https://learn.microsoft.com/dotnet/api/system.type)\[\]

The base constraint types.

### TypeSelectorAttribute\(string\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor_System_String_}

Creates an attribute constrained to a single base type named by a string.

```csharp
public TypeSelectorAttribute(string assemblyQualifiedName)
```

#### Parameters

`assemblyQualifiedName` [string](https://learn.microsoft.com/dotnet/api/system.string)

Either an <b>assembly-qualified type name</b> (e.g. <code>"MyGame.IWeapon, MyGame"</code>) resolved with
[`GetType`](https://learn.microsoft.com/dotnet/api/system.type.gettype), or the <b>name of a member</b> on the same object that supplies
the constraint at inspector time — see the constructor remarks.

#### Examples


```csharp
// Constrain the picker to the value of another field, resolved live:
[SerializeField] private SerializableType _category;

[TypeSelector(nameof(_category))]
[SerializeField] private string _subType;
```


#### Remarks

Resolved <b>member-first</b>: if the string is a C# identifier matching an instance field or property on the
target object, that member's current value supplies the base type(s) — so the constraint can be driven live by
another field. Otherwise, it is treated as an assembly-qualified type name.

<p>
A member may be a [`Type`](https://learn.microsoft.com/dotnet/api/system.type), <code>string</code> (assembly-qualified name),
[`SerializableType`](Aspid.FastTools.Types.SerializableType.md) / [`SerializableType<T>`](Aspid.FastTools.Types.SerializableType-1.md), or an array of these.
Prefer <code>nameof(...)</code> so a rename keeps the reference intact; use the
[`TypeSelectorAttribute.%23ctor`](Aspid.FastTools.Types.TypeSelectorAttribute.md) overload when <code>typeof(...)</code> is possible.
</p>

A name that resolves to nothing is surfaced as an inline inspector notice.

### TypeSelectorAttribute\(params string\[\]\) {#Aspid_FastTools_Types_TypeSelectorAttribute__ctor_System_String___}

Creates an attribute constrained to one or more base types, each named by a string.

```csharp
public TypeSelectorAttribute(params string[] assemblyQualifiedNames)
```

#### Parameters

`assemblyQualifiedNames` [string](https://learn.microsoft.com/dotnet/api/system.string)\[\]

Each entry is resolved independently, member-first: an identifier matching an instance field/property on the
target object supplies its value as a constraint, otherwise the entry is an assembly-qualified type name. See
[`TypeSelectorAttribute.%23ctor`](Aspid.FastTools.Types.TypeSelectorAttribute.md) for the full contract and the supported member types.

## Fields

### AssemblyQualifiedNames {#Aspid_FastTools_Types_TypeSelectorAttribute_AssemblyQualifiedNames}

The assembly-qualified names of the base types that constrain the selection.

```csharp
public readonly string[] AssemblyQualifiedNames
```

#### Field Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)\[\]

## Properties

### Allow {#Aspid_FastTools_Types_TypeSelectorAttribute_Allow}

Which special type categories (abstract classes, interfaces) the picker should
include in addition to plain concrete classes. Defaults to [`TypeAllow.All`](Aspid.FastTools.Types.TypeAllow.md),
so a type-name field (a <code>string</code> or a [`SerializableType`](Aspid.FastTools.Types.SerializableType.md)) offers abstract
classes and interfaces too — set [`TypeAllow.None`](Aspid.FastTools.Types.TypeAllow.md) to restrict it to concrete types.
Ignored on a <code>[SerializeReference]</code> managed reference: that path always lists only
concrete, instantiable types regardless of this value.

```csharp
public TypeAllow Allow { get; set; }
```

#### Property Value

 [TypeAllow](Aspid.FastTools.Types.TypeAllow.md)

### Required {#Aspid_FastTools_Types_TypeSelectorAttribute_Required}

Requires the field to hold a value. When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, an unset field shows an inline "required"
warning in the inspector and counts as a violation for the build/CI gate. Defaults to <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>
(the field may be left empty).

```csharp
public bool Required { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Examples


```csharp
[TypeSelector(typeof(IWeapon), Required = true)]
[SerializeReference] private IWeapon _weapon;

[TypeSelector(typeof(MonoBehaviour), Required = true)]
[SerializeField] private string _behaviourType;
```


#### Remarks

What counts as "unset" depends on the field shape this attribute drives:

<ul><li>a <code>[SerializeReference]</code> managed reference — unset when it is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>;</li><li>a <code>string</code> field (assembly-qualified name) — unset when it is empty;</li><li>a [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) / [`SerializableType<T>`](Aspid.FastTools.Types.SerializableType-1.md) field — unset when its stored type name is empty.</li></ul>

A managed reference that <i>is</i> set but whose type can no longer be resolved (renamed or deleted class) is
<b>not</b> a <code>Required</code> violation: that broken-data case is handled by the separate missing-type
notice/gate, which fires regardless of this flag.

