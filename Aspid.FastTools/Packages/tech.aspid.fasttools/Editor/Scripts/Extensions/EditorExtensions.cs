using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools.Editors
{
    /// <summary>
    /// Provides extension methods for resolving Unity object display names.
    /// </summary>
    public static class EditorExtensions
    {
        /// <summary>
        /// Returns the last segment of the <see cref="AddComponentMenu"/> path declared on the object's own type, or the nicified type name.
        /// </summary>
        /// <remarks>
        /// An attribute inherited from a base class, an empty path or a path ending with <c>/</c> falls back to the type name.
        /// Unlike <see cref="ObjectNames.GetInspectorTitle(Object)"/>, the result never carries the <c>(Script)</c> or <c>(Deprecated)</c> suffix.
        /// </remarks>
        /// <param name="obj">The object whose display name to resolve.</param>
        /// <returns>The display name; otherwise, <see cref="string.Empty"/> if <paramref name="obj"/> is <see langword="null"/> or destroyed.</returns>
        public static string GetDisplayName(this Object obj)
        {
            if (!obj) return string.Empty;

            var targetType = obj.GetType();
            return TryGetComponentMenuTitle(targetType, out var title)
                ? title
                : ObjectNames.NicifyVariableName(targetType.Name);
        }

        /// <summary>
        /// Returns the component display name with a one-based suffix when its object has multiple components of the exact same type.
        /// </summary>
        /// <param name="targetComponent">The component whose indexed display name to resolve.</param>
        /// <returns>The display name, indexed in component order when duplicates exist; otherwise, <see cref="string.Empty"/> if <paramref name="targetComponent"/> is <see langword="null"/> or destroyed.</returns>
        public static string GetDisplayNameWithIndex(this Component targetComponent)
        {
            if (!targetComponent) return string.Empty;

            var type = targetComponent.GetType();
            var displayName = targetComponent.GetDisplayName();
            using var pooled = ListPool<Component>.Get(out var components);
            targetComponent.GetComponents(type, components);

            var count = 0;
            var index = 0;

            foreach (var component in components)
            {
                if (!component || component.GetType() != type) continue;

                count++;
                if (component == targetComponent)
                    index = count;
            }

            return count > 1 && index > 0
                ? $"{displayName} ({index})"
                : displayName;
        }

        // Mirrors the title rule of ObjectNames.GetInspectorTitle, which reads only the attribute declared on the type itself.
        private static bool TryGetComponentMenuTitle(Type type, out string title)
        {
            var attribute = (AddComponentMenu)Attribute.GetCustomAttribute(type, typeof(AddComponentMenu), inherit: false);
            title = attribute?.componentMenu?.Trim();
            if (string.IsNullOrEmpty(title)) return false;

            var separatorIndex = title.LastIndexOf('/');
            if (separatorIndex == title.Length - 1) return false;

            title = title[(separatorIndex + 1)..];
            return true;
        }
    }
}
