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
        /// Returns the inspector title when an inherited <see cref="AddComponentMenu"/> exists, or the nicified type name.
        /// </summary>
        /// <param name="obj">The object whose display name to resolve.</param>
        /// <returns>The display name; otherwise, <see cref="string.Empty"/> if <paramref name="obj"/> is <see langword="null"/> or destroyed.</returns>
        public static string GetDisplayName(this Object obj)
        {
            if (!obj) return string.Empty;

            var targetType = obj.GetType();
            return Attribute.IsDefined(targetType, typeof(AddComponentMenu), inherit: true)
                ? ObjectNames.GetInspectorTitle(obj)
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
#if d

  #endif
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
    }
}
