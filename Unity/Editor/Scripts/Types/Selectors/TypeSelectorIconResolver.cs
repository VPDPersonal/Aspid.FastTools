using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Resolves the <see cref="TypeSelectorDisplayAttribute.Icon"/> string to a <see cref="Texture"/>. A plain name is
    /// tried as an editor built-in icon (<see cref="EditorGUIUtility.IconContent"/>) first and a <c>Resources</c>
    /// texture path second; a path-shaped value (one containing <c>/</c>) reverses that order, so probing a Resources
    /// path through <see cref="EditorGUIUtility.IconContent"/> does not spam the console with "Unable to load icon"
    /// warnings on every miss. Results (including misses) are cached for the lifetime of the domain to keep row
    /// binding cheap.
    /// </summary>
    internal static class TypeSelectorIconResolver
    {
        private static readonly Dictionary<string, Texture> Cache = new();

        public static Texture Resolve(string icon)
        {
            if (string.IsNullOrWhiteSpace(icon)) return null;

            if (Cache.TryGetValue(icon, out var cached))
                return cached;

            var texture = LoadIcon(icon);
            Cache[icon] = texture;
            return texture;
        }

        private static Texture LoadIcon(string icon)
        {
            // A slash signals a Resources path (e.g. "Icons/MyIcon") rather than a built-in editor icon name. Probing
            // such a string through IconContent first logs a "Unable to load icon" warning to the console on every
            // miss, so for path-shaped strings the Resources load is tried first and IconContent is only the fallback.
            if (icon.Contains('/'))
            {
                var resource = Resources.Load<Texture>(icon);
                if (resource is not null) return resource;

                var pathContent = EditorGUIUtility.IconContent(icon);
                return pathContent?.image;
            }

            // Built-in editor icon (e.g. "d_ScriptableObject Icon"). IconContent never throws but may
            // return an empty content whose image is null.
            var content = EditorGUIUtility.IconContent(icon);
            if (content?.image is not null) return content.image;

            // Fall back to a Resources texture path.
            return Resources.Load<Texture>(icon);
        }
    }
}
