using System;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class ManagedReferenceFilter
    {
        public static TypeSelectorFilter For(Type constraint, bool includeHidden = false) =>
            For(constraint is null ? null : new[] { constraint }, includeHidden);

        // Fields of different types can share one reference, so a candidate must fit every constraint.
        public static TypeSelectorFilter For(Type[] constraints, bool includeHidden = false)
        {
            var baseType = constraints is { Length: > 0 } ? constraints[0] : typeof(object);
            var narrowTypes = constraints is { Length: > 1 } ? constraints[1..] : null;

            return new TypeSelectorFilter
            {
                Types = constraints is { Length: > 0 } ? constraints : new[] { baseType },
                Predicate = SerializeReferenceHelpers.IsAssignableManagedReference,
                AdditionalTypes = baseType == typeof(object) ? null : GenericTypeResolver.GetAssignableGenericDefinitions(baseType, narrowTypes, SerializeReferenceHelpers.IsAcceptableGenericArgument),
                ArgumentFilter = SerializeReferenceHelpers.IsValidGenericArgument,
                InferredArgumentFilter = SerializeReferenceHelpers.IsAcceptableGenericArgument,
                IncludeHidden = includeHidden,
            };
        }
    }
}
