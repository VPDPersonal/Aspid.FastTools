using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // Detects whether a property carries [TypeSelector(Required = true)] and whether it is currently violated,
    // for the inspector notice and the build/CI gate alike. "Empty" means null for a managed reference and a
    // null-or-empty string for a type-name field; a SerializableType resolves its attribute from the wrapper field
    // and checks the violation on the backing string.
    internal static class TypeSelectorRequiredGate
    {
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags DeclaredFieldFlags = FieldFlags | BindingFlags.DeclaredOnly;

        // Resolves the [TypeSelector] attribute on this property's declared field when it opts in with
        // TypeSelectorAttribute.Required; returns false otherwise.
        internal static bool TryGetRequired(SerializedProperty property, out TypeSelectorAttribute selector)
        {
            selector = null;
            if (property is null) return false;
            if (property.propertyType is not (SerializedPropertyType.ManagedReference or SerializedPropertyType.String))
                return false;

            var typeSelector = GetAttributeField(property)?.GetCustomAttribute<TypeSelectorAttribute>();
            if (typeSelector is null || !typeSelector.Required) return false;

            selector = typeSelector;
            return true;
        }

        // The field carrying the property's user attributes — normally its own backing field, but the string
        // nested inside a SerializableType wrapper redirects to the wrapper field, where they are declared.
        private static FieldInfo GetAttributeField(SerializedProperty property)
        {
            var field = property.GetFieldInfo();
            if (field?.Name != SerializableTypeUtility.BackingFieldName) return field;

            // The property targets the string inside the wrapper — its parent property is the wrapper field itself.
            var path = property.propertyPath;
            var lastDotIndex = path.LastIndexOf('.');
            if (lastDotIndex < 0) return field;

            using var parentProperty = property.serializedObject.FindProperty(path[..lastDotIndex]);
            var parentField = parentProperty?.GetFieldInfo();

            return parentField is not null && IsSerializableTypeField(parentField.FieldType)
                ? parentField
                : field;
        }

        // True when the property is required and currently unset. For a managed reference that means an empty value
        // (a missing-type reference is NOT a required violation — it has its own notice/gate); for a string type field
        // it means a null-or-empty assembly-qualified name.
        internal static bool IsViolation(SerializedProperty property)
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

        // The serialized fields opting into the required check, classified by kind — what the pure-YAML scene scan
        // needs without a live SerializedObject. It recurses into plain by-value containers, whose fields nest as
        // child keys, recording the chain in Parents. Collections of containers and fields behind a
        // [SerializeReference] hop are out of scope, since their values live outside the document's top-level
        // mapping. Cached per type, which is stable until a domain reload.
        internal static IReadOnlyList<RequiredFieldDescriptor> GetRequiredFields(Type type)
        {
            if (type is null) return Array.Empty<RequiredFieldDescriptor>();
            if (RequiredFieldCache.TryGetValue(type, out var cached)) return cached;

            var result = new List<RequiredFieldDescriptor>();
            CollectRequiredFields(type, Array.Empty<string>(), new HashSet<Type>(), new HashSet<string>(StringComparer.Ordinal), result);

            IReadOnlyList<RequiredFieldDescriptor> readOnly = result;
            RequiredFieldCache[type] = readOnly;
            return readOnly;
        }

        // Unity refuses deeper by-value nesting at serialize time ("Serialization depth limit 10 exceeded"), so a
        // longer container chain cannot exist in a saved file.
        private const int MaxContainerDepth = 10;

        private static void CollectRequiredFields(
            Type type, string[] parents, HashSet<Type> visiting, HashSet<string> seen, List<RequiredFieldDescriptor> result)
        {
            // The visiting set prunes self-referential container shapes; Unity cannot serialize them, but the raw
            // reflected type still declares the cycle and the walk must terminate on it.
            if (parents.Length >= MaxContainerDepth || !visiting.Add(type)) return;

            try
            {
                // Walk the hierarchy declared-only per level so a base field is read once; a `new`-shadowed name (one
                // YAML key) is de-duplicated by its full path so it is never reported twice.
                for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
                {
                    foreach (var field in current.GetFields(DeclaredFieldFlags))
                    {
                        var selector = field.GetCustomAttribute<TypeSelectorAttribute>();
                        if (selector is not null && selector.Required)
                        {
                            if (!seen.Add(parents.Length == 0 ? field.Name : string.Join(".", parents) + "." + field.Name))
                                continue;

                            if (field.FieldType == typeof(string))
                                result.Add(new RequiredFieldDescriptor(parents, field.Name, RequiredFieldKind.String));
                            else if (IsSerializableTypeField(field.FieldType))
                                result.Add(new RequiredFieldDescriptor(parents, field.Name, RequiredFieldKind.SerializableType));
                            else if (field.IsDefined(typeof(SerializeReference), inherit: false))
                                result.Add(new RequiredFieldDescriptor(parents, field.Name, RequiredFieldKind.ManagedReference));
                            // A required [TypeSelector] on any other shape is a misuse the analyzer flags; skip it here.

                            continue;
                        }

                        if (!IsSerializedContainerField(field)) continue;

                        var childParents = new string[parents.Length + 1];
                        Array.Copy(parents, childParents, parents.Length);
                        childParents[parents.Length] = field.Name;
                        CollectRequiredFields(field.FieldType, childParents, visiting, seen, result);
                    }
                }
            }
            finally
            {
                visiting.Remove(type);
            }
        }

        // A field Unity serializes by value as a nested mapping — the only shape whose children the YAML scan can
        // address by key. [SerializeReference] hops (RefIds), collections (indexed elements), UnityEngine.Object
        // references (external pointers) and the SerializableType wrapper (a leaf) are all excluded.
        private static bool IsSerializedContainerField(FieldInfo field)
        {
            if (field.IsNotSerialized) return false;
            if (!field.IsPublic && !field.IsDefined(typeof(SerializeField), inherit: false)) return false;
            if (field.IsDefined(typeof(SerializeReference), inherit: false)) return false;

            var type = field.FieldType;
            if (type.IsPrimitive || type.IsEnum || type == typeof(string)) return false;
            if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))) return false;
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return false;
            if (IsSerializableTypeField(type)) return false;

            return type.IsDefined(typeof(SerializableAttribute), inherit: false);
        }

        // Per-type memo for GetRequiredFields — the reflected field set is stable until a domain reload clears statics.
        private static readonly Dictionary<Type, IReadOnlyList<RequiredFieldDescriptor>> RequiredFieldCache = new();

        private static bool IsSerializableTypeField(Type fieldType) =>
            SerializableTypeUtility.IsSerializableTypeField(fieldType);
    }
}
