using System;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The candidate filter every managed-reference picker in the References window shares, so an inline card fix and
    /// a bulk group fix can never offer different type sets for the same constraint.
    /// </summary>
    internal static class ManagedReferenceFilter
    {
        /// <summary>
        /// Concrete types assignable to <paramref name="constraint"/>, plus the open generic definitions that can
        /// close over it. A <see langword="null"/> or <see cref="object"/> constraint falls back to unconstrained
        /// (any managed-reference type).
        /// </summary>
        /// <param name="constraint">The declared field type the candidates must be assignable to.</param>
        /// <param name="includeHidden">
        /// Pass <see langword="true"/> from a picker that <b>repairs</b> an entry — a missing card or a bulk group
        /// fix. A <c>[TypeSelectorDisplay(Hidden = true)]</c> type is withheld from authoring, but data already
        /// holding it has to stay re-pointable, or the gate keeps failing with no way to clear it.
        /// </param>
        public static TypeSelectorFilter For(Type constraint, bool includeHidden = false)
        {
            var baseType = constraint ?? typeof(object);

            return new TypeSelectorFilter
            {
                Types = new[] { baseType },
                Predicate = SerializeReferenceHelpers.IsAssignableManagedReference,
                AdditionalTypes = baseType == typeof(object) ? null : GenericTypeResolver.GetAssignableGenericDefinitions(baseType, null, SerializeReferenceHelpers.IsValidGenericArgument),
                ArgumentFilter = SerializeReferenceHelpers.IsValidGenericArgument,
                IncludeHidden = includeHidden,
            };
        }
    }
}
