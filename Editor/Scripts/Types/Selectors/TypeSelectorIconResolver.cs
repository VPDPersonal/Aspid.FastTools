using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
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

            if (icon.Contains('/'))
            {
                var resource = Resources.Load<Texture>(icon);
                if (resource is not null) return resource;

                var pathContent = EditorGUIUtility.IconContent(icon);
                return pathContent?.image;
            }

            var content = EditorGUIUtility.IconContent(icon);
            return content?.image ?? Resources.Load<Texture>(icon);
        }

        private static Texture LoadTypeFallbackIcon(string assemblyQualifiedName)
        {
            var type = TypeUtility.GetTypeOrNull(assemblyQualifiedName);

            if (type is not null)
            {
                var thumbnail = AssetPreview.GetMiniTypeThumbnail(type);
                if (thumbnail is not null) return thumbnail;

                if (typeof(ScriptableObject).IsAssignableFrom(type))
                    return Resolve(ScriptableObjectFallbackIcon);
            }

            return Resolve(TypeFallbackIcon);
        }
    }
}
