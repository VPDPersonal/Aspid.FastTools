#nullable enable
using System;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Draws the field with the type-selector window.
    /// </summary>
    /// <remarks>
    /// With several base types the picker shows only types assignable to all of them.
    /// </remarks>
    /// <example>
    /// <code>
    /// [TypeSelector(typeof(MonoBehaviour))]
    /// [SerializeField] private string _behaviorType;
    /// </code>
    /// <code>
    /// [TypeSelector]
    /// [SerializeField] private string _anyType;
    /// </code>
    /// <code>
    /// [TypeSelector(typeof(IDisposable), typeof(ScriptableObject))]
    /// [SerializeField] private string _type;
    /// </code>
    /// </example>
    [Conditional(conditionString: "UNITY_EDITOR")]
    public sealed class TypeSelectorAttribute : PropertyAttribute
    {
        /// <summary>
        /// Gets the raw constraint arguments: assembly-qualified names of base types, or names of members supplying
        /// them (see <see cref="TypeSelectorAttribute(string)"/>). Empty for an unconstrained selector.
        /// </summary>
        public string[] AssemblyQualifiedNames { get; }

        /// <summary>
        /// Gets or sets which special type categories the picker offers besides concrete classes;
        /// <see cref="TypeAllow.All"/> by default.
        /// </summary>
        /// <remarks>
        /// Ignored on a <c>[SerializeReference]</c> field, which always lists only instantiable types.
        /// </remarks>
        public TypeAllow Allow { get; set; } = TypeAllow.All;

        /// <summary>
        /// Gets or sets a value indicating whether an unset field shows an inline "required" warning and counts as a
        /// violation for the build/CI gate.
        /// </summary>
        /// <remarks>
        /// "Unset" means <see langword="null"/> for a <c>[SerializeReference]</c> field and an empty name for a
        /// <c>string</c> or <see cref="SerializableType"/> field. A reference that is set but whose type no longer
        /// resolves is not a violation of this flag — the separate missing-type gate covers that.
        /// </remarks>
        /// <example>
        /// <code>
        /// [TypeSelector(typeof(IWeapon), Required = true)]
        /// [SerializeReference] private IWeapon _weapon;
        /// </code>
        /// </example>
        public bool Required { get; set; }

        /// <summary>
        /// Creates an unconstrained attribute: any type is offered.
        /// </summary>
        public TypeSelectorAttribute()
            : this(Array.Empty<Type>()) { }

        /// <summary>
        /// Creates an attribute constrained to a single base type.
        /// </summary>
        /// <param name="type">The base constraint type.</param>
        public TypeSelectorAttribute(Type type)
            : this(types: type) { }

        /// <summary>
        /// Creates an attribute constrained to one or more base types.
        /// </summary>
        /// <param name="types">The base constraint types.</param>
        public TypeSelectorAttribute(params Type[] types)
        {
            // A generic type parameter has no assembly-qualified name; skip it rather than store a null entry.
            AssemblyQualifiedNames = types?
                .Select(type => type?.AssemblyQualifiedName)
                .OfType<string>()
                .ToArray() ?? Array.Empty<string>();
        }

        /// <summary>
        /// Creates an attribute constrained to a single base type named by a string.
        /// </summary>
        /// <remarks>
        /// Resolved member-first: an identifier matching an instance field or property on the target object supplies
        /// the constraint from its current value, so it can be driven live by another field; anything else is treated
        /// as an assembly-qualified type name. A member may be a <see cref="System.Type"/>, a <c>string</c>, a
        /// <see cref="SerializableType"/>, or an array of these. Prefer <c>nameof(...)</c> so a rename keeps the
        /// reference intact. A name that resolves to nothing is surfaced as an inline inspector notice.
        /// </remarks>
        /// <param name="assemblyQualifiedName">
        /// An assembly-qualified type name (<c>"MyGame.IWeapon, MyGame"</c>) or the name of a member supplying the
        /// constraint — see the remarks.
        /// </param>
        /// <example>
        /// <code>
        /// [SerializeField] private SerializableType _category;
        /// </code>
        /// <code>
        /// [TypeSelector(nameof(_category))]
        /// [SerializeField] private string _subType;
        /// </code>
        /// </example>
        public TypeSelectorAttribute(string assemblyQualifiedName)
            : this(assemblyQualifiedNames: assemblyQualifiedName) { }

        /// <summary>
        /// Creates an attribute constrained to one or more base types, each named by a string.
        /// </summary>
        /// <param name="assemblyQualifiedNames">
        /// Each entry is resolved independently, member-first; see <see cref="TypeSelectorAttribute(string)"/>.
        /// </param>
        public TypeSelectorAttribute(params string[]? assemblyQualifiedNames)
        {
            AssemblyQualifiedNames = assemblyQualifiedNames?
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToArray() ?? Array.Empty<string>();
        }
    }
}
