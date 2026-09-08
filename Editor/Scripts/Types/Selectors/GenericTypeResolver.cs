using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class GenericTypeResolver
    {
        // Additional candidates bypass the normal scan; validate narrowing constraints here and close fully
        // inferred rows before displaying them.
        internal static IEnumerable<Type> GetAssignableGenericDefinitions(
            Type fieldType,
            Type[] narrowTypes,
            GenericArgumentFilter argumentFilter = null)
        {
            if (fieldType is null) yield break;

            foreach (var type in TypeUtility.DomainTypes)
            {
                if (!IsAssignableGenericDefinition(type)) continue;
                if (!CanCloseToFieldType(type, fieldType)) continue;
                if (!CanCloseToAllNarrowing(type, narrowTypes)) continue;

                yield return TryInferFromFieldType(fieldType, type, out var closed, argumentFilter) &&
                             IsAssignableToFieldTypes(closed, narrowTypes)
                    ? closed
                    : type;
            }
        }

        // Unify generic views rather than copying positional arguments; inferred arguments must pass the same
        // checks as manual selections.
        internal static bool TryInferFromFieldType(Type fieldType, Type openDefinition, out Type closed,
            GenericArgumentFilter argumentFilter = null)
        {
            closed = null;

            if (fieldType is null || fieldType.ContainsGenericParameters) return false;

            foreach (var view in ClosedGenericViews(fieldType))
            {
                if (!TryBindParameters(openDefinition, view, argumentFilter, out var arguments)) continue;
                if (TryConstruct(openDefinition, arguments, new[] { fieldType }, out closed, out _)) return true;
            }

            closed = null;
            return false;
        }

        private static IEnumerable<Type> ClosedGenericViews(Type type)
        {
            if (type.IsGenericType) yield return type;

            for (var current = type.BaseType; current is not null; current = current.BaseType)
                if (current.IsGenericType) yield return current;

            foreach (var contract in type.GetInterfaces())
                if (contract.IsGenericType) yield return contract;
        }

        // A definition can implement multiple matching generic views; try each to avoid depending on reflection
        // order.
        private static bool TryBindParameters(Type openDefinition, Type closedView, GenericArgumentFilter argumentFilter,
            out Type[] arguments)
        {
            arguments = null;

            var viewDefinition = closedView.GetGenericTypeDefinition();
            var parameters = openDefinition.GetGenericArguments();

            foreach (var openView in OpenGenericViews(openDefinition))
            {
                if (openView.GetGenericTypeDefinition() != viewDefinition) continue;

                var bindings = new Type[parameters.Length];
                if (!TryBind(openView.GetGenericArguments(), closedView.GetGenericArguments(), parameters, bindings))
                    continue;

                if (!IsFullyBound(bindings)) continue;
                if (!PassesArgumentFilter(openDefinition, parameters, bindings, argumentFilter)) continue;

                arguments = bindings;
                return true;
            }

            return false;
        }

        private static bool IsFullyBound(Type[] bindings)
        {
            foreach (var binding in bindings)
                if (binding is null) return false;

            return true;
        }

        private static bool PassesArgumentFilter(Type openDefinition, Type[] parameters, Type[] bindings,
            GenericArgumentFilter argumentFilter)
        {
            if (argumentFilter is null) return true;

            for (var index = 0; index < bindings.Length; index++)
                if (!argumentFilter(openDefinition, parameters[index], bindings[index])) return false;

            return true;
        }

        private static IEnumerable<Type> OpenGenericViews(Type openDefinition)
        {
            if (openDefinition.IsGenericType) yield return openDefinition;

            for (var current = openDefinition.BaseType; current is not null; current = current.BaseType)
                if (current.IsGenericType) yield return current;

            foreach (var contract in openDefinition.GetInterfaces())
                if (contract.IsGenericType) yield return contract;
        }

        // Matches an open argument list against a concrete one, recording what each parameter must be; a parameter
        // appearing twice must resolve to the same type both times.
        private static bool TryBind(Type[] openArguments, Type[] concreteArguments, Type[] parameters, Type[] bindings)
        {
            if (openArguments.Length != concreteArguments.Length) return false;

            for (var index = 0; index < openArguments.Length; index++)
            {
                var open = openArguments[index];
                var concrete = concreteArguments[index];

                if (open.IsGenericParameter)
                {
                    var parameterIndex = Array.IndexOf(parameters, open);
                    if (parameterIndex < 0) return false;

                    if (bindings[parameterIndex] is null) bindings[parameterIndex] = concrete;
                    else if (bindings[parameterIndex] != concrete) return false;

                    continue;
                }

                if (open.ContainsGenericParameters)
                {
                    if (!open.IsGenericType || !concrete.IsGenericType) return false;
                    if (open.GetGenericTypeDefinition() != concrete.GetGenericTypeDefinition()) return false;
                    if (!TryBind(open.GetGenericArguments(), concrete.GetGenericArguments(), parameters, bindings)) return false;

                    continue;
                }

                if (open != concrete) return false;
            }

            return true;
        }

        internal static Type[] GetConstraintBaseTypes(Type parameter)
        {
            var constraints = parameter.GetGenericParameterConstraints()
                .Where(constraint => !constraint.IsGenericParameter && !constraint.ContainsGenericParameters)
                .ToArray();

            return constraints.Length > 0 ? constraints : new[] { typeof(object) };
        }

        internal static bool SatisfiesSpecialConstraints(Type parameter, Type candidate)
        {
            if (candidate is null) return false;

            var special = parameter.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
            var requireValueType = (special & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;
            var requireReferenceType = (special & GenericParameterAttributes.ReferenceTypeConstraint) != 0;
            var requireDefaultCtor = (special & GenericParameterAttributes.DefaultConstructorConstraint) != 0;

            if (requireValueType && (!candidate.IsValueType || Nullable.GetUnderlyingType(candidate) is not null)) return false;
            if (requireReferenceType && candidate.IsValueType) return false;

            return !requireDefaultCtor ||
                candidate.IsValueType ||
                (!candidate.IsAbstract && candidate.GetConstructor(Type.EmptyTypes) is not null);
        }

        internal static bool TryConstruct(Type openDefinition, Type[] arguments, Type[] fieldTypes, out Type closed, out string error)
        {
            closed = null;
            error = null;

            try
            {
                closed = openDefinition.MakeGenericType(arguments);
            }
            catch (Exception exception)
            {
                error = $"Cannot construct {FormatDefinitionName(openDefinition)}: {exception.Message}";
                return false;
            }

            // Arguments can satisfy the parameters' own constraints and still produce a type the field cannot
            // hold, which Unity would drop.
            if (fieldTypes is not null)
            {
                foreach (var fieldType in fieldTypes)
                {
                    if (fieldType is null || fieldType == typeof(object)) continue;
                    if (fieldType.IsAssignableFrom(closed)) continue;

                    error = $"{closed.Name} is not assignable to {fieldType.Name}.";
                    closed = null;
                    return false;
                }
            }

            return true;
        }

        internal static bool IsAssignableToFieldTypes(Type closed, Type[] fieldTypes)
        {
            if (closed is null) return false;
            if (fieldTypes is null) return true;

            foreach (var fieldType in fieldTypes)
            {
                if (fieldType is null || fieldType == typeof(object)) continue;
                if (!fieldType.IsAssignableFrom(closed)) return false;
            }

            return true;
        }

        // The open definitions that can be offered once closed: non-abstract generic classes that are neither
        // UnityEngine.Object nor delegates, and not compiler-generated. The last exclusion has to happen here
        // because these definitions are injected verbatim, bypassing the checks applied to ordinary candidates.
        private static bool IsAssignableGenericDefinition(Type type) =>
            type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: true } &&
            !typeof(UnityEngine.Object).IsAssignableFrom(type) &&
            !typeof(Delegate).IsAssignableFrom(type) &&
            !IsCompilerGenerated(type);

        private static bool IsCompilerGenerated(Type type) =>
            type.IsDefined(typeof(CompilerGeneratedAttribute), false)
            || type.Name.Contains('<')
            || type.Name.Contains('>');

        private static bool CanCloseToAllNarrowing(Type openDefinition, Type[] narrowTypes)
        {
            if (narrowTypes is null) return true;

            foreach (var narrowType in narrowTypes)
            {
                if (narrowType is null || narrowType == typeof(object)) continue;
                if (!CanCloseToFieldType(openDefinition, narrowType)) return false;
            }

            return true;
        }

        private static string FormatDefinitionName(Type definition)
        {
            var baseName = TypeUtility.StripArity(definition.Name);
            var arguments = string.Join(", ", definition.GetGenericArguments().Select(argument => argument.Name));
            return $"{baseName}<{arguments}>";
        }

        // Reject incompatible fixed arguments before offering a generic row, while preserving choices allowed by
        // variance or unresolved parameters.
        private static bool CanCloseToFieldType(Type openDefinition, Type fieldType)
        {
            if (fieldType.IsGenericType)
            {
                var fieldDefinition = fieldType.GetGenericTypeDefinition();
                var fieldArguments = fieldType.GetGenericArguments();
                var fieldParameters = fieldDefinition.GetGenericArguments();
                var parameters = openDefinition.GetGenericArguments();

                foreach (var openView in OpenGenericViews(openDefinition))
                {
                    if (openView.GetGenericTypeDefinition() != fieldDefinition) continue;

                    if (CanCloseArguments(openView.GetGenericArguments(), fieldArguments, fieldParameters,
                            parameters, new Type[parameters.Length]))
                        return true;
                }

                return false;
            }

            if (fieldType.IsAssignableFrom(openDefinition)) return true;
            if (openDefinition.GetInterfaces().Contains(fieldType)) return true;

            for (var current = openDefinition.BaseType; current is not null; current = current.BaseType)
                if (current == fieldType) return true;

            return false;
        }

        // Bind invariant positions before checking variant ones, which may refer to parameters fixed later in
        // declaration order.
        private static bool CanCloseArguments(Type[] openArguments, Type[] fieldArguments, Type[] fieldParameters,
            Type[] parameters, Type[] bindings)
        {
            if (openArguments.Length != fieldArguments.Length) return false;

            for (var index = 0; index < openArguments.Length; index++)
            {
                if (!PinsArgumentExactly(fieldParameters[index], fieldArguments[index])) continue;
                if (!CanBindPinnedArgument(openArguments[index], fieldArguments[index], parameters, bindings))
                    return false;
            }

            for (var index = 0; index < openArguments.Length; index++)
            {
                if (PinsArgumentExactly(fieldParameters[index], fieldArguments[index])) continue;

                var open = openArguments[index];
                var resolved = open.IsGenericParameter ? Binding(open, parameters, bindings) : open;
                if (resolved is null || resolved.ContainsGenericParameters) continue;

                if (!IsVarianceCompatible(resolved, fieldArguments[index], Variance(fieldParameters[index])))
                    return false;
            }

            return true;
        }

        // True when the field admits exactly one argument at this position, so the candidate must name it: an
        // invariant parameter, or any parameter the field closed over a value type. The latter is where variance
        // stops at the boundary of the reference world.
        private static bool PinsArgumentExactly(Type fieldParameter, Type fieldArgument) =>
            Variance(fieldParameter) is GenericParameterAttributes.None || fieldArgument.IsValueType;

        private static GenericParameterAttributes Variance(Type fieldParameter) =>
            fieldParameter.GenericParameterAttributes & GenericParameterAttributes.VarianceMask;

        private static Type Binding(Type parameter, Type[] parameters, Type[] bindings)
        {
            var parameterIndex = Array.IndexOf(parameters, parameter);
            return parameterIndex < 0 ? null : bindings[parameterIndex];
        }

        // A pinned position leaves no slack: the candidate must name the field's argument exactly. Records what
        // that forces each parameter to be, and rejects a second, conflicting demand on the same one.
        private static bool CanBindPinnedArgument(Type openArgument, Type fieldArgument, Type[] parameters,
            Type[] bindings)
        {
            if (openArgument.IsGenericParameter)
            {
                // A parameter the definition does not own cannot be recorded here, so nothing is rejected.
                var parameterIndex = Array.IndexOf(parameters, openArgument);
                if (parameterIndex < 0) return true;

                bindings[parameterIndex] ??= fieldArgument;
                return bindings[parameterIndex] == fieldArgument;
            }

            if (!openArgument.ContainsGenericParameters) return openArgument == fieldArgument;

            if (openArgument.IsArray)
            {
                return fieldArgument.IsArray &&
                       openArgument.GetArrayRank() == fieldArgument.GetArrayRank() &&
                       CanBindPinnedArgument(openArgument.GetElementType(), fieldArgument.GetElementType(),
                           parameters, bindings);
            }

            if (!openArgument.IsGenericType || !fieldArgument.IsGenericType) return false;
            if (openArgument.GetGenericTypeDefinition() != fieldArgument.GetGenericTypeDefinition()) return false;

            // Identity is required all the way down, so a nested definition's own variance never applies.
            var nestedOpen = openArgument.GetGenericArguments();
            var nestedField = fieldArgument.GetGenericArguments();
            if (nestedOpen.Length != nestedField.Length) return false;

            for (var index = 0; index < nestedOpen.Length; index++)
                if (!CanBindPinnedArgument(nestedOpen[index], nestedField[index], parameters, bindings)) return false;

            return true;
        }

        // CLR variance permits reference conversions only; IsAssignableFrom would also accept boxing without
        // this value-type guard.
        private static bool IsVarianceCompatible(Type openArgument, Type fieldArgument,
            GenericParameterAttributes variance)
        {
            if (openArgument == fieldArgument) return true;
            if (openArgument.IsValueType || fieldArgument.IsValueType) return false;

            return variance is GenericParameterAttributes.Covariant
                ? fieldArgument.IsAssignableFrom(openArgument)
                : openArgument.IsAssignableFrom(fieldArgument);
        }
    }
}
