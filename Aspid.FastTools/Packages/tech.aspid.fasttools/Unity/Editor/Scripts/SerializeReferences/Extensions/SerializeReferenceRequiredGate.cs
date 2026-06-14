using System;
using UnityEditor;
using System.Reflection;
using Aspid.FastTools.Types;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Shared logic for the <c>[SerializeReferenceRequired]</c> marker: detecting whether a property carries it (via the
    /// field reflected from the property path) and whether it is currently violated (a genuinely empty reference). Used
    /// by the inspector notice and by the build/CI gate's per-property check.
    /// </summary>
    internal static class SerializeReferenceRequiredGate
    {
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>Resolves the <c>[SerializeReferenceRequired]</c> attribute on this property's declared field, if any.</summary>
        public static bool TryGetRequired(SerializedProperty property, out SerializeReferenceRequiredAttribute required)
        {
            required = null;
            if (property is null || property.propertyType != SerializedPropertyType.ManagedReference) return false;

            var field = ResolveFieldInfo(property);
            required = field?.GetCustomAttribute<SerializeReferenceRequiredAttribute>();
            return required is not null;
        }

        /// <summary>
        /// True when the property is required and currently unset (its managed reference is empty). A missing-type
        /// reference is NOT a required violation here — it has its own notice/gate.
        /// </summary>
        public static bool IsViolation(SerializedProperty property)
        {
            if (!TryGetRequired(property, out _)) return false;
            if (SerializeReferenceHelpers.IsMissingType(property)) return false; // handled by the missing notice
            return property.managedReferenceValue is null;
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
