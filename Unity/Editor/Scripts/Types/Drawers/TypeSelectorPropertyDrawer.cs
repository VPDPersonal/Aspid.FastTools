using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEngine.UIElements;
using System.Collections.Generic;
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
    /// </summary>
    [CustomPropertyDrawer(typeof(TypeSelectorAttribute))]
    internal sealed class TypeSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (TryGetSerializableTypeContainer(property, out var nameProperty, out _))
                return TypeIMGUIPropertyDrawer.GetHeight(nameProperty);

            return property.propertyType == SerializedPropertyType.ManagedReference
                ? SerializeReferenceIMGUIPropertyDrawer.GetHeight(property)
                : TypeIMGUIPropertyDrawer.GetHeight(property);
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
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

        private Type[] GetTypesFromAttribute(SerializedProperty property)
        {
            var typeSelectorAttribute = (TypeSelectorAttribute)attribute;

            var assemblyQualifiedNames = typeSelectorAttribute.AssemblyQualifiedNames
                .Where(assemblyQualifiedName => !string.IsNullOrWhiteSpace(assemblyQualifiedName))
                .ToArray();

            if (assemblyQualifiedNames.Length is 0)
                return Array.Empty<Type>();

            var targetObject = property.serializedObject.targetObject;
            var targetType = targetObject.GetType();
            var types = new List<Type>();

            foreach (var name in assemblyQualifiedNames)
            {
                var member = GetMemberFromHierarchy(targetType, name);

                if (member is not null)
                {
                    AddTypesFromMember(types, member, targetObject);
                }
                else
                {
                    var type = Type.GetType(name, throwOnError: false);

                    if (type is not null)
                        types.Add(type);
                }
            }

            return types.ToArray();
        }

        private static MemberInfo GetMemberFromHierarchy(Type type, string memberName)
        {
            const BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

            var currentType = type;
            while (currentType is not null)
            {
                var members = currentType.GetMember(memberName, bindingAttr);
                if (members.Length > 0)
                    return members[0];

                currentType = currentType.BaseType;
            }

            return null;
        }

        private static void AddTypesFromMember(List<Type> types, MemberInfo member, object targetObject)
        {
            var value = member switch
            {
                FieldInfo fieldInfo => fieldInfo.GetValue(targetObject),
                PropertyInfo propertyInfo => propertyInfo.GetValue(targetObject),
                _ => null
            };

            switch (value)
            {
                case null: return;

                case Type type:
                    types.Add(type);
                    return;

                case Type[] typeArray:
                    types.AddRange(typeArray.Where(t => t is not null));
                    return;

                case string assemblyQualifiedName:
                {
                    var type = Type.GetType(assemblyQualifiedName, throwOnError: false);
                    if (type is not null)
                        types.Add(type);
                    return;
                }

                case string[] assemblyQualifiedNames:
                {
                    foreach (var assemblyQualifiedName in assemblyQualifiedNames)
                    {
                        if (string.IsNullOrWhiteSpace(assemblyQualifiedName)) continue;

                        var type = Type.GetType(assemblyQualifiedName, throwOnError: false);
                        if (type is not null)
                            types.Add(type);
                    }
                    return;
                }
            }
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
