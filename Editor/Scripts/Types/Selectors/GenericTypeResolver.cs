using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // Resolves an open generic definition into a concrete closed type inside the picker's argument-selection flow:
    // candidate definitions, argument inference, constraint filters and closed-type construction. It depends on no
    // particular feature — an argument's Unity-serializability is supplied by the caller as a separate filter.
    internal static class GenericTypeResolver
    {
        // The generic candidates whose closed form could be assigned to the field type and, when narrowing types
        // are given, to every one of them. A candidate the field already determines comes back closed; one that
        // still needs a choice comes back as its open definition.
        //
        // The narrowing check matters because these entries are injected verbatim through the selector's additional
        // types, which otherwise bypass the filter applied to the ordinary candidate scan.
        //
        // Closing here is what makes the row honest: selecting a determined candidate never opens the argument page,
        // so listing it under its parameter names would promise a choice the picker will not offer. The substitution
        // repeats the checks that page would have applied, so a row is closed only when the same arguments would
        // have survived the manual path.
        internal static IEnumerable<Type> GetAssignableGenericDefinitions(
            Type fieldType,
            Type[] narrowTypes,
            GenericArgumentFilter argumentFilter = null)
        {
            if (fieldType is null) yield break;

            // TypeUtility caches the domain sweep; uncached it runs once per parameter page and stalls large
            // projects.
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

        // Closes the definition against the field type, so a field that already determines the arguments skips the
        // argument-selection page. False when the field leaves a parameter undetermined, or when the inferred type
        // violates a constraint or is not assignable to the field.
        //
        // The arguments are unified rather than copied positionally, so the field need not name the definition
        // itself: a non-generic IConverterString : IConverter<string, string> field still determines T of a
        // SequenceConverters<T> : IConverter<T, T> candidate — one parameter bound from two arguments, which
        // positional copying cannot express.
        //
        // Inference never shows the argument page, so every rule that page would have applied has to be applied
        // here, or a field shape that happens to determine its arguments silently accepts what the manual path
        // refuses. The filter is asked per parameter rather than per type, since the caller's rule can depend on
        // where the parameter lands.
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

        // The closed generic types a type is known by — itself when generic, then bases, then interfaces — most
        // specific first. These are the shapes an open definition can be unified against.
        private static IEnumerable<Type> ClosedGenericViews(Type type)
        {
            if (type.IsGenericType) yield return type;

            for (var current = type.BaseType; current is not null; current = current.BaseType)
                if (current.IsGenericType) yield return current;

            foreach (var contract in type.GetInterfaces())
                if (contract.IsGenericType) yield return contract;
        }

        // Binds every parameter of the definition by unifying the open form of the closed view's definition, as the
        // definition implements it, with the view's own arguments.
        //
        // One definition can be implemented more than once and GetInterfaces returns them in no particular order,
        // so every matching view is tried: settling for the first would make inference depend on reflection order
        // and differ between recompiles or machines.
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

        // A view can leave parameters untouched, and an undetermined parameter is exactly what the argument page
        // exists to collect.
        private static bool IsFullyBound(Type[] bindings)
        {
            foreach (var binding in bindings)
                if (binding is null) return false;

            return true;
        }

        // Each binding is judged against the parameter it was bound to, since the caller's rule can depend on where
        // that parameter ends up inside the definition.
        private static bool PassesArgumentFilter(Type openDefinition, Type[] parameters, Type[] bindings,
            GenericArgumentFilter argumentFilter)
        {
            if (argumentFilter is null) return true;

            for (var index = 0; index < bindings.Length; index++)
                if (!argumentFilter(openDefinition, parameters[index], bindings[index])) return false;

            return true;
        }

        // The generic types the definition is known by, still carrying its own parameters.
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

        // The parameter's explicit base-type constraints, excluding other type parameters, or object when it has
        // none — the base-type filter for the argument's candidate list.
        internal static Type[] GetConstraintBaseTypes(Type parameter)
        {
            var constraints = parameter.GetGenericParameterConstraints()
                .Where(constraint => !constraint.IsGenericParameter && !constraint.ContainsGenericParameters)
                .ToArray();

            return constraints.Length > 0 ? constraints : new[] { typeof(object) };
        }

        // True when the candidate satisfies the special struct / class / new() constraints on the parameter.
        internal static bool SatisfiesSpecialConstraints(Type parameter, Type candidate)
        {
            if (candidate is null) return false;

            var special = parameter.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
            var requireValueType = (special & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;
            var requireReferenceType = (special & GenericParameterAttributes.ReferenceTypeConstraint) != 0;
            var requireDefaultCtor = (special & GenericParameterAttributes.DefaultConstructorConstraint) != 0;

            if (requireValueType && !candidate.IsValueType) return false;
            if (requireReferenceType && candidate.IsValueType) return false;

            return !requireDefaultCtor || candidate.IsValueType || candidate.GetConstructor(Type.EmptyTypes) is not null;
        }

        // Closes openDefinition over arguments and validates the result
        // against every entry of fieldTypes. Returns false with a
        // human-readable error when construction throws (a violated parameter constraint)
        // or the closed type is not assignable to the field.
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

        // TryConstruct's assignability guard, for a caller that already holds a constructed closed type.
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

        // Nulls and the unconstrained object sentinel impose no restriction, as in the concrete-type filter.
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

        // Short display form of an open definition with its parameter names (Modifier<T>).
        private static string FormatDefinitionName(Type definition)
        {
            var baseName = TypeUtility.StripArity(definition.Name);
            var arguments = string.Join(", ", definition.GetGenericArguments().Select(argument => argument.Name));
            return $"{baseName}<{arguments}>";
        }

        // True when the definition could, under some choice of arguments, produce a type assignable to the field.
        // A pre-filter that rules a definition out from shape alone; it never promises the closed type exists.
        //
        // For a generic field, matching the two definitions is not enough: a candidate can implement the field's
        // definition while fixing an argument itself, so ToString<TFrom> : IConverter<TFrom, string> is an
        // IConverter<,> yet no TFrom makes it an IConverter<float, float>. Letting it through leaves a dead row —
        // inference correctly fails, the caller falls back to the open definition, and then every argument the user
        // picks is refused. So the arguments are compared here too, position by position.
        //
        // The comparison honors declared variance, because assignability does. Demanding that a candidate spell its
        // arguments exactly as the field does would trade this defect for its mirror image: a usable candidate
        // missing from the list.
        //
        // A position that still admits a family of arguments is never rejected — which one it takes is what
        // inference or the argument page decides, and proving none of them converts would mean sweeping the domain.
        // Constraints are left to that same validation: this check is about what the field demands, not about what
        // a parameter accepts.
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

        // Compares one view's arguments against the field's, position by position, under the variance the field's
        // definition declares. The bindings collect what the pinned positions force each parameter to be, so a
        // parameter demanded to be two types at once is rejected.
        //
        // Pinned positions are taken first and variant ones judged afterwards, because a parameter is only judgeable
        // once something has forced it — and the position that forces it may be declared second. One pass would
        // make the verdict depend on declaration order.
        //
        // Partial bindings are deliberately accepted: an undetermined parameter is the whole reason the argument
        // page exists, so requiring a full binding here would delete exactly the rows that page is meant to finish.
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

                // Only a position resolved to a concrete type can be judged; anything else leaves a family of
                // arguments open, which the argument page decides.
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

        // Whether a candidate spelling one argument where the field spells another is still assignable, given the
        // position's variance. The CLR applies variance only across an implicit reference conversion, so a value
        // type on either side leaves identity as the only match; IsAssignableFrom alone would accept the boxing
        // conversion, hence the explicit guard in front of it.
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
