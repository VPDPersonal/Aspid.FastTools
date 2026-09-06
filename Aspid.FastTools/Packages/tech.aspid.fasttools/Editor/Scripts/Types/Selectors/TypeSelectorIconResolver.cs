using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // Resolves TypeSelectorDisplayAttribute.Icon strings (asset path, Resources path or built-in editor icon name) and
    // the per-type fallback icon. Hits are cached for the domain lifetime; misses are not, so a later-imported asset is
    // picked up on the next bind.
    internal static class TypeSelectorIconResolver
    {
        private const string TypeFallbackIcon = "d_cs Script Icon";
        private const string ScriptableObjectFallbackIcon = "d_ScriptableObject Icon";

        private static readonly Dictionary<string, Texture> _iconCache = new();
        private static readonly Dictionary<string, Texture> _typeFallbackCache = new();

        internal static Texture Resolve(string icon) =>
            string.IsNullOrWhiteSpace(icon) ? null : GetOrLoad(_iconCache, icon, LoadIcon);

        internal static Texture ResolveForType(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName))
                return Resolve(TypeFallbackIcon);

            return GetOrLoad(_typeFallbackCache, assemblyQualifiedName, LoadTypeFallbackIcon);
        }

        private static Texture GetOrLoad(Dictionary<string, Texture> cache, string key, Func<string, Texture> load)
        {
            if (cache.TryGetValue(key, out var cached))
            {
                // Unity-lifetime check, not a C# null check: a cached texture can be DESTROYED later (asset deleted,
                // Resources unloaded on play-mode load) — serving it binds an invisible icon forever.
                if (cached) return cached;
                cache.Remove(key);
            }

            var texture = load(key);

            // Only cache hits: a miss may be a not-yet-imported / freshly-renamed asset, so leave it uncached and
            // retry on the next bind instead of pinning a null for the whole domain lifetime.
            if (texture is not null)
                cache[key] = texture;

            return texture;
        }

        private static Texture LoadIcon(string icon)
        {
            // A project-relative asset path (e.g. "Assets/Art/Icons/MyIcon.png") is loaded straight through the
            // AssetDatabase, so the icon can live anywhere in the project — not only inside a Resources folder. The path
            // must carry its file extension, exactly as the AssetDatabase expects.
            if (icon.StartsWith("Assets/", StringComparison.Ordinal) ||
                icon.StartsWith("Packages/", StringComparison.Ordinal))
                return AssetDatabase.LoadAssetAtPath<Texture>(icon);

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
            return content?.image ?? Resources.Load<Texture>(icon);
        }

        private static Texture LoadTypeFallbackIcon(string assemblyQualifiedName)
        {
            var type = TypeUtility.GetTypeOrNull(assemblyQualifiedName);

            if (type is not null)
            {
                // GetMiniTypeThumbnail honors a custom icon assigned on the script's .meta and yields the
                // ScriptableObject icon for ScriptableObject-derived types.
                var thumbnail = AssetPreview.GetMiniTypeThumbnail(type);
                if (thumbnail is not null) return thumbnail;

                // Safety net when Unity has no cached thumbnail for the type yet.
                if (typeof(ScriptableObject).IsAssignableFrom(type))
                    return Resolve(ScriptableObjectFallbackIcon);
            }

            return Resolve(TypeFallbackIcon);
        }
    }
}
