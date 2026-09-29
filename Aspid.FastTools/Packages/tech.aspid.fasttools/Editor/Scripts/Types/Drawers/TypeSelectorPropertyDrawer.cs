using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.SerializeReferences.Editors;

using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    [CustomPropertyDrawer(typeof(TypeSelectorAttribute))]
    internal sealed class TypeSelectorPropertyDrawer : PropertyDrawer
    {
        private const string UnsupportedFieldMessage =
            "[TypeSelector] can only be applied to a string field, a SerializableType / SerializableMonoScript field " +
            "(plain or <T>), a [SerializeReference] managed-reference field, or an array or List<T> of these.";

        private static float UnsupportedFieldHeight => EditorGUIUtility.singleLineHeight * 2f;

        private IReadOnlyList<string> _constraintWarnings;

        private TypeSelectorAttribute TypeSelector => (TypeSelectorAttribute)attribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!TryGetShape(property, out var shape, out var nameProperty, out var wrapperBaseType))
            {
                EditorGUI.HelpBox(position, UnsupportedFieldMessage, MessageType.Error);
                return;
            }

            var warnings = GetConstraintWarnings(property);
            var noticeHeight = GetConstraintNoticeHeight(warnings);

            var fieldRect = position;
            fieldRect.height = position.height - noticeHeight;
            DrawField(fieldRect, property, label, shape, nameProperty, wrapperBaseType);

            if (noticeHeight <= 0f) return;

            var noticeRect = new Rect(position.x, fieldRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width, EditorGUIUtility.singleLineHeight);
            InspectorNoticeGUI.DrawRequiredNotice(noticeRect, GetNoticeMessage(warnings), GetNoticeDetail(warnings));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!TryGetShape(property, out var shape, out var nameProperty, out _))
                return UnsupportedFieldHeight;

            return GetFieldHeight(property, label, shape, nameProperty) +
                   GetConstraintNoticeHeight(GetConstraintWarnings(property));
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (!TryGetShape(property, out var shape, out var nameProperty, out var wrapperBaseType))
                return new HelpBox(UnsupportedFieldMessage, HelpBoxMessageType.Error);

            var field = CreateField(preferredLabel, property, shape, nameProperty, wrapperBaseType, out var applyResolvedTypes);

            if (TypeSelector.AssemblyQualifiedNames.Length is 0) return field;

            var container = new VisualElement().AddChild(field);
            var notice = new InspectorNotice();

            // A string argument may reference a member of the target object whose value changes while the
            // inspector is open; every change to the object re-resolves the constraint and pushes the fresh
            // base types and warnings into the field (the IMGUI path re-resolves per OnGUI instead).
            container.TrackSerializedObjectValue(property.serializedObject, OnSerializedObjectChanged);
            UpdateNotice();

            return container;

            void OnSerializedObjectChanged(SerializedObject serializedObject)
            {
                if (serializedObject.targetObject == null) return;

                applyResolvedTypes();
                UpdateNotice();
            }

            void UpdateNotice()
            {
                var warnings = _constraintWarnings;
                if (warnings.Count == 0)
                {
                    notice.RemoveFromHierarchy();
                    return;
                }

                notice.Set(
                    message: GetNoticeMessage(warnings),
                    actionText: string.Empty,
                    detail: GetNoticeDetail(warnings),
                    onAction: null);

                if (notice.parent is null) container.AddChild(notice);
            }
        }

        private void DrawField(
            Rect position,
            SerializedProperty property,
            GUIContent label,
            FieldShape shape,
            SerializedProperty nameProperty,
            Type wrapperBaseType)
        {
            switch (shape)
            {
                case FieldShape.Wrapper:
                    TypeIMGUIPropertyDrawer.Draw(
                        position: position,
                        property: nameProperty,
                        label: label,
                        allow: TypeSelector.Allow,
                        types: GetWrapperBaseTypes(property, wrapperBaseType));
                    break;

                case FieldShape.MonoScriptWrapper:
                    MonoScriptIMGUIPropertyDrawer.Draw(
                        position: position,
                        label: label,
                        wrapperProperty: property,
                        allow: TypeSelector.Allow,
                        types: GetWrapperBaseTypes(property, wrapperBaseType));
                    break;

                case FieldShape.ManagedReference:
                    SerializeReferenceIMGUIPropertyDrawer.Draw(
                        position: position,
                        label: label,
                        property: property,
                        baseTypes: GetTypesFromAttribute(property));
                    break;

                // A list box takes the indent as a whole, as Unity's own list does; its rows start from zero inside.
                case FieldShape.ManagedReferenceList:
                {
                    var listRect = EditorGUI.IndentedRect(position);
                    using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel))
                    {
                        SerializeReferenceIMGUIList.Draw(
                            position: listRect,
                            listProperty: property,
                            label: label,
                            elementType: SerializeReferenceHelpers.GetArrayElementType(property),
                            baseTypes: GetTypesFromAttribute(property),
                            depth: 0);
                    }
                    break;
                }

                case FieldShape.List:
                {
                    var listRect = EditorGUI.IndentedRect(position);
                    using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel))
                    {
                        TypeSelectorIMGUIList.Draw(
                            position: listRect,
                            listProperty: property,
                            label: label,
                            getElementHeight: GetElementHeight,
                            drawElement: DrawElement);
                    }
                    break;
                }

                default:
                    TypeIMGUIPropertyDrawer.Draw(
                        position: position,
                        property: property,
                        label: label,
                        allow: TypeSelector.Allow,
                        types: GetTypesFromAttribute(property));
                    break;
            }
        }

        private float GetFieldHeight(SerializedProperty property, GUIContent label, FieldShape shape, SerializedProperty nameProperty) =>
            shape switch
            {
                FieldShape.Wrapper => TypeIMGUIPropertyDrawer.GetHeight(nameProperty),
                FieldShape.MonoScriptWrapper => MonoScriptIMGUIPropertyDrawer.GetHeight(property),
                FieldShape.ManagedReference => SerializeReferenceIMGUIPropertyDrawer.GetHeight(property),
                FieldShape.ManagedReferenceList => SerializeReferenceIMGUIList.GetHeight(
                    listProperty: property,
                    label: label,
                    elementType: SerializeReferenceHelpers.GetArrayElementType(property),
                    baseTypes: GetTypesFromAttribute(property),
                    depth: 0),
                FieldShape.List => TypeSelectorIMGUIList.GetHeight(
                    listProperty: property,
                    label: label,
                    getElementHeight: GetElementHeight,
                    drawElement: DrawElement),
                _ => TypeIMGUIPropertyDrawer.GetHeight(property),
            };

        // An element of a type-name or wrapper list: the attribute reaches the list, so the element is drawn here.
        private float GetElementHeight(SerializedProperty element) =>
            TryGetShape(element, out var shape, out var nameProperty, out _)
                ? GetFieldHeight(element, GUIContent.none, shape, nameProperty)
                : UnsupportedFieldHeight;

        private void DrawElement(Rect position, SerializedProperty element, GUIContent label)
        {
            if (TryGetShape(element, out var shape, out var nameProperty, out var wrapperBaseType))
                DrawField(position, element, label, shape, nameProperty, wrapperBaseType);
            else
                EditorGUI.HelpBox(position, UnsupportedFieldMessage, MessageType.Error);
        }

        // applyResolvedTypes keeps member-referenced base types live while the inspector is open. It runs long after
        // this call returns, so it works on a persistent copy: the property Unity hands a drawer is not guaranteed
        // to stay valid past CreatePropertyGUI.
        private VisualElement CreateField(
            string label,
            SerializedProperty property,
            FieldShape shape,
            SerializedProperty nameProperty,
            Type wrapperBaseType,
            out Action applyResolvedTypes)
        {
            var persistent = property.Persistent();

            switch (shape)
            {
                case FieldShape.ManagedReference:
                {
                    var element = SerializeReferenceUIToolkitPropertyDrawer.Draw(
                        label: label,
                        property: property,
                        baseTypes: GetTypesFromAttribute(property),
                        field: out var referenceField);

                    applyResolvedTypes = () => referenceField.SetBaseTypes(GetTypesFromAttribute(persistent));
                    return element;
                }

                case FieldShape.ManagedReferenceList:
                {
                    var list = new SerializeReferenceListField(
                        label: label,
                        property: property,
                        elementType: SerializeReferenceHelpers.GetArrayElementType(property),
                        baseTypes: GetTypesFromAttribute(property));

                    applyResolvedTypes = () => list.SetBaseTypes(GetTypesFromAttribute(persistent));
                    return list;
                }

                case FieldShape.List:
                {
                    // Every element shares the list's constraint, so it is resolved once here rather than per element.
                    var types = GetWrapperBaseTypes(property, wrapperBaseType);
                    var list = new TypeSelectorListField(label: label, property: property, createElementField: CreateElementField);

                    applyResolvedTypes = () =>
                    {
                        types = GetWrapperBaseTypes(persistent, wrapperBaseType);
                        list.Query<InspectorTypeField>().ForEach(field => field.Types = types);
                    };
                    return list;

                    VisualElement CreateElementField(SerializedProperty element) =>
                        TryGetShape(element, out var elementShape, out var elementNameProperty, out _)
                            ? CreateTypeField(element.displayName, element, elementShape, elementNameProperty, types, out _)
                            : new HelpBox(UnsupportedFieldMessage, HelpBoxMessageType.Error);
                }

                default:
                {
                    var element = CreateTypeField(
                        label: label,
                        property: property,
                        shape: shape,
                        nameProperty: nameProperty,
                        types: GetWrapperBaseTypes(property, wrapperBaseType),
                        field: out var typeField);

                    applyResolvedTypes = () => typeField.Types = GetWrapperBaseTypes(persistent, wrapperBaseType);
                    return element;
                }
            }
        }

        // A type-name, wrapper or MonoScript-wrapper field over already resolved types.
        private VisualElement CreateTypeField(
            string label,
            SerializedProperty property,
            FieldShape shape,
            SerializedProperty nameProperty,
            Type[] types,
            out InspectorTypeField field)
        {
            switch (shape)
            {
                case FieldShape.Wrapper:
                    return TypeUIToolkitPropertyDrawer.Draw(
                        label: label,
                        property: nameProperty,
                        allow: TypeSelector.Allow,
                        types: types,
                        field: out field);

                case FieldShape.MonoScriptWrapper:
                    return MonoScriptUIToolkitPropertyDrawer.Draw(
                        label: label,
                        wrapperProperty: property,
                        allow: TypeSelector.Allow,
                        types: types,
                        field: out field);

                default:
                    return TypeUIToolkitPropertyDrawer.Draw(
                        label: label,
                        property: property,
                        allow: TypeSelector.Allow,
                        types: types,
                        field: out field);
            }
        }

        private static float GetConstraintNoticeHeight(IReadOnlyList<string> warnings) =>
            warnings.Count > 0
                ? EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight
                : 0f;

        private static string GetNoticeMessage(IReadOnlyList<string> warnings) =>
            warnings.Count == 1
                ? "TypeSelector constraint could not be resolved"
                : $"{warnings.Count} TypeSelector constraints could not be resolved";

        private static string GetNoticeDetail(IReadOnlyList<string> warnings) => string.Join("\n", warnings);

        private bool TryGetShape(
            SerializedProperty property,
            out FieldShape shape,
            out SerializedProperty nameProperty,
            out Type wrapperBaseType)
        {
            nameProperty = null;
            wrapperBaseType = null;

            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    shape = FieldShape.String;
                    return true;

                case SerializedPropertyType.ManagedReference:
                    shape = FieldShape.ManagedReference;
                    return true;

                // The attribute applies to the collection. An empty one has no element to inspect, so it is judged by
                // its declared element type.
                case SerializedPropertyType.Generic when property.isArray:
                    if (SerializeReferenceHelpers.IsManagedReferenceArray(property))
                    {
                        shape = FieldShape.ManagedReferenceList;
                        return true;
                    }

                    shape = FieldShape.List;
                    var elementType = fieldInfo?.FieldType.GetCollectionElementTypeOrSelf();
                    if (elementType == typeof(string)) return true;
                    if (elementType is null || !SerializableTypeUtility.TryGetBaseType(elementType, out var elementBaseType))
                        return false;

                    wrapperBaseType = elementBaseType == typeof(object) ? null : elementBaseType;
                    return true;

                case SerializedPropertyType.Generic
                    when fieldInfo is not null
                         && SerializableTypeUtility.TryGetBaseType(fieldInfo.FieldType, out var baseType)
                         && SerializableTypeUtility.GetBackingProperty(property) is { } backing:
                    shape = SerializableMonoScriptUtility.IsMonoScriptWrapperField(fieldInfo.FieldType)
                        ? FieldShape.MonoScriptWrapper
                        : FieldShape.Wrapper;
                    nameProperty = backing;
                    wrapperBaseType = baseType == typeof(object) ? null : baseType;
                    return true;

                default:
                    shape = default;
                    return false;
            }
        }

        private Type[] GetWrapperBaseTypes(SerializedProperty property, Type wrapperBaseType)
        {
            var attributeTypes = GetTypesFromAttribute(property);
            if (wrapperBaseType is null) return attributeTypes;

            var types = new List<Type>(attributeTypes.Length + 1) { wrapperBaseType };
            types.AddRange(attributeTypes);
            return types.ToArray();
        }

        private Type[] GetTypesFromAttribute(SerializedProperty property)
        {
            if (TypeSelector.AssemblyQualifiedNames.Length is 0)
            {
                _constraintWarnings = Array.Empty<string>();
                return Array.Empty<Type>();
            }

            var resolution = TypeSelectorConstraintResolver.Resolve(property, TypeSelector.AssemblyQualifiedNames);

            // Overwrite (never ??=): a member-referenced constraint re-resolves while the inspector is
            // open, and the warnings must follow the latest resolution rather than freeze on the first.
            _constraintWarnings = resolution.Warnings;
            return resolution.Types;
        }

        private IReadOnlyList<string> GetConstraintWarnings(SerializedProperty property)
        {
            if (_constraintWarnings is null) GetTypesFromAttribute(property);
            return _constraintWarnings;
        }

        private enum FieldShape
        {
            String,
            Wrapper,
            MonoScriptWrapper,
            ManagedReference,
            ManagedReferenceList,
            List,
        }
    }
}
