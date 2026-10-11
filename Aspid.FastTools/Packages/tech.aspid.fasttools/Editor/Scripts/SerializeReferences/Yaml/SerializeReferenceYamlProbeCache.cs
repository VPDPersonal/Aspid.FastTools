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
        private static readonly Dictionary<string, ProbedFile> _cache = new(StringComparer.Ordinal);

        // The returned array is shared, so callers must treat it as read-only. Empty for a missing path.
        public static string[] ReadAllLines(string assetPath) =>
            Read(assetPath)?.Lines ?? Array.Empty<string>();

        // The cached version of the file, or null for a missing path. Its document index and the reads made from it live
        // as long as the version does, so a repaint does not parse an unchanged file again.
        public static ProbedFile Read(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return null;

            var writeTimeUtc = File.GetLastWriteTimeUtc(assetPath);
            if (_cache.TryGetValue(assetPath, out var cached) && cached.WriteTimeUtc == writeTimeUtc)
                return cached;

            var file = new ProbedFile(writeTimeUtc, File.ReadAllLines(assetPath));

            // A re-read at a newer write-time replaces the entry in place without re-enqueuing it; only a genuinely new
            // key grows the FIFO order, so the cap counts distinct assets, not reads.
            if (!_cache.ContainsKey(assetPath)) _cacheOrder.Enqueue(assetPath);
            _cache[assetPath] = file;

            while (_cacheOrder.Count > CacheCapacity)
            {
                var evicted = _cacheOrder.Dequeue();
                _cache.Remove(evicted);
            }

            return file;
        }

        public static void ClearCache()
        {
            _cache.Clear();
            _cacheOrder.Clear();
        }

        // One version of a probed file: its lines, an index of its documents and the reads SerializeReferenceYamlEditor
        // made from it.
        internal sealed class ProbedFile
        {
            public readonly string[] Lines;
            public readonly DateTime WriteTimeUtc;

            // Keyed by the object's fileId, the rid the path starts in (or none) and the path.
            public readonly Dictionary<(long fileId, long anchorRid, string path), (bool found, long rid, ManagedTypeName type)>
                StoredTypes = new();

            public readonly Dictionary<(long fileId, long anchorRid, string path), long[]> ListIds = new();

            // The stored type of every RefIds entry of a document, keyed by the document's fileId.
            public readonly Dictionary<long, Dictionary<long, ManagedTypeName>> EntryTypes = new();

            private int _headerCount;
            private int _firstHeader = -1;
            private Dictionary<long, (int start, int end)> _documents;

            public ProbedFile(DateTime writeTimeUtc, string[] lines)
            {
                Lines = lines;
                WriteTimeUtc = writeTimeUtc;
            }

            // The [start, end) line range of the document anchored at fileId, as SerializeReferenceYamlEditor finds it: the
            // first document with that anchor, or the only document of a one-object asset. (-1, -1) when there is none.
            public (int start, int end) FindDocumentRange(long fileId)
            {
                _documents ??= IndexDocuments();

                if (_documents.TryGetValue(fileId, out var range)) return range;
                return _headerCount == 1 ? (_firstHeader, Lines.Length) : (-1, -1);
            }

            private Dictionary<long, (int start, int end)> IndexDocuments()
            {
                var documents = new Dictionary<long, (int start, int end)>();

                var current = -1;
                var currentId = 0L;

                for (var i = 0; i <= Lines.Length; i++)
                {
                    if (i < Lines.Length && !SerializeReferenceYaml.IsDocumentStart(Lines[i])) continue;

                    // Any "--- " line ends a document, so a range never spans a neighbour whose header failed to parse.
                    if (current >= 0 && !documents.ContainsKey(currentId)) documents[currentId] = (current, i);
                    if (i == Lines.Length) break;

                    _headerCount++;
                    if (_firstHeader < 0) _firstHeader = i;

                    var match = SerializeReferenceYaml.DocumentHeader.Match(Lines[i]);
                    current = match.Success && long.TryParse(match.Groups["id"].Value, out currentId) ? i : -1;
                }

                return documents;
            }
        }
    }
}
