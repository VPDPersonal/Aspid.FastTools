using System;
using UnityEditor;
using System.Reflection;
using Aspid.FastTools.Types;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Shared logic for the <c>[TypeSelector(Required = true)]</c> marker: detecting whether a property carries it (via
    /// the field reflected from the property path) and whether it is currently violated (a genuinely empty value). Used
    /// by the inspector notice and by the build/CI gate's per-property check. Applies to both a <c>[SerializeReference]</c>
    /// managed reference (empty == null) and a <c>string</c> type field (empty == null-or-empty).
    /// </summary>
    internal static class SerializeReferenceRequiredGate
    {
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Resolves the <c>[TypeSelector]</c> attribute on this property's declared field when it opts in with
        /// <see cref="TypeSelectorAttribute.Required"/>; returns <see langword="false"/> otherwise.
        /// </summary>
        public static bool TryGetRequired(SerializedProperty property, out TypeSelectorAttribute selector)
        {
            selector = null;
            if (property is null) return false;
            if (property.propertyType is not (SerializedPropertyType.ManagedReference or SerializedPropertyType.String))
                return false;

            var typeSelector = ResolveFieldInfo(property)?.GetCustomAttribute<TypeSelectorAttribute>();
            if (typeSelector is null || !typeSelector.Required) return false;

            selector = typeSelector;
            return true;
        }

        /// <summary>
        /// True when the property is required and currently unset. For a managed reference that means an empty value
        /// (a missing-type reference is NOT a required violation — it has its own notice/gate); for a string type field
        /// it means a null-or-empty assembly-qualified name.
        /// </summary>
        public static bool IsViolation(SerializedProperty property)
        {
            if (!TryGetRequired(property, out _)) return false;

            return property.propertyType switch
            {
                SerializedPropertyType.ManagedReference =>
                    !SerializeReferenceHelpers.IsMissingType(property) && property.managedReferenceValue is null,
                SerializedPropertyType.String => string.IsNullOrEmpty(property.stringValue),
                _ => false,
            };
        }

        // Walks the property path against the target object's type to find the declared field (which carries the
        // attribute). For a list/array element the field is the collection itself, matching PropertyDrawer.fieldInfo.
        private static FieldInfo ResolveFieldInfo(SerializedProperty property)
        {
            var type = property.serializedObject?.targetObject?.GetType();
            if (type is null) return null;

            // "_slots.Array.data[0]._weapon" -> "_slots[0]._weapon"
            var path = property.propertyPath.Replace(".Array.data[", "[");
            FieldInfo field = null;

            foreach (var rawSegment in path.Split('.'))
            {
                var segment = rawSegment;
                var bracket = segment.IndexOf('[');
                var isElement = bracket >= 0;
                if (isElement) segment = segment[..bracket];

                field = GetFieldIncludingBase(type, segment);
                if (field is null) return null;

                type = isElement ? GetElementType(field.FieldType) : field.FieldType;
                if (type is null) return null;
            }

            return field;
        }

        private static FieldInfo GetFieldIncludingBase(Type type, string name)
        {
            for (var current = type; current is not null; current = current.BaseType)
            {
                var field = current.GetField(name, FieldFlags);
                if (field is not null) return field;
            }

            return null;
        }

        private static Type GetElementType(Type collectionType)
        {
            if (collectionType.IsArray) return collectionType.GetElementType();
            if (collectionType.IsGenericType)
            {
                var args = collectionType.GetGenericArguments();
                if (args.Length == 1) return args[0];
            }

            return collectionType;
        }
    }
}
