using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Supplies presentation metadata for a type shown in the type-selector window:
    /// a custom display name or category placement, a tooltip, an ordering hint and an icon.
    /// </summary>
    /// <remarks>
    /// Only compiled in editor assemblies (<c>UNITY_EDITOR</c>); the attribute carries no
    /// runtime behaviour and references no <c>UnityEditor</c> types (the <see cref="Icon"/> is a
    /// plain string the editor resolves lazily).
    /// </remarks>
    /// <example>
    /// Re-home the type under a category and give it a tooltip:
    /// <code>
    /// [TypeSelectorItem("Combat/Damage Modifier", Tooltip = "Scales incoming damage", Order = 10)]
    /// public sealed class DamageModifier { }
    /// </code>
    ///
    /// Rename the leaf in place (no <c>/</c> means the type keeps its namespace location):
    /// <code>
    /// [TypeSelectorItem("Damage Modifier")]
    /// public sealed class DamageModifier { }
    /// </code>
    /// </example>
    [Conditional(conditionString: "UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class TypeSelectorItemAttribute : Attribute
    {
        /// <summary>
        /// The display path of the type in the selector.
        /// A value containing <c>/</c> (e.g. <c>"Combat/Damage Modifier"</c>) re-homes the type under
        /// those category nodes instead of its namespace path; a plain value (e.g. <c>"Damage Modifier"</c>)
        /// only renames the leaf in place. <see langword="null"/> or empty keeps the default type name.
        /// </summary>
        public string DisplayPath { get; }

        /// <summary>
        /// Tooltip shown when hovering the type's row. <see langword="null"/> means no tooltip override.
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// Ordering hint within the type's group: lower values appear higher. Defaults to <c>0</c>;
        /// ties are broken alphabetically by display name.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Editor icon to show left of the label. Either an <c>EditorGUIUtility.IconContent</c> name
        /// (e.g. <c>"d_ScriptableObject Icon"</c>) or a <c>Resources</c> texture path. The editor resolves
        /// the value lazily; <see langword="null"/> means no icon.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Creates the attribute with an optional display path.
        /// </summary>
        /// <param name="displayPath">
        /// Either a <c>/</c>-separated category path that re-homes the type, or a plain leaf name that
        /// renames it in place. Pass <see langword="null"/> to keep the default type name and only set
        /// <see cref="Tooltip"/>, <see cref="Order"/> or <see cref="Icon"/>.
        /// </param>
        public TypeSelectorItemAttribute(string displayPath = null)
        {
            DisplayPath = displayPath;
        }
    }
}
