using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static partial class SerializeReferenceYamlEditor
    {
        // The inline type mapping Unity writes for the null sentinel RefIds entry — an empty type identity.
        private const string NullSentinelType = "type: {class: , ns: , asm: }";

        // Replaces the entry's type mapping. The caller reimports the asset.
        public static bool TryRewriteType(string assetPath, long fileId, long rid, ManagedTypeName newType) =>
            RewriteTypes(assetPath, new[] { new MissingReferenceEntry(fileId, rid, storedType: default) }, newType) == 1;

        // Replaces the type mapping of every entry with one read and one write of the file. A stale entry is skipped.
        // Returns how many entries were rewritten; the caller reimports the asset.
        public static int RewriteTypes(string assetPath, IReadOnlyList<MissingReferenceEntry> entries, ManagedTypeName newType)
        {
            if (entries is null || entries.Count == 0) return 0;

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return 0;

                var lines = ReadAllLines(assetPath, out var original, out var encoding);
                if (!LooksLikeUnityYaml(lines)) return 0;

                var refIdsBlocks = new Dictionary<long, (int RefIdsStart, int End)>();
                var applied = 0;

                foreach (var entry in entries)
                {
                    // Single scan shared with the diff preview: compute the edit, then apply exactly that line so the
                    // preview and the applied result can never diverge.
                    if (!TryComputeRewrite(lines, assetPath, entry.FileId, entry.Rid, newType, refIdsBlocks, out var edit)) continue;

                    lines[edit.LineNumber] = edit.NewLine;
                    applied++;
                }

                if (applied == 0 || !TryWritePreservingNewlines(assetPath, lines, original, encoding)) return 0;

                // Same-tick writes can leave the modification-time key unchanged, so bust the probe cache explicitly.
                SerializeReferenceYamlProbeCache.ClearCache();
                return applied;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[TypeSelector] Failed to rewrite managed-reference types in '{assetPath}': {exception}");
                return 0;
            }
        }

        // Computes, without writing, the line change TryRewriteType would make. The rewrite applies the returned
        // edit verbatim, so the bulk-fix preview shows exactly what will be written.
        public static bool TryComputeRewrite(string assetPath, long fileId, long rid, ManagedTypeName newType, out RewriteEdit edit)
        {
            edit = ComputeRewrites(assetPath, new[] { new MissingReferenceEntry(fileId, rid, storedType: default) }, newType)[0];
            return edit.IsValid;
        }

        // Computes the edit of every entry with one read of the file. The result has one slot per entry, in the same
        // order; a slot whose IsValid is false is an entry that could not be computed.
        public static RewriteEdit[] ComputeRewrites(string assetPath, IReadOnlyList<MissingReferenceEntry> entries, ManagedTypeName newType)
        {
            var edits = new RewriteEdit[entries?.Count ?? 0];
            if (edits.Length == 0) return edits;

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return edits;

                var lines = File.ReadAllLines(assetPath);
                if (!LooksLikeUnityYaml(lines)) return edits;

                var refIdsBlocks = new Dictionary<long, (int RefIdsStart, int End)>();
                for (var i = 0; i < edits.Length; i++)
                    TryComputeRewrite(lines, assetPath, entries[i].FileId, entries[i].Rid, newType, refIdsBlocks, out edits[i]);

                return edits;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[TypeSelector] Failed to compute managed-reference rewrite in '{assetPath}': {exception}");
                return new RewriteEdit[edits.Length];
            }
        }

        // refIdsBlocks caches the RefIds block of each document for one batch. A type-line edit never adds, removes or
        // moves a line, so the cached ranges stay valid while the batch rewrites lines.
        private static bool TryComputeRewrite(string[] lines, string assetPath, long fileId, long rid, ManagedTypeName newType,
            Dictionary<long, (int RefIdsStart, int End)> refIdsBlocks, out RewriteEdit edit)
        {
            edit = default;

            if (!refIdsBlocks.TryGetValue(fileId, out var block))
            {
                var (start, end) = FindDocumentRange(lines, fileId);

                // Field pointers ("_sidearms:\n  - rid: 1002") share the "- rid:" shape with RefIds entries, so confine
                // the search to the RefIds block — the entries are the only ones with a following type:.
                block = (start < 0 ? -1 : FindRefIdsStart(lines, start, end), end);
                refIdsBlocks.Add(fileId, block);
            }

            if (block.RefIdsStart < 0) return false;

            // Only the entry header counts: a nested "- rid: N" list element in an earlier entry's data block would
            // aim the rewrite at the type line of whichever entry follows it.
            var headerIndex = FindEntryHeader(lines, block.RefIdsStart, block.End, rid, out var entryIndent);
            if (headerIndex < 0) return false;

            var typeLine = FindEntryTypeLine(lines, headerIndex, FindEntryEnd(lines, headerIndex, block.End, entryIndent));
            if (typeLine < 0) return false;

            var match = new Regex(@"^(?<indent>\s*type:\s*)\{.*\}\s*$").Match(lines[typeLine]);
            if (!match.Success) return false;

            edit = new RewriteEdit(assetPath, typeLine, lines[typeLine], match.Groups["indent"].Value + newType.ToYamlType());
            return true;
        }

        // Deletes a whole RefIds entry, dropping an orphaned payload no field points at. Confined to the RefIds
        // block, so a same-shaped field pointer is never touched. Not undoable, so callers confirm first.
        public static bool TryRemoveEntry(string assetPath, long fileId, long rid)
        {
            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return false;

                var lines = File.ReadAllLines(assetPath);
                if (!LooksLikeUnityYaml(lines)) return false;
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                var refIdsStart = FindRefIdsStart(lines, start, end);
                if (refIdsStart < 0) return false;

                // A nested "- rid: N" list element in another entry's data block is a pointer, not this entry.
                var headerIndex = FindEntryHeader(lines, refIdsStart, end, rid, out var entryIndent);
                if (headerIndex < 0) return false;

                // The entry runs until the next list item at its own indent, or until the block dedents out of it —
                // the same bounding rule the data-block reader uses.
                var entryEnd = FindEntryEnd(lines, headerIndex, end, entryIndent);

                // Unexpected (tab / mixed) indentation in the entry block means IndentOf and the "- rid:" \s* regex
                // can disagree on where the block ends — bail rather than write a possibly mis-bounded deletion.
                if (!BlockIndentIsTrusted(lines, headerIndex, entryEnd)) return false;

                var remaining = new List<string>(lines.Length - (entryEnd - headerIndex));
                for (var k = 0; k < headerIndex; k++) remaining.Add(lines[k]);
                for (var k = entryEnd; k < lines.Length; k++) remaining.Add(lines[k]);

                if (!TryWritePreservingNewlines(assetPath, remaining)) return false;
                SerializeReferenceYamlProbeCache.ClearCache();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Aspid FastTools] Failed to remove RefIds entry rid {rid} in '{assetPath}': {exception}");
                return false;
            }
        }

        // Nulls a managed reference: every pointer holding the rid is rewritten to the null id, the orphaned entry
        // is removed, and the null sentinel entry is added if a null pointer was introduced and it is absent. That
        // reproduces exactly what Unity writes for a cleared field — an array element cannot be dropped, so it must
        // point at -2, and that pointer is only valid while the sentinel entry exists, or the load errors with
        // "serialized array … is missing entry for Refid -2". Not undoable: the broken payload is discarded.
        public static bool TryNullReference(string assetPath, long fileId, long rid) =>
            NullReferences(assetPath, new[] { new MissingReferenceEntry(fileId, rid, storedType: default) }) == 1;

        // Nulls every entry as TryNullReference does, one after another, with one read and one write of the file. A
        // stale entry is skipped. Returns how many entries were nulled; the caller reimports the asset.
        public static int NullReferences(string assetPath, IReadOnlyList<MissingReferenceEntry> entries)
        {
            if (entries is null || entries.Count == 0) return 0;

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return 0;

                var lines = ReadAllLines(assetPath, out var original, out var encoding);
                if (!LooksLikeUnityYaml(lines)) return 0;

                var applied = 0;
                foreach (var entry in entries)
                {
                    if (!TryNullReference(lines, entry.FileId, entry.Rid, out var nulled)) continue;

                    lines = nulled;
                    applied++;
                }

                if (applied == 0 || !TryWritePreservingNewlines(assetPath, lines, original, encoding)) return 0;

                SerializeReferenceYamlProbeCache.ClearCache();
                return applied;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Aspid FastTools] Failed to null managed references in '{assetPath}': {exception}");
                return 0;
            }
        }

        // Works on a copy, so source stays unchanged when the entry bails after its pointers were nulled.
        private static bool TryNullReference(string[] source, long fileId, long rid, out string[] result)
        {
            result = null;

            var lines = (string[])source.Clone();
            var (start, end) = FindDocumentRange(lines, fileId);
            if (start < 0) return false;

            var refIdsStart = FindRefIdsStart(lines, start, end);
            if (refIdsStart < 0) return false;

            // RefIds entry headers sit at the shallowest "- rid:" indent under RefIds; a pointer to the rid lives
            // anywhere else — a field/array element before "references:" or a nested reference inside another entry's
            // data block. The header for this rid is removed; every pointer to it becomes the null id.
            var entryIndent = FindRefIdsEntryIndent(lines, refIdsStart, end);
            if (entryIndent < 0) return false;

            var pointerToken = BuildPointerPattern(rid);

            var headerIndex = -1;
            var pointerNulled = false;

            for (var i = start; i < end; i++)
            {
                // The entry header and every pointer shape below contain "rid:", so other lines skip both regexes.
                if (lines[i].IndexOf("rid:", StringComparison.Ordinal) < 0) continue;

                // This rid's own RefIds entry header (a "- rid: N" under RefIds at the entry indent) is removed
                // below, not nulled — skip it so it isn't rewritten to the null id.
                if (headerIndex < 0 && i > refIdsStart
                    && SerializeReferenceYaml.TryMatchEntryHeader(lines[i], entryIndent, out var headerRid)
                    && headerRid == rid)
                {
                    headerIndex = i;
                    continue;
                }

                // Null every pointer to the rid — a "- rid: N" array element, a "rid: N" scalar field or an inline
                // "{rid: N}" — so no dangling pointer survives the entry's removal (which errors on array fields).
                // The anchored pattern preserves each pointer's structural prefix/suffix and only rewrites the id.
                if (pointerToken.IsMatch(lines[i]))
                {
                    lines[i] = pointerToken.Replace(lines[i], $"${{prefix}}rid: {NullRid}${{suffix}}");
                    pointerNulled = true;
                }
            }

            if (headerIndex < 0 && !pointerNulled) return false;

            var blockStart = headerIndex;
            var blockEnd = headerIndex >= 0 ? FindEntryEnd(lines, headerIndex, end, entryIndent) : -1;

            // The entry block we're about to drop must use Unity's space-only indentation; a tab / mixed prefix can
            // mis-bound it (IndentOf vs the "- rid:" \s* regex), so bail before this non-undoable rewrite. (Pointer
            // nulling above is line-local and indent-agnostic, so no write has reached disk yet.)
            if (headerIndex >= 0 && !BlockIndentIsTrusted(lines, blockStart, blockEnd)) return false;

            // A "- rid: -2" pointer is valid only while the RefIds list carries Unity's null sentinel entry; add it
            // when we just introduced a null pointer and the document does not already have one (a shared singleton).
            var needsNullEntry = pointerNulled && !HasNullSentinelEntry(lines, refIdsStart, end, entryIndent);
            var dash = new string(' ', entryIndent);
            var typeIndent = new string(' ', entryIndent + 2);

            var nulled = new List<string>(lines.Length + 2);
            for (var i = 0; i < lines.Length; i++)
            {
                if (headerIndex >= 0 && i >= blockStart && i < blockEnd) continue;

                nulled.Add(lines[i]);

                if (needsNullEntry && i == refIdsStart)
                {
                    nulled.Add($"{dash}- rid: {NullRid}");
                    nulled.Add($"{typeIndent}{NullSentinelType}");
                }
            }

            result = nulled.ToArray();
            return true;
        }

        // How many slots TryNullReference would null. A missing reference can be aliased across several, so the
        // confirm dialog names the count before the irreversible rewrite. The entry's own header is excluded, since
        // it is the entry rather than a pointer to it. 0 means the count is unknown, not that there are none.
        public static int CountPointersTo(string assetPath, long fileId, long rid)
        {
            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return 0;

                var lines = File.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return 0;

                var refIdsStart = FindRefIdsStart(lines, start, end);
                if (refIdsStart < 0) return 0;

                var entryIndent = FindRefIdsEntryIndent(lines, refIdsStart, end);
                if (entryIndent < 0) return 0;

                var pointerToken = BuildPointerPattern(rid);

                var headerSkipped = false;
                var count = 0;

                for (var i = start; i < end; i++)
                {
                    // Skip this rid's own RefIds entry header exactly once — it is the entry, not a pointer. Mirrors
                    // the header skip in TryNullReference so the count equals the pointers that path would rewrite.
                    if (!headerSkipped && i > refIdsStart
                        && SerializeReferenceYaml.TryMatchEntryHeader(lines[i], entryIndent, out var headerRid)
                        && headerRid == rid)
                    {
                        headerSkipped = true;
                        continue;
                    }

                    if (pointerToken.IsMatch(lines[i])) count++;
                }

                return count;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Aspid FastTools] Failed to count managed-reference pointers to rid {rid} in '{assetPath}': {exception}");
                return 0;
            }
        }

        // Anchored matcher for a real "rid: N" pointer — never a bare "rid: N" substring inside a string field value.
        // Only Unity's three pointer shapes match: a line-anchored "- rid: N" item, a line-anchored "rid: N" scalar,
        // or an inline "{rid: N}" mapping; the structural prefix/suffix are captured so a rewrite replaces only the id.
        private static Regex BuildPointerPattern(long rid) => new(
            $@"(?<prefix>^\s*(?:-\s+)?)rid:\s*{rid}(?<suffix>\s*$)|(?<prefix>\{{\s*)rid:\s*{rid}(?<suffix>\s*\}})");

        private static bool HasNullSentinelEntry(string[] lines, int refIdsStart, int end, int entryIndent)
        {
            for (var i = refIdsStart + 1; i < end; i++)
            {
                if (SerializeReferenceYaml.TryMatchEntryHeader(lines[i], entryIndent, out var headerRid)
                    && headerRid == NullRid) return true;
            }

            return false;
        }
    }
}
