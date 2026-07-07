using System;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Instructs the Unity Editor to use the type-selector window.
    /// </summary>
    /// <remarks>
    /// Only compiled in editor assemblies (<c>UNITY_EDITOR</c>).
    /// One or more base types can be supplied; the picker will show only their subtypes.
    /// </remarks>
    /// <example>
    /// Constrain to a single base type:
    /// <code>
    /// [TypeSelector(typeof(MonoBehaviour))]
    /// [SerializeField] private string _behaviourType;
    /// </code>
    ///
    /// Accept any type (unconstrained):
    /// <code>
    /// [TypeSelector]
    /// [SerializeField] private string _anyType;
    /// </code>
    ///
    /// Allow multiple independent base types:
    /// <code>
    /// [TypeSelector(typeof(IDisposable), typeof(ScriptableObject))]
    /// [SerializeField] private string _type;
    /// </code>
    /// </example>
    [Conditional(conditionString: "UNITY_EDITOR")]
    public sealed class TypeSelectorAttribute : PropertyAttribute
    {
        /// <summary>
        /// The assembly-qualified names of the base types that constrain the selection.
        /// </summary>
        public readonly string[] AssemblyQualifiedNames;

        /// <summary>
        /// Which special type categories (abstract classes, interfaces) the picker should
        /// include in addition to plain concrete classes. Defaults to <see cref="TypeAllow.All"/>,
        /// so a type-name field (a <c>string</c> or a <see cref="SerializableType"/>) offers abstract
        /// classes and interfaces too — set <see cref="TypeAllow.None"/> to restrict it to concrete types.
        /// Ignored on a <c>[SerializeReference]</c> managed reference: that path always lists only
        /// concrete, instantiable types regardless of this value.
        /// </summary>
        public TypeAllow Allow { get; set; } = TypeAllow.All;

        /// <summary>
        /// When <see langword="true"/>, an unset field is flagged: a <c>[SerializeReference]</c> managed reference left
        /// null, or a <c>string</c> field left empty, shows an inline "required" warning in the inspector and counts as a
        /// violation for the build/CI gate. A present-but-missing managed-reference type is not a required violation here —
        /// it has its own missing-type notice/gate. Defaults to <see langword="false"/>.
        /// <para>
        /// Also covers a <see cref="SerializableType"/> / <see cref="SerializableType{T}"/> field: an unset one — its stored
        /// type name left empty — shows the same inline notice and counts as a build/CI gate violation.
        /// </para>
        /// </summary>
        /// <example>
        /// <code>
        /// [SerializeReference, TypeSelector(typeof(IWeapon), Required = true)]
        /// private IWeapon _weapon;
        ///
        /// [TypeSelector(typeof(MonoBehaviour), Required = true)]
        /// [SerializeField] private string _behaviourType;
        /// </code>
        /// </example>
        public bool Required { get; set; }

        /// <summary>
        /// Creates an unconstrained attribute (base type is <see cref="object"/>).
        ///</summary>
        public TypeSelectorAttribute()
            : this(typeof(object)) { }

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
            AssemblyQualifiedNames = types
                .Where(type => type is not null)
                .Select(type => type.AssemblyQualifiedName)
                .ToArray();
        }

        /// <summary>
        /// Creates an attribute constrained to a single base type named by a string.
        /// </summary>
        /// <param name="assemblyQualifiedName">
        /// Either an <b>assembly-qualified type name</b> (e.g. <c>"MyGame.IWeapon, MyGame"</c>) resolved with
        /// <see cref="System.Type.GetType(string)"/>, or the <b>name of a member</b> on the same object that supplies
        /// the constraint at inspector time — see the constructor remarks.
        /// </param>
        /// <remarks>
        /// The string is resolved <b>member-first</b>: if it is a valid C# identifier and matches an instance field or
        /// property on the target object, that member's <i>current value</i> supplies the base type(s) — the constraint
        /// becomes dynamic, driven by another field. Otherwise the string is treated as an assembly-qualified type name.
        /// A member may be of type <see cref="System.Type"/>, <c>Type[]</c>, <c>string</c> (an assembly-qualified name),
        /// <c>string[]</c>, or a <see cref="SerializableType"/> / <see cref="SerializableType{T}"/> (and arrays of these).
        /// Prefer <c>nameof(...)</c> so a rename keeps the reference intact. When <c>typeof(...)</c> is possible, the
        /// <see cref="TypeSelectorAttribute(System.Type)"/> overload is safer; the string overloads exist for types the
        /// call site cannot reference (e.g. across an editor/asmdef boundary). Misuse (an unknown member, or a member of
        /// an unusable type) is reported at compile time by analyzer rules <c>AFT0006</c>–<c>AFT0008</c>, and as a quiet
        /// inline notice in the inspector for cases the analyzer cannot see (precompiled assemblies, renamed members).
        /// </remarks>
        /// <example>
        /// <code>
        /// // Constrain the picker to the value of another field, resolved live:
        /// [SerializeField] private SerializableType _category;
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
        /// Each entry is resolved independently, member-first: an identifier matching an instance field/property on the
        /// target object supplies its value as a constraint, otherwise the entry is an assembly-qualified type name. See
        /// <see cref="TypeSelectorAttribute(string)"/> for the full contract and the supported member types.
        /// </param>
        public TypeSelectorAttribute(params string[] assemblyQualifiedNames)
        {
            AssemblyQualifiedNames = assemblyQualifiedNames;
        }
    }
}
