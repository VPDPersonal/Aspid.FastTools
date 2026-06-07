using System;
using UnityEditor;
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
        /// Returns <see langword="true"/> when the property stores a type name that no longer resolves to a
        /// loadable type (a renamed or deleted implementation), so the value reads back as <see langword="null"/>.
        /// </summary>
        public static bool IsMissingType(SerializedProperty property) =>
            property.managedReferenceValue is null &&
            !string.IsNullOrEmpty(property.managedReferenceFullTypename);

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
    }
}
