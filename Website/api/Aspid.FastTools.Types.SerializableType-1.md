---
title: "Class SerializableType<T>"
sidebar_label: "SerializableType<T>"
description: "Class SerializableType<T> — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class SerializableType\<T\> {#Aspid_FastTools_Types_SerializableType_1}

Namespace: [Aspid.FastTools.Types](Aspid.FastTools.Types.md)  
Assembly: Aspid.FastTools.dll  

[`SerializableType`](Aspid.FastTools.Types.SerializableType.md) constrained to types assignable to <code class="typeparamref">T</code>.

```csharp
[Serializable]
public sealed class SerializableType<T> : SerializableType, ISerializableType, ISerializationCallbackReceiver
```

#### Type Parameters

`T` 

Base constraint type; the picker offers only types assignable to it.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[SerializableTypeBase](Aspid.FastTools.Types.SerializableTypeBase.md) ← 
[SerializableType](Aspid.FastTools.Types.SerializableType.md) ← 
[SerializableType\<T\>](Aspid.FastTools.Types.SerializableType-1.md)

#### Implements

[ISerializableType](Aspid.FastTools.Types.ISerializableType.md),
ISerializationCallbackReceiver


#### Extension Methods

[TextInputBaseFieldTextSelectionExtensions.AddOnCursorIndexChange\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_AddOnCursorIndexChange__2___0_System_Action_),
[TextInputBaseFieldTextSelectionExtensions.AddOnSelectIndexChange\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_AddOnSelectIndexChange__2___0_System_Action_),
[INotifyValueChangedExtensions.AddValueChanged\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[ProfilerMarkerExtensionsForGenerator.Marker\<SerializableType\<T\>\>\(SerializableType\<T\>\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker__1___0_),
[TextInputBaseFieldTextSelectionExtensions.RemoveOnCursorIndexChange\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_RemoveOnCursorIndexChange__2___0_System_Action_),
[TextInputBaseFieldTextSelectionExtensions.RemoveOnSelectIndexChange\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, Action\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_RemoveOnSelectIndexChange__2___0_System_Action_),
[INotifyValueChangedExtensions.RemoveValueChanged\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___),
[TextInputBaseFieldExtensions.SetAutoCorrection\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetAutoCorrection__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetCursorIndex\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetCursorIndex__2___0_System_Int32_),
[TextInputBaseFieldExtensions.SetDelayed\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetDelayed__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetDoubleClickSelectsWord\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetDoubleClickSelectsWord__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHideMobileInput\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHideMobileInput__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHidePlaceholderOnFocus\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHidePlaceholderOnFocus__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetHideSoftKeyboard\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetHideSoftKeyboard__2___0_System_Boolean_),
[SliderExtensions.SetHighValue\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_),
[TextInputBaseFieldExtensions.SetKeyboardType\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, TouchScreenKeyboardType\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetKeyboardType__2___0_UnityEngine_TouchScreenKeyboardType_),
[BaseFieldExtensions.SetLabel\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_),
[SliderExtensions.SetLowValue\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_),
[TextInputBaseFieldExtensions.SetMaskChar\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, char\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetMaskChar__2___0_System_Char_),
[TextInputBaseFieldExtensions.SetMaxLength\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetMaxLength__2___0_System_Int32_),
[TextInputBaseFieldExtensions.SetPassword\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetPassword__2___0_System_Boolean_),
[TextInputBaseFieldExtensions.SetPlaceholder\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, string\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetPlaceholder__2___0_System_String_),
[TextInputBaseFieldExtensions.SetReadOnly\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldExtensions_SetReadOnly__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectAllOnFocus\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectAllOnFocus__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectAllOnMouseUp\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectAllOnMouseUp__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectIndex\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, int\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectIndex__2___0_System_Int32_),
[TextInputBaseFieldTextSelectionExtensions.SetSelectable\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetSelectable__2___0_System_Boolean_),
[TextInputBaseFieldTextSelectionExtensions.SetTripleClickSelectsLine\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, bool\)](Aspid.FastTools.UIElements.TextInputBaseFieldTextSelectionExtensions.md#Aspid_FastTools_UIElements_TextInputBaseFieldTextSelectionExtensions_SetTripleClickSelectsLine__2___0_System_Boolean_),
[INotifyValueChangedExtensions.SetValue\<SerializableType\<T\>, TValue\>\(SerializableType\<T\>, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_)

## Examples


```csharp
public class MyComponent : MonoBehaviour
{
    [SerializeField] private SerializableType<MonoBehaviour>; _behaviorType;

    private void Start()
    {
        Type type = _behaviorType;  // always a MonoBehaviour subtype or null
        if (type != null)
            gameObject.AddComponent(type);
    }
}
```


## Remarks

Unity serializes a field by its declared type, so a [`SerializableType<T>`](Aspid.FastTools.Types.SerializableType-1.md) assigned from code to a
field declared as [`SerializableType`](Aspid.FastTools.Types.SerializableType.md) is reloaded unconstrained: the type survives, the constraint
does not.

## Constructors

### SerializableType\(Type?\) {#Aspid_FastTools_Types_SerializableType_1__ctor_System_Type_}

Creates a wrapper holding <code class="paramref">type</code>.

```csharp
public SerializableType(Type? type)
```

#### Parameters

`type` [Type](https://learn.microsoft.com/dotnet/api/system.type)?

The type to store, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for an empty wrapper.

#### Exceptions

 [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

Thrown when <code class="paramref">type</code> is not assignable to <code class="typeparamref">T</code>.

## Properties

### BaseType {#Aspid_FastTools_Types_SerializableType_1_BaseType}

Gets the constraint the stored type must satisfy; [`Object`](https://learn.microsoft.com/dotnet/api/system.object) when unconstrained.

```csharp
public override Type BaseType { get; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

