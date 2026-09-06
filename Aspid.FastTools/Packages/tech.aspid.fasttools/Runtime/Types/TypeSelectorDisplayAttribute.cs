#nullable enable
using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Supplies presentation metadata for a type in the type-selector window — display name, group, tooltip and
    /// icon — or keeps the type out of the picker entirely with <see cref="Hidden"/>.
    /// </summary>
    /// <remarks>
    /// <c>[Conditional("UNITY_EDITOR")]</c> keeps this metadata out of player builds. The compiler evaluates the
    /// symbol at the use site, so a type compiled outside Unity — a plugin <c>.dll</c> built by
    /// <c>dotnet build</c> — carries no usage at all and none of these settings apply to it,
    /// <see cref="Hidden"/> included. Declare the attribute from inside the Unity project.
    /// </remarks>
    /// <example>
    /// <code>
    /// [TypeSelectorDisplay(
    ///     Name = "Damage ×",
    ///     Group = "Combat/Modifiers",
    ///     Tooltip = "Scales incoming damage",
    ///     Icon = "d_ScriptableObject Icon")]
    /// public sealed class DamageModifier { }
    /// </code>
    /// <code>
    /// [TypeSelectorDisplay(Hidden = true)]
    /// public sealed class DelegateModifier : IModifier { }
    /// </code>
    /// </example>
    [Conditional(conditionString: "UNITY_EDITOR")]
    [AttributeUsage(
        validOn: AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
        Inherited = false)]
    public sealed class TypeSelectorDisplayAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name shown instead of the type's short name. <see langword="null"/> or whitespace means
        /// no override.
        /// </summary>
        /// <remarks>
        /// Search still matches the real name, the tooltip still shows the full identity, and a generic type keeps
        /// its formatted arguments appended (<c>Mod&lt;Single&gt;</c>).
        /// </remarks>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets an explicit picker path, with <c>/</c> separating levels (<c>"Combat/Melee"</c>).
        /// <see langword="null"/> or whitespace keeps the type under its namespace.
        /// </summary>
        /// <remarks>
        /// The path replaces the type's namespace placement, so the type appears only under it. Empty segments are
        /// ignored.
        /// </remarks>
        public string? Group { get; set; }

        /// <summary>
        /// Gets or sets the tooltip shown on the type's row; <see langword="null"/> means no override.
        /// </summary>
        public string? Tooltip { get; set; }

        /// <summary>
        /// Gets or sets the icon shown left of the label: an <c>EditorGUIUtility.IconContent</c> name
        /// (<c>"d_ScriptableObject Icon"</c>), a project-relative asset path with extension
        /// (<c>"Assets/Art/Icons/Damage.png"</c>), or a <c>Resources</c> path without extension
        /// (<c>"Icons/Damage"</c>). Resolved lazily; <see langword="null"/> means no icon.
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the picker never offers this type — for types that are assignable
        /// but not meant to be authored in the Inspector.
        /// </summary>
        /// <remarks>
        /// Assigning from code is unaffected, and a value already stored in a field keeps rendering. Not inherited,
        /// so hiding a base type never hides the subclasses meant to be picked instead.
        /// </remarks>
        public bool Hidden { get; set; }
    }
}
