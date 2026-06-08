using System;
using UnityEngine;
using UnityEditor;
using Aspid.FastTools.Types;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Types.Editors;
using System.Runtime.Serialization;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Shared helpers for the <c>[SerializeReferenceSelector]</c> drawers: resolving the declared
    /// managed-reference field type, filtering candidate types, instantiating the selected type, and
    /// parsing Unity's managed-reference type-name format. The open-generic argument flow itself lives in
    /// the shared <see cref="Aspid.FastTools.Types.Editors.GenericTypeResolver"/> /
    /// <see cref="Aspid.FastTools.Types.Editors.TypeSelectorWindow"/>; <see cref="IsValidGenericArgument"/>
    /// is supplied to the selector as its argument filter.
    /// </summary>
    internal static class SerializeReferenceHelpers
    {
        /// <summary>
        /// Resolves the declared element type of a managed-reference property — the base type that
        /// constrains the candidate list. Uses <see cref="SerializedProperty.managedReferenceFieldTypename"/>,
        /// which already reports the element type for array/list entries.
        /// </summary>
        public static Type GetFieldType(SerializedProperty property) =>
            GetTypeFromTypename(property.managedReferenceFieldTypename) ?? typeof(object);

        /// <summary>
        /// Resolves the concrete type currently stored in the managed reference, or <see langword="null"/>
        /// when the reference is empty or its stored type can no longer be loaded.
        /// </summary>
        public static Type GetCurrentType(SerializedProperty property) =>
            property.managedReferenceValue?.GetType();

        /// <summary>
        /// Returns <see langword="true"/> when this property holds a managed reference whose type can no longer be
        /// loaded (renamed / moved / deleted). Unity does not expose a missing type through the per-property API
        /// (the value reads back <see langword="null"/> and <see cref="SerializedProperty.managedReferenceFullTypename"/>
        /// is empty) and even drops it from the live object on prefabs / GameObjects, so detection reads the stored
        /// reference straight from the asset YAML: a null value whose recorded type cannot be resolved is missing.
        /// </summary>
        public static bool IsMissingType(SerializedProperty property) =>
            TryGetMissingType(property, out _, out _);

        // Core missing-type probe shared by the public helpers: reads the property's stored id and type from the
        // asset YAML and reports it missing when the recorded type no longer resolves to a loadable Type.
        private static bool TryGetMissingType(SerializedProperty property, out long referenceId, out ManagedTypeName storedType)
        {
            referenceId = 0;
            storedType = default;

            if (property.propertyType != SerializedPropertyType.ManagedReference) return false;
            if (property.managedReferenceValue is not null) return false;
            if (!TryGetAssetLocation(property, out var assetPath, out var fileId)) return false;
            if (!SerializeReferenceYamlEditor.TryReadStoredType(assetPath, fileId, property.propertyPath, out referenceId, out storedType))
                return false;

            return !storedType.IsEmpty && !StoredTypeResolves(storedType);
        }

        // True when the YAML-recorded type identity can be loaded — i.e. the reference is intact, not missing.
        public static bool StoredTypeResolves(ManagedTypeName name)
        {
            if (string.IsNullOrEmpty(name.Class)) return false;

            var className = name.Class.Replace('/', '+');
            var fullName = string.IsNullOrEmpty(name.Namespace) ? className : $"{name.Namespace}.{className}";
            var assemblyQualified = string.IsNullOrEmpty(name.Assembly) ? fullName : $"{fullName}, {name.Assembly}";

            return Type.GetType(assemblyQualified, throwOnError: false) is not null;
        }

        /// <summary>
        /// Predicate identifying types that can legally be assigned to a <c>[SerializeReference]</c> field:
        /// concrete reference types that are neither <see cref="Object"/>, open generics, strings, nor delegates.
        /// </summary>
        public static bool IsAssignableManagedReference(Type type) =>
            type is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false } &&
            type != typeof(string) &&
            !typeof(Object).IsAssignableFrom(type) &&
            !typeof(Delegate).IsAssignableFrom(type);

        /// <summary>
        /// Creates an instance of <paramref name="type"/> for assignment to a managed reference.
        /// Prefers a (public or non-public) parameterless constructor so field initializers run, and
        /// falls back to an uninitialized instance for types that expose no parameterless constructor.
        /// </summary>
        public static object CreateInstance(Type type)
        {
            if (type is null) return null;

            try
            {
                return Activator.CreateInstance(type, nonPublic: true);
            }
            catch (MissingMethodException)
            {
                return FormatterServices.GetUninitializedObject(type);
            }
        }

        /// <summary>
        /// Creates an instance of <paramref name="newType"/> and carries over the data of <paramref name="previous"/>
        /// for every field the two types share by name and serialized shape. Mirrors Unity's own type-change
        /// behaviour: the old value is serialized to JSON and overwritten onto the new instance, so matching fields
        /// survive a type switch (e.g. a shared <c>_radius</c>) while the rest fall back to the new type's defaults.
        /// A structural mismatch simply leaves the new instance untouched.
        /// </summary>
        public static object CreateInstancePreservingData(Type newType, object previous)
        {
            var instance = CreateInstance(newType);
            if (instance is null || previous is null) return instance;

            try
            {
                var json = JsonUtility.ToJson(previous);
                if (!string.IsNullOrEmpty(json) && json != "{}")
                    JsonUtility.FromJsonOverwrite(json, instance);
            }
            catch (Exception)
            {
                // Best effort: incompatible layouts just mean nothing is carried over.
            }

            return instance;
        }

        /// <summary>
        /// Parses Unity's managed-reference type-name format (<c>"AssemblyName Namespace.TypeName"</c>)
        /// into a <see cref="Type"/>, or <see langword="null"/> when it is empty or cannot be loaded.
        /// </summary>
        public static Type GetTypeFromTypename(string typename)
        {
            if (string.IsNullOrEmpty(typename)) return null;

            var separator = typename.IndexOf(' ');
            if (separator < 0) return Type.GetType(typename, throwOnError: false);

            var assembly = typename[..separator];
            var fullName = typename[(separator + 1)..];
            return Type.GetType($"{fullName}, {assembly}", throwOnError: false);
        }

        /// <summary>
        /// Predicate identifying types usable as a generic argument of a serialized managed reference:
        /// concrete, non-generic types Unity can serialize as a field value (primitives, <see cref="string"/>,
        /// enums, <see cref="Object"/>-derived references, or <c>[Serializable]</c> structs/classes). Passed to
        /// <see cref="Aspid.FastTools.Types.Editors.TypeSelectorWindow.Show"/> as the argument filter.
        /// </summary>
        public static bool IsValidGenericArgument(Type type)
        {
            if (type is null) return false;
            if (type.IsAbstract || type.IsInterface || type.ContainsGenericParameters) return false;
            if (typeof(Delegate).IsAssignableFrom(type)) return false;

            return type.IsPrimitive ||
                   type.IsEnum ||
                   type == typeof(string) ||
                   typeof(Object).IsAssignableFrom(type) ||
                   (type.IsValueType && type.IsSerializable) ||
                   (type.IsClass && type.IsSerializable);
        }

        #region Missing-type repair
        /// <summary>
        /// Resolves the stored (now unloadable) type identity of this property's missing managed reference, read from
        /// the asset YAML, for display in the caption / warning. Returns <see langword="default"/> when the property
        /// is not a recognised missing reference.
        /// </summary>
        public static ManagedTypeName GetMissingTypeName(SerializedProperty property) =>
            TryGetMissingType(property, out _, out var storedType) ? storedType : default;

        /// <summary>
        /// Human-readable <c>Namespace.Class</c> of this property's missing type, for the dropdown caption and the
        /// warning message, or an empty string when the property is not a recognised missing reference.
        /// </summary>
        public static string GetMissingTypeDisplayName(SerializedProperty property)
        {
            var name = GetMissingTypeName(property);
            if (name.IsEmpty) return string.Empty;
            return string.IsNullOrEmpty(name.Namespace) ? name.Class : $"{name.Namespace}.{name.Class}";
        }

        /// <summary>
        /// Resolves the on-disk asset path and the target object's local file id (the YAML document anchor) backing
        /// this property. Returns <see langword="false"/> for scene objects and prefab instances, which have no
        /// editable asset file — the YAML repair flow only applies to saved assets (ScriptableObjects, prefabs).
        /// </summary>
        public static bool TryGetAssetLocation(SerializedProperty property, out string assetPath, out long fileId)
        {
            fileId = 0;
            var target = property.serializedObject.targetObject;
            assetPath = AssetDatabase.GetAssetPath(target);

            if (string.IsNullOrEmpty(assetPath)) return false;
            return AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out _, out fileId);
        }

        /// <summary>
        /// Finds the <c>RefIds</c> id of the missing managed reference this property points at, read from the asset
        /// YAML. Detection is strict and per-property: only a field whose own recorded type fails to resolve counts
        /// as missing, so legitimately-empty fields are never flagged.
        /// </summary>
        public static bool TryGetMissingReferenceId(SerializedProperty property, out long referenceId) =>
            TryGetMissingType(property, out referenceId, out _);

        /// <summary>
        /// Opens the same hierarchical type picker the dropdown uses, anchored at <paramref name="screenRect"/>, to
        /// choose the existing type a missing reference should resolve to. The chosen type is written into the asset
        /// YAML (re-pointing the reference and keeping its stored data); <paramref name="onFixed"/> runs on success.
        /// </summary>
        public static void ShowFixTypeSelector(SerializedProperty property, Rect screenRect, Action onFixed)
        {
            var fieldType = GetFieldType(property);

            TypeSelectorWindow.Show(
                screenRect: screenRect,
                types: new[] { fieldType },
                currentAqn: string.Empty,
                allow: TypeAllow.None,
                onSelected: assemblyQualifiedName =>
                {
                    var type = string.IsNullOrEmpty(assemblyQualifiedName)
                        ? null
                        : Type.GetType(assemblyQualifiedName, throwOnError: false);

                    if (type is not null && TryFixMissingType(property, ManagedTypeName.FromType(type)))
                        onFixed?.Invoke();
                },
                filter: IsAssignableManagedReference,
                additionalTypes: GenericTypeResolver.GetAssignableGenericDefinitions(fieldType),
                argumentFilter: IsValidGenericArgument);
        }

        /// <summary>
        /// Rewrites the missing type of this property to <paramref name="newType"/> directly in the asset YAML and
        /// reimports it. Returns <see langword="true"/> on success; the caller refreshes the inspector.
        /// </summary>
        public static bool TryFixMissingType(SerializedProperty property, ManagedTypeName newType)
        {
            if (!TryGetAssetLocation(property, out var assetPath, out var fileId)) return false;
            if (!TryGetMissingReferenceId(property, out var referenceId)) return false;
            if (!SerializeReferenceYamlEditor.TryRewriteType(assetPath, fileId, referenceId, newType)) return false;

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            property.serializedObject.Update();
            return true;
        }
        #endregion

        #region Cross references
        /// <summary>
        /// Returns <see langword="true"/> when another managed-reference property in the same object aliases this
        /// one (shares its <see cref="SerializedProperty.managedReferenceId"/>) — which happens after duplicating an
        /// array element or pasting, leaving two fields backed by a single instance so edits to one bleed into the other.
        /// </summary>
        public static bool HasSharedReference(SerializedProperty property)
        {
            if (property.managedReferenceValue is null) return false;

            var id = property.managedReferenceId;
            var shared = false;
            var path = property.propertyPath;

            TraverseManagedReferences(property.serializedObject, other =>
            {
                if (other.propertyPath != path && other.managedReferenceId == id)
                {
                    shared = true;
                    return true;
                }

                return false;
            });

            return shared;
        }

        /// <summary>
        /// Breaks an aliased managed reference by replacing it with an independent clone that carries the same data
        /// (a fresh instance gets a new <see cref="SerializedProperty.managedReferenceId"/> on assignment), so the
        /// two formerly shared fields no longer affect each other.
        /// </summary>
        public static void MakeReferenceUnique(SerializedProperty property)
        {
            var persistent = property.Persistent();
            var current = persistent.managedReferenceValue;
            if (current is null) return;

            persistent.SetManagedReferenceAndApply(CreateInstancePreservingData(current.GetType(), current));
        }

        // Visits every managed-reference property in the object, descending into nested values; stops early when
        // the visitor returns true.
        private static void TraverseManagedReferences(SerializedObject serializedObject, Func<SerializedProperty, bool> visit)
        {
            using var iterator = serializedObject.GetIterator();
            if (!iterator.Next(enterChildren: true)) return;

            do
            {
                if (iterator.propertyType == SerializedPropertyType.ManagedReference && visit(iterator))
                    return;
            }
            while (iterator.Next(enterChildren: true));
        }
        #endregion
    }
}
