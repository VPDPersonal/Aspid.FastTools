---
title: "Class IdField"
sidebar_label: "IdField"
description: "Class IdField — Aspid.FastTools API reference"
hide_title: true
pagination_prev: null
pagination_next: null
---
# Class IdField {#Aspid_FastTools_Ids_Editors_IdField}

Namespace: [Aspid.FastTools.Ids.Editors](Aspid.FastTools.Ids.Editors.md)  
Assembly: Aspid.FastTools.Unity.Editor.dll  

UIToolkit field that displays an [`IId`](Aspid.FastTools.Ids.IId.md)-style integer id as an EnumField-style
dropdown backed by [`Editors.IdSelectorWindow`](Aspid.FastTools.Ids.Editors.md). Optionally bound to an
[`IId`](Aspid.FastTools.Ids.IId.md) struct [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) whose generated children
(<code>_id</code> and <code>__stringId</code>) are updated together; when no [`IdRegistry`](Aspid.FastTools.Ids.IdRegistry.md)
is bound to [`IdField.IdType`](Aspid.FastTools.Ids.Editors.IdField.md#Aspid_FastTools_Ids_Editors_IdField_IdType) or the id cannot be resolved to a name, the field renders
a <code>&lt;Missing&gt;</code> caption instead of silently clearing.

```csharp
[UxmlElement]
public class IdField : BaseField<int>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IBindable, INotifyValueChanged<int>, IMixedValueSupport
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
CallbackEventHandler ← 
Focusable ← 
VisualElement ← 
BindableElement ← 
BaseField\<int\> ← 
[IdField](Aspid.FastTools.Ids.Editors.IdField.md)

#### Derived

[InspectorIdField](Aspid.FastTools.Ids.Editors.InspectorIdField.md)

#### Implements

IEventHandler, 
IResolvedStyle, 
ITransform, 
ITransitionAnimations, 
IExperimentalFeatures, 
IVisualElementScheduler, 
IBindable, 
INotifyValueChanged\<int\>, 
IMixedValueSupport


#### Extension Methods

[VisualElementExtensions.AddBoldUnityFontStyleAndWeight\<IdField\>\(IdField\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddBoldUnityFontStyleAndWeight__1___0_), 
[VisualElementExtensions.AddChild\<IdField\>\(IdField, VisualElement\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddChild__1___0_UnityEngine_UIElements_VisualElement_), 
[VisualElementExtensions.AddChildIf\<IdField\>\(IdField, bool, VisualElement\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildIf__1___0_System_Boolean_UnityEngine_UIElements_VisualElement_), 
[VisualElementExtensions.AddChildren\<IdField\>\(IdField, Span\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildren__1___0_System_Span_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.AddChildren\<IdField\>\(IdField, List\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildren__1___0_System_Collections_Generic_List_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.AddChildren\<IdField\>\(IdField, params VisualElement\[\]\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildren__1___0_UnityEngine_UIElements_VisualElement___), 
[VisualElementExtensions.AddChildren\<IdField\>\(IdField, IEnumerable\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildren__1___0_System_Collections_Generic_IEnumerable_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.AddChildren\<IdField\>\(IdField, ReadOnlySpan\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildren__1___0_System_ReadOnlySpan_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.AddChildrenIf\<IdField\>\(IdField, bool, Span\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildrenIf__1___0_System_Boolean_System_Span_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.AddChildrenIf\<IdField\>\(IdField, bool, List\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildrenIf__1___0_System_Boolean_System_Collections_Generic_List_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.AddChildrenIf\<IdField\>\(IdField, bool, params VisualElement\[\]\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildrenIf__1___0_System_Boolean_UnityEngine_UIElements_VisualElement___), 
[VisualElementExtensions.AddChildrenIf\<IdField\>\(IdField, bool, IEnumerable\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildrenIf__1___0_System_Boolean_System_Collections_Generic_IEnumerable_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.AddChildrenIf\<IdField\>\(IdField, bool, ReadOnlySpan\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddChildrenIf__1___0_System_Boolean_System_ReadOnlySpan_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.AddClass\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddClass__1___0_System_String_), 
[ManipulatorExtensions.AddClickable\<IdField\>\(IdField, Action\)](Aspid.FastTools.UIElements.ManipulatorExtensions.md#Aspid_FastTools_UIElements_ManipulatorExtensions_AddClickable__1___0_System_Action_), 
[ManipulatorExtensions.AddClickable\<IdField\>\(IdField, Action, out Clickable\)](Aspid.FastTools.UIElements.ManipulatorExtensions.md#Aspid_FastTools_UIElements_ManipulatorExtensions_AddClickable__1___0_System_Action_UnityEngine_UIElements_Clickable__), 
[ManipulatorExtensions.AddClickable\<IdField\>\(IdField, Action\<EventBase\>\)](Aspid.FastTools.UIElements.ManipulatorExtensions.md#Aspid_FastTools_UIElements_ManipulatorExtensions_AddClickable__1___0_System_Action_UnityEngine_UIElements_EventBase__), 
[ManipulatorExtensions.AddClickable\<IdField\>\(IdField, Action\<EventBase\>, out Clickable\)](Aspid.FastTools.UIElements.ManipulatorExtensions.md#Aspid_FastTools_UIElements_ManipulatorExtensions_AddClickable__1___0_System_Action_UnityEngine_UIElements_EventBase__UnityEngine_UIElements_Clickable__), 
[ManipulatorExtensions.AddClickable\<IdField\>\(IdField, Action, long, long\)](Aspid.FastTools.UIElements.ManipulatorExtensions.md#Aspid_FastTools_UIElements_ManipulatorExtensions_AddClickable__1___0_System_Action_System_Int64_System_Int64_), 
[ManipulatorExtensions.AddClickable\<IdField\>\(IdField, Action, long, long, out Clickable\)](Aspid.FastTools.UIElements.ManipulatorExtensions.md#Aspid_FastTools_UIElements_ManipulatorExtensions_AddClickable__1___0_System_Action_System_Int64_System_Int64_UnityEngine_UIElements_Clickable__), 
[ManipulatorExtensions.AddContextualMenuManipulator\<IdField\>\(IdField, Action\<ContextualMenuPopulateEvent\>\)](Aspid.FastTools.UIElements.ManipulatorExtensions.md#Aspid_FastTools_UIElements_ManipulatorExtensions_AddContextualMenuManipulator__1___0_System_Action_UnityEngine_UIElements_ContextualMenuPopulateEvent__), 
[ManipulatorExtensions.AddContextualMenuManipulator\<IdField\>\(IdField, Action\<ContextualMenuPopulateEvent\>, out ContextualMenuManipulator\)](Aspid.FastTools.UIElements.ManipulatorExtensions.md#Aspid_FastTools_UIElements_ManipulatorExtensions_AddContextualMenuManipulator__1___0_System_Action_UnityEngine_UIElements_ContextualMenuPopulateEvent__UnityEngine_UIElements_ContextualMenuManipulator__), 
[VisualElementExtensions.AddItalicUnityFontStyleAndWeight\<IdField\>\(IdField\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddItalicUnityFontStyleAndWeight__1___0_), 
[ManipulatorExtensions.AddKeyboardNavigationManipulator\<IdField\>\(IdField, Action\<KeyboardNavigationOperation, EventBase\>\)](Aspid.FastTools.UIElements.ManipulatorExtensions.md#Aspid_FastTools_UIElements_ManipulatorExtensions_AddKeyboardNavigationManipulator__1___0_System_Action_UnityEngine_UIElements_KeyboardNavigationOperation_UnityEngine_UIElements_EventBase__), 
[ManipulatorExtensions.AddKeyboardNavigationManipulator\<IdField\>\(IdField, Action\<KeyboardNavigationOperation, EventBase\>, out KeyboardNavigationManipulator\)](Aspid.FastTools.UIElements.ManipulatorExtensions.md#Aspid_FastTools_UIElements_ManipulatorExtensions_AddKeyboardNavigationManipulator__1___0_System_Action_UnityEngine_UIElements_KeyboardNavigationOperation_UnityEngine_UIElements_EventBase__UnityEngine_UIElements_KeyboardNavigationManipulator__), 
[ManipulatorExtensions.AddManipulatorSelf\<IdField\>\(IdField, IManipulator\)](Aspid.FastTools.UIElements.ManipulatorExtensions.md#Aspid_FastTools_UIElements_ManipulatorExtensions_AddManipulatorSelf__1___0_UnityEngine_UIElements_IManipulator_), 
[VisualElementExtensions.AddOpenScriptCommand\<IdField\>\(IdField, Object\)](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_AddOpenScriptCommand__1___0_UnityEngine_Object_), 
[VisualElementExtensions.AddStyleSheets\<IdField\>\(IdField, StyleSheet\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddStyleSheets__1___0_UnityEngine_UIElements_StyleSheet_), 
[VisualElementExtensions.AddStyleSheetsFromResource\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_AddStyleSheetsFromResource__1___0_System_String_), 
[INotifyValueChangedExtensions.AddValueChanged\<IdField\>\(IdField, EventCallback\<ChangeEvent\<int\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__1___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent_System_Int32___), 
[INotifyValueChangedExtensions.AddValueChanged\<IdField, TValue\>\(IdField, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_AddValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[IBindableExtensions.BindPropertyTo\<IdField\>\(IdField, SerializedProperty\)](Aspid.FastTools.UIElements.Editors.IBindableExtensions.md#Aspid_FastTools_UIElements_Editors_IBindableExtensions_BindPropertyTo__1___0_UnityEditor_SerializedProperty_), 
[IBindableExtensions.BindTo\<IdField\>\(IdField, SerializedObject, string\)](Aspid.FastTools.UIElements.Editors.IBindableExtensions.md#Aspid_FastTools_UIElements_Editors_IBindableExtensions_BindTo__1___0_UnityEditor_SerializedObject_System_String_), 
[VisualElementExtensions.BindTo\<IdField\>\(IdField, SerializedObject\)](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_BindTo__1___0_UnityEditor_SerializedObject_), 
[VisualElementExtensions.ClearChildren\<IdField\>\(IdField\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_ClearChildren__1___0_), 
[VisualElementExtensions.ClearChildrenIf\<IdField\>\(IdField, bool\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_ClearChildrenIf__1___0_System_Boolean_), 
[VisualElementExtensions.ClearClasses\<IdField\>\(IdField\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_ClearClasses__1___0_), 
[VisualElementExtensions.EnableInClass\<IdField\>\(IdField, string, bool\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_EnableInClass__1___0_System_String_System_Boolean_), 
[VisualElementExtensions.GetOwnerWindow\(VisualElement\)](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_GetOwnerWindow_UnityEngine_UIElements_VisualElement_), 
[VisualElementExtensions.InsertChild\<IdField\>\(IdField, int, VisualElement\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChild__1___0_System_Int32_UnityEngine_UIElements_VisualElement_), 
[VisualElementExtensions.InsertChildIf\<IdField\>\(IdField, bool, int, VisualElement\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildIf__1___0_System_Boolean_System_Int32_UnityEngine_UIElements_VisualElement_), 
[VisualElementExtensions.InsertChildren\<IdField\>\(IdField, int, Span\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildren__1___0_System_Int32_System_Span_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.InsertChildren\<IdField\>\(IdField, int, List\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildren__1___0_System_Int32_System_Collections_Generic_List_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.InsertChildren\<IdField\>\(IdField, int, params VisualElement\[\]\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildren__1___0_System_Int32_UnityEngine_UIElements_VisualElement___), 
[VisualElementExtensions.InsertChildren\<IdField\>\(IdField, int, IEnumerable\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildren__1___0_System_Int32_System_Collections_Generic_IEnumerable_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.InsertChildren\<IdField\>\(IdField, int, ReadOnlySpan\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildren__1___0_System_Int32_System_ReadOnlySpan_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.InsertChildrenIf\<IdField\>\(IdField, bool, int, Span\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildrenIf__1___0_System_Boolean_System_Int32_System_Span_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.InsertChildrenIf\<IdField\>\(IdField, bool, int, List\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildrenIf__1___0_System_Boolean_System_Int32_System_Collections_Generic_List_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.InsertChildrenIf\<IdField\>\(IdField, bool, int, params VisualElement\[\]\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildrenIf__1___0_System_Boolean_System_Int32_UnityEngine_UIElements_VisualElement___), 
[VisualElementExtensions.InsertChildrenIf\<IdField\>\(IdField, bool, int, IEnumerable\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildrenIf__1___0_System_Boolean_System_Int32_System_Collections_Generic_IEnumerable_UnityEngine_UIElements_VisualElement__), 
[VisualElementExtensions.InsertChildrenIf\<IdField\>\(IdField, bool, int, ReadOnlySpan\<VisualElement\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_InsertChildrenIf__1___0_System_Boolean_System_Int32_System_ReadOnlySpan_UnityEngine_UIElements_VisualElement__), 
[FocusableExtensions.IsFocus\(Focusable\)](Aspid.FastTools.UIElements.FocusableExtensions.md#Aspid_FastTools_UIElements_FocusableExtensions_IsFocus_UnityEngine_UIElements_Focusable_), 
[ProfilerMarkerExtensionsForGenerator.Marker\(object\)](ProfilerMarkerExtensionsForGenerator.md#ProfilerMarkerExtensionsForGenerator_Marker_System_Object_), 
[VisualElementExtensions.RemoveBoldUnityFontStyleAndWeight\<IdField\>\(IdField\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveBoldUnityFontStyleAndWeight__1___0_), 
[VisualElementExtensions.RemoveChild\<IdField\>\(IdField, VisualElement\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveChild__1___0_UnityEngine_UIElements_VisualElement_), 
[VisualElementExtensions.RemoveChildAt\<IdField\>\(IdField, int\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveChildAt__1___0_System_Int32_), 
[VisualElementExtensions.RemoveChildAtIf\<IdField\>\(IdField, bool, int\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveChildAtIf__1___0_System_Boolean_System_Int32_), 
[VisualElementExtensions.RemoveChildIf\<IdField\>\(IdField, bool, VisualElement\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveChildIf__1___0_System_Boolean_UnityEngine_UIElements_VisualElement_), 
[VisualElementExtensions.RemoveClass\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveClass__1___0_System_String_), 
[VisualElementExtensions.RemoveItalicUnityFontStyleAndWeight\<IdField\>\(IdField\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveItalicUnityFontStyleAndWeight__1___0_), 
[ManipulatorExtensions.RemoveManipulatorSelf\<IdField\>\(IdField, IManipulator\)](Aspid.FastTools.UIElements.ManipulatorExtensions.md#Aspid_FastTools_UIElements_ManipulatorExtensions_RemoveManipulatorSelf__1___0_UnityEngine_UIElements_IManipulator_), 
[VisualElementExtensions.RemoveStyleSheets\<IdField\>\(IdField, StyleSheet\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveStyleSheets__1___0_UnityEngine_UIElements_StyleSheet_), 
[VisualElementExtensions.RemoveStyleSheetsFromResource\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_RemoveStyleSheetsFromResource__1___0_System_String_), 
[INotifyValueChangedExtensions.RemoveValueChanged\<IdField\>\(IdField, EventCallback\<ChangeEvent\<int\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__1___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent_System_Int32___), 
[INotifyValueChangedExtensions.RemoveValueChanged\<IdField, TValue\>\(IdField, EventCallback\<ChangeEvent\<TValue\>\>\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_RemoveValueChanged__2___0_UnityEngine_UIElements_EventCallback_UnityEngine_UIElements_ChangeEvent___1___), 
[VisualElementExtensions.SetAlignContent\<IdField\>\(IdField, StyleEnum\<Align\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetAlignContent__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Align__), 
[VisualElementExtensions.SetAlignContent\<IdField\>\(IdField, Align\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetAlignContent__1___0_UnityEngine_UIElements_Align_), 
[VisualElementExtensions.SetAlignItems\<IdField\>\(IdField, StyleEnum\<Align\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetAlignItems__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Align__), 
[VisualElementExtensions.SetAlignItems\<IdField\>\(IdField, Align\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetAlignItems__1___0_UnityEngine_UIElements_Align_), 
[VisualElementExtensions.SetAlignSelf\<IdField\>\(IdField, StyleEnum\<Align\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetAlignSelf__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Align__), 
[VisualElementExtensions.SetAlignSelf\<IdField\>\(IdField, Align\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetAlignSelf__1___0_UnityEngine_UIElements_Align_), 
[VisualElementExtensions.SetAspectRatio\<IdField\>\(IdField, StyleRatio\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetAspectRatio__1___0_UnityEngine_UIElements_StyleRatio_), 
[VisualElementExtensions.SetBackgroundColor\<IdField\>\(IdField, StyleColor\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundColor__1___0_UnityEngine_UIElements_StyleColor_), 
[VisualElementExtensions.SetBackgroundColor\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundColor__1___0_System_String_), 
[VisualElementExtensions.SetBackgroundImage\<IdField\>\(IdField, StyleBackground\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundImage__1___0_UnityEngine_UIElements_StyleBackground_), 
[VisualElementExtensions.SetBackgroundImageFromResource\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundImageFromResource__1___0_System_String_), 
[VisualElementExtensions.SetBackgroundPosition\<IdField\>\(IdField, StyleBackgroundPosition\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundPosition__1___0_UnityEngine_UIElements_StyleBackgroundPosition_), 
[VisualElementExtensions.SetBackgroundPosition\<IdField\>\(IdField, StyleBackgroundPosition?, StyleBackgroundPosition?\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundPosition__1___0_System_Nullable_UnityEngine_UIElements_StyleBackgroundPosition__System_Nullable_UnityEngine_UIElements_StyleBackgroundPosition__), 
[VisualElementExtensions.SetBackgroundPositionX\<IdField\>\(IdField, StyleBackgroundPosition\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundPositionX__1___0_UnityEngine_UIElements_StyleBackgroundPosition_), 
[VisualElementExtensions.SetBackgroundPositionY\<IdField\>\(IdField, StyleBackgroundPosition\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundPositionY__1___0_UnityEngine_UIElements_StyleBackgroundPosition_), 
[VisualElementExtensions.SetBackgroundRepeat\<IdField\>\(IdField, StyleBackgroundRepeat\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundRepeat__1___0_UnityEngine_UIElements_StyleBackgroundRepeat_), 
[VisualElementExtensions.SetBackgroundSize\<IdField\>\(IdField, StyleBackgroundSize\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBackgroundSize__1___0_UnityEngine_UIElements_StyleBackgroundSize_), 
[IBindableExtensions.SetBindingPath\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.Editors.IBindableExtensions.md#Aspid_FastTools_UIElements_Editors_IBindableExtensions_SetBindingPath__1___0_System_String_), 
[FocusableExtensions.SetBlur\<IdField\>\(IdField\)](Aspid.FastTools.UIElements.FocusableExtensions.md#Aspid_FastTools_UIElements_FocusableExtensions_SetBlur__1___0_), 
[VisualElementExtensions.SetBorderColor\<IdField\>\(IdField, StyleColor\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColor__1___0_UnityEngine_UIElements_StyleColor_), 
[VisualElementExtensions.SetBorderColor\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColor__1___0_System_String_), 
[VisualElementExtensions.SetBorderColor\<IdField\>\(IdField, StyleColor?, StyleColor?, StyleColor?, StyleColor?\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColor__1___0_System_Nullable_UnityEngine_UIElements_StyleColor__System_Nullable_UnityEngine_UIElements_StyleColor__System_Nullable_UnityEngine_UIElements_StyleColor__System_Nullable_UnityEngine_UIElements_StyleColor__), 
[VisualElementExtensions.SetBorderColorBottom\<IdField\>\(IdField, StyleColor\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorBottom__1___0_UnityEngine_UIElements_StyleColor_), 
[VisualElementExtensions.SetBorderColorBottom\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorBottom__1___0_System_String_), 
[VisualElementExtensions.SetBorderColorLeft\<IdField\>\(IdField, StyleColor\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorLeft__1___0_UnityEngine_UIElements_StyleColor_), 
[VisualElementExtensions.SetBorderColorLeft\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorLeft__1___0_System_String_), 
[VisualElementExtensions.SetBorderColorRight\<IdField\>\(IdField, StyleColor\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorRight__1___0_UnityEngine_UIElements_StyleColor_), 
[VisualElementExtensions.SetBorderColorRight\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorRight__1___0_System_String_), 
[VisualElementExtensions.SetBorderColorTop\<IdField\>\(IdField, StyleColor\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorTop__1___0_UnityEngine_UIElements_StyleColor_), 
[VisualElementExtensions.SetBorderColorTop\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorTop__1___0_System_String_), 
[VisualElementExtensions.SetBorderColorX\<IdField\>\(IdField, StyleColor\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorX__1___0_UnityEngine_UIElements_StyleColor_), 
[VisualElementExtensions.SetBorderColorX\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorX__1___0_System_String_), 
[VisualElementExtensions.SetBorderColorY\<IdField\>\(IdField, StyleColor\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorY__1___0_UnityEngine_UIElements_StyleColor_), 
[VisualElementExtensions.SetBorderColorY\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderColorY__1___0_System_String_), 
[VisualElementExtensions.SetBorderRadius\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadius__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetBorderRadius\<IdField\>\(IdField, StyleLength?, StyleLength?, StyleLength?, StyleLength?\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadius__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__), 
[VisualElementExtensions.SetBorderRadiusBottom\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusBottom__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetBorderRadiusBottomLeft\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusBottomLeft__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetBorderRadiusBottomRight\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusBottomRight__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetBorderRadiusLeft\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusLeft__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetBorderRadiusRight\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusRight__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetBorderRadiusTop\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusTop__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetBorderRadiusTopLeft\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusTopLeft__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetBorderRadiusTopRight\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderRadiusTopRight__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetBorderWidth\<IdField\>\(IdField, StyleFloat\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidth__1___0_UnityEngine_UIElements_StyleFloat_), 
[VisualElementExtensions.SetBorderWidth\<IdField\>\(IdField, StyleFloat?, StyleFloat?, StyleFloat?, StyleFloat?\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidth__1___0_System_Nullable_UnityEngine_UIElements_StyleFloat__System_Nullable_UnityEngine_UIElements_StyleFloat__System_Nullable_UnityEngine_UIElements_StyleFloat__System_Nullable_UnityEngine_UIElements_StyleFloat__), 
[VisualElementExtensions.SetBorderWidthBottom\<IdField\>\(IdField, StyleFloat\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidthBottom__1___0_UnityEngine_UIElements_StyleFloat_), 
[VisualElementExtensions.SetBorderWidthLeft\<IdField\>\(IdField, StyleFloat\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidthLeft__1___0_UnityEngine_UIElements_StyleFloat_), 
[VisualElementExtensions.SetBorderWidthRight\<IdField\>\(IdField, StyleFloat\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidthRight__1___0_UnityEngine_UIElements_StyleFloat_), 
[VisualElementExtensions.SetBorderWidthTop\<IdField\>\(IdField, StyleFloat\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidthTop__1___0_UnityEngine_UIElements_StyleFloat_), 
[VisualElementExtensions.SetBorderWidthX\<IdField\>\(IdField, StyleFloat\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidthX__1___0_UnityEngine_UIElements_StyleFloat_), 
[VisualElementExtensions.SetBorderWidthY\<IdField\>\(IdField, StyleFloat\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBorderWidthY__1___0_UnityEngine_UIElements_StyleFloat_), 
[VisualElementExtensions.SetBottom\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetBottom__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetColor\<IdField\>\(IdField, StyleColor\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetColor__1___0_UnityEngine_UIElements_StyleColor_), 
[VisualElementExtensions.SetColor\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetColor__1___0_System_String_), 
[VisualElementExtensions.SetCursor\<IdField\>\(IdField, StyleCursor\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetCursor__1___0_UnityEngine_UIElements_StyleCursor_), 
[VisualElementExtensions.SetDataSource\<IdField\>\(IdField, object\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetDataSource__1___0_System_Object_), 
[VisualElementExtensions.SetDataSourcePath\<IdField\>\(IdField, PropertyPath\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetDataSourcePath__1___0_Unity_Properties_PropertyPath_), 
[VisualElementExtensions.SetDataSourceType\<IdField\>\(IdField, Type\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetDataSourceType__1___0_System_Type_), 
[FocusableExtensions.SetDelegatesFocus\<IdField\>\(IdField, bool\)](Aspid.FastTools.UIElements.FocusableExtensions.md#Aspid_FastTools_UIElements_FocusableExtensions_SetDelegatesFocus__1___0_System_Boolean_), 
[VisualElementExtensions.SetDisablePlayModeTint\<IdField\>\(IdField, bool\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetDisablePlayModeTint__1___0_System_Boolean_), 
[VisualElementExtensions.SetDisplay\<IdField\>\(IdField, StyleEnum\<DisplayStyle\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetDisplay__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_DisplayStyle__), 
[VisualElementExtensions.SetDisplay\<IdField\>\(IdField, DisplayStyle\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetDisplay__1___0_UnityEngine_UIElements_DisplayStyle_), 
[VisualElementExtensions.SetDistance\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetDistance__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetDistance\<IdField\>\(IdField, StyleLength?, StyleLength?, StyleLength?, StyleLength?\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetDistance__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__), 
[VisualElementExtensions.SetDistanceX\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetDistanceX__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetDistanceY\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetDistanceY__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetEnabledSelf\<IdField\>\(IdField, bool\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetEnabledSelf__1___0_System_Boolean_), 
[VisualElementExtensions.SetFilter\<IdField\>\(IdField, StyleList\<FilterFunction\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetFilter__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_FilterFunction__), 
[VisualElementExtensions.SetFlexBasis\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexBasis__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetFlexDirection\<IdField\>\(IdField, StyleEnum\<FlexDirection\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexDirection__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_FlexDirection__), 
[VisualElementExtensions.SetFlexDirection\<IdField\>\(IdField, FlexDirection\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexDirection__1___0_UnityEngine_UIElements_FlexDirection_), 
[VisualElementExtensions.SetFlexGrow\<IdField\>\(IdField, StyleFloat\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexGrow__1___0_UnityEngine_UIElements_StyleFloat_), 
[VisualElementExtensions.SetFlexShrink\<IdField\>\(IdField, StyleFloat\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexShrink__1___0_UnityEngine_UIElements_StyleFloat_), 
[VisualElementExtensions.SetFlexWrap\<IdField\>\(IdField, StyleEnum\<Wrap\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexWrap__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Wrap__), 
[VisualElementExtensions.SetFlexWrap\<IdField\>\(IdField, Wrap\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetFlexWrap__1___0_UnityEngine_UIElements_Wrap_), 
[FocusableExtensions.SetFocus\<IdField\>\(IdField\)](Aspid.FastTools.UIElements.FocusableExtensions.md#Aspid_FastTools_UIElements_FocusableExtensions_SetFocus__1___0_), 
[FocusableExtensions.SetFocusable\<IdField\>\(IdField, bool\)](Aspid.FastTools.UIElements.FocusableExtensions.md#Aspid_FastTools_UIElements_FocusableExtensions_SetFocusable__1___0_System_Boolean_), 
[VisualElementExtensions.SetFontSize\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetFontSize__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetHeight\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetHeight__1___0_UnityEngine_UIElements_StyleLength_), 
[SliderExtensions.SetHighValue\<IdField, TValue\>\(IdField, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetHighValue__2___0___1_), 
[VisualElementExtensions.SetJustifyContent\<IdField\>\(IdField, StyleEnum\<Justify\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetJustifyContent__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Justify__), 
[VisualElementExtensions.SetJustifyContent\<IdField\>\(IdField, Justify\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetJustifyContent__1___0_UnityEngine_UIElements_Justify_), 
[BaseFieldExtensions.SetLabel\<IdField, TValue\>\(IdField, string\)](Aspid.FastTools.UIElements.BaseFieldExtensions.md#Aspid_FastTools_UIElements_BaseFieldExtensions_SetLabel__2___0_System_String_), 
[BaseFieldExtensionsSetLabelInt.SetLabel\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.BaseFieldExtensionsSetLabelInt.md#Aspid_FastTools_UIElements_BaseFieldExtensionsSetLabelInt_SetLabel__1___0_System_String_), 
[VisualElementExtensions.SetLanguageDirection\<IdField\>\(IdField, LanguageDirection\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetLanguageDirection__1___0_UnityEngine_UIElements_LanguageDirection_), 
[VisualElementExtensions.SetLeft\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetLeft__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetLetterSpacing\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetLetterSpacing__1___0_UnityEngine_UIElements_StyleLength_), 
[SliderExtensions.SetLowValue\<IdField, TValue\>\(IdField, TValue\)](Aspid.FastTools.UIElements.SliderExtensions.md#Aspid_FastTools_UIElements_SliderExtensions_SetLowValue__2___0___1_), 
[VisualElementExtensions.SetMargin\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMargin__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetMargin\<IdField\>\(IdField, StyleLength?, StyleLength?, StyleLength?, StyleLength?\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMargin__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__), 
[VisualElementExtensions.SetMarginBottom\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMarginBottom__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetMarginLeft\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMarginLeft__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetMarginRight\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMarginRight__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetMarginTop\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMarginTop__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetMarginX\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMarginX__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetMarginY\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMarginY__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetMaxHeight\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMaxHeight__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetMaxSize\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMaxSize__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetMaxSize\<IdField\>\(IdField, StyleLength?, StyleLength?\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMaxSize__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__), 
[VisualElementExtensions.SetMaxWidth\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMaxWidth__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetMinHeight\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMinHeight__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetMinSize\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMinSize__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetMinSize\<IdField\>\(IdField, StyleLength?, StyleLength?\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMinSize__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__), 
[VisualElementExtensions.SetMinWidth\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetMinWidth__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetName\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetName__1___0_System_String_), 
[VisualElementExtensions.SetNormalUnityFontStyleAndWeight\<IdField\>\(IdField\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetNormalUnityFontStyleAndWeight__1___0_), 
[VisualElementExtensions.SetOpacity\<IdField\>\(IdField, StyleFloat\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetOpacity__1___0_UnityEngine_UIElements_StyleFloat_), 
[VisualElementExtensions.SetOverflow\<IdField\>\(IdField, StyleEnum\<Overflow\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetOverflow__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Overflow__), 
[VisualElementExtensions.SetOverflow\<IdField\>\(IdField, Overflow\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetOverflow__1___0_UnityEngine_UIElements_Overflow_), 
[VisualElementExtensions.SetPadding\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetPadding__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetPadding\<IdField\>\(IdField, StyleLength?, StyleLength?, StyleLength?, StyleLength?\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetPadding__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__), 
[VisualElementExtensions.SetPaddingBottom\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetPaddingBottom__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetPaddingLeft\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetPaddingLeft__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetPaddingRight\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetPaddingRight__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetPaddingTop\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetPaddingTop__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetPaddingX\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetPaddingX__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetPaddingY\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetPaddingY__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetPickingMode\<IdField\>\(IdField, PickingMode\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetPickingMode__1___0_UnityEngine_UIElements_PickingMode_), 
[VisualElementExtensions.SetPosition\<IdField\>\(IdField, StyleEnum\<Position\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetPosition__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Position__), 
[VisualElementExtensions.SetPosition\<IdField\>\(IdField, Position\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetPosition__1___0_UnityEngine_UIElements_Position_), 
[VisualElementExtensions.SetRight\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetRight__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetRotate\<IdField\>\(IdField, StyleRotate\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetRotate__1___0_UnityEngine_UIElements_StyleRotate_), 
[VisualElementExtensions.SetScale\<IdField\>\(IdField, StyleScale\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetScale__1___0_UnityEngine_UIElements_StyleScale_), 
[IMixedValueSupportExtensions.SetShowMixedValue\<IdField\>\(IdField, bool\)](Aspid.FastTools.UIElements.IMixedValueSupportExtensions.md#Aspid_FastTools_UIElements_IMixedValueSupportExtensions_SetShowMixedValue__1___0_System_Boolean_), 
[VisualElementExtensions.SetSize\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetSize__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetSize\<IdField\>\(IdField, StyleLength?, StyleLength?\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetSize__1___0_System_Nullable_UnityEngine_UIElements_StyleLength__System_Nullable_UnityEngine_UIElements_StyleLength__), 
[FocusableExtensions.SetTabIndex\<IdField\>\(IdField, int\)](Aspid.FastTools.UIElements.FocusableExtensions.md#Aspid_FastTools_UIElements_FocusableExtensions_SetTabIndex__1___0_System_Int32_), 
[VisualElementExtensions.SetTextOverflow\<IdField\>\(IdField, StyleEnum\<TextOverflow\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetTextOverflow__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_TextOverflow__), 
[VisualElementExtensions.SetTextOverflow\<IdField\>\(IdField, TextOverflow\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetTextOverflow__1___0_UnityEngine_UIElements_TextOverflow_), 
[VisualElementExtensions.SetTextShadow\<IdField\>\(IdField, StyleTextShadow\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetTextShadow__1___0_UnityEngine_UIElements_StyleTextShadow_), 
[VisualElementExtensions.SetTooltip\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetTooltip__1___0_System_String_), 
[VisualElementExtensions.SetTop\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetTop__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetTransformOrigin\<IdField\>\(IdField, StyleTransformOrigin\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetTransformOrigin__1___0_UnityEngine_UIElements_StyleTransformOrigin_), 
[VisualElementExtensions.SetTransitionDelay\<IdField\>\(IdField, StyleList\<TimeValue\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetTransitionDelay__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_TimeValue__), 
[VisualElementExtensions.SetTransitionDuration\<IdField\>\(IdField, StyleList\<TimeValue\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetTransitionDuration__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_TimeValue__), 
[VisualElementExtensions.SetTransitionProperty\<IdField\>\(IdField, StyleList\<StylePropertyName\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetTransitionProperty__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_StylePropertyName__), 
[VisualElementExtensions.SetTransitionTimingFunction\<IdField\>\(IdField, StyleList\<EasingFunction\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetTransitionTimingFunction__1___0_UnityEngine_UIElements_StyleList_UnityEngine_UIElements_EasingFunction__), 
[VisualElementExtensions.SetTranslate\<IdField\>\(IdField, StyleTranslate\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetTranslate__1___0_UnityEngine_UIElements_StyleTranslate_), 
[VisualElementExtensions.SetUnityBackgroundImageTintColor\<IdField\>\(IdField, StyleColor\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityBackgroundImageTintColor__1___0_UnityEngine_UIElements_StyleColor_), 
[VisualElementExtensions.SetUnityBackgroundImageTintColor\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityBackgroundImageTintColor__1___0_System_String_), 
[VisualElementExtensions.SetUnityEditorTextRenderingMode\<IdField\>\(IdField, StyleEnum\<EditorTextRenderingMode\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityEditorTextRenderingMode__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_EditorTextRenderingMode__), 
[VisualElementExtensions.SetUnityEditorTextRenderingMode\<IdField\>\(IdField, EditorTextRenderingMode\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityEditorTextRenderingMode__1___0_UnityEngine_UIElements_EditorTextRenderingMode_), 
[VisualElementExtensions.SetUnityFont\<IdField\>\(IdField, StyleFont\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityFont__1___0_UnityEngine_UIElements_StyleFont_), 
[VisualElementExtensions.SetUnityFontDefinition\<IdField\>\(IdField, StyleFontDefinition\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityFontDefinition__1___0_UnityEngine_UIElements_StyleFontDefinition_), 
[VisualElementExtensions.SetUnityFontStyleAndWeight\<IdField\>\(IdField, StyleEnum\<FontStyle\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityFontStyleAndWeight__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_FontStyle__), 
[VisualElementExtensions.SetUnityFontStyleAndWeight\<IdField\>\(IdField, FontStyle\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityFontStyleAndWeight__1___0_UnityEngine_FontStyle_), 
[VisualElementExtensions.SetUnityMaterial\<IdField\>\(IdField, StyleMaterialDefinition\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityMaterial__1___0_UnityEngine_UIElements_StyleMaterialDefinition_), 
[VisualElementExtensions.SetUnityOverflowClipBox\<IdField\>\(IdField, StyleEnum\<OverflowClipBox\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityOverflowClipBox__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_OverflowClipBox__), 
[VisualElementExtensions.SetUnityOverflowClipBox\<IdField\>\(IdField, OverflowClipBox\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityOverflowClipBox__1___0_UnityEngine_UIElements_OverflowClipBox_), 
[VisualElementExtensions.SetUnityParagraphSpacing\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityParagraphSpacing__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetUnitySlice\<IdField\>\(IdField, StyleInt\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySlice__1___0_UnityEngine_UIElements_StyleInt_), 
[VisualElementExtensions.SetUnitySlice\<IdField\>\(IdField, StyleInt?, StyleInt?, StyleInt?, StyleInt?\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySlice__1___0_System_Nullable_UnityEngine_UIElements_StyleInt__System_Nullable_UnityEngine_UIElements_StyleInt__System_Nullable_UnityEngine_UIElements_StyleInt__System_Nullable_UnityEngine_UIElements_StyleInt__), 
[VisualElementExtensions.SetUnitySliceBottom\<IdField\>\(IdField, StyleInt\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceBottom__1___0_UnityEngine_UIElements_StyleInt_), 
[VisualElementExtensions.SetUnitySliceLeft\<IdField\>\(IdField, StyleInt\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceLeft__1___0_UnityEngine_UIElements_StyleInt_), 
[VisualElementExtensions.SetUnitySliceRight\<IdField\>\(IdField, StyleInt\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceRight__1___0_UnityEngine_UIElements_StyleInt_), 
[VisualElementExtensions.SetUnitySliceScale\<IdField\>\(IdField, StyleFloat\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceScale__1___0_UnityEngine_UIElements_StyleFloat_), 
[VisualElementExtensions.SetUnitySliceTop\<IdField\>\(IdField, StyleInt\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceTop__1___0_UnityEngine_UIElements_StyleInt_), 
[VisualElementExtensions.SetUnitySliceType\<IdField\>\(IdField, StyleEnum\<SliceType\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceType__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_SliceType__), 
[VisualElementExtensions.SetUnitySliceType\<IdField\>\(IdField, SliceType\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceType__1___0_UnityEngine_UIElements_SliceType_), 
[VisualElementExtensions.SetUnitySliceX\<IdField\>\(IdField, StyleInt\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceX__1___0_UnityEngine_UIElements_StyleInt_), 
[VisualElementExtensions.SetUnitySliceY\<IdField\>\(IdField, StyleInt\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnitySliceY__1___0_UnityEngine_UIElements_StyleInt_), 
[VisualElementExtensions.SetUnityTextAlign\<IdField\>\(IdField, StyleEnum\<TextAnchor\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextAlign__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_TextAnchor__), 
[VisualElementExtensions.SetUnityTextAlign\<IdField\>\(IdField, TextAnchor\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextAlign__1___0_UnityEngine_TextAnchor_), 
[VisualElementExtensions.SetUnityTextAutoSize\<IdField\>\(IdField, StyleTextAutoSize\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextAutoSize__1___0_UnityEngine_UIElements_StyleTextAutoSize_), 
[VisualElementExtensions.SetUnityTextGenerator\<IdField\>\(IdField, StyleEnum\<TextGeneratorType\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextGenerator__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_TextGeneratorType__), 
[VisualElementExtensions.SetUnityTextGenerator\<IdField\>\(IdField, TextGeneratorType\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextGenerator__1___0_UnityEngine_TextGeneratorType_), 
[VisualElementExtensions.SetUnityTextOutlineColor\<IdField\>\(IdField, StyleColor\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextOutlineColor__1___0_UnityEngine_UIElements_StyleColor_), 
[VisualElementExtensions.SetUnityTextOutlineColor\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextOutlineColor__1___0_System_String_), 
[VisualElementExtensions.SetUnityTextOutlineWidth\<IdField\>\(IdField, StyleFloat\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextOutlineWidth__1___0_UnityEngine_UIElements_StyleFloat_), 
[VisualElementExtensions.SetUnityTextOverflowPosition\<IdField\>\(IdField, StyleEnum\<TextOverflowPosition\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextOverflowPosition__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_TextOverflowPosition__), 
[VisualElementExtensions.SetUnityTextOverflowPosition\<IdField\>\(IdField, TextOverflowPosition\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUnityTextOverflowPosition__1___0_UnityEngine_UIElements_TextOverflowPosition_), 
[VisualElementExtensions.SetUsageHints\<IdField\>\(IdField, UsageHints\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUsageHints__1___0_UnityEngine_UIElements_UsageHints_), 
[VisualElementExtensions.SetUserData\<IdField\>\(IdField, object\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetUserData__1___0_System_Object_), 
[INotifyValueChangedExtensions.SetValue\<IdField\>\(IdField, int, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__1___0_System_Int32_System_Boolean_), 
[INotifyValueChangedExtensions.SetValue\<IdField, TValue\>\(IdField, TValue, bool\)](Aspid.FastTools.UIElements.INotifyValueChangedExtensions.md#Aspid_FastTools_UIElements_INotifyValueChangedExtensions_SetValue__2___0___1_System_Boolean_), 
[VisualElementExtensions.SetViewDataKey\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetViewDataKey__1___0_System_String_), 
[VisualElementExtensions.SetVisibility\<IdField\>\(IdField, StyleEnum\<Visibility\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetVisibility__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_Visibility__), 
[VisualElementExtensions.SetVisibility\<IdField\>\(IdField, Visibility\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetVisibility__1___0_UnityEngine_UIElements_Visibility_), 
[VisualElementExtensions.SetVisible\<IdField\>\(IdField, bool\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetVisible__1___0_System_Boolean_), 
[VisualElementExtensions.SetWhiteSpace\<IdField\>\(IdField, StyleEnum\<WhiteSpace\>\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetWhiteSpace__1___0_UnityEngine_UIElements_StyleEnum_UnityEngine_UIElements_WhiteSpace__), 
[VisualElementExtensions.SetWhiteSpace\<IdField\>\(IdField, WhiteSpace\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetWhiteSpace__1___0_UnityEngine_UIElements_WhiteSpace_), 
[VisualElementExtensions.SetWidth\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetWidth__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.SetWordSpacing\<IdField\>\(IdField, StyleLength\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_SetWordSpacing__1___0_UnityEngine_UIElements_StyleLength_), 
[VisualElementExtensions.ToggleInClass\<IdField\>\(IdField, string\)](Aspid.FastTools.UIElements.VisualElementExtensions.md#Aspid_FastTools_UIElements_VisualElementExtensions_ToggleInClass__1___0_System_String_), 
[VisualElementExtensions.UnbindFrom\<IdField\>\(IdField\)](Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md#Aspid_FastTools_UIElements_Editors_VisualElementExtensions_UnbindFrom__1___0_)

## Remarks

Designed to be inheritable so subclasses (e.g. [`InspectorIdField`](Aspid.FastTools.Ids.Editors.InspectorIdField.md)) can layer
Inspector-specific styling on top of the base behaviour. Set [`IdField.IdType`](Aspid.FastTools.Ids.Editors.IdField.md#Aspid_FastTools_Ids_Editors_IdField_IdType) to the
id struct type that selects the registry — without it the dropdown does not open.

## Constructors

### IdField\(\) {#Aspid_FastTools_Ids_Editors_IdField__ctor}

```csharp
public IdField()
```

### IdField\(SerializedProperty\) {#Aspid_FastTools_Ids_Editors_IdField__ctor_UnityEditor_SerializedProperty_}

```csharp
public IdField(SerializedProperty property)
```

#### Parameters

`property` SerializedProperty

### IdField\(string, SerializedProperty\) {#Aspid_FastTools_Ids_Editors_IdField__ctor_System_String_UnityEditor_SerializedProperty_}

```csharp
public IdField(string label, SerializedProperty property)
```

#### Parameters

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

`property` SerializedProperty

### IdField\(string, int\) {#Aspid_FastTools_Ids_Editors_IdField__ctor_System_String_System_Int32_}

```csharp
public IdField(string label, int defaultValue = 0)
```

#### Parameters

`label` [string](https://learn.microsoft.com/dotnet/api/system.string)

`defaultValue` [int](https://learn.microsoft.com/dotnet/api/system.int32)

## Properties

### IdType {#Aspid_FastTools_Ids_Editors_IdField_IdType}

Id struct type — selects the [`IdRegistry`](Aspid.FastTools.Ids.IdRegistry.md) via
`Find`. The dropdown is disabled while this is <code>null</code>.
Setting this refreshes the rendered caption against the (possibly newly available) registry.

```csharp
public Type IdType { get; set; }
```

#### Property Value

 [Type](https://learn.microsoft.com/dotnet/api/system.type)

## Methods

### RefreshFromBoundProperty\(\) {#Aspid_FastTools_Ids_Editors_IdField_RefreshFromBoundProperty}

Re-reads the bound [`SerializedProperty`](https://docs.unity3d.com/ScriptReference/SerializedProperty.html) (when constructed from one)
and refreshes both the rendered caption and the cached name. No-op for unbound fields.

```csharp
public void RefreshFromBoundProperty()
```

### SetValueFromNameWithoutNotify\(string\) {#Aspid_FastTools_Ids_Editors_IdField_SetValueFromNameWithoutNotify_System_String_}

Sets the field value from a registry name without raising a change event.
If the name cannot be resolved (or [`IdField.IdType`](Aspid.FastTools.Ids.Editors.IdField.md#Aspid_FastTools_Ids_Editors_IdField_IdType) is <code>null</code>), the original
string is preserved so the field can render a <code>&lt;Missing&gt;</code> caption instead
of silently clearing.

```csharp
public void SetValueFromNameWithoutNotify(string nameId)
```

#### Parameters

`nameId` [string](https://learn.microsoft.com/dotnet/api/system.string)

### SetValueWithoutNotify\(int\) {#Aspid_FastTools_Ids_Editors_IdField_SetValueWithoutNotify_System_Int32_}

```csharp
public override sealed void SetValueWithoutNotify(int newValue)
```

#### Parameters

`newValue` [int](https://learn.microsoft.com/dotnet/api/system.int32)

