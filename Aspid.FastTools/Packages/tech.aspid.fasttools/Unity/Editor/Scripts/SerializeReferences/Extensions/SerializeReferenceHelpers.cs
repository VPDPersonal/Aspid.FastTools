using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Shared helpers for the <c>[SerializeReferenceSelector]</c> drawers: resolving the declared
    /// managed-reference field type, filtering candidate types, instantiating the selected type, and
    /// parsing Unity's managed-reference type-name format.
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

        #region Generics

        /// <summary>
        /// Predicate identifying open generic type definitions that can be offered for a
        /// <c>[SerializeReference]</c> field once closed over concrete arguments: non-abstract generic
        /// classes that are neither <see cref="Object"/> nor delegates. Mirrors
        /// <see cref="IsAssignableManagedReference"/> but for the open-generic case.
        /// </summary>
        public static bool IsAssignableGenericDefinition(Type type) =>
            type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: true } &&
            !typeof(Object).IsAssignableFrom(type) &&
            !typeof(Delegate).IsAssignableFrom(type);

        /// <summary>
        /// Predicate identifying types usable as a generic argument of a serialized managed reference:
        /// concrete, non-generic types Unity can serialize as a field value (primitives, <see cref="string"/>,
        /// enums, <see cref="Object"/>-derived references, or <c>[Serializable]</c> structs/classes).
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

        /// <summary>
        /// Enumerates the open generic type definitions whose closed form could be assigned to
        /// <paramref name="fieldType"/> — i.e. generic classes that implement/inherit the field's type
        /// (matched by generic definition for a generic field, or directly for a non-generic field).
        /// These are offered alongside the concrete candidates; selecting one resolves its type arguments
        /// via <see cref="ResolveGenericType"/>.
        /// </summary>
        public static IEnumerable<Type> GetAssignableGenericDefinitions(Type fieldType)
        {
            if (fieldType is null) yield break;

            foreach (var type in EnumerateDomainTypes())
            {
                if (!IsAssignableGenericDefinition(type)) continue;
                if (CanCloseToFieldType(type, fieldType)) yield return type;
            }
        }

        /// <summary>
        /// Resolves a closed generic type from the selected open <paramref name="openDefinition"/>: when the
        /// arguments can be inferred from a closed-generic <paramref name="fieldType"/> the closed type is
        /// produced directly; otherwise a <see cref="GenericArgumentSelectorWindow"/> is opened at
        /// <paramref name="anchor"/> so the user picks each argument. The resulting closed type is passed to
        /// <paramref name="onResolved"/> (never invoked if the user cancels).
        /// </summary>
        public static void ResolveGenericType(Type openDefinition, Type fieldType, Rect anchor, Action<Type> onResolved)
        {
            if (openDefinition is null) return;

            if (TryMakeConcreteFromField(fieldType, openDefinition, out var closed))
                onResolved?.Invoke(closed);
            else
                GenericArgumentSelectorWindow.Show(anchor, openDefinition, fieldType, onResolved);
        }

        /// <summary>
        /// Attempts to close <paramref name="openDefinition"/> using the type arguments of a closed-generic
        /// <paramref name="fieldType"/> (e.g. a <c>Modifier&lt;float&gt;</c> field directly determines the
        /// argument of a <c>Modifier&lt;&gt;</c> candidate). Returns <see langword="false"/> when the field is
        /// not a closed generic or the inferred type would not be assignable.
        /// </summary>
        public static bool TryMakeConcreteFromField(Type fieldType, Type openDefinition, out Type closed)
        {
            closed = null;

            if (fieldType is null || !fieldType.IsGenericType || fieldType.ContainsGenericParameters) return false;

            var fieldArguments = fieldType.GetGenericArguments();
            if (fieldArguments.Length != openDefinition.GetGenericArguments().Length) return false;

            var fieldDefinition = fieldType.GetGenericTypeDefinition();
            var matchesDefinition = GenericBaseDefinitions(openDefinition)
                .Any(definition => definition == fieldDefinition);

            if (!matchesDefinition) return false;

            try
            {
                closed = openDefinition.MakeGenericType(fieldArguments);
            }
            catch (Exception)
            {
                closed = null;
                return false;
            }

            return fieldType.IsAssignableFrom(closed);
        }

        private static bool CanCloseToFieldType(Type openDefinition, Type fieldType)
        {
            if (fieldType.IsGenericType)
            {
                var fieldDefinition = fieldType.GetGenericTypeDefinition();
                return GenericBaseDefinitions(openDefinition).Any(definition => definition == fieldDefinition);
            }

            if (fieldType.IsAssignableFrom(openDefinition)) return true;
            if (openDefinition.GetInterfaces().Contains(fieldType)) return true;

            for (var current = openDefinition.BaseType; current is not null; current = current.BaseType)
                if (current == fieldType) return true;

            return false;
        }

        /// <summary>
        /// Enumerates the generic-type-definition view of <paramref name="openDefinition"/> itself, its base
        /// class chain, and its interfaces (only the generic ones, reduced to their definitions). Used to
        /// match a candidate generic against a generic field's definition.
        /// </summary>
        private static IEnumerable<Type> GenericBaseDefinitions(Type openDefinition)
        {
            if (openDefinition.IsGenericType)
                yield return openDefinition.GetGenericTypeDefinition();

            for (var current = openDefinition.BaseType; current is not null; current = current.BaseType)
                if (current.IsGenericType)
                    yield return current.GetGenericTypeDefinition();

            foreach (var contract in openDefinition.GetInterfaces())
                if (contract.IsGenericType)
                    yield return contract.GetGenericTypeDefinition();
        }

        private static IEnumerable<Type> EnumerateDomainTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(type => type is not null).ToArray();
                }

                foreach (var type in types)
                    yield return type;
            }
        }

        #endregion
    }
}
