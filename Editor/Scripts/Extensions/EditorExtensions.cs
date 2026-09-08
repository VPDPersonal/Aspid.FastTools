using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
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
        public static string GetScriptName(this Object obj)
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
        /// <returns>The display name, indexed in component order when duplicates exist; otherwise, <see langword="null"/> if <paramref name="targetComponent"/> is <see langword="null"/> or destroyed.</returns>
        public static string GetScriptNameWithIndex(this Component targetComponent)
        {
            if (!targetComponent) return null;

            var type = targetComponent.GetType();
            var components = targetComponent.GetComponents(type)
                .Where(component => component.GetType() == type)
                .ToArray();

            if (components.Length <= 1)
                return targetComponent.GetScriptName();

            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] == targetComponent)
                    return $"{targetComponent.GetScriptName()} ({i + 1})";
            }

            return targetComponent.GetScriptName();
        }
    }
}
