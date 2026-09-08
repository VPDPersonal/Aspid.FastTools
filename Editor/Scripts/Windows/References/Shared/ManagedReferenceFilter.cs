using System;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class ManagedReferenceFilter
    {
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
