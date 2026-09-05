---
title: "Class SerializePropertyExtensions"
sidebar_label: "SerializePropertyExtensions"
description: "Class SerializePropertyExtensions — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializePropertyExtensions {#Aspid_FastTools_Editors_SerializePropertyExtensions}

Namespace: [Aspid.FastTools.Editors](Aspid.FastTools.Editors.md)  
Assembly: Aspid.FastTools.Unity.Editor.dll  

Fluent extension methods for [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) providing chainable wrappers
around [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html) synchronization and typed value setters.

```csharp
public static class SerializePropertyExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializePropertyExtensions](Aspid.FastTools.Editors.SerializePropertyExtensions.md)


#### Extension Methods

[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_)

## Methods

### AddArraySize\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_AddArraySize__1___0_System_Int32_}

Increases [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) by <code class="paramref">value</code> and returns the property for chaining.

```csharp
public static T AddArraySize<T>(this T property, int value = 1) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Amount to add to the current array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### AddArraySizeAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_AddArraySizeAndApply__1___0_System_Int32_}

Increases [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) by <code class="paramref">value</code> then applies modified properties.

```csharp
public static T AddArraySizeAndApply<T>(this T property, int value = 1) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Amount to add to the current array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### ApplyModifiedProperties\<T\>\(T\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_ApplyModifiedProperties__1___0_}

Calls [`ApplyModifiedProperties`](https://docs.unity3d.com/ScriptReference/SerializedObject-ApplyModifiedProperties.html) on the property's serialized object and returns the property for chaining.

```csharp
public static T ApplyModifiedProperties<T>(this T property) where T : SerializedProperty
```

#### Parameters

`property` T

The property whose serialized object changes should be applied.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### ApplyModifiedPropertiesWithoutUndo\<T\>\(T\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_ApplyModifiedPropertiesWithoutUndo__1___0_}

Calls [`ApplyModifiedPropertiesWithoutUndo`](https://docs.unity3d.com/ScriptReference/SerializedObject-ApplyModifiedPropertiesWithoutUndo.html) on the property's serialized object and returns the property for chaining.

```csharp
public static T ApplyModifiedPropertiesWithoutUndo<T>(this T property) where T : SerializedProperty
```

#### Parameters

`property` T

The property whose serialized object changes should be applied without registering an undo step.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### GetDeclaringInstance\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_GetDeclaringInstance_UnityEditor_SerializedProperty_}

Traverses the [`propertyPath`](https://docs.unity3d.com/ScriptReference/SerializedProperty-propertyPath.html) to return the runtime object on which the
property's backing field is declared — the direct container, not the root <code>targetObject</code>.
For an array/list element property the instance owning the collection field is returned.

```csharp
public static object GetDeclaringInstance(this SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

The property whose declaring instance should be resolved.

#### Returns

 [object](https://learn.microsoft.com/dotnet/api/system.object)

The instance declaring the property's backing field (the root <code>targetObject</code> for a top-level property),
or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if the path cannot be resolved.

#### Examples

For <code>_inventory._slots.Array.data[2]._weapon</code> the returned instance is the slot element
<code>_slots[2]</code> — the object whose class declares the <code>_weapon</code> field:


```csharp
var slot = property.GetDeclaringInstance() as InventorySlot;
```


#### Remarks

When the declaring instance is a struct, the returned object is a boxed <b>copy</b> — mutating it does not
affect the serialized object. Any resolution failure (missing field, a <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> value
or an out-of-range element index along the path) returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

### GetFieldInfo\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_GetFieldInfo_UnityEditor_SerializedProperty_}

Resolves the [`FieldInfo`](https://learn.microsoft.com/dotnet/api/system.reflection.fieldinfo) that backs this [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html),
looked up on the runtime type of the property's declaring instance (see [`SerializePropertyExtensions.GetDeclaringInstance`](Aspid.FastTools.Editors.SerializePropertyExtensions.md#Aspid_FastTools_Editors_SerializePropertyExtensions_GetDeclaringInstance_UnityEditor_SerializedProperty_)).

```csharp
public static FieldInfo GetFieldInfo(this SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

The property whose backing field should be located.

#### Returns

 [FieldInfo](https://learn.microsoft.com/dotnet/api/system.reflection.fieldinfo)

The resolved [`FieldInfo`](https://learn.microsoft.com/dotnet/api/system.reflection.fieldinfo), or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if it cannot be found.

#### Remarks

Base classes are searched too. For a list/array element the collection field itself is returned
(matching <code>PropertyDrawer.fieldInfo</code>); a <code>[SerializeReference]</code> segment resolves naturally
through the live managed reference's runtime type.

### GetMemberName\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_GetMemberName_UnityEditor_SerializedProperty_}

The name of the member that backs <code class="paramref">property</code>.

```csharp
public static string GetMemberName(this SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Remarks

Equals [`name`](https://docs.unity3d.com/ScriptReference/SerializedProperty-name.html) for a regular field, but differs for an array/list element:
its path ends with <code>Array.data[i]</code>, so <code>name</code> is just <code>data</code> — here <code>_slots.Array.data[0]</code>
yields the collection field's name <code>_slots</code> instead.

### GetPropertyType\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_GetPropertyType_UnityEditor_SerializedProperty_}

Returns the [`Type`](https://learn.microsoft.com/dotnet/api/system.type) of the field that backs this [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html).
For an array/list element property the element type is returned.

```csharp
public static Type GetPropertyType(this SerializedProperty serializedProperty)
```

#### Parameters

`serializedProperty` SerializedProperty

The property to inspect.

#### Returns

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

The [`FieldType`](https://learn.microsoft.com/dotnet/api/system.reflection.fieldinfo.fieldtype) of the backing field
(the element type when the property is an array/list element),
or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if the field cannot be resolved.

### HasFoldout\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_HasFoldout_UnityEditor_SerializedProperty_}

Returns <code>true</code> when the property is a [`Generic`](https://docs.unity3d.com/ScriptReference/SerializedPropertyType-Generic.html)
value (a plain serializable struct/class) with visible children — i.e. draws as an
expandable foldout. Single-line values return <code>false</code>.

```csharp
public static bool HasFoldout(this SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

The property whose drawing shape is being queried.

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

<code>true</code> if the property draws with a foldout arrow; otherwise <code>false</code>.

#### Remarks

Managed references ([`ManagedReference`](https://docs.unity3d.com/ScriptReference/SerializedPropertyType-ManagedReference.html)) also render
with a foldout but are not covered by this check.

### IsArrayElement\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_IsArrayElement_UnityEditor_SerializedProperty_}

True when <code class="paramref">property</code> is an element of an array or [`List<T>`](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)
— its [`propertyPath`](https://docs.unity3d.com/ScriptReference/SerializedProperty-propertyPath.html) ends with an <code>Array.data[i]</code>
segment, e.g. <code>_slots.Array.data[0]</code>.

```csharp
public static bool IsArrayElement(this SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Remarks

Not to be confused with [`isArray`](https://docs.unity3d.com/ScriptReference/SerializedProperty-isArray.html), which is true for the
collection itself (<code>_slots</code>), not for a single element inside it.

### Persistent\(SerializedProperty\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_Persistent_UnityEditor_SerializedProperty_}

Returns a copy of the property backed by its own new [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html),
independent of the source and safe to store.

```csharp
public static SerializedProperty Persistent(this SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

Source property — its [`propertyPath`](https://docs.unity3d.com/ScriptReference/SerializedProperty-propertyPath.html) is reused.

#### Returns

 SerializedProperty

A new [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) at the same path on a new [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html).

#### Remarks

Use when the property must outlive the original [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html),
e.g. cached on a long-lived UIToolkit element or captured in a deferred callback.
The new [`SerializedObject`](https://docs.unity3d.com/ScriptReference/SerializedObject.html) is owned by the caller and is never disposed by this method.
The copy reads the target's current serialized state: changes pending on the source
without [`ApplyModifiedProperties`](https://docs.unity3d.com/ScriptReference/SerializedObject-ApplyModifiedProperties.html) are not visible to it.

### RemoveArraySize\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_RemoveArraySize__1___0_System_Int32_}

Decreases [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) by <code class="paramref">value</code> and returns the property for chaining.

```csharp
public static T RemoveArraySize<T>(this T property, int value = 1) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Amount to subtract from the current array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### RemoveArraySizeAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_RemoveArraySizeAndApply__1___0_System_Int32_}

Decreases [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) by <code class="paramref">value</code> then applies modified properties.

```csharp
public static T RemoveArraySizeAndApply<T>(this T property, int value = 1) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Amount to subtract from the current array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetAnimationCurve\<T\>\(T, AnimationCurve\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetAnimationCurve__1___0_UnityEngine_AnimationCurve_}

Sets [`animationCurveValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-animationCurveValue.html) and returns the property for chaining.

```csharp
public static T SetAnimationCurve<T>(this T property, AnimationCurve value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` AnimationCurve

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetAnimationCurveAndApply\<T\>\(T, AnimationCurve\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetAnimationCurveAndApply__1___0_UnityEngine_AnimationCurve_}

Sets [`animationCurveValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-animationCurveValue.html) then applies modified properties.

```csharp
public static T SetAnimationCurveAndApply<T>(this T property, AnimationCurve value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` AnimationCurve

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetArraySize\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetArraySize__1___0_System_Int32_}

Sets [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) and returns the property for chaining.

```csharp
public static T SetArraySize<T>(this T property, int size) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`size` [int](https://learn.microsoft.com/dotnet/api/system.int32)

New array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetArraySizeAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetArraySizeAndApply__1___0_System_Int32_}

Sets [`arraySize`](https://docs.unity3d.com/ScriptReference/SerializedProperty-arraySize.html) then applies modified properties.

```csharp
public static T SetArraySizeAndApply<T>(this T property, int size) where T : SerializedProperty
```

#### Parameters

`property` T

Target array property.

`size` [int](https://learn.microsoft.com/dotnet/api/system.int32)

New array size.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBool\<T\>\(T, bool\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBool__1___0_System_Boolean_}

Sets [`boolValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boolValue.html) and returns the property for chaining.

```csharp
public static T SetBool<T>(this T property, bool value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoolAndApply\<T\>\(T, bool\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoolAndApply__1___0_System_Boolean_}

Sets [`boolValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boolValue.html) then applies modified properties.

```csharp
public static T SetBoolAndApply<T>(this T property, bool value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBounds\<T\>\(T, Bounds\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBounds__1___0_UnityEngine_Bounds_}

Sets [`boundsValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsValue.html) and returns the property for chaining.

```csharp
public static T SetBounds<T>(this T property, Bounds value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Bounds

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoundsAndApply\<T\>\(T, Bounds\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoundsAndApply__1___0_UnityEngine_Bounds_}

Sets [`boundsValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsValue.html) then applies modified properties.

```csharp
public static T SetBoundsAndApply<T>(this T property, Bounds value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Bounds

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoundsInt\<T\>\(T, BoundsInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoundsInt__1___0_UnityEngine_BoundsInt_}

Sets [`boundsIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsIntValue.html) and returns the property for chaining.

```csharp
public static T SetBoundsInt<T>(this T property, BoundsInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` BoundsInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoundsIntAndApply\<T\>\(T, BoundsInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoundsIntAndApply__1___0_UnityEngine_BoundsInt_}

Sets [`boundsIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsIntValue.html) then applies modified properties.

```csharp
public static T SetBoundsIntAndApply<T>(this T property, BoundsInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` BoundsInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoxed\<T\>\(T, object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoxed__1___0_System_Object_}

Sets [`boxedValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boxedValue.html) and returns the property for chaining.

```csharp
public static T SetBoxed<T>(this T property, object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [object](https://learn.microsoft.com/dotnet/api/system.object)

Boxed value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetBoxedAndApply\<T\>\(T, object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetBoxedAndApply__1___0_System_Object_}

Sets [`boxedValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boxedValue.html) then applies modified properties.

```csharp
public static T SetBoxedAndApply<T>(this T property, object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [object](https://learn.microsoft.com/dotnet/api/system.object)

Boxed value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetColor\<T\>\(T, Color\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetColor__1___0_UnityEngine_Color_}

Sets [`colorValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-colorValue.html) and returns the property for chaining.

```csharp
public static T SetColor<T>(this T property, Color value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Color

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetColorAndApply\<T\>\(T, Color\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetColorAndApply__1___0_UnityEngine_Color_}

Sets [`colorValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-colorValue.html) then applies modified properties.

```csharp
public static T SetColorAndApply<T>(this T property, Color value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Color

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetDouble\<T\>\(T, double\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetDouble__1___0_System_Double_}

Sets [`doubleValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-doubleValue.html) and returns the property for chaining.

```csharp
public static T SetDouble<T>(this T property, double value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [double](https://learn.microsoft.com/dotnet/api/system.double)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetDoubleAndApply\<T\>\(T, double\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetDoubleAndApply__1___0_System_Double_}

Sets [`doubleValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-doubleValue.html) then applies modified properties.

```csharp
public static T SetDoubleAndApply<T>(this T property, double value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [double](https://learn.microsoft.com/dotnet/api/system.double)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetEntityId\<T\>\(T, EntityId\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEntityId__1___0_UnityEngine_EntityId_}

Sets [`entityIdValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-entityIdValue.html) and returns the property for chaining.

```csharp
public static T SetEntityId<T>(this T property, EntityId value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` EntityId

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetEntityIdAndApply\<T\>\(T, EntityId\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEntityIdAndApply__1___0_UnityEngine_EntityId_}

Sets [`entityIdValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-entityIdValue.html) then applies modified properties.

```csharp
public static T SetEntityIdAndApply<T>(this T property, EntityId value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` EntityId

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetEnumFlag\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEnumFlag__1___0_System_Int32_}

Sets [`enumValueFlag`](https://docs.unity3d.com/ScriptReference/SerializedProperty-enumValueFlag.html) and returns the property for chaining.

```csharp
public static T SetEnumFlag<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Flag value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

#### Remarks

There is no <code>SetValue&lt;T&gt;(int)</code> alias for enum flags because it would conflict with
[`SerializePropertyExtensions.SetInt%60<T>`](Aspid.FastTools.Editors.SerializePropertyExtensions.md). Call [`SerializePropertyExtensions.SetEnumFlag%60<T>`](Aspid.FastTools.Editors.SerializePropertyExtensions.md) explicitly.

### SetEnumFlagAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEnumFlagAndApply__1___0_System_Int32_}

Sets [`enumValueFlag`](https://docs.unity3d.com/ScriptReference/SerializedProperty-enumValueFlag.html) then applies modified properties.

```csharp
public static T SetEnumFlagAndApply<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Flag value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

#### Remarks

There is no <code>SetValue&lt;T&gt;(int)</code> alias for enum flags because it would conflict with
[`SerializePropertyExtensions.SetInt%60<T>`](Aspid.FastTools.Editors.SerializePropertyExtensions.md). Call [`SerializePropertyExtensions.SetEnumFlag%60<T>`](Aspid.FastTools.Editors.SerializePropertyExtensions.md) explicitly.

### SetEnumIndex\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEnumIndex__1___0_System_Int32_}

Sets [`enumValueIndex`](https://docs.unity3d.com/ScriptReference/SerializedProperty-enumValueIndex.html) and returns the property for chaining.

```csharp
public static T SetEnumIndex<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Index value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

#### Remarks

There is no <code>SetValue&lt;T&gt;(int)</code> alias for enum index because it would conflict with
[`SerializePropertyExtensions.SetInt%60<T>`](Aspid.FastTools.Editors.SerializePropertyExtensions.md). Call [`SerializePropertyExtensions.SetEnumIndex%60<T>`](Aspid.FastTools.Editors.SerializePropertyExtensions.md) explicitly.

### SetEnumIndexAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetEnumIndexAndApply__1___0_System_Int32_}

Sets [`enumValueIndex`](https://docs.unity3d.com/ScriptReference/SerializedProperty-enumValueIndex.html) then applies modified properties.

```csharp
public static T SetEnumIndexAndApply<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Index value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

#### Remarks

There is no <code>SetValue&lt;T&gt;(int)</code> alias for enum index because it would conflict with
[`SerializePropertyExtensions.SetInt%60<T>`](Aspid.FastTools.Editors.SerializePropertyExtensions.md). Call [`SerializePropertyExtensions.SetEnumIndex%60<T>`](Aspid.FastTools.Editors.SerializePropertyExtensions.md) explicitly.

### SetExposedReference\<T\>\(T, Object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetExposedReference__1___0_UnityEngine_Object_}

Sets [`exposedReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-exposedReferenceValue.html) and returns the property for chaining.

```csharp
public static T SetExposedReference<T>(this T property, Object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Object

[`Object`](https://docs.unity3d.com/ScriptReference/Object.html) exposed reference to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetExposedReferenceAndApply\<T\>\(T, Object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetExposedReferenceAndApply__1___0_UnityEngine_Object_}

Sets [`exposedReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-exposedReferenceValue.html) then applies modified properties.

```csharp
public static T SetExposedReferenceAndApply<T>(this T property, Object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Object

[`Object`](https://docs.unity3d.com/ScriptReference/Object.html) exposed reference to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetFloat\<T\>\(T, float\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetFloat__1___0_System_Single_}

Sets [`floatValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-floatValue.html) and returns the property for chaining.

```csharp
public static T SetFloat<T>(this T property, float value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetFloatAndApply\<T\>\(T, float\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetFloatAndApply__1___0_System_Single_}

Sets [`floatValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-floatValue.html) then applies modified properties.

```csharp
public static T SetFloatAndApply<T>(this T property, float value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetGradient\<T\>\(T, Gradient\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetGradient__1___0_UnityEngine_Gradient_}

Sets [`gradientValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-gradientValue.html) and returns the property for chaining.

```csharp
public static T SetGradient<T>(this T property, Gradient value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Gradient

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetGradientAndApply\<T\>\(T, Gradient\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetGradientAndApply__1___0_UnityEngine_Gradient_}

Sets [`gradientValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-gradientValue.html) then applies modified properties.

```csharp
public static T SetGradientAndApply<T>(this T property, Gradient value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Gradient

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetHash128\<T\>\(T, Hash128\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetHash128__1___0_UnityEngine_Hash128_}

Sets [`hash128Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-hash128Value.html) and returns the property for chaining.

```csharp
public static T SetHash128<T>(this T property, Hash128 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Hash128

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetHash128AndApply\<T\>\(T, Hash128\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetHash128AndApply__1___0_UnityEngine_Hash128_}

Sets [`hash128Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-hash128Value.html) then applies modified properties.

```csharp
public static T SetHash128AndApply<T>(this T property, Hash128 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Hash128

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetInt\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetInt__1___0_System_Int32_}

Sets [`intValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-intValue.html) and returns the property for chaining.

```csharp
public static T SetInt<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetIntAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetIntAndApply__1___0_System_Int32_}

Sets [`intValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-intValue.html) then applies modified properties.

```csharp
public static T SetIntAndApply<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetLong\<T\>\(T, long\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetLong__1___0_System_Int64_}

Sets [`longValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-longValue.html) and returns the property for chaining.

```csharp
public static T SetLong<T>(this T property, long value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetLongAndApply\<T\>\(T, long\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetLongAndApply__1___0_System_Int64_}

Sets [`longValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-longValue.html) then applies modified properties.

```csharp
public static T SetLongAndApply<T>(this T property, long value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetManagedReference\<T\>\(T, object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetManagedReference__1___0_System_Object_}

Sets [`managedReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-managedReferenceValue.html) and returns the property for chaining.

```csharp
public static T SetManagedReference<T>(this T property, object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property (must be a <code>[SerializeReference]</code> field).

`value` [object](https://learn.microsoft.com/dotnet/api/system.object)

Managed reference value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetManagedReferenceAndApply\<T\>\(T, object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetManagedReferenceAndApply__1___0_System_Object_}

Sets [`managedReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-managedReferenceValue.html) then applies modified properties.

```csharp
public static T SetManagedReferenceAndApply<T>(this T property, object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property (must be a <code>[SerializeReference]</code> field).

`value` [object](https://learn.microsoft.com/dotnet/api/system.object)

Managed reference value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetObjectReference\<T\>\(T, Object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetObjectReference__1___0_UnityEngine_Object_}

Sets [`objectReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-objectReferenceValue.html) and returns the property for chaining.

```csharp
public static T SetObjectReference<T>(this T property, Object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Object

[`Object`](https://docs.unity3d.com/ScriptReference/Object.html) reference to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetObjectReferenceAndApply\<T\>\(T, Object\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetObjectReferenceAndApply__1___0_UnityEngine_Object_}

Sets [`objectReferenceValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-objectReferenceValue.html) then applies modified properties.

```csharp
public static T SetObjectReferenceAndApply<T>(this T property, Object value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Object

[`Object`](https://docs.unity3d.com/ScriptReference/Object.html) reference to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetQuaternion\<T\>\(T, Quaternion\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetQuaternion__1___0_UnityEngine_Quaternion_}

Sets [`quaternionValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-quaternionValue.html) and returns the property for chaining.

```csharp
public static T SetQuaternion<T>(this T property, Quaternion value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Quaternion

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetQuaternionAndApply\<T\>\(T, Quaternion\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetQuaternionAndApply__1___0_UnityEngine_Quaternion_}

Sets [`quaternionValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-quaternionValue.html) then applies modified properties.

```csharp
public static T SetQuaternionAndApply<T>(this T property, Quaternion value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Quaternion

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetRect\<T\>\(T, Rect\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetRect__1___0_UnityEngine_Rect_}

Sets [`rectValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectValue.html) and returns the property for chaining.

```csharp
public static T SetRect<T>(this T property, Rect value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Rect

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetRectAndApply\<T\>\(T, Rect\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetRectAndApply__1___0_UnityEngine_Rect_}

Sets [`rectValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectValue.html) then applies modified properties.

```csharp
public static T SetRectAndApply<T>(this T property, Rect value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Rect

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetRectInt\<T\>\(T, RectInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetRectInt__1___0_UnityEngine_RectInt_}

Sets [`rectIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectIntValue.html) and returns the property for chaining.

```csharp
public static T SetRectInt<T>(this T property, RectInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` RectInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetRectIntAndApply\<T\>\(T, RectInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetRectIntAndApply__1___0_UnityEngine_RectInt_}

Sets [`rectIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectIntValue.html) then applies modified properties.

```csharp
public static T SetRectIntAndApply<T>(this T property, RectInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` RectInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetString\<T\>\(T, string\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetString__1___0_System_String_}

Sets [`stringValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-stringValue.html) and returns the property for chaining.

```csharp
public static T SetString<T>(this T property, string value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetStringAndApply\<T\>\(T, string\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetStringAndApply__1___0_System_String_}

Sets [`stringValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-stringValue.html) then applies modified properties.

```csharp
public static T SetStringAndApply<T>(this T property, string value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetUint\<T\>\(T, uint\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetUint__1___0_System_UInt32_}

Sets [`uintValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-uintValue.html) and returns the property for chaining.

```csharp
public static T SetUint<T>(this T property, uint value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [uint](https://learn.microsoft.com/dotnet/api/system.uint32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetUintAndApply\<T\>\(T, uint\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetUintAndApply__1___0_System_UInt32_}

Sets [`uintValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-uintValue.html) then applies modified properties.

```csharp
public static T SetUintAndApply<T>(this T property, uint value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [uint](https://learn.microsoft.com/dotnet/api/system.uint32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetUlong\<T\>\(T, ulong\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetUlong__1___0_System_UInt64_}

Sets [`ulongValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-ulongValue.html) and returns the property for chaining.

```csharp
public static T SetUlong<T>(this T property, ulong value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetUlongAndApply\<T\>\(T, ulong\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetUlongAndApply__1___0_System_UInt64_}

Sets [`ulongValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-ulongValue.html) then applies modified properties.

```csharp
public static T SetUlongAndApply<T>(this T property, ulong value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_Int32_}

Sets [`intValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-intValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, uint\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_UInt32_}

Sets [`uintValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-uintValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, uint value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [uint](https://learn.microsoft.com/dotnet/api/system.uint32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, long\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_Int64_}

Sets [`longValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-longValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, long value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, ulong\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_UInt64_}

Sets [`ulongValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-ulongValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, ulong value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, float\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_Single_}

Sets [`floatValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-floatValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, float value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, double\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_Double_}

Sets [`doubleValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-doubleValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, double value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [double](https://learn.microsoft.com/dotnet/api/system.double)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, bool\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_Boolean_}

Sets [`boolValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boolValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, bool value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Rect\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Rect_}

Sets [`rectValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Rect value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Rect

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, RectInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_RectInt_}

Sets [`rectIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectIntValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, RectInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` RectInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Bounds\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Bounds_}

Sets [`boundsValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Bounds value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Bounds

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, BoundsInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_BoundsInt_}

Sets [`boundsIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsIntValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, BoundsInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` BoundsInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Color\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Color_}

Sets [`colorValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-colorValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Color value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Color

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Gradient\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Gradient_}

Sets [`gradientValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-gradientValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Gradient value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Gradient

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Hash128\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Hash128_}

Sets [`hash128Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-hash128Value.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Hash128 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Hash128

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Vector4\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Vector4_}

Sets [`vector4Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector4Value.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Vector4 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector4

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Vector3\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Vector3_}

Sets [`vector3Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3Value.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Vector3 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Vector3Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Vector3Int_}

Sets [`vector3IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3IntValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Vector3Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Vector2\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Vector2_}

Sets [`vector2Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2Value.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Vector2 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Vector2Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Vector2Int_}

Sets [`vector2IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2IntValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Vector2Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, Quaternion\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_Quaternion_}

Sets [`quaternionValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-quaternionValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, Quaternion value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Quaternion

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, string\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_System_String_}

Sets [`stringValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-stringValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, string value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, AnimationCurve\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_AnimationCurve_}

Sets [`animationCurveValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-animationCurveValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, AnimationCurve value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` AnimationCurve

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValue\<T\>\(T, EntityId\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValue__1___0_UnityEngine_EntityId_}

Sets [`entityIdValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-entityIdValue.html) and returns the property for chaining.

```csharp
public static T SetValue<T>(this T property, EntityId value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` EntityId

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_Int32_}

Sets [`intValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-intValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, uint\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_UInt32_}

Sets [`uintValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-uintValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, uint value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [uint](https://learn.microsoft.com/dotnet/api/system.uint32)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, long\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_Int64_}

Sets [`longValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-longValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, long value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [long](https://learn.microsoft.com/dotnet/api/system.int64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, ulong\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_UInt64_}

Sets [`ulongValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-ulongValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, ulong value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, float\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_Single_}

Sets [`floatValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-floatValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, float value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [float](https://learn.microsoft.com/dotnet/api/system.single)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, double\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_Double_}

Sets [`doubleValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-doubleValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, double value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [double](https://learn.microsoft.com/dotnet/api/system.double)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, bool\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_Boolean_}

Sets [`boolValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boolValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, bool value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Rect\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Rect_}

Sets [`rectValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Rect value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Rect

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, RectInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_RectInt_}

Sets [`rectIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-rectIntValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, RectInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` RectInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Bounds\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Bounds_}

Sets [`boundsValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Bounds value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Bounds

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, BoundsInt\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_BoundsInt_}

Sets [`boundsIntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-boundsIntValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, BoundsInt value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` BoundsInt

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Color\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Color_}

Sets [`colorValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-colorValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Color value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Color

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Gradient\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Gradient_}

Sets [`gradientValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-gradientValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Gradient value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Gradient

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Hash128\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Hash128_}

Sets [`hash128Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-hash128Value.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Hash128 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Hash128

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Vector4\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Vector4_}

Sets [`vector4Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector4Value.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Vector4 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector4

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Vector3\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Vector3_}

Sets [`vector3Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3Value.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Vector3 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Vector3Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Vector3Int_}

Sets [`vector3IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3IntValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Vector3Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Vector2\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Vector2_}

Sets [`vector2Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2Value.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Vector2 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Vector2Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Vector2Int_}

Sets [`vector2IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2IntValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Vector2Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, Quaternion\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_Quaternion_}

Sets [`quaternionValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-quaternionValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, Quaternion value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Quaternion

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, string\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_System_String_}

Sets [`stringValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-stringValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, string value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, AnimationCurve\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_AnimationCurve_}

Sets [`animationCurveValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-animationCurveValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, AnimationCurve value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` AnimationCurve

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetValueAndApply\<T\>\(T, EntityId\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetValueAndApply__1___0_UnityEngine_EntityId_}

Sets [`entityIdValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-entityIdValue.html) then applies modified properties.

```csharp
public static T SetValueAndApply<T>(this T property, EntityId value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` EntityId

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector2\<T\>\(T, Vector2\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector2__1___0_UnityEngine_Vector2_}

Sets [`vector2Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2Value.html) and returns the property for chaining.

```csharp
public static T SetVector2<T>(this T property, Vector2 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector2AndApply\<T\>\(T, Vector2\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector2AndApply__1___0_UnityEngine_Vector2_}

Sets [`vector2Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2Value.html) then applies modified properties.

```csharp
public static T SetVector2AndApply<T>(this T property, Vector2 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector2Int\<T\>\(T, Vector2Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector2Int__1___0_UnityEngine_Vector2Int_}

Sets [`vector2IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2IntValue.html) and returns the property for chaining.

```csharp
public static T SetVector2Int<T>(this T property, Vector2Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector2IntAndApply\<T\>\(T, Vector2Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector2IntAndApply__1___0_UnityEngine_Vector2Int_}

Sets [`vector2IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector2IntValue.html) then applies modified properties.

```csharp
public static T SetVector2IntAndApply<T>(this T property, Vector2Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector2Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector3\<T\>\(T, Vector3\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector3__1___0_UnityEngine_Vector3_}

Sets [`vector3Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3Value.html) and returns the property for chaining.

```csharp
public static T SetVector3<T>(this T property, Vector3 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector3AndApply\<T\>\(T, Vector3\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector3AndApply__1___0_UnityEngine_Vector3_}

Sets [`vector3Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3Value.html) then applies modified properties.

```csharp
public static T SetVector3AndApply<T>(this T property, Vector3 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector3Int\<T\>\(T, Vector3Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector3Int__1___0_UnityEngine_Vector3Int_}

Sets [`vector3IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3IntValue.html) and returns the property for chaining.

```csharp
public static T SetVector3Int<T>(this T property, Vector3Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector3IntAndApply\<T\>\(T, Vector3Int\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector3IntAndApply__1___0_UnityEngine_Vector3Int_}

Sets [`vector3IntValue`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector3IntValue.html) then applies modified properties.

```csharp
public static T SetVector3IntAndApply<T>(this T property, Vector3Int value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector3Int

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector4\<T\>\(T, Vector4\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector4__1___0_UnityEngine_Vector4_}

Sets [`vector4Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector4Value.html) and returns the property for chaining.

```csharp
public static T SetVector4<T>(this T property, Vector4 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector4

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### SetVector4AndApply\<T\>\(T, Vector4\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_SetVector4AndApply__1___0_UnityEngine_Vector4_}

Sets [`vector4Value`](https://docs.unity3d.com/ScriptReference/SerializedProperty-vector4Value.html) then applies modified properties.

```csharp
public static T SetVector4AndApply<T>(this T property, Vector4 value) where T : SerializedProperty
```

#### Parameters

`property` T

Target property.

`value` Vector4

Value to assign.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### Update\<T\>\(T\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_Update__1___0_}

Calls [`Update`](https://docs.unity3d.com/ScriptReference/SerializedObject-Update.html) on the property's serialized object and returns the property for chaining.

```csharp
public static T Update<T>(this T property) where T : SerializedProperty
```

#### Parameters

`property` T

The property whose serialized object should be updated.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

### UpdateIfRequiredOrScript\<T\>\(T\) {#Aspid_FastTools_Editors_SerializePropertyExtensions_UpdateIfRequiredOrScript__1___0_}

Calls [`UpdateIfRequiredOrScript`](https://docs.unity3d.com/ScriptReference/SerializedObject-UpdateIfRequiredOrScript.html) on the property's serialized object and returns the property for chaining.

```csharp
public static T UpdateIfRequiredOrScript<T>(this T property) where T : SerializedProperty
```

#### Parameters

`property` T

The property whose serialized object should be conditionally updated.

#### Returns

 T

The same <code class="paramref">property</code> instance.

#### Type Parameters

`T` 

Concrete [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) type.

