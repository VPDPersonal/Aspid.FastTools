using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Resolves the <see cref="TypeSelectorItemAttribute.Icon"/> string to a <see cref="Texture"/>.
    /// The value is first tried as an editor built-in icon (<see cref="EditorGUIUtility.IconContent"/>)
    /// and then as a <c>Resources</c> texture path. Results (including misses) are cached for the
    /// lifetime of the domain to keep row binding cheap.
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
            // Built-in editor icon (e.g. "d_ScriptableObject Icon"). IconContent never throws but may
            // return an empty content whose image is null.
            var content = EditorGUIUtility.IconContent(icon);
            if (content?.image is not null) return content.image;

            // Fall back to a Resources texture path.
            return Resources.Load<Texture>(icon);
        }
    }
}
