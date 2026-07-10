using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Property drawer for <see cref="TypeSelectorAttribute"/>. Dispatches on the property kind: a
    /// <c>[SerializeReference]</c> managed reference gets the hierarchical instance selector (the attribute's
    /// base types optionally narrow the candidates below the declared field type), while a <c>string</c> field —
    /// or a <see cref="SerializableType"/> / <see cref="SerializableType{T}"/> container, whose backing
    /// <c>_assemblyQualifiedName</c> string this drawer targets — gets the assembly-qualified-name picker
    /// constrained by the attribute's base types (intersected with the generic argument <c>T</c>) and
    /// <see cref="TypeAllow"/>.
    /// <para>
    /// The attribute's string arguments are resolved by <see cref="TypeSelectorConstraintResolver"/> (member-first,
    /// then assembly-qualified name). Any argument that resolves to nothing surfaces as a quiet warning notice below
    /// the field.
    /// </para>
    /// </summary>
    [CustomPropertyDrawer(typeof(TypeSelectorAttribute))]
    internal sealed class TypeSelectorPropertyDrawer : PropertyDrawer
    {
        // Config warnings depend only on the attribute strings and the target type — both fixed for this drawer
        // instance — so they are resolved once (null until the first GetTypesFromAttribute call) and reused.
        private List<string> _constraintWarnings;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var warnings = GetConstraintWarnings(property);
            var noticeHeight = GetConstraintNoticeHeight(warnings);

            var fieldRect = position;
            fieldRect.height = position.height - noticeHeight;
            DrawField(fieldRect, property, label);

            if (noticeHeight <= 0f) return;

            var noticeRect = new Rect(position.x, fieldRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width, EditorGUIUtility.singleLineHeight);
            SerializeReferenceIMGUIPropertyDrawer.DrawRequiredNotice(noticeRect, GetNoticeMessage(warnings), GetNoticeDetail(warnings));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            GetFieldHeight(property) + GetConstraintNoticeHeight(GetConstraintWarnings(property));

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = CreateField(property);

            var warnings = GetConstraintWarnings(property);
            if (warnings.Count == 0) return field;

            var container = new VisualElement().AddChild(field);
            var notice = new SerializeReferenceNotice();
            notice.Set(
                message: GetNoticeMessage(warnings),
                actionText: string.Empty,
                detail: GetNoticeDetail(warnings),
                onAction: null);

            return container.AddChild(notice);
        }

        // IMGUI field portion (no config notice). Drawn into a rect of exactly GetFieldHeight, so each sub-drawer
        // receives its natural height at its natural origin regardless of the notice reserved below.
        private void DrawField(Rect position, SerializedProperty property, GUIContent label)
        {
            if (TryGetSerializableTypeContainer(property, out var nameProperty, out var genericBaseType))
            {
                TypeIMGUIPropertyDrawer.Draw(
                    position: position,
                    property: nameProperty,
                    label: label,
                    allow: GetTypeAllow(),
                    types: GetSerializableTypeBaseTypes(property, genericBaseType));
                return;
            }

            ThrowExceptionIfInvalidProperty(property);

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                SerializeReferenceIMGUIPropertyDrawer.Draw(
                    position: position,
                    label: label,
                    property: property,
                    baseTypes: GetTypesFromAttribute(property));
                return;
            }

            TypeIMGUIPropertyDrawer.Draw(
                position: position,
                property: property,
                label: label,
                allow: GetTypeAllow(),
                types: GetTypesFromAttribute(property));
        }

        // UIToolkit field portion (no config notice).
        private VisualElement CreateField(SerializedProperty property)
        {
            if (TryGetSerializableTypeContainer(property, out var nameProperty, out var genericBaseType))
            {
                return TypeUIToolkitPropertyDrawer.Draw(
                    label: preferredLabel,
                    property: nameProperty,
                    allow: GetTypeAllow(),
                    types: GetSerializableTypeBaseTypes(property, genericBaseType));
            }

            ThrowExceptionIfInvalidProperty(property);

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                return SerializeReferenceUIToolkitPropertyDrawer.Draw(
                    label: preferredLabel,
                    property: property,
                    baseTypes: GetTypesFromAttribute(property));
            }

            return TypeUIToolkitPropertyDrawer.Draw(
                label: preferredLabel,
                property: property,
                allow: GetTypeAllow(),
                types: GetTypesFromAttribute(property));
        }

        private float GetFieldHeight(SerializedProperty property)
        {
            if (TryGetSerializableTypeContainer(property, out var nameProperty, out _))
                return TypeIMGUIPropertyDrawer.GetHeight(nameProperty);

            return property.propertyType == SerializedPropertyType.ManagedReference
                ? SerializeReferenceIMGUIPropertyDrawer.GetHeight(property)
                : TypeIMGUIPropertyDrawer.GetHeight(property);
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

        private TypeAllow GetTypeAllow()
        {
            var typeSelectorAttribute = (TypeSelectorAttribute)attribute;
            return typeSelectorAttribute.Allow;
        }

        // A SerializableType / SerializableType<T> serializes as a Generic property with a child
        // _assemblyQualifiedName string. Unity picks this attribute drawer over SerializableTypePropertyDrawer,
        // so this is where [TypeSelector] on a SerializableType field is handled — we drive the string picker on
        // the child name property instead of throwing on the Generic kind. Detection reads the field type (arrays
        // and List<T> unwrapped to their element) so an element in a SerializableType[] / List<SerializableType> matches too.
        private bool TryGetSerializableTypeContainer(SerializedProperty property, out SerializedProperty nameProperty, out Type genericBaseType)
        {
            nameProperty = null;
            genericBaseType = null;

            if (property.propertyType is not SerializedPropertyType.Generic) return false;
            if (fieldInfo is null) return false;

            if (!SerializableTypeUtility.TryGetBaseType(fieldInfo.FieldType, out var baseType)) return false;

            // typeof(object) is the unconstrained wrapper's BaseType — nothing to narrow the picker with.
            genericBaseType = baseType == typeof(object) ? null : baseType;
            nameProperty = property.FindPropertyRelative("_assemblyQualifiedName");
            return nameProperty is not null;
        }

        // Base types for a SerializableType picker: the attribute's types plus the generic argument T (for
        // SerializableType<T>). The picker intersects — a candidate must be assignable to ALL entries
        // (TypeInfo.GetAllTypeInfos) — so appending T narrows the attribute's set to T's subtypes.
        private Type[] GetSerializableTypeBaseTypes(SerializedProperty property, Type genericBaseType)
        {
            var attributeTypes = GetTypesFromAttribute(property);
            if (genericBaseType is null) return attributeTypes;

            var types = new List<Type>(attributeTypes.Length + 1) { genericBaseType };
            types.AddRange(attributeTypes);
            return types.ToArray();
        }

        // Resolves the attribute's base types against the target object each call (member values can change between
        // repaints); the accompanying config warnings are computed once and cached in _constraintWarnings.
        private Type[] GetTypesFromAttribute(SerializedProperty property)
        {
            var typeSelectorAttribute = (TypeSelectorAttribute)attribute;

            if (typeSelectorAttribute.AssemblyQualifiedNames.Length is 0)
            {
                _constraintWarnings ??= new List<string>();
                return Array.Empty<Type>();
            }

            var targetObject = property.serializedObject.targetObject;

            if (_constraintWarnings is null)
            {
                var warnings = new List<string>();
                var typesWithWarnings = TypeSelectorConstraintResolver.Resolve(
                    typeSelectorAttribute.AssemblyQualifiedNames, targetObject, warnings);
                _constraintWarnings = warnings;
                return typesWithWarnings;
            }

            return TypeSelectorConstraintResolver.Resolve(
                typeSelectorAttribute.AssemblyQualifiedNames, targetObject);
        }

        private IReadOnlyList<string> GetConstraintWarnings(SerializedProperty property)
        {
            if (_constraintWarnings is null) GetTypesFromAttribute(property);
            return _constraintWarnings;
        }

        private static void ThrowExceptionIfInvalidProperty(SerializedProperty property)
        {
            if (property.propertyType is not (SerializedPropertyType.String or SerializedPropertyType.ManagedReference))
            {
                throw new ArgumentException(
                    "[TypeSelector] can only be applied to a string field, a SerializableType / SerializableType<T> field, " +
                    "or a [SerializeReference] managed-reference field.",
                    nameof(property));
            }
        }
    }
}
