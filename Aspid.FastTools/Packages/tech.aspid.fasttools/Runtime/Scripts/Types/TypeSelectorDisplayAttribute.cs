#nullable enable
using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Sets how the type-selector window shows a type, or hides the type with <see cref="Hidden"/>.
    /// </summary>
    /// <remarks>
    /// Has no effect on a type compiled outside Unity, such as a <c>dotnet build</c> DLL:
    /// <c>[Conditional("UNITY_EDITOR")]</c> drops the usage there.
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
        /// Gets or sets the name shown instead of the type's short name; <see langword="null"/> or whitespace keeps
        /// the short name.
        /// </summary>
        /// <remarks>
        /// Search still matches the real name, and a generic type keeps its arguments appended
        /// (<c>Mod&lt;Single&gt;</c>).
        /// </remarks>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the picker path that replaces the namespace, with <c>/</c> separating levels
        /// (<c>"Combat/Melee"</c>); <see langword="null"/> or whitespace keeps the namespace.
        /// </summary>
        public string? Group { get; set; }

        /// <summary>
        /// Gets or sets the tooltip shown on the type's row; <see langword="null"/> means no override.
        /// </summary>
        public string? Tooltip { get; set; }

        /// <summary>
        /// Gets or sets the icon shown left of the label: an <c>EditorGUIUtility.IconContent</c> name
        /// (<c>"d_ScriptableObject Icon"</c>), an asset path with extension (<c>"Assets/Art/Icons/Damage.png"</c>) or
        /// a <c>Resources</c> path without one (<c>"Icons/Damage"</c>); <see langword="null"/> for none.
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the picker hides this type.
        /// </summary>
        /// <remarks>
        /// Not inherited by subclasses. Assigning from code and values already stored are unaffected.
        /// </remarks>
        public bool Hidden { get; set; }
    }
}
