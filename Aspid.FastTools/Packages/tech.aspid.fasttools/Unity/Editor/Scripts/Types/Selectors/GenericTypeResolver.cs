using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Helpers for resolving an open generic type definition into a concrete closed type inside the
    /// <see cref="TypeSelectorWindow"/> argument-selection flow: candidate definitions, argument
    /// inference, constraint filters and closed-type construction/validation.
    /// </summary>
    /// <remarks>
    /// Carries no dependency on any particular feature (e.g. <c>[SerializeReference]</c>); the
    /// Unity-serializability of an argument is supplied by the caller as a separate filter.
    /// </remarks>
    internal static class GenericTypeResolver
    {
        // Cached once per domain: the open-generic flow sweeps every domain type per parameter page (twice, with the
        // candidate scan), which stalls large projects. Static state is cleared on every domain reload, so the cache is
        // implicitly invalidated whenever assemblies could change. Built lazily so touching the constraint-only helpers
        // never pays for (or fails on) the full domain sweep.
        private static List<Type> _domainTypes;

        private static List<Type> DomainTypes => _domainTypes ??= TypeUtility.EnumerateDomainTypes().ToList();

        /// <summary>
        /// Enumerates the generic candidates whose closed form could be assigned to <paramref name="fieldType"/>
        /// and, when <paramref name="narrowTypes"/> are supplied, to every one of them. A candidate the field
        /// already determines is returned closed (<c>Converter&lt;String, String&gt;</c>); one that still needs a
        /// choice is returned as its open definition (<c>Converter&lt;TFrom, TTo&gt;</c>).
        /// </summary>
        /// <remarks>
        /// The narrowing check matters because these entries are injected verbatim via the selector's
        /// <c>additionalTypes</c> path, which otherwise bypasses the narrowing filter applied to the
        /// ordinary candidate scan.
        /// <para>
        /// Closing here is what makes the row honest: selecting a determined candidate never opens the argument
        /// page, so listing it under its parameter names promises a choice the picker will not offer. The
        /// substitution repeats the checks that page would have applied — <paramref name="argumentFilter"/> and
        /// assignability to every narrowing type — so a row is only closed when the same arguments would have
        /// survived the manual path.
        /// </para>
        /// </remarks>
        internal static IEnumerable<Type> GetAssignableGenericDefinitions(
            Type fieldType,
            Type[] narrowTypes,
            Func<Type, bool> argumentFilter = null)
        {
            if (fieldType is null) yield break;

            foreach (var type in DomainTypes)
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

        /// <summary>
        /// Attempts to close <paramref name="openDefinition"/> against <paramref name="fieldType"/>, so a field
        /// that already determines the arguments skips the argument-selection page entirely (e.g. a
        /// <c>Modifier&lt;float&gt;</c> field determines the argument of a <c>Modifier&lt;&gt;</c> candidate).
        /// Returns <see langword="false"/> when the field leaves any parameter undetermined, or when the
        /// inferred type violates a constraint or is not assignable to the field.
        /// </summary>
        /// <remarks>
        /// The arguments are unified rather than copied positionally, so the field need not name the definition
        /// itself: a non-generic <c>IConverterString : IConverter&lt;string, string&gt;</c> field still determines
        /// <c>T</c> of a <c>SequenceConverters&lt;T&gt; : IConverter&lt;T, T&gt;</c> candidate — one parameter
        /// bound from two arguments, which positional copying cannot express.
        /// <para>
        /// <paramref name="argumentFilter"/> is the same predicate the argument-selection page applies to the
        /// types it offers. Inference emits its result without ever showing that page, so the predicate has to be
        /// enforced here too, or a field shape that happens to determine its arguments would silently accept what
        /// the manual path refuses.
        /// </para>
        /// </remarks>
        internal static bool TryInferFromFieldType(Type fieldType, Type openDefinition, out Type closed,
            Func<Type, bool> argumentFilter = null)
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

        /// <summary>
        /// Enumerates the closed generic types <paramref name="type"/> is known by — itself when generic, then its
        /// base types, then its interfaces — most specific first. These are the shapes an open definition can be
        /// unified against.
        /// </summary>
        private static IEnumerable<Type> ClosedGenericViews(Type type)
        {
            if (type.IsGenericType) yield return type;

            for (var current = type.BaseType; current is not null; current = current.BaseType)
                if (current.IsGenericType) yield return current;

            foreach (var contract in type.GetInterfaces())
                if (contract.IsGenericType) yield return contract;
        }

        /// <summary>
        /// Binds every type parameter of <paramref name="openDefinition"/> by unifying the open form of
        /// <paramref name="closedView"/>'s definition — as <paramref name="openDefinition"/> implements it —
        /// with <paramref name="closedView"/>'s own arguments.
        /// </summary>
        /// <remarks>
        /// One definition can be implemented more than once (<c>Multi&lt;T&gt; : IThing&lt;List&lt;T&gt;&gt;,
        /// IThing&lt;int&gt;</c>), and <see cref="Type.GetInterfaces"/> returns interfaces in no particular order,
        /// so every matching view is tried: settling for the first would make inference depend on reflection
        /// order and behave differently between recompiles or machines.
        /// </remarks>
        private static bool TryBindParameters(Type openDefinition, Type closedView, Func<Type, bool> argumentFilter,
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
                if (!PassesArgumentFilter(bindings, argumentFilter)) continue;

                arguments = bindings;
                return true;
            }

            return false;
        }

        // A view can leave parameters untouched (e.g. `Pair<TKey, TValue> : IKeyed<TKey>` seen as IKeyed<string>);
        // an undetermined parameter is exactly what the argument-selection page exists to collect.
        private static bool IsFullyBound(Type[] bindings)
        {
            foreach (var binding in bindings)
                if (binding is null) return false;

            return true;
        }

        private static bool PassesArgumentFilter(Type[] bindings, Func<Type, bool> argumentFilter)
        {
            if (argumentFilter is null) return true;

            foreach (var binding in bindings)
                if (!argumentFilter(binding)) return false;

            return true;
        }

        /// <summary>
        /// Enumerates the generic types <paramref name="openDefinition"/> is known by, still carrying its own
        /// parameters (<c>SequenceConverters&lt;T&gt;</c> is also known as <c>IConverter&lt;T, T&gt;</c>).
        /// </summary>
        private static IEnumerable<Type> OpenGenericViews(Type openDefinition)
        {
            if (openDefinition.IsGenericType) yield return openDefinition;

            for (var current = openDefinition.BaseType; current is not null; current = current.BaseType)
                if (current.IsGenericType) yield return current;

            foreach (var contract in openDefinition.GetInterfaces())
                if (contract.IsGenericType) yield return contract;
        }

        /// <summary>
        /// Structurally matches an open argument list against a concrete one, recording what each parameter of the
        /// definition must be. A parameter appearing twice must resolve to the same type both times.
        /// </summary>
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

        /// <summary>
        /// Returns the explicit base-type/interface constraints of <paramref name="parameter"/> (excluding
        /// other type parameters), or <c>{ typeof(object) }</c> when it has none. Used as the base-type filter
        /// for the argument's candidate list.
        /// </summary>
        internal static Type[] GetConstraintBaseTypes(Type parameter)
        {
            var constraints = parameter.GetGenericParameterConstraints()
                .Where(constraint => !constraint.IsGenericParameter && !constraint.ContainsGenericParameters)
                .ToArray();

            return constraints.Length > 0 ? constraints : new[] { typeof(object) };
        }

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="candidate"/> satisfies the special
        /// (<c>struct</c>/<c>class</c>/<c>new()</c>) constraints declared on <paramref name="parameter"/>.
        /// </summary>
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

        /// <summary>
        /// Closes <paramref name="openDefinition"/> over <paramref name="arguments"/> and validates the result
        /// against every entry of <paramref name="fieldTypes"/>. Returns <see langword="false"/> with a
        /// human-readable <paramref name="error"/> when construction throws (a violated parameter constraint)
        /// or the closed type is not assignable to the field.
        /// </summary>
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

            // The chosen arguments may satisfy the type parameters' own constraints yet still produce a type
            // that is not assignable to the managed-reference field — guard against a value Unity would drop.
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

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="closed"/> is assignable to every meaningful entry of
        /// <paramref name="fieldTypes"/> (nulls and the unconstrained <see cref="object"/> sentinel impose no
        /// restriction). Mirrors the assignability guard <see cref="TryConstruct"/> applies, for callers that already
        /// hold a constructed closed type and only need to validate it.
        /// </summary>
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

        /// <summary>
        /// Predicate identifying open generic type definitions that can be offered for a field once closed
        /// over concrete arguments: non-abstract generic classes that are neither
        /// <see cref="UnityEngine.Object"/> nor delegates, and that are not compiler-generated.
        /// </summary>
        /// <remarks>
        /// Compiler-emitted types (anonymous types, closure/iterator display classes such as
        /// <c>&lt;&gt;c__11&lt;T&gt;</c> or <c>&lt;&gt;f__AnonymousType0&lt;…&gt;</c>) must be excluded
        /// here because these definitions are added verbatim via the selector's <c>additionalTypes</c>
        /// path, which bypasses the name/<see cref="CompilerGeneratedAttribute"/> checks applied to
        /// ordinary candidates.
        /// </remarks>
        private static bool IsAssignableGenericDefinition(Type type) =>
            type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: true } &&
            !typeof(UnityEngine.Object).IsAssignableFrom(type) &&
            !typeof(Delegate).IsAssignableFrom(type) &&
            !IsCompilerGenerated(type);

        private static bool IsCompilerGenerated(Type type) =>
            type.IsDefined(typeof(CompilerGeneratedAttribute), false)
            || type.Name.Contains('<')
            || type.Name.Contains('>');

        // Nulls and the unconstrained `object` sentinel impose no restriction, mirroring the
        // concrete-type narrowing filter.
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

        /// <summary>
        /// Short display form of an open definition with its parameter names (<c>Modifier&lt;T&gt;</c>).
        /// </summary>
        private static string FormatDefinitionName(Type definition)
        {
            var baseName = TypeUtility.StripArity(definition.Name);
            var arguments = string.Join(", ", definition.GetGenericArguments().Select(argument => argument.Name));
            return $"{baseName}<{arguments}>";
        }

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="openDefinition"/> could, under some choice of its
        /// arguments, produce a type assignable to <paramref name="fieldType"/>. A pre-filter for the candidate
        /// sweep: it rules a definition out from shape alone, it never promises the closed type exists.
        /// </summary>
        /// <remarks>
        /// For a generic field, matching the two generic <em>definitions</em> is not enough. A candidate can
        /// implement the field's definition while fixing an argument itself:
        /// <c>ToString&lt;TFrom&gt; : IConverter&lt;TFrom, string&gt;</c> is an <c>IConverter&lt;,&gt;</c>, yet no
        /// <c>TFrom</c> turns it into an <c>IConverter&lt;float, float&gt;</c>. Letting such a candidate through
        /// leaves a dead row in the picker: inference fails (correctly), the caller falls back to the open
        /// definition, and then every argument the user picks on the argument page is refused by
        /// <see cref="TryConstruct"/>. So the arguments are compared here too, position by position.
        /// <para>
        /// The comparison honours declared variance, because assignability does: with
        /// <c>IConverter&lt;in TFrom, out TTo&gt;</c> an <c>ObjectToString : IConverter&lt;object, string&gt;</c>
        /// <em>is</em> an <c>IConverter&lt;string, string&gt;</c>. Demanding that a candidate spell its arguments
        /// exactly as the field does would trade this defect for its mirror image — a usable candidate missing
        /// from the list.
        /// </para>
        /// <para>
        /// A position that still admits a family of arguments is never rejected: which one it takes is precisely
        /// what inference, or the argument page, is there to decide, and proving that none of them converts would
        /// mean sweeping the domain — work the page already does, validating each choice through
        /// <see cref="TryConstruct"/>. Constraints are left to that same validation: the check here is about what
        /// the field demands, not about what a parameter accepts.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Compares one view's arguments against the field's, position by position, under the variance the field's
        /// definition declares for each. <paramref name="bindings"/> collects what the pinned positions force each
        /// parameter of the candidate to be, so a parameter demanded to be two types at once is rejected.
        /// </summary>
        /// <remarks>
        /// Pinned positions are taken first and the variant ones judged afterwards, because a parameter is only
        /// judgeable once something has forced it: <c>Sequence&lt;T&gt; : IConverter&lt;T, T&gt;</c> against an
        /// <c>IConverter&lt;float, string&gt;</c> field is impossible only when the value-typed first position —
        /// which may be declared second — has already fixed <c>T</c> as <c>float</c>. Running in one pass would make
        /// the verdict depend on the order the parameters happen to be declared in.
        /// <para>
        /// Partial bindings are deliberately accepted — <c>Pair&lt;TKey, TValue&gt; : IKeyed&lt;TKey&gt;</c> against
        /// an <c>IKeyed&lt;string&gt;</c> field leaves <c>TValue</c> free, and that undetermined parameter is the
        /// whole reason the argument page exists. Requiring a full binding here would delete exactly the rows the
        /// page is meant to finish.
        /// </para>
        /// </remarks>
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

                // Only a position that has resolved to a concrete type can be judged. An unbound parameter, or one
                // still spelled through another generic (`List<T>`), leaves a family of arguments open — see the
                // remarks on CanCloseToFieldType for why that family is left to the argument page.
                var open = openArguments[index];
                var resolved = open.IsGenericParameter ? Binding(open, parameters, bindings) : open;
                if (resolved is null || resolved.ContainsGenericParameters) continue;

                if (!IsVarianceCompatible(resolved, fieldArguments[index], Variance(fieldParameters[index])))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns <see langword="true"/> when the field admits exactly one argument at this position, so the
        /// candidate has to name it: an invariant parameter, or any parameter the field closed over a value type.
        /// </summary>
        /// <remarks>
        /// The value-type half is what makes variance stop at the boundary of the reference world: an
        /// <c>IConverter&lt;in TFrom, out TTo&gt;</c> field closed as <c>IConverter&lt;float, …&gt;</c> accepts a
        /// candidate converting from <c>float</c> and nothing else, exactly as an invariant position would.
        /// </remarks>
        private static bool PinsArgumentExactly(Type fieldParameter, Type fieldArgument) =>
            Variance(fieldParameter) is GenericParameterAttributes.None || fieldArgument.IsValueType;

        private static GenericParameterAttributes Variance(Type fieldParameter) =>
            fieldParameter.GenericParameterAttributes & GenericParameterAttributes.VarianceMask;

        private static Type Binding(Type parameter, Type[] parameters, Type[] bindings)
        {
            var parameterIndex = Array.IndexOf(parameters, parameter);
            return parameterIndex < 0 ? null : bindings[parameterIndex];
        }

        /// <summary>
        /// Matches one argument of a pinned position, where assignability leaves no slack: the closed candidate has
        /// to name the field's argument exactly. Records what that forces each parameter of the candidate to be, and
        /// rejects a second, conflicting demand on the same parameter.
        /// </summary>
        private static bool CanBindPinnedArgument(Type openArgument, Type fieldArgument, Type[] parameters,
            Type[] bindings)
        {
            if (openArgument.IsGenericParameter)
            {
                // A parameter the definition does not own (an enclosing type's, on a nested generic) cannot be
                // recorded against these bindings — nothing is proven, so nothing is rejected.
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

            // Identity is required all the way down, so a nested definition's own variance never applies here:
            // `IThingOf<List<T>>` is the field's `IThingOf<List<string>>` only for T = string exactly.
            var nestedOpen = openArgument.GetGenericArguments();
            var nestedField = fieldArgument.GetGenericArguments();
            if (nestedOpen.Length != nestedField.Length) return false;

            for (var index = 0; index < nestedOpen.Length; index++)
                if (!CanBindPinnedArgument(nestedOpen[index], nestedField[index], parameters, bindings)) return false;

            return true;
        }

        /// <summary>
        /// Returns <see langword="true"/> when a candidate spelling <paramref name="openArgument"/> where the field
        /// spells <paramref name="fieldArgument"/> is still assignable to the field, given the position's
        /// <paramref name="variance"/>.
        /// </summary>
        /// <remarks>
        /// The CLR applies variance only across an implicit <em>reference</em> conversion, so a value type on either
        /// side leaves identity as the only match — a <c>Sequence&lt;T&gt;</c> whose <c>T</c> another position already
        /// pinned to <c>float</c> cannot answer a covariant <c>string</c>. <see cref="Type.IsAssignableFrom"/> on its
        /// own would accept it, counting the boxing conversion from <c>float</c> to a reference type, hence the
        /// explicit guard in front of it. A field argument that is itself a value type never reaches here:
        /// <see cref="PinsArgumentExactly"/> has already routed that position to the exact match.
        /// </remarks>
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
