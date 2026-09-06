using System;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The candidate filter every managed-reference picker in the References window shares, so an inline card fix and
    // a bulk group fix can never offer different type sets for one constraint.
    internal static class ManagedReferenceFilter
    {
        // Concrete types assignable to the constraint, plus the open generic definitions that can close over it; a
        // null or object constraint means unconstrained. includeHidden is for a picker that REPAIRS an entry: a
        // hidden type is withheld from authoring, but data already holding it must stay re-pointable, or the gate
        // keeps failing with no way to clear it.
        public static TypeSelectorFilter For(Type constraint, bool includeHidden = false)
        {
            var baseType = constraint ?? typeof(object);

            return new TypeSelectorFilter
            {
                Types = new[] { baseType },
                Predicate = SerializeReferenceHelpers.IsAssignableManagedReference,
                AdditionalTypes = baseType == typeof(object) ? null : GenericTypeResolver.GetAssignableGenericDefinitions(baseType, null, SerializeReferenceHelpers.IsAcceptableGenericArgument),
                ArgumentFilter = SerializeReferenceHelpers.IsValidGenericArgument,
                InferredArgumentFilter = SerializeReferenceHelpers.IsAcceptableGenericArgument,
                IncludeHidden = includeHidden,
            };
        }
    }
}
