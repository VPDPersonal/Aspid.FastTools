using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Shared constants and formatting helpers used by the type-selector UI
    /// (<see cref="TypeSelectorWindow"/>, <see cref="TypeField"/>, IMGUI drawers).
    /// </summary>
    internal static class TypeSelectorHelpers
    {
        /// <summary>
        /// Display string used for the "no type selected" option.
        /// </summary>
        public const string NoneOption = "<None>";

        /// <summary>
        /// Display string used for types that have no namespace (global namespace).
        /// </summary>
        public const string GlobalNamespace = "<Global>";

        /// <summary>
        /// Formats a caption for the type-selector dropdown.
        /// Returns the type's short name when <paramref name="value"/> is resolved,
        /// a <c>&lt;Missing ...&gt;</c> marker when the assembly-qualified name is non-empty
        /// but the type could not be resolved, or <see cref="NoneOption"/> when neither is provided.
        /// </summary>
        /// <param name="value">The resolved <see cref="Type"/>, or <see langword="null"/> if unresolved.</param>
        /// <param name="assemblyQualifiedName">
        /// The assembly-qualified name that was attempted. Pass non-null only when the type
        /// could not be resolved — passing it for a successfully resolved type forces the
        /// <c>&lt;Missing&gt;</c> branch.
        /// </param>
        public static string GetTypeSelectorTitle(Type value, string assemblyQualifiedName = null)
        {
            if (value is not null) return TypeExtensions.FormatGenericName(value);

            return string.IsNullOrWhiteSpace(assemblyQualifiedName)
                ? NoneOption
                : $"<Missing {assemblyQualifiedName}>";
        }

        /// <summary>
        /// Formats the hover tooltip for a resolved type shown in the type-selector dropdown — the full
        /// <c>Namespace.Class, Assembly</c> identity (generic arguments spelled out) — or <see langword="null"/> for
        /// no type. The caption shows only the short name, so the tooltip is where the complete identity stays
        /// readable; the format mirrors the missing-type tooltip's <c>ManagedTypeName.FullName</c>.
        /// </summary>
        public static string GetTypeSelectorTooltip(Type value)
        {
            if (value is null) return null;

            var name = TypeExtensions.FormatGenericName(value);
            var displayName = string.IsNullOrEmpty(value.Namespace) ? name : $"{value.Namespace}.{name}";
            return $"{displayName}, {value.Assembly.GetName().Name}";
        }
    }
}
