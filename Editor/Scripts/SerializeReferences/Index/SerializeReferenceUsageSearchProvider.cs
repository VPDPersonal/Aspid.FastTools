using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.Search;
using System.Collections.Generic;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceUsageSearchProvider
    {
        private const string ProviderId = "sr";
        private const string FilterId = "sr:";
        private const string ExactPrefix = "=";
        private const string DisplayName = "Managed References";

        [SearchItemProvider]
        public static SearchProvider CreateProvider() =>
            new(ProviderId, DisplayName)
            {
                filterId = FilterId,
                // Explicit-only, or the provider would join every general search and the first keystroke would warm
                // a cold index — a modal full-project sweep.
                isExplicitProvider = true,
                priority = 9000,
                showDetailsOptions = ShowDetailsOptions.Description | ShowDetailsOptions.Preview,
                fetchItems = (context, items, provider) => FetchItems(context, items, provider),
                fetchThumbnail = (item, context) => AssetThumbnail(item),
                toObject = (item, type) => LoadAsset(item),
                trackSelection = (item, context) => Ping(item),
            };

        public static void OpenSearch(Type type)
        {
            if (type is null) return;
            var context = SearchService.CreateContext(ProviderId, QueryFor(type));
            SearchService.ShowWindow(context, "Find Usages", saveFilters: false);
        }

        // The menu opens on one concrete type, so its query names the stored identity exactly: a class-name substring
        // would also list PistolMk2 and namesakes from other namespaces or assemblies.
        internal static string QueryFor(Type type) =>
            $"{FilterId}{ExactPrefix}{ManagedTypeName.FromType(type).FullName}";

        internal static string Token(string query)
        {
            var token = (query ?? string.Empty).Trim();
            return token.StartsWith(FilterId, StringComparison.OrdinalIgnoreCase)
                ? token[FilterId.Length..].Trim()
                : token;
        }

        // An '=' token is the exact "Namespace.Class, Assembly" the menu writes; a free-typed token keeps matching any
        // stored class name that contains it.
        internal static bool Matches(string token, ManagedTypeName storedType)
        {
            if (string.IsNullOrEmpty(token)) return false;

            if (token.StartsWith(ExactPrefix, StringComparison.Ordinal))
                return string.Equals(storedType.FullName, token[ExactPrefix.Length..].Trim(), StringComparison.Ordinal);

            return (storedType.Class ?? string.Empty).IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // Null is the synchronous fetch convention.
        private static object FetchItems(SearchContext context, List<SearchItem> items, SearchProvider provider)
        {
            var token = Token(context.searchQuery);
            if (token.Length == 0) return null;

            foreach (var usage in SerializeReferenceTypeUsageIndex.AllUsages())
            {
                if (!Matches(token, usage.StoredType)) continue;

                var className = usage.StoredType.Class ?? string.Empty;

                var path = AssetDatabase.GUIDToAssetPath(usage.Guid);
                if (string.IsNullOrEmpty(path)) continue;

                var id = ItemId(usage);
                var missing = usage.Resolves ? string.Empty : "  (missing)";
                var label = $"{className}{missing}";
                var description = $"{path}  —  rid {usage.Rid}{(usage.IsOverride ? "  (prefab override)" : string.Empty)}";

                var item = provider.CreateItem(context, id, label, description, null, path);
                items.Add(item);
            }

            return null;
        }

        // Unity Search dedupes items by id, so it carries the Usage identity: two components of one instance can
        // override the same rid.
        public static string ItemId(SerializeReferenceTypeUsageIndex.Usage usage) => usage.IsOverride
            ? $"{usage.Guid}:{usage.FileId}:{usage.Rid}:{usage.TargetGuid}:{usage.TargetFileId}"
            : $"{usage.Guid}:{usage.FileId}:{usage.Rid}";

        private static Object LoadAsset(SearchItem item) =>
            item?.data is string path ? AssetDatabase.LoadAssetAtPath<Object>(path) : null;

        private static Texture2D AssetThumbnail(SearchItem item) =>
            item?.data is string path ? AssetDatabase.GetCachedIcon(path) as Texture2D : null;

        private static void Ping(SearchItem item)
        {
            var asset = LoadAsset(item);
            if (asset != null) EditorGUIUtility.PingObject(asset);
        }
    }
}
