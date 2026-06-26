using System;
using System.IO;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Caches the raw lines of an asset's YAML keyed by (path, last-write-time) so the per-property missing/stored-type
    /// probe — which runs on every IMGUI repaint via <see cref="SerializeReferenceYamlEditor.TryReadStoredType"/> /
    /// <see cref="SerializeReferenceYamlEditor.TryReadReferenceId"/> — does not hit the disk every frame. The
    /// modification-time component auto-invalidates an out-of-band edit; writers and repair sites additionally call
    /// <see cref="ClearCache"/> so a same-frame rewrite is never served stale. Mirrors the FIFO-capped cache pattern of
    /// <see cref="SerializeReferenceRepairSuggestions"/>.
    /// </summary>
    /// <remarks>
    /// The one-shot project sweep (<see cref="SerializeReferenceYamlEditor.FindMissingReferences"/>) deliberately bypasses
    /// this cache: it reads every candidate once behind a progress bar and would otherwise bloat the cache with large,
    /// never-reused scene files.
    /// </remarks>
    internal static class SerializeReferenceYamlProbeCache
    {
        private const int CacheCapacity = 64;

        private static readonly Dictionary<string, (DateTime writeTimeUtc, string[] lines)> Cache =
            new(StringComparer.Ordinal);

        private static readonly Queue<string> CacheOrder = new();

        /// <summary>
        /// Returns the asset's lines from the cache when the file's last-write-time is unchanged, otherwise reads them
        /// from disk and stores them. Returns an empty array for a missing/empty path. The returned array is shared —
        /// callers must treat it as read-only.
        /// </summary>
        public static string[] ReadAllLines(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return Array.Empty<string>();

            var writeTimeUtc = File.GetLastWriteTimeUtc(assetPath);
            if (Cache.TryGetValue(assetPath, out var cached) && cached.writeTimeUtc == writeTimeUtc)
                return cached.lines;

            var lines = File.ReadAllLines(assetPath);

            // A re-read at a newer write-time replaces the entry in place without re-enqueuing it; only a genuinely new
            // key grows the FIFO order, so the cap counts distinct assets, not reads.
            if (!Cache.ContainsKey(assetPath)) CacheOrder.Enqueue(assetPath);
            Cache[assetPath] = (writeTimeUtc, lines);

            while (CacheOrder.Count > CacheCapacity)
            {
                var evicted = CacheOrder.Dequeue();
                Cache.Remove(evicted);
            }

            return lines;
        }

        /// <summary>Drops every cached file — called after a rewrite and from the import post-processor.</summary>
        public static void ClearCache()
        {
            Cache.Clear();
            CacheOrder.Clear();
        }
    }
}
