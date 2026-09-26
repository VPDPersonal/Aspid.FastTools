using System;
using System.IO;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceTypeUsageIndex
    {
        // Identity is (asset, document, rid, override target); the rest is payload.
        public readonly struct Usage : IEquatable<Usage>
        {
            public readonly string Guid;
            public readonly long FileId;
            public readonly long Rid;
            public readonly bool Resolves;
            public readonly ManagedTypeName StoredType;

            // Set by a prefab instance override: FileId is the PrefabInstance document, which the YAML repair cannot
            // edit.
            public readonly bool IsOverride;

            // The overridden object of an override, zero and empty otherwise. Two components of one instance can
            // override the same rid, so it is part of the identity.
            public readonly long TargetFileId;
            public readonly string TargetGuid;

            public Usage(string guid, long fileId, long rid, bool resolves, ManagedTypeName storedType, bool isOverride = false,
                long targetFileId = 0, string targetGuid = null)
            {
                Guid = guid ?? string.Empty;
                FileId = fileId;
                Rid = rid;
                Resolves = resolves;
                StoredType = storedType;
                IsOverride = isOverride;
                TargetFileId = targetFileId;
                TargetGuid = targetGuid ?? string.Empty;
            }

            public bool Equals(Usage other) =>
                string.Equals(Guid, other.Guid, StringComparison.Ordinal) && FileId == other.FileId && Rid == other.Rid &&
                TargetFileId == other.TargetFileId && string.Equals(TargetGuid, other.TargetGuid, StringComparison.Ordinal);

            public override bool Equals(object obj) => obj is Usage other && Equals(other);

            public override int GetHashCode() => unchecked(
                (((Guid.GetHashCode() * 397 ^ FileId.GetHashCode()) * 397 ^ Rid.GetHashCode()) * 397 ^
                    TargetFileId.GetHashCode()) * 397 ^ TargetGuid.GetHashCode());
        }

        private static Dictionary<string, HashSet<Usage>> _index;

        // Consumers on the import or domain-reload path must check this and NOT warm a cold index: warming runs a
        // modal full-project YAML sweep, which a routine import must never trigger.
        public static bool IsWarm => _index is not null;

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

        public static IEnumerable<Usage> EnumerateUnresolved()
        {
            EnsureBuilt();
            foreach (var set in _index.Values)
                foreach (var usage in set)
                    if (!usage.Resolves)
                        yield return usage;
        }

        public static IEnumerable<Usage> AllUsages()
        {
            EnsureBuilt();
            foreach (var set in _index.Values)
                foreach (var usage in set)
                    yield return usage;
        }

        public static void Reset() => _index = null;

        public static void ClearCache() => Reset();

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
            foreach (var usage in CollectUsages(path, guid))
                AddUsage(SerializeReferenceHelpers.StoredTypeKey(usage.StoredType), usage);
        }

        // RefIds entries plus the types set by prefab instance overrides, which live outside any RefIds block.
        public static IEnumerable<Usage> CollectUsages(string path, string guid)
        {
            // One read feeds both passes, so a project sweep reads each file once.
            var lines = ReadLines(path);
            if (lines is null) yield break;

            // Data-only: resolving display names would load every asset.
            foreach (var document in SerializeReferenceGraphScanner.Build(lines))
            {
                foreach (var node in document.Nodes)
                {
                    if (node.StoredType.IsEmpty) continue;
                    yield return new Usage(guid, document.FileId, node.Rid, node.Resolves, node.StoredType);
                }
            }

            foreach (var entry in SerializeReferenceYamlEditor.FindPrefabOverrideReferences(lines))
            {
                yield return new Usage(guid, entry.FileId, entry.Rid,
                    SerializeReferenceHelpers.StoredTypeResolves(entry.StoredType), entry.StoredType, isOverride: true,
                    entry.TargetFileId, entry.TargetGuid);
            }
        }

        private static string[] ReadLines(string path)
        {
            try
            {
                return string.IsNullOrEmpty(path) || !File.Exists(path) ? null : File.ReadAllLines(path);
            }
            catch (Exception)
            {
                // Best effort, like the scanners: an unreadable file contributes no usages.
                return null;
            }
        }

        private static void AddUsage(string key, Usage usage)
        {
            if (!_index.TryGetValue(key, out var set))
            {
                set = new HashSet<Usage>();
                _index[key] = set;
            }

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
