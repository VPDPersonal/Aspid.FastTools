using System;
using System.Reflection;
using System.Collections.Generic;

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

        // Normalized [TypeSelectorDisplay(Name)] per type (null = no override). Attributes only change with a
        // recompile, which resets this with the domain; IMGUI resolves the caption every repaint, hence the cache.
        private static readonly Dictionary<Type, string> CustomDisplayNames = new();

        /// <summary>
        /// The normalized <see cref="TypeSelectorDisplayAttribute.Name"/> override for <paramref name="value"/>,
        /// or <see langword="null"/> when the type declares none (a whitespace-only value counts as none, and so
        /// does a value equal to the <see cref="NoneOption"/> sentinel — a real type must not impersonate it).
        /// A generic keeps its formatted arguments (or parameters) after the custom base name — <c>Mod&lt;Single&gt;</c>
        /// for a constructed form (whose override comes from its generic type definition), <c>Mod&lt;T&gt;</c> for the
        /// open definition — so two closed forms stay tellable apart and an open definition still reads as generic.
        /// </summary>
        public static string GetCustomDisplayName(Type value)
        {
            if (value is null) return null;
            if (CustomDisplayNames.TryGetValue(value, out var cached)) return cached;

            var attribute = value.GetCustomAttribute<TypeSelectorDisplayAttribute>(inherit: false);
            var name = string.IsNullOrWhiteSpace(attribute?.Name) ? null : attribute.Name.Trim();

            if (name == NoneOption) name = null;

            if (name is not null && value.IsGenericType)
            {
                var formatted = TypeExtensions.FormatGenericName(value);
                var angle = formatted.IndexOf('<');
                if (angle >= 0) name += formatted[angle..];
            }

            CustomDisplayNames[value] = name;
            return name;
        }

        /// <summary>
        /// Formats a caption for the type-selector dropdown.
        /// Returns the type's <see cref="TypeSelectorDisplayAttribute.Name"/> override (or its short name)
        /// when <paramref name="value"/> is resolved, a <c>&lt;Missing ...&gt;</c> marker when the
        /// assembly-qualified name is non-empty but the type could not be resolved, or
        /// <see cref="NoneOption"/> when neither is provided.
        /// </summary>
        /// <param name="value">The resolved <see cref="Type"/>, or <see langword="null"/> if unresolved.</param>
        /// <param name="assemblyQualifiedName">
        /// The assembly-qualified name that was attempted. Pass non-null only when the type
        /// could not be resolved — passing it for a successfully resolved type forces the
        /// <c>&lt;Missing&gt;</c> branch.
        /// </param>
        public static string GetTypeSelectorTitle(Type value, string assemblyQualifiedName = null)
        {
            if (value is not null)
                return GetCustomDisplayName(value) ?? TypeExtensions.FormatGenericName(value);

            return string.IsNullOrWhiteSpace(assemblyQualifiedName)
                ? NoneOption
                : $"<Missing {assemblyQualifiedName}>";
        }

        /// <summary>
        /// Formats the hover tooltip for a resolved type shown in the type-selector dropdown — the full
        /// <c>Namespace.Class, Assembly</c> identity (generic arguments spelled out) — or <see langword="null"/> for
        /// no type. The caption shows only the short name (or its <see cref="TypeSelectorDisplayAttribute.Name"/>
        /// override), so the tooltip is where the real identity stays readable; the format mirrors the missing-type
        /// tooltip's <c>ManagedTypeName.FullName</c>.
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
