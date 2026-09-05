using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Answers whether closing a type parameter obliges its argument to be Unity-serializable — that is, whether the
    /// parameter reaches a field the engine writes <b>by value</b>.
    /// </summary>
    /// <remarks>
    /// "Is this type serializable?" is the wrong question to ask of a generic argument, and asking it costs real
    /// candidates. <c>SequenceConverters&lt;T&gt;</c> stores nothing but a
    /// <c>[SerializeReference] IConverter&lt;T, T&gt;[]</c>: the engine writes references there, never <c>T</c>'s
    /// layout, so <i>any</i> <c>T</c> closes it safely — including one Unity could not serialize by value.
    /// <para>
    /// The walk proves the <i>absence</i> of an obligation and never its presence: anything it cannot follow —
    /// a shape it does not recognise, a nesting deeper than it descends — keeps the obligation. A missed rule can
    /// therefore only leave today's behaviour in place; it can never let through an argument whose data Unity would
    /// silently drop. Direction matters more than coverage here, because the two errors are not symmetrical.
    /// </para>
    /// <para>
    /// Which fields Unity serializes was measured, not recalled: public instance fields are written unless marked
    /// <see cref="NonSerializedAttribute"/>, private ones only with <see cref="SerializeField"/>, and <c>static</c>,
    /// <c>const</c> and <c>readonly</c> never are. A <c>[Serializable]</c> generic type nested in a field carries the
    /// obligation inward (Unity serializes those by value since 2020.1); one without the attribute is dropped whole,
    /// so it carries nothing.
    /// </para>
    /// </remarks>
    internal static class GenericArgumentRequirement
    {
        // Unity stops descending into nested serialized data at a fixed depth it does not expose. Erring high only
        // makes the walk conservative — hitting the bottom keeps the obligation — so this need not match exactly.
        private const int MaxDepth = 8;

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="parameter"/> of <paramref name="openDefinition"/>
        /// reaches a field Unity writes by value, so its argument has to be a type the engine can serialize.
        /// </summary>
        internal static bool RequiresSerializableArgument(Type openDefinition, Type parameter)
        {
            if (openDefinition is null || parameter is null) return true;
            if (!openDefinition.IsGenericTypeDefinition) return true;

            return ReachesSerializedValue(openDefinition, parameter, MaxDepth, new HashSet<Type>());
        }

        private static bool ReachesSerializedValue(Type owner, Type parameter, int depth, HashSet<Type> visited)
        {
            // Out of budget: the parameter may still be down there, and an unproven absence is not an absence.
            if (depth <= 0) return true;

            // A type already walked contributes no path the first visit did not already follow — and a managed
            // reference graph may well be cyclic (`Node<T> { public Node<T> Next; }`).
            if (!visited.Add(owner)) return false;

            for (var current = owner; current is not null && current != typeof(object); current = current.BaseType)
            {
                const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Instance | BindingFlags.DeclaredOnly;

                foreach (var field in current.GetFields(declared))
                {
                    if (!IsSerializedByUnity(field)) continue;

                    // The engine stores a reference here, not the layout of what it points at, so the parameter
                    // inside this field's type is under no obligation. This is the case the whole walk exists for.
                    if (field.IsDefined(typeof(SerializeReference), inherit: false)) continue;

                    if (CarriesParameter(field.FieldType, parameter, depth, visited)) return true;
                }
            }

            return false;
        }

        private static bool CarriesParameter(Type fieldType, Type parameter, int depth, HashSet<Type> visited)
        {
            var value = UnwrapContainer(fieldType);

            if (value == parameter) return true;
            if (!value.ContainsGenericParameters) return false;

            // A field typed as a UnityEngine.Object is stored as a reference, whatever its parameters say.
            if (typeof(Object).IsAssignableFrom(value)) return false;

            // Unity writes a nested custom type by value only when it can serialize the type itself; without
            // [Serializable] the whole field is dropped, and a field that is never written obliges nothing.
            if (!value.IsSerializable) return false;

            return ReachesSerializedValue(value, parameter, depth - 1, visited);
        }

        // Unity flattens exactly one level of container: T[] and List<T> are stored as a sequence of T. Anything
        // deeper (a jagged array, a list of lists) is not serialized at all, and falls out as an unrecognised shape.
        private static Type UnwrapContainer(Type fieldType)
        {
            if (fieldType.IsArray && fieldType.GetArrayRank() == 1) return fieldType.GetElementType();

            return fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>)
                ? fieldType.GetGenericArguments()[0]
                : fieldType;
        }

        private static bool IsSerializedByUnity(FieldInfo field)
        {
            if (field.IsStatic || field.IsLiteral || field.IsInitOnly) return false;
            if (field.IsDefined(typeof(NonSerializedAttribute), inherit: false)) return false;

            return field.IsPublic || field.IsDefined(typeof(SerializeField), inherit: false);
        }
    }
}
