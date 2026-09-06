using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.SerializeReferences.Editors;

using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // Four field shapes: a string (the picked assembly-qualified name is stored), a SerializableType / SerializableType<T>
    // wrapper and a SerializableMonoScript / SerializableMonoScript<T> wrapper (the attribute's constraints intersect the
    // wrapper's) and a [SerializeReference] managed reference (the picked type is instantiated). Any other shape renders
    // an error box instead of the field.
    [CustomPropertyDrawer(typeof(TypeSelectorAttribute))]
    internal sealed class TypeSelectorPropertyDrawer : PropertyDrawer
    {
        private const string UnsupportedFieldMessage =
            "[TypeSelector] can only be applied to a string field, a SerializableType / SerializableMonoScript field " +
            "(plain or <T>), or a [SerializeReference] managed-reference field.";

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
                return EditorGUIUtility.singleLineHeight * 2f;

            var fieldHeight = shape switch
            {
                FieldShape.Wrapper => TypeIMGUIPropertyDrawer.GetHeight(nameProperty),
                FieldShape.MonoScriptWrapper => MonoScriptIMGUIPropertyDrawer.GetHeight(property),
                FieldShape.ManagedReference => SerializeReferenceIMGUIPropertyDrawer.GetHeight(property),
                _ => TypeIMGUIPropertyDrawer.GetHeight(property),
            };

            return fieldHeight + GetConstraintNoticeHeight(GetConstraintWarnings(property));
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (!TryGetShape(property, out var shape, out var nameProperty, out var wrapperBaseType))
                return new HelpBox(UnsupportedFieldMessage, HelpBoxMessageType.Error);

            var field = CreateField(property, shape, nameProperty, wrapperBaseType, out var applyResolvedTypes);

            // Without string arguments the constraint is static — nothing to re-resolve, no warnings possible.
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

        // applyResolvedTypes keeps member-referenced base types live while the inspector is open. It runs long after
        // this call returns, so it works on a persistent copy: the property Unity hands a drawer is not guaranteed
        // to stay valid past CreatePropertyGUI.
        private VisualElement CreateField(
            SerializedProperty property,
            FieldShape shape,
            SerializedProperty nameProperty,
            Type wrapperBaseType,
            out Action applyResolvedTypes)
        {
            var persistent = property.Persistent();

            switch (shape)
            {
                case FieldShape.Wrapper:
                {
                    var element = TypeUIToolkitPropertyDrawer.Draw(
                        label: preferredLabel,
                        property: nameProperty,
                        allow: TypeSelector.Allow,
                        types: GetWrapperBaseTypes(property, wrapperBaseType),
                        field: out var wrapperTypeField);

                    applyResolvedTypes = () => wrapperTypeField.Types = GetWrapperBaseTypes(persistent, wrapperBaseType);
                    return element;
                }

                case FieldShape.MonoScriptWrapper:
                {
                    var element = MonoScriptUIToolkitPropertyDrawer.Draw(
                        label: preferredLabel,
                        wrapperProperty: property,
                        allow: TypeSelector.Allow,
                        types: GetWrapperBaseTypes(property, wrapperBaseType),
                        field: out var monoScriptField);

                    applyResolvedTypes = () => monoScriptField.Types = GetWrapperBaseTypes(persistent, wrapperBaseType);
                    return element;
                }

                case FieldShape.ManagedReference:
                {
                    var element = SerializeReferenceUIToolkitPropertyDrawer.Draw(
                        label: preferredLabel,
                        property: property,
                        baseTypes: GetTypesFromAttribute(property),
                        field: out var referenceField);

                    applyResolvedTypes = () => referenceField.SetBaseTypes(GetTypesFromAttribute(persistent));
                    return element;
                }

                default:
                {
                    var element = TypeUIToolkitPropertyDrawer.Draw(
                        label: preferredLabel,
                        property: property,
                        allow: TypeSelector.Allow,
                        types: GetTypesFromAttribute(property),
                        field: out var stringTypeField);

                    applyResolvedTypes = () => stringTypeField.Types = GetTypesFromAttribute(persistent);
                    return element;
                }
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

        // Classifies the property. For a wrapper (SerializableType or SerializableMonoScript, plain or <T>), nameProperty
        // is the backing type-name string and wrapperBaseType the wrapper's own constraint (null when unconstrained).
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

            var resolution = TypeSelectorConstraintResolver.Resolve(
                property.serializedObject.targetObject, TypeSelector.AssemblyQualifiedNames);

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
        }
    }
}
