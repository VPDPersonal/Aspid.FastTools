using UnityEngine;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences
{
    /// <summary>
    /// Replaces the default <c>[SerializeReference]</c> inspector with a hierarchical type-selector
    /// dropdown, letting the user pick which concrete implementation of the field's declared type
    /// is instantiated and assigned to the managed reference.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Apply this attribute <b>together</b> with <c>[SerializeReference]</c>. The declared type of the
    /// field (interface, abstract class, or base class) defines the set of candidate types; the dropdown
    /// lists every concrete, non-<see cref="Object"/> class assignable to it. Picking a type creates a
    /// fresh instance, <c>&lt;None&gt;</c> clears the reference, and a type whose serialized data no longer
    /// resolves is surfaced as a missing-reference warning.
    /// </para>
    /// <para>
    /// Works on plain fields, arrays, and <see cref="System.Collections.Generic.List{T}"/> of a
    /// <c>[SerializeReference]</c> type. Only compiled in editor assemblies (<c>UNITY_EDITOR</c>).
    /// </para>
    /// </remarks>
    /// <example>
    /// Single polymorphic field:
    /// <code>
    /// [SerializeReference, SerializeReferenceSelector]
    /// private IWeapon _weapon;
    /// </code>
    ///
    /// List of polymorphic elements:
    /// <code>
    /// [SerializeReference, SerializeReferenceSelector]
    /// private List&lt;IWeapon&gt; _weapons;
    /// </code>
    /// </example>
    [Conditional(conditionString: "UNITY_EDITOR")]
    public sealed class SerializeReferenceSelectorAttribute : PropertyAttribute { }
}
