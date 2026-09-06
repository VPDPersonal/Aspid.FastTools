using System;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Lazy, incrementally-updated project-wide index mapping each stored-type identity to the assets, documents and
    // rids using it: a cold null sentinel rebuilt on first lookup, patched per asset on import and fully reset on
    // delete or move. Powers Find Usages, MonoScript delete protection and the Repair window's fast project scan.
    // Each usage carries its own Resolves flag and stored type, so consumers never re-read the file.
    internal static class SerializeReferenceTypeUsageIndex
    {
        // Identity is (asset, document, rid); the rest is payload.
        public readonly struct Usage : IEquatable<Usage>
        {
            public readonly string Guid;
            public readonly long FileId;
            public readonly long Rid;
            public readonly bool Resolves;
            public readonly ManagedTypeName StoredType;

            public Usage(string guid, long fileId, long rid, bool resolves, ManagedTypeName storedType)
            {
                Guid = guid ?? string.Empty;
                FileId = fileId;
                Rid = rid;
                Resolves = resolves;
                StoredType = storedType;
            }

            public bool Equals(Usage other) =>
                string.Equals(Guid, other.Guid, StringComparison.Ordinal) && FileId == other.FileId && Rid == other.Rid;

            public override bool Equals(object obj) => obj is Usage other && Equals(other);

            public override int GetHashCode() => unchecked((Guid.GetHashCode() * 397 ^ FileId.GetHashCode()) * 397 ^ Rid.GetHashCode());
        }

        // Null is the cold sentinel, rebuilt lazily on first lookup.
        private static Dictionary<string, HashSet<Usage>> _index;

        // Consumers on the import or domain-reload path must check this and NOT warm a cold index: warming runs a
        // modal full-project YAML sweep, which a routine import must never trigger.
        public static bool IsWarm => _index is not null;

        // Warms the index if cold.
        public static IReadOnlyCollection<Usage> FindUsages(string storedTypeKey)
        {
            if (string.IsNullOrEmpty(storedTypeKey)) return Array.Empty<Usage>();
            EnsureBuilt();
            return _index.TryGetValue(storedTypeKey, out var set) ? set : Array.Empty<Usage>();
        }

        // Keyed on the open-generic identity, since a generic type's script resolves to the open definition while
        // YAML stores each closed instantiation under its own key. A non-generic type has just the one key.
        public static IReadOnlyCollection<Usage> FindUsages(Type type) =>
            type is null ? Array.Empty<Usage>() : FindUsagesByOpenKey(SerializeReferenceHelpers.OpenTypeKey(ManagedTypeName.FromType(type)));

        // Aggregates across the closed-form keys a generic type splits into; warms the index if cold.
        public static IReadOnlyCollection<Usage> FindUsagesByOpenKey(string openTypeKey)
        {
            if (string.IsNullOrEmpty(openTypeKey)) return Array.Empty<Usage>();
            EnsureBuilt();

            HashSet<Usage> result = null;
            foreach (var (key, set) in _index)
            {
                if (!string.Equals(SerializeReferenceHelpers.OpenTypeKey(key), openTypeKey, StringComparison.Ordinal))
                    continue;

                (result ??= new HashSet<Usage>()).UnionWith(set);
            }

            return (IReadOnlyCollection<Usage>)result ?? Array.Empty<Usage>();
        }

        public static int CountUsages(Type type) => FindUsages(type).Count;

        // The fast-scan source for the Repair window.
        public static IEnumerable<Usage> EnumerateUnresolved()
        {
            EnsureBuilt();
            foreach (var set in _index.Values)
                foreach (var usage in set)
                    if (!usage.Resolves)
                        yield return usage;
        }

        // The source the Find Usages search provider filters; warms the index.
        public static IEnumerable<Usage> AllUsages()
        {
            EnsureBuilt();
            foreach (var set in _index.Values)
                foreach (var usage in set)
                    yield return usage;
        }

        // Drops the whole index; the next lookup rebuilds it.
        public static void Reset() => _index = null;

        public static void ClearCache() => Reset();

        // Re-extracts one asset's usages in place; a no-op while the index is cold.
        public static void RebuildAsset(string path)
        {
            if (_index is null) return;

            var guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid)) return;

            RemoveGuid(guid);
            AddAsset(path, guid);
        }

        private static void EnsureBuilt()
        {
            if (_index is not null) return;

            _index = new Dictionary<string, HashSet<Usage>>(StringComparer.Ordinal);

            var paths = AssetDatabase.GetAllAssetPaths()
                .Where(SerializeReferenceHelpers.IsScanCandidate)
                .ToArray();

            // Non-cancelable: the sentinel is already replaced, so cancelling would mark a partial index warm.
            try
            {
                for (var i = 0; i < paths.Length; i++)
                {
                    EditorUtility.DisplayProgressBar(
                        "Indexing Managed References",
                        $"{paths[i]}  ({i + 1}/{paths.Length})",
                        (float)i / Math.Max(1, paths.Length));

                    var guid = AssetDatabase.AssetPathToGUID(paths[i]);
                    if (string.IsNullOrEmpty(guid)) continue;

                    AddAsset(paths[i], guid);
                }
            }
            catch (Exception)
            {
                // A failed warm-up must not masquerade as warm.
                _index = null;
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void AddAsset(string path, string guid)
        {
            // Data-only: resolving display names would load every asset.
            foreach (var document in SerializeReferenceGraphScanner.Build(path, resolveTypeNames: false))
            {
                foreach (var node in document.Nodes)
                {
                    // With no recorded type identity there is nothing to look up and nothing missing.
                    if (node.StoredType.IsEmpty) continue;

                    var key = SerializeReferenceHelpers.StoredTypeKey(node.StoredType);
                    AddUsage(key, new Usage(guid, document.FileId, node.Rid, node.Resolves, node.StoredType));
                }
            }
        }

        private static void AddUsage(string key, Usage usage)
        {
            if (!_index.TryGetValue(key, out var set))
            {
                set = new HashSet<Usage>();
                _index[key] = set;
            }

            // Removed first so a changed payload replaces the stale entry rather than joining it.
            set.Remove(usage);
            set.Add(usage);
        }

        private static void RemoveGuid(string guid)
        {
            List<string> emptied = null;
            foreach (var (key, set) in _index)
            {
                if (set.RemoveWhere(u => string.Equals(u.Guid, guid, StringComparison.Ordinal)) > 0 && set.Count == 0)
                    (emptied ??= new List<string>()).Add(key);
            }

            if (emptied is null) return;
            foreach (var key in emptied) _index.Remove(key);
        }
    }
}
