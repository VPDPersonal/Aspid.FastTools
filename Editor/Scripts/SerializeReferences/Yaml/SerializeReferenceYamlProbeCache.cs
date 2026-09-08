using System;
using System.IO;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceYamlProbeCache
    {
        private const int CacheCapacity = 64;

        private static readonly Queue<string> _cacheOrder = new();
        private static readonly Dictionary<string, (DateTime writeTimeUtc, string[] lines)> _cache = new(StringComparer.Ordinal);

        // The returned array is shared, so callers must treat it as read-only. Empty for a missing path.
        public static string[] ReadAllLines(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return Array.Empty<string>();

            var writeTimeUtc = File.GetLastWriteTimeUtc(assetPath);
            if (_cache.TryGetValue(assetPath, out var cached) && cached.writeTimeUtc == writeTimeUtc)
                return cached.lines;

            var lines = File.ReadAllLines(assetPath);

            // A re-read at a newer write-time replaces the entry in place without re-enqueuing it; only a genuinely new
            // key grows the FIFO order, so the cap counts distinct assets, not reads.
            if (!_cache.ContainsKey(assetPath)) _cacheOrder.Enqueue(assetPath);
            _cache[assetPath] = (writeTimeUtc, lines);

            while (_cacheOrder.Count > CacheCapacity)
            {
                var evicted = _cacheOrder.Dequeue();
                _cache.Remove(evicted);
            }

            return lines;
        }

        public static void ClearCache()
        {
            _cache.Clear();
            _cacheOrder.Clear();
        }
    }
}
