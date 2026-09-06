using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Answers whether closing a type parameter obliges its argument to be Unity-serializable — that is, whether the
    // parameter reaches a field the engine writes BY VALUE. Asking "is this type serializable?" instead would cost
    // real candidates: a type storing only a [SerializeReference] IConverter<T, T>[] never writes T's layout, so any
    // T closes it safely.
    //
    // The walk proves the ABSENCE of an obligation and never its presence: anything it cannot follow keeps the
    // obligation. A missed rule can only leave today's behavior in place; it can never let through an argument whose
    // data Unity would silently drop.
    internal static class GenericArgumentRequirement
    {
        // Unity's own nesting limit is not exposed. Erring high only makes the walk conservative, since hitting the
        // bottom keeps the obligation.
        private const int MaxDepth = 8;

        internal static bool RequiresSerializableArgument(Type openDefinition, Type parameter)
        {
            if (openDefinition is null || parameter is null) return true;
            if (!openDefinition.IsGenericTypeDefinition) return true;

            return ReachesSerializedValue(openDefinition, parameter, MaxDepth, new HashSet<Type>());
        }

        private static bool ReachesSerializedValue(Type owner, Type parameter, int depth, HashSet<Type> visited)
        {
            // Out of budget: an unproven absence is not an absence.
            if (depth <= 0) return true;

            // A type already walked adds no new path, and the graph may be cyclic.
            if (!visited.Add(owner)) return false;

            for (var current = owner; current is not null && current != typeof(object); current = current.BaseType)
            {
                const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Instance | BindingFlags.DeclaredOnly;

                foreach (var field in current.GetFields(declared))
                {
                    if (!IsSerializedByUnity(field)) continue;

                    // The engine stores a reference here, not the layout of what it points at — the case the whole
                    // walk exists for.
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

            // Without [Serializable] the whole field is dropped, and a field that is never written obliges nothing.
            if (!value.IsSerializable) return false;

            return ReachesSerializedValue(value, parameter, depth - 1, visited);
        }

        // Unity flattens exactly one level: T[] and List<T> store a sequence of T. Anything deeper is not serialized
        // at all and falls out as an unrecognized shape.
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
