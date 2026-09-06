using System;
using System.Reflection;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // Shared constants and formatting helpers for the type-selector UI.
    internal static class TypeSelectorHelpers
    {
        internal const string None = "○";
        internal const string Check = "✓";
        internal const string StarEmpty = "☆";
        internal const string StarFilled = "★";
        internal const string NoneOption = "<None>";
        internal const string GlobalNamespace = "<Global>";

        private static readonly Dictionary<Type, string> _customDisplayNames = new();

        // The type's display-name override, or null when it declares none. Whitespace counts as none, as does a
        // value equal to the <None> sentinel, which a real type must not impersonate. A generic keeps its formatted
        // arguments after the custom name, so closed forms stay distinguishable.
        internal static string GetCustomDisplayName(Type value)
        {
            if (value is null) return null;
            if (_customDisplayNames.TryGetValue(value, out var cached)) return cached;

            var attribute = value.GetCustomAttribute<TypeSelectorDisplayAttribute>(inherit: false);
            var name = string.IsNullOrWhiteSpace(attribute?.Name)
                ? null
                : attribute.Name.Trim();

            if (name == NoneOption)
                name = null;

            if (name is not null && value.IsGenericType)
            {
                var formatted = TypeUtility.FormatGenericName(value);
                name += formatted[formatted.IndexOf('<')..];
            }

            _customDisplayNames[value] = name;
            return name;
        }

        // Governs what may be authored, not what may be repaired: a picker re-pointing a broken reference ignores
        // this, while the Smart Fix suggestion honors it. Not inherited, so hiding a base type leaves the subclasses
        // meant to replace it offered.
        internal static bool IsHiddenFromPicker(Type value) =>
            value?.GetCustomAttribute<TypeSelectorDisplayAttribute>(inherit: false)?.Hidden ?? false;

        // The dropdown caption: the type's display name when resolved, a <Missing …> marker when only an
        // unresolvable name is given, and <None> when neither is.
        internal static string GetTypeSelectorTitle(Type value, string assemblyQualifiedName = null)
        {
            if (value is not null)
                return GetCustomDisplayName(value) ?? TypeUtility.FormatGenericName(value);

            return string.IsNullOrWhiteSpace(assemblyQualifiedName)
                ? NoneOption
                : $"<Missing {assemblyQualifiedName}>";
        }

        // The full Namespace.Class, Assembly identity, since the caption shows only the short name.
        internal static string GetTypeSelectorTooltip(Type value)
        {
            if (value is null) return null;

            var name = TypeUtility.FormatGenericName(value);
            var displayName = string.IsNullOrEmpty(value.Namespace) ? name : $"{value.Namespace}.{name}";

            return $"{displayName}, {value.Assembly.GetName().Name}";
        }
    }
}
