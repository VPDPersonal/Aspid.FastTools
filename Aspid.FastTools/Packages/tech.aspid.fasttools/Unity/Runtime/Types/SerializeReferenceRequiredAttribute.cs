#nullable enable
using UnityEngine;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Marks a <c>[SerializeReference]</c> managed-reference field (or list element) as required: an unset reference is
    /// flagged with an inline notice in the inspector and counts as a violation for the build/CI gate.
    /// </summary>
    /// <remarks>
    /// Editor-only by <see cref="ConditionalAttribute"/> (stripped from player builds), with no <c>UnityEditor</c>
    /// dependency, so it ships safely in the runtime assembly alongside <c>[SerializeReference]</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// [SerializeReference, TypeSelector(typeof(IWeapon)), SerializeReferenceRequired]
    /// private IWeapon _weapon;
    /// </code>
    /// </example>
    [Conditional(conditionString: "UNITY_EDITOR")]
    public sealed class SerializeReferenceRequiredAttribute : PropertyAttribute
    {
        /// <summary>Optional custom text shown in the inline "required" notice instead of the default message.</summary>
        public string? Message { get; set; }

        /// <summary>
        /// When <see langword="true"/>, only a null reference is a violation; a present-but-missing type is left to the
        /// independent missing-type notice/gate. Defaults to <see langword="false"/>.
        /// </summary>
        public bool AllowMissingType { get; set; }
    }
}
