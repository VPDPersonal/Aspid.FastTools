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
        /// Gets or sets the base types the candidates must all be assignable to.
        /// <see langword="null"/> stands for <see cref="object"/>.
        /// </summary>
        public Type[] Types { get; set; }

        /// <summary>
        /// Gets or sets which type kinds the list includes.
        /// </summary>
        public TypeAllow Allow { get; set; }

        /// <summary>
        /// Gets or sets the predicate applied to each candidate after the base-type and <see cref="Allow"/> checks,
        /// returning <see langword="false"/> to hide a type. <see langword="null"/> keeps every matching type.
        /// </summary>
        public Func<Type, bool> Predicate { get; set; }

        /// <summary>
        /// Gets or sets extra types appended verbatim, bypassing the base-type and <see cref="Allow"/> checks — for
        /// entries the assignability scan cannot match, such as open generic definitions.
        /// </summary>
        public IEnumerable<Type> AdditionalTypes { get; set; }

        /// <summary>
        /// Gets or sets the predicate applied to the types offered for an open generic's arguments, on top of the
        /// parameter's own constraints. <see langword="null"/> accepts any constraint-satisfying type.
        /// </summary>
        public Func<Type, bool> ArgumentFilter { get; set; }

        /// <summary>
        /// Gets or sets the filter applied to an argument the selector infers from the field instead of asking for
        /// it. <see langword="null"/> accepts whatever the field determines.
        /// </summary>
        /// <remarks>
        /// Separate from <see cref="ArgumentFilter"/>, which curates a page a human reads and must stay a finite
        /// list. This one judges a single argument the field has already fixed, so it can ask the exact question per
        /// parameter and admit an argument the page would not have offered.
        /// </remarks>
        public GenericArgumentFilter InferredArgumentFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether types marked <c>[TypeSelectorDisplay(Hidden = true)]</c> are
        /// offered.
        /// </summary>
        /// <remarks>
        /// Set it only on a picker that repairs a reference: hiding a type means "do not offer this for new work",
        /// not "make existing data holding it unfixable".
        /// </remarks>
        public bool IncludeHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <c>&lt;None&gt;</c> row is left out of the root page.
        /// </summary>
        /// <remarks>
        /// Set it on a picker whose target must always hold a type, such as one swapping a component's script. By
        /// default the row is offered and reports <see langword="null"/> when selected.
        /// </remarks>
        public bool HideNoneOption { get; set; }
    }
}
