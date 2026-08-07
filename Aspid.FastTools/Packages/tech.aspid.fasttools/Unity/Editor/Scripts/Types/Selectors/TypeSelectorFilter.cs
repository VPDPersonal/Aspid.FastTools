using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Describes which types the selector offers: the base-type and kind constraints, an optional per-type
    /// predicate, any verbatim extra entries, and the argument predicate for open generics. Bundles the
    /// candidate-defining inputs of <see cref="TypeSelectorWindow.Show"/> and the <see cref="TypeSelectorView"/>
    /// constructor into a single value so they travel together.
    /// </summary>
    public struct TypeSelectorFilter
    {
        /// <summary>
        /// Base types used to filter which concrete types are shown. Only types assignable to all entries are listed.
        /// Defaults to <see cref="object"/> when left <c>null</c>.
        /// </summary>
        public Type[] Types { get; set; }

        /// <summary>
        /// Which type kinds are included in the list.
        /// </summary>
        public TypeAllow Allow { get; set; }

        /// <summary>
        /// Optional predicate applied to each candidate type after the base-type and <see cref="Allow"/> checks.
        /// Return <c>false</c> to hide a type. Leave <c>null</c> to keep every matching type.
        /// </summary>
        public Func<Type, bool> Predicate { get; set; }

        /// <summary>
        /// Optional extra types appended to the list verbatim, bypassing the base-type and <see cref="Allow"/> checks —
        /// used to inject entries the assignability scan cannot match, such as open generic definitions.
        /// </summary>
        public IEnumerable<Type> AdditionalTypes { get; set; }

        /// <summary>
        /// Optional predicate applied to candidate types offered for an open generic's type arguments (in addition to
        /// the parameter's own constraints). Used to restrict arguments to, e.g., Unity-serializable types. Leave
        /// <c>null</c> to accept any constraint-satisfying type.
        /// </summary>
        public Func<Type, bool> ArgumentFilter { get; set; }

        /// <summary>
        /// Includes types marked <c>[TypeSelectorDisplay(Hidden = true)]</c>, which the picker leaves out by default.
        /// Set it only on a picker that <b>repairs</b> a reference rather than authors one: hiding a type means "do
        /// not offer this for new work", not "make existing data holding it unfixable".
        /// </summary>
        public bool IncludeHidden { get; set; }
    }
}
