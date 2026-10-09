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
        private const string UnmanagedAttributeName = "System.Runtime.CompilerServices.IsUnmanagedAttribute";

        private static readonly Dictionary<Type, bool> _unmanagedParameters = new();

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

                if (TryInferFromFieldType(fieldType, type, out var closed, argumentFilter) &&
                    IsAssignableToFieldTypes(closed, narrowTypes))
                {
                    yield return closed;
                    continue;
                }

                // At a variant position the field's own argument is one fit of several. Offer it closed as a
                // shortcut, and offer the open row for the others.
                if (TryInfer(fieldType, type, exact: true, argumentFilter, out closed) &&
                    IsAssignableToFieldTypes(closed, narrowTypes))
                    yield return closed;

                yield return type;
            }
        }

        // Unify generic views rather than copying positional arguments; inferred arguments must pass the same
        // checks as manual selections. Only arguments that the field decides count. At a variant position other
        // arguments fit too, so the argument page collects them.
        internal static bool TryInferFromFieldType(Type fieldType, Type openDefinition, out Type closed,
            GenericArgumentFilter argumentFilter = null) =>
            TryInfer(fieldType, openDefinition, exact: false, argumentFilter, out closed);

        // With exact, every position binds to the field's own argument, as if no variance applied.
        private static bool TryInfer(Type fieldType, Type openDefinition, bool exact,
            GenericArgumentFilter argumentFilter, out Type closed)
        {
            closed = null;

            if (fieldType is null || fieldType.ContainsGenericParameters) return false;

            foreach (var view in ClosedGenericViews(fieldType))
            {
                if (!TryBindParameters(openDefinition, view, exact, argumentFilter, out var arguments)) continue;
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

        // A definition can implement more than one matching generic view, and the result must not depend on
        // reflection order. Exact binding takes the first view that binds every parameter. Otherwise every view
        // that fits must give the same arguments. A view that fits but leaves a parameter free, or gives other
        // arguments, shows that the field does not decide them.
        private static bool TryBindParameters(Type openDefinition, Type closedView, bool exact,
            GenericArgumentFilter argumentFilter, out Type[] arguments)
        {
            arguments = null;

            var viewDefinition = closedView.GetGenericTypeDefinition();
            var viewParameters = viewDefinition.GetGenericArguments();
            var viewArguments = closedView.GetGenericArguments();
            var parameters = openDefinition.GetGenericArguments();
            var variance = exact ? GenericParameterAttributes.None : GenericParameterAttributes.Covariant;

            foreach (var openView in OpenGenericViews(openDefinition))
            {
                if (openView.GetGenericTypeDefinition() != viewDefinition) continue;

                var bindings = new Type[parameters.Length];
                if (!TryBind(openView.GetGenericArguments(), viewArguments, viewParameters, variance, parameters,
                        bindings))
                    continue;

                if (exact)
                {
                    if (!IsFullyBound(bindings)) continue;
                    if (!PassesArgumentFilter(openDefinition, parameters, bindings, argumentFilter)) continue;

                    arguments = bindings;
                    return true;
                }

                if (!IsFullyBound(bindings)) return false;
                if (arguments is not null && !arguments.SequenceEqual(bindings)) return false;

                arguments = bindings;
            }

            if (arguments is not null && PassesArgumentFilter(openDefinition, parameters, arguments, argumentFilter))
                return true;

            arguments = null;
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
        // appearing twice must resolve to the same type both times. variance belongs to the position of the whole
        // list. None demands the same types, Covariant lets the candidate's argument be a subtype, and
        // Contravariant lets it be a supertype.
        private static bool TryBind(Type[] openArguments, Type[] concreteArguments, Type[] definitionParameters,
            GenericParameterAttributes variance, Type[] parameters, Type[] bindings)
        {
            if (openArguments.Length != concreteArguments.Length) return false;

            for (var index = 0; index < openArguments.Length; index++)
            {
                var positionVariance = Compose(variance, Variance(definitionParameters[index]));

                if (!TryBindArgument(openArguments[index], concreteArguments[index], positionVariance, parameters,
                        bindings))
                    return false;
            }

            return true;
        }

        private static bool TryBindArgument(Type open, Type concrete, GenericParameterAttributes variance,
            Type[] parameters, Type[] bindings)
        {
            // Variance converts references only.
            if (concrete.IsValueType) variance = GenericParameterAttributes.None;

            if (open.IsGenericParameter)
            {
                var parameterIndex = Array.IndexOf(parameters, open);
                if (parameterIndex < 0) return false;

                // Other types fit this position too, so it does not decide the parameter.
                if (AdmitsOtherArguments(concrete, variance)) return true;

                bindings[parameterIndex] ??= concrete;
                return bindings[parameterIndex] == concrete;
            }

            if (!open.ContainsGenericParameters)
            {
                return variance is GenericParameterAttributes.None
                    ? open == concrete
                    : IsVarianceCompatible(open, concrete, variance);
            }

            // An array of references varies with its element, so the element keeps the position's variance.
            if (open.IsArray)
            {
                return concrete.IsArray &&
                       open.GetArrayRank() == concrete.GetArrayRank() &&
                       TryBindArgument(open.GetElementType(), concrete.GetElementType(), variance, parameters,
                           bindings);
            }

            if (!open.IsGenericType || !concrete.IsGenericType) return false;

            var definition = open.GetGenericTypeDefinition();
            if (definition != concrete.GetGenericTypeDefinition()) return false;

            return TryBind(open.GetGenericArguments(), concrete.GetGenericArguments(), definition.GetGenericArguments(),
                variance, parameters, bindings);
        }

        // Whether a type other than the reference type `type` fits this position. A covariant position takes a
        // subtype, and a sealed type has none. A contravariant position takes a supertype, and only object has none.
        private static bool AdmitsOtherArguments(Type type, GenericParameterAttributes variance) => variance switch
        {
            GenericParameterAttributes.Covariant => type.IsArray
                ? AdmitsOtherArguments(type.GetElementType(), variance)
                : !type.IsSealed,
            GenericParameterAttributes.Contravariant => type != typeof(object),
            _ => false,
        };

        // The variance of a nested position as seen from the outermost one. Identity anywhere demands identity
        // inside it, and two contravariant steps cancel out.
        private static GenericParameterAttributes Compose(GenericParameterAttributes outer,
            GenericParameterAttributes inner)
        {
            if (outer is GenericParameterAttributes.None || inner is GenericParameterAttributes.None)
                return GenericParameterAttributes.None;

            return outer == inner ? GenericParameterAttributes.Covariant : GenericParameterAttributes.Contravariant;
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

            // An open definition is judged once its own arguments are chosen.
            if (requireValueType && !candidate.ContainsGenericParameters && HasUnmanagedConstraint(parameter) &&
                !IsUnmanaged(candidate))
                return false;

            return !requireDefaultCtor ||
                candidate.IsValueType ||
                (!candidate.IsAbstract && candidate.GetConstructor(Type.EmptyTypes) is not null);
        }

        // The argument page checks each candidate with this. The arguments so far, with the candidate last, must
        // meet their constraints, and the definition must still close to every field type. An argument that is an
        // open definition is checked on its own argument pages.
        internal static bool CanCloseWithArguments(Type openDefinition, Type[] arguments, Type[] fieldTypes)
        {
            var parameters = openDefinition.GetGenericArguments();
            var known = new Type[parameters.Length];
            Array.Copy(arguments, known, Math.Min(arguments.Length, known.Length));

            foreach (var argument in known)
                if (argument is not null && argument.ContainsGenericParameters) return true;

            for (var index = 0; index < known.Length; index++)
            {
                if (known[index] is null) continue;
                if (!SatisfiesConstraints(parameters[index], known[index], parameters, known)) return false;
            }

            if (fieldTypes is null) return true;

            foreach (var fieldType in fieldTypes)
            {
                if (fieldType is null || fieldType == typeof(object)) continue;
                if (!CanCloseToFieldType(openDefinition, fieldType, known)) return false;
            }

            return true;
        }

        // Also checks constraints that name parameters, such as T : IComparable<T> or U : T, closed over the known
        // arguments. While such a constraint names an open parameter, TryConstruct checks it later.
        private static bool SatisfiesConstraints(Type parameter, Type argument, Type[] parameters, Type[] arguments)
        {
            if (!SatisfiesSpecialConstraints(parameter, argument)) return false;

            foreach (var constraint in parameter.GetGenericParameterConstraints())
            {
                var closedConstraint = Substitute(constraint, parameters, arguments);
                if (closedConstraint is not null && !closedConstraint.IsAssignableFrom(argument)) return false;
            }

            return true;
        }

        // Replaces the definition's parameters in `type` with their arguments. Returns null while a parameter is
        // open, or when the result cannot be constructed.
        private static Type Substitute(Type type, Type[] parameters, Type[] arguments)
        {
            if (!type.ContainsGenericParameters) return type;

            if (type.IsGenericParameter)
            {
                var parameterIndex = Array.IndexOf(parameters, type);
                return parameterIndex < 0 ? null : arguments[parameterIndex];
            }

            if (type.IsArray)
            {
                var element = Substitute(type.GetElementType(), parameters, arguments);
                if (element is null) return null;

                var rank = type.GetArrayRank();
                return rank == 1 ? element.MakeArrayType() : element.MakeArrayType(rank);
            }

            if (!type.IsGenericType) return null;

            var typeArguments = type.GetGenericArguments();

            for (var index = 0; index < typeArguments.Length; index++)
            {
                typeArguments[index] = Substitute(typeArguments[index], parameters, arguments);
                if (typeArguments[index] is null) return null;
            }

            try
            {
                return type.GetGenericTypeDefinition().MakeGenericType(typeArguments);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // The CLR sees unmanaged as a plain struct constraint; only this compiler attribute tells them apart.
        private static bool HasUnmanagedConstraint(Type parameter)
        {
            if (_unmanagedParameters.TryGetValue(parameter, out var result)) return result;

            result = parameter.GetCustomAttributesData()
                .Any(attribute => attribute.AttributeType.FullName == UnmanagedAttributeName);

            _unmanagedParameters[parameter] = result;
            return result;
        }

        // C#'s rule: a primitive, enum or pointer, or a struct whose instance fields are all unmanaged.
        private static bool IsUnmanaged(Type type)
        {
            if (type.IsPrimitive || type.IsEnum || type.IsPointer) return true;
            if (!type.IsValueType) return false;

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (!IsUnmanaged(field.FieldType)) return false;

            return true;
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

            // MakeGenericType accepts a struct holding references for an unmanaged parameter.
            var parameters = openDefinition.GetGenericArguments();

            for (var index = 0; index < parameters.Length; index++)
            {
                if (!HasUnmanagedConstraint(parameters[index]) || IsUnmanaged(arguments[index])) continue;

                error = $"Cannot construct {FormatDefinitionName(openDefinition)}: " +
                        $"{arguments[index].Name} is not an unmanaged type.";
                closed = null;
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
        // variance or unresolved parameters. arguments holds the parameters already chosen, null where still open.
        private static bool CanCloseToFieldType(Type openDefinition, Type fieldType, Type[] arguments = null)
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

                    var bindings = new Type[parameters.Length];
                    arguments?.CopyTo(bindings, 0);

                    // A parameter the field fixes must also meet its own constraints, or no argument page choice
                    // can close the row.
                    if (CanCloseArguments(openView.GetGenericArguments(), fieldArguments, fieldParameters, parameters,
                            bindings) &&
                        SatisfiesBoundConstraints(parameters, bindings))
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

        private static bool SatisfiesBoundConstraints(Type[] parameters, Type[] bindings)
        {
            for (var index = 0; index < parameters.Length; index++)
            {
                if (bindings[index] is null) continue;
                if (!SatisfiesConstraints(parameters[index], bindings[index], parameters, bindings)) return false;
            }

            return true;
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

                var variance = Variance(fieldParameters[index]);
                var resolved = Substitute(openArguments[index], parameters, bindings);

                if (resolved is not null)
                {
                    if (!IsVarianceCompatible(resolved, fieldArguments[index], variance)) return false;
                }
                else if (!CanVaryTo(openArguments[index], fieldArguments[index], variance))
                {
                    return false;
                }
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

        // Whether an argument with open parameters can vary into the field's reference argument, judged from its
        // shape. A value type and a struct-constrained parameter never vary. A generic class or interface must
        // reach the field's argument through its bases and interfaces.
        private static bool CanVaryTo(Type openArgument, Type fieldArgument, GenericParameterAttributes variance)
        {
            if (openArgument.IsGenericParameter)
            {
                return (openArgument.GenericParameterAttributes &
                        GenericParameterAttributes.NotNullableValueTypeConstraint) == 0;
            }

            if (openArgument.IsValueType) return false;
            if (!openArgument.IsGenericType) return true;

            var definition = openArgument.GetGenericTypeDefinition();

            return variance is GenericParameterAttributes.Covariant
                ? CanCloseToFieldType(definition, fieldArgument)
                : ClosedGenericViews(fieldArgument).Any(view => view.GetGenericTypeDefinition() == definition);
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
