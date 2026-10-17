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
    /// On an array or <see cref="System.Collections.Generic.List{T}"/> field each element gets a picker, and the add
    /// button of a <c>[SerializeReference]</c> list opens one.
    /// </remarks>
    /// <example>
    /// <code>
    /// [TypeSelector(typeof(MonoBehaviour))]
    /// [SerializeField] private string _behaviorType;
    /// </code>
    /// <code>
    /// [TypeSelector(typeof(IDisposable), typeof(ScriptableObject))]
    /// [SerializeField] private string _type;
    /// </code>
    /// <code>
    /// [SerializeField] private SerializableType _category;
    /// [TypeSelector(nameof(_category))]
    /// [SerializeField] private string _subType;
    /// </code>
    /// <code>
    /// [TypeSelector(typeof(IWeapon), Required = true)]
    /// [SerializeReference] private IWeapon _weapon;
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
        /// Gets or sets a value indicating whether an unset field shows a "required" warning and is reported as a
        /// violation.
        /// </summary>
        /// <remarks>
        /// Applies to <c>[SerializeReference]</c>, <see cref="string"/> and <see cref="SerializableTypeBase"/> fields;
        /// a type that no longer resolves is reported by the missing-type check instead.
        /// </remarks>
        public bool Required { get; set; }

        /// <summary>
        /// Creates an unconstrained attribute: any type is offered.
        /// </summary>
        public TypeSelectorAttribute()
            : this(types: Array.Empty<Type>()) { }

        /// <summary>
        /// Creates an attribute constrained to a single base type.
        /// </summary>
        /// <param name="type">The base type.</param>
        public TypeSelectorAttribute(Type type)
            : this(types: type) { }

        /// <summary>
        /// Creates an attribute constrained to one or more base types.
        /// </summary>
        /// <param name="types">The base types; the picker offers only types assignable to all of them.</param>
        public TypeSelectorAttribute(params Type[]? types)
            : base(applyToCollection: true)
        {
            // A generic type parameter has no assembly-qualified name; skip it rather than store a null entry.
            AssemblyQualifiedNames = types?
                .Select(type => type?.AssemblyQualifiedName)
                .OfType<string>()
                .ToArray() ?? Array.Empty<string>();
        }

        /// <summary>
        /// Creates an attribute constrained by a type name or by a member that supplies the base types.
        /// </summary>
        /// <remarks>
        /// An instance field or property of that name supplies the types from its current value: a
        /// <see cref="System.Type"/>, a <see cref="string"/>, an <see cref="ISerializableType"/> or an array of these.
        /// Any other string is an assembly-qualified type name.
        /// </remarks>
        /// <param name="assemblyQualifiedName">
        /// An assembly-qualified type name (<c>"MyGame.IWeapon, MyGame"</c>) or a member name
        /// (<c>nameof(_category)</c>).
        /// </param>
        public TypeSelectorAttribute(string assemblyQualifiedName)
            : this(assemblyQualifiedNames: assemblyQualifiedName) { }

        /// <summary>
        /// Creates an attribute constrained by one or more type or member names.
        /// </summary>
        /// <param name="assemblyQualifiedNames">
        /// Type or member names, each resolved as in <see cref="TypeSelectorAttribute(string)"/>.
        /// </param>
        public TypeSelectorAttribute(params string[]? assemblyQualifiedNames)
            : base(applyToCollection: true)
        {
            AssemblyQualifiedNames = assemblyQualifiedNames?
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToArray() ?? Array.Empty<string>();
        }
    }
}
