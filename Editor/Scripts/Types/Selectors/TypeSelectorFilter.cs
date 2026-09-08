using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Represents the constraints deciding which types the selector offers.
    /// </summary>
    public struct TypeSelectorFilter
    {
        /// <summary>
        /// Gets or sets the base types that every candidate must be assignable to.
        /// </summary>
        /// <remarks>A <see langword="null"/> array applies no base-type constraint.</remarks>
        public Type[] Types { get; set; }

        /// <summary>
        /// Gets or sets which type kinds the list includes.
        /// </summary>
        public TypeAllow Allow { get; set; }

        /// <summary>
        /// Gets or sets the predicate that rejects candidates after the base-type and kind checks.
        /// </summary>
        /// <remarks>A <see langword="null"/> predicate accepts every matching type.</remarks>
        public Func<Type, bool> Predicate { get; set; }

        /// <summary>
        /// Gets or sets extra candidates that bypass the base-type and kind checks.
        /// </summary>
        /// <remarks>These candidates also bypass <see cref="Predicate"/>; the hidden-type filter still applies.</remarks>
        public IEnumerable<Type> AdditionalTypes { get; set; }

        /// <summary>
        /// Gets or sets the additional predicate for manually selected generic arguments.
        /// </summary>
        /// <remarks>A <see langword="null"/> predicate accepts every argument satisfying its parameter constraints.</remarks>
        public Func<Type, bool> ArgumentFilter { get; set; }

        /// <summary>
        /// Gets or sets the predicate for generic arguments inferred from the field type.
        /// </summary>
        /// <remarks>A <see langword="null"/> predicate accepts every inferred argument satisfying its parameter constraints.</remarks>
        public GenericArgumentFilter InferredArgumentFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether types with <see cref="TypeSelectorDisplayAttribute.Hidden"/> are offered.
        /// </summary>
        public bool IncludeHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the empty selection is hidden on the root page.
        /// </summary>
        public bool HideNoneOption { get; set; }
    }
}
