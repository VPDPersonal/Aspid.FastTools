using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Aspid.FastTools.Types;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Shared logic for the <c>[TypeSelector(Required = true)]</c> marker: detecting whether a property carries it (via
    /// the field reflected from the property path) and whether it is currently violated (a genuinely empty value). Used
    /// by the inspector notice and by the build/CI gate's per-property check. Applies to a <c>[SerializeReference]</c>
    /// managed reference (empty == null), a <c>string</c> type field (empty == null-or-empty), and a
    /// <see cref="SerializableType"/> field (empty == its nested <c>_assemblyQualifiedName</c> is null-or-empty; the
    /// attribute is resolved from the wrapper field, and violation is checked on its backing string).
    /// </summary>
    internal static class SerializeReferenceRequiredGate
    {
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags DeclaredFieldFlags = FieldFlags | BindingFlags.DeclaredOnly;

        // The backing string field a SerializableType / SerializableType<T> serializes its type name into.
        private const string SerializableTypeNameField = "_assemblyQualifiedName";

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

        /// <summary>
        /// Reflects the serialized fields of <paramref name="type"/> that opt into the required check
        /// (<c>[TypeSelector(Required = true)]</c>), classifying each as a <c>string</c> field, a
        /// <see cref="SerializableType"/> wrapper or a <c>[SerializeReference]</c> managed reference. Drives the
        /// pure-YAML scene scan, which needs the field keys and kinds without a live <see cref="SerializedObject"/>.
        /// Recurses into plain <c>[Serializable]</c> by-value containers (their fields nest as child keys in YAML),
        /// recording the container chain in <see cref="RequiredFieldDescriptor.Parents"/>. Collections of containers
        /// and fields behind a <c>[SerializeReference]</c> hop are out of scope — their values live outside the
        /// document's top-level mapping (indexed elements / <c>RefIds</c> data). Cached per type (stable until a
        /// domain reload).
        /// </summary>
        public static IReadOnlyList<RequiredFieldDescriptor> GetRequiredFields(Type type)
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

        // The reflected field is stable per (type, path) until a domain reload; IsViolation/TryGetRequired run every
        // IMGUI repaint, so each path is reflected only once. Paths crossing a [SerializeReference] hop resolve
        // through the live instance's runtime type, which can differ per object — those bypass the cache.
        private static readonly Dictionary<(Type, string), FieldInfo> ResolvedFieldCache = new();

        // For a list/array element the resolved field is the collection itself, matching PropertyDrawer.fieldInfo.
        private static FieldInfo ResolveFieldInfo(SerializedProperty property)
        {
            var type = property.serializedObject?.targetObject?.GetType();
            if (type is null) return null;

            var cacheKey = (type, property.propertyPath);
            if (ResolvedFieldCache.TryGetValue(cacheKey, out var cachedField)) return cachedField;

            var field = ResolveFieldInfoUncached(type, property, out var cacheable);
            if (cacheable) ResolvedFieldCache[cacheKey] = field;
            return field;
        }

        private static FieldInfo ResolveFieldInfoUncached(Type targetType, SerializedProperty property, out bool cacheable)
        {
            cacheable = true;
            var type = targetType;

            // "_slots.Array.data[0]._weapon" -> "_slots[0]._weapon"
            var path = property.propertyPath.Replace(".Array.data[", "[");
            var segments = path.Split('.');

            FieldInfo field = null;
            FieldInfo previousField = null;
            string traversedPath = null;

            for (var i = 0; i < segments.Length; i++)
            {
                previousField = field;

                var segment = segments[i];
                var bracket = segment.IndexOf('[');
                var isElement = bracket >= 0;

                // Rebuild the original property path up to this segment ("[" only ever comes from the simplification).
                var originalSegment = segment.Replace("[", ".Array.data[");
                traversedPath = traversedPath is null ? originalSegment : traversedPath + "." + originalSegment;

                if (isElement) segment = segment[..bracket];

                field = GetFieldIncludingBase(type, segment);
                if (field is null) return null;

                type = isElement ? GetElementType(field.FieldType) : field.FieldType;
                if (type is null) return null;

                // A [SerializeReference] hop is polymorphic: the next segment's field lives on the instance's runtime
                // type (a derived class or an interface implementation), not on the declared field type — walk on from
                // the live managed reference instead. Falls back to the declared type when no instance is loaded.
                if (i == segments.Length - 1 || !field.IsDefined(typeof(SerializeReference), inherit: false)) continue;

                cacheable = false;
                using var hop = property.serializedObject.FindProperty(traversedPath);
                if (hop is { propertyType: SerializedPropertyType.ManagedReference, managedReferenceValue: not null })
                    type = hop.managedReferenceValue.GetType();
            }

            // A required [TypeSelector] lives on the SerializableType field, not on the wrapper's backing
            // _assemblyQualifiedName string the drawer targets — redirect to the parent field so the attribute is found.
            if (previousField is not null &&
                field.Name == SerializableTypeNameField &&
                IsSerializableTypeField(previousField.FieldType))
                return previousField;

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

        // True for an ISerializableType wrapper field, or an array / List<T> of them.
        private static bool IsSerializableTypeField(Type fieldType) =>
            SerializableTypeUtility.IsSerializableTypeField(fieldType);
    }
}
