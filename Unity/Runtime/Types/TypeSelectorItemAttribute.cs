using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Supplies presentation metadata for a type shown in the type-selector window: a tooltip and an icon.
    /// </summary>
    /// <remarks>
    /// Only compiled in editor assemblies (<c>UNITY_EDITOR</c>); the attribute carries no
    /// runtime behaviour and references no <c>UnityEditor</c> types (the <see cref="Icon"/> is a
    /// plain string the editor resolves lazily).
    /// </remarks>
    /// <example>
    /// Give the type a tooltip and an icon:
    /// <code>
    /// [TypeSelectorItem(Tooltip = "Scales incoming damage", Icon = "d_ScriptableObject Icon")]
    /// public sealed class DamageModifier { }
    /// </code>
    /// </example>
    [Conditional(conditionString: "UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class TypeSelectorItemAttribute : Attribute
    {
        /// <summary>
        /// Tooltip shown when hovering the type's row. <see langword="null"/> means no tooltip override.
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// Editor icon to show left of the label. Either an <c>EditorGUIUtility.IconContent</c> name
        /// (e.g. <c>"d_ScriptableObject Icon"</c>) or a <c>Resources</c> texture path. The editor resolves
        /// the value lazily; <see langword="null"/> means no icon.
        /// </summary>
        public string Icon { get; set; }
    }
}
