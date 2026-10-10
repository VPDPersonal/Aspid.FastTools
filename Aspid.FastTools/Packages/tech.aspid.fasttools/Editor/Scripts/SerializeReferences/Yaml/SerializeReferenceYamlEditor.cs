using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static partial class SerializeReferenceYamlEditor
    {
        private const long NullRid = -2;

        private static Regex DocumentHeader => SerializeReferenceYaml.DocumentHeader;

        // Returns the [start, end) line range of the document whose anchor equals fileId. Falls back to the single
        // document of a one-object asset (the common ScriptableObject case) when the anchor cannot be matched. Any
        // "--- " line ends a document, so the range never spans a neighbour whose header failed to parse.
        private static (int start, int end) FindDocumentRange(string[] lines, long fileId)
        {
            var start = -1;
            var end = lines.Length;
            var headerCount = 0;
            var firstHeader = -1;

            for (var i = 0; i < lines.Length; i++)
            {
                if (!SerializeReferenceYaml.IsDocumentStart(lines[i])) continue;

                headerCount++;
                if (firstHeader < 0) firstHeader = i;

                if (start >= 0)
                {
                    end = i;
                    break;
                }

                var match = DocumentHeader.Match(lines[i]);
                if (match.Success && TryParseId(match.Groups["id"].Value, out var anchor) && anchor == fileId)
                    start = i;
            }

            if (start >= 0) return (start, end);
            return headerCount == 1 ? (firstHeader, lines.Length) : (-1, -1);
        }

        private static int FindRefIdsStart(string[] lines, int start, int end) =>
            SerializeReferenceYaml.FindRefIdsStart(lines, start, end);

        private static int FindRefIdsEntryIndent(string[] lines, int refIdsStart, int end) =>
            SerializeReferenceYaml.FindRefIdsEntryIndent(lines, refIdsStart, end);

        private static int FindEntryHeader(string[] lines, int refIdsStart, int end, long rid, out int entryIndent) =>
            SerializeReferenceYaml.FindEntryHeader(lines, refIdsStart, end, rid, out entryIndent);

        private static int FindEntryTypeLine(string[] lines, int headerIndex, int entryEnd) =>
            SerializeReferenceYaml.FindEntryTypeLine(lines, headerIndex, entryEnd);

        private static int FindEntryEnd(string[] lines, int headerIndex, int end, int entryIndent) =>
            SerializeReferenceYaml.FindEntryEnd(lines, headerIndex, end, entryIndent);

        private static int IndentOf(string line) =>
            SerializeReferenceYaml.IndentOf(line);

        private static bool TryParseInlineType(string body, out ManagedTypeName type) =>
            SerializeReferenceYaml.TryParseInlineType(body, out type);

        private static bool TryParseId(string text, out long id) =>
            SerializeReferenceYaml.TryParseId(text, out id);

        private static string FormatId(long id) =>
            SerializeReferenceYaml.FormatId(id);

        // Writes the edited lines back preserving the source's newline style, trailing-newline state and encoding (a
        // UTF-8 BOM stays). Unity writes its YAML with LF on every platform; File.WriteAllLines would re-emit
        // Environment.NewLine (CRLF on Windows) and churn the whole file for a one-line edit. `source` holds the lines
        // the edit was computed from, and removedRid and addedRid name the RefIds entries of document fileId that the
        // edit removes or adds (see IsVerifiedEdit). Returns false, with the reason logged, when the file is not valid
        // UTF-8, changed after `source` was read, cannot be made editable or fails the check; nothing is written then.
        internal static bool TryWritePreservingNewlines(string assetPath, string[] source, IReadOnlyList<string> lines,
            long fileId = 0, long? removedRid = null, long? addedRid = null)
        {
            if (!TryReadUtf8(assetPath, out var original, out var encoding)) return false;

            // Applied to a newer revision, the edit would also revert the changes of that revision.
            if (!HasLines(original, source))
            {
                Debug.LogError($"[Aspid FastTools] '{assetPath}' changed on disk while the fix was prepared; it was not changed. Retry the fix.");
                return false;
            }

            if (!IsVerifiedEdit(source, lines, fileId, removedRid, addedRid))
            {
                Debug.LogError($"[Aspid FastTools] The fix of '{assetPath}' failed its safety check and was not written; the asset was not changed.");
                return false;
            }

            if (!TryMakeEditable(assetPath)) return false;

            // A checkout may fetch a newer revision; the edit was computed from the old one, so it must not be applied.
            if (!TryReadUtf8(assetPath, out var checkedOut, out _)) return false;
            if (checkedOut != original)
            {
                Debug.LogError($"[Aspid FastTools] '{assetPath}' changed on disk while it was checked out; it was not changed. Retry the fix.");
                return false;
            }

            var newline = DominantNewline(original);

            var builder = new StringBuilder(original.Length);
            for (var i = 0; i < lines.Count; i++)
            {
                builder.Append(lines[i]);
                if (i < lines.Count - 1) builder.Append(newline);
            }

            if (original.Length > 0 && original[^1] == '\n') builder.Append(newline);

            WriteAtomically(assetPath, builder.ToString(), encoding);
            return true;
        }

        // Re-scans an edit before it is written. The result must still be Unity YAML with the same document headers,
        // and every document must keep its RefIds entries with their type lines, except that document fileId loses the
        // removedRid entry and gains the addedRid entry; a type rewrite removes and adds the same rid. An edit that
        // names an entry must also keep the line count of every other entry, and change the document's line count by
        // the sizes of the named entries only. So an edit that cuts into another entry, a type line or a document
        // header fails, and so does one that leaves a removed entry or a part of it behind.
        internal static bool IsVerifiedEdit(string[] source, IReadOnlyList<string> result, long fileId,
            long? removedRid = null, long? addedRid = null)
        {
            var edited = result as string[] ?? result.ToArray();
            if (!LooksLikeUnityYaml(edited)) return false;

            var before = ScanEntries(source);
            var after = ScanEntries(edited);
            if (before.Count != after.Count) return false;

            // A type name rewrite names no entry and may collapse a folded scalar inside one, so sizes may change.
            var keepsSizes = removedRid.HasValue || addedRid.HasValue;

            var target = -1;
            if (keepsSizes)
            {
                var (start, _) = FindDocumentRange(source, fileId);
                target = before.FindIndex(document => document.HeaderLine == start);
                if (target < 0) return false;
            }

            for (var i = 0; i < before.Count; i++)
            {
                if (!string.Equals(source[before[i].HeaderLine], edited[after[i].HeaderLine], StringComparison.Ordinal))
                    return false;

                var expected = new List<(long Rid, string Type, int Lines)>(before[i].Entries);
                var expectedLines = before[i].Lines;

                if (i == target && removedRid.HasValue)
                {
                    var removed = expected.FindIndex(entry => entry.Rid == removedRid.Value);
                    if (removed < 0) return false;

                    expectedLines -= expected[removed].Lines;
                    expected.RemoveAt(removed);
                }

                if (i == target && addedRid.HasValue)
                {
                    var added = after[i].Entries.FindIndex(entry => entry.Rid == addedRid.Value && entry.Type != null);
                    if (added < 0) return false;

                    expectedLines += after[i].Entries[added].Lines;
                    expected.Add(after[i].Entries[added]);
                }

                if (keepsSizes && expectedLines != after[i].Lines) return false;
                if (!HaveSameEntries(expected, after[i].Entries, keepsSizes)) return false;
            }

            return true;
        }

        // Every document by its header line and line count, with its RefIds entries as (rid, trimmed type line, line
        // count) triples; the type is null for an entry without a type line.
        private static List<(int HeaderLine, int Lines, List<(long Rid, string Type, int Lines)> Entries)> ScanEntries(
            string[] lines)
        {
            var documents = new List<(int HeaderLine, int Lines, List<(long Rid, string Type, int Lines)> Entries)>();

            for (var start = 0; start < lines.Length; start++)
            {
                if (!SerializeReferenceYaml.IsDocumentStart(lines[start])) continue;

                var end = SerializeReferenceYaml.FindDocumentEnd(lines, start + 1);
                var entries = new List<(long Rid, string Type, int Lines)>();

                var refIdsStart = FindRefIdsStart(lines, start, end);
                var entryIndent = refIdsStart < 0 ? -1 : FindRefIdsEntryIndent(lines, refIdsStart, end);

                for (var i = refIdsStart + 1; entryIndent >= 0 && i < end; i++)
                {
                    if (!SerializeReferenceYaml.TryMatchEntryHeader(lines[i], entryIndent, out var rid)) continue;

                    var entryEnd = FindEntryEnd(lines, i, end, entryIndent);
                    var typeLine = FindEntryTypeLine(lines, i, entryEnd);
                    entries.Add((rid, typeLine < 0 ? null : lines[typeLine].Trim(), entryEnd - i));
                }

                documents.Add((start, end - start, entries));
                start = end - 1;
            }

            return documents;
        }

        // Compares as multisets: an edit may insert an entry anywhere in the list.
        private static bool HaveSameEntries(List<(long Rid, string Type, int Lines)> expected,
            List<(long Rid, string Type, int Lines)> actual, bool compareLines)
        {
            if (expected.Count != actual.Count) return false;

            var left = expected.OrderBy(entry => entry.Rid).ThenBy(entry => entry.Type, StringComparer.Ordinal).ToArray();
            var right = actual.OrderBy(entry => entry.Rid).ThenBy(entry => entry.Type, StringComparer.Ordinal).ToArray();

            for (var i = 0; i < left.Length; i++)
            {
                if (left[i].Rid != right[i].Rid || !string.Equals(left[i].Type, right[i].Type, StringComparison.Ordinal))
                    return false;

                if (compareLines && left[i].Lines != right[i].Lines) return false;
            }

            return true;
        }

        // Whether the text splits into exactly these lines, by the rules of File.ReadAllLines.
        private static bool HasLines(string text, string[] lines)
        {
            using var reader = new StringReader(text);

            var count = 0;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (count >= lines.Length || !string.Equals(line, lines[count], StringComparison.Ordinal)) return false;
                count++;
            }

            return count == lines.Length;
        }

        // Checks the asset out through the version control provider, as Unity's own saves do, and logs why when it
        // stays unwritable. Without a provider MakeEditable returns true even for a read-only file, so the read-only
        // flag is checked on its own.
        public static bool TryMakeEditable(string assetPath)
        {
            if (!AssetDatabase.MakeEditable(assetPath))
            {
                Debug.LogError($"[Aspid FastTools] '{assetPath}' could not be checked out in version control; it was not changed.");
                return false;
            }

            if ((File.GetAttributes(assetPath) & FileAttributes.ReadOnly) != 0)
            {
                Debug.LogError($"[Aspid FastTools] '{assetPath}' is read-only; check it out or make it writable, then retry. It was not changed.");
                return false;
            }

            return true;
        }

        // Reads the asset as strict UTF-8, with the encoding to write it back in: the byte-order mark stays. A byte
        // that is not valid UTF-8 fails the read with the reason logged; decoded to U+FFFD, it would be lost on write.
        private static bool TryReadUtf8(string path, out string text, out Encoding encoding)
        {
            var bytes = File.ReadAllBytes(path);
            var hasByteOrderMark = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            var offset = hasByteOrderMark ? 3 : 0;

            encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: hasByteOrderMark, throwOnInvalidBytes: true);

            try
            {
                text = encoding.GetString(bytes, offset, bytes.Length - offset);
                return true;
            }
            catch (DecoderFallbackException)
            {
                text = null;
                Debug.LogError($"[Aspid FastTools] '{path}' is not valid UTF-8; it was not changed.");
                return false;
            }
        }

        // File.WriteAllText truncates the asset first, so a failed write (full disk, killed process) would leave it cut
        // short. The text goes to a sibling temp file that replaces the asset in one step. The leading dot and the
        // .tmp extension keep Unity from importing the temp file.
        private static void WriteAtomically(string path, string text, Encoding encoding)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(path)) ?? string.Empty;
            var tempPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");

            try
            {
                File.WriteAllText(tempPath, text, encoding);
                File.Replace(tempPath, path, destinationBackupFileName: null);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        // The newline style that dominates the source by line count — a majority pick keeps a one-line edit on a
        // mixed-ending file from flipping every other line's terminator. LF (Unity's invariant terminator) wins ties;
        // a lone CR maps to LF since this writer only emits "\r\n" or "\n".
        private static string DominantNewline(string text)
        {
            var crlf = 0;
            var loneLf = 0;

            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] != '\n') continue;
                if (i > 0 && text[i - 1] == '\r') crlf++;
                else loneLf++;
            }

            return crlf > loneLf ? "\r\n" : "\n";
        }

        private static bool BlockIndentIsTrusted(string[] lines, int start, int end)
        {
            for (var i = Math.Max(start, 0); i < end && i < lines.Length; i++)
            {
                if (!IndentIsSpaceOnly(lines[i])) return false;
            }

            return true;
        }

        // A line whose leading indentation is spaces only — Unity's invariant for serialized YAML. Tab / mixed
        // indentation is where IndentOf and the "- rid:" \s* regexes can measure nesting differently, so callers
        // about to delete a bounded block bail rather than risk a mis-bounded, non-undoable write.
        private static bool IndentIsSpaceOnly(string line)
        {
            // A blank / whitespace-only line carries no indentation to measure — FindEntryEnd spans blank lines inside
            // an entry, so a stray tab in such a line must not abort an otherwise valid (space-indented) block removal.
            if (string.IsNullOrWhiteSpace(line)) return true;

            foreach (var character in line)
            {
                if (character == ' ') continue;
                return !char.IsWhiteSpace(character);
            }

            return true;
        }

        // A Unity-serialized YAML asset carries the "%TAG !u!" directive before its first document. Guards the
        // destructive writes so a hand-authored or foreign YAML file can never be surgically rewritten.
        private static bool LooksLikeUnityYaml(string[] lines)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                var line = (i == 0 ? StripByteOrderMark(lines[i]) : lines[i]).TrimStart();
                if (line.Length == 0) continue;
                if (line.StartsWith("%TAG !u!", StringComparison.Ordinal)) return true;
                if (line.StartsWith("---", StringComparison.Ordinal)) return false;
            }

            return false;
        }

        // File.ReadAllLines strips a UTF-8 BOM in practice, but guard the first line defensively so the %TAG sniff is
        // never thrown off by a leading byte-order mark.
        private static string StripByteOrderMark(string line) =>
            line.Length > 0 && line[0] == '\uFEFF' ? line[1..] : line;
    }
}
