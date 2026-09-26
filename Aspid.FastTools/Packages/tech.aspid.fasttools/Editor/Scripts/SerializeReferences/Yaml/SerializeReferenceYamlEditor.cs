using System;
using System.IO;
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
        // document of a one-object asset (the common ScriptableObject case) when the anchor cannot be matched.
        private static (int start, int end) FindDocumentRange(string[] lines, long fileId)
        {
            var start = -1;
            var end = lines.Length;
            var headerCount = 0;
            var firstHeader = -1;

            for (var i = 0; i < lines.Length; i++)
            {
                var match = DocumentHeader.Match(lines[i]);
                if (!match.Success) continue;

                headerCount++;
                if (firstHeader < 0) firstHeader = i;

                if (start >= 0)
                {
                    end = i;
                    break;
                }

                if (long.TryParse(match.Groups["id"].Value, out var anchor) && anchor == fileId)
                    start = i;
            }

            if (start >= 0) return (start, end);
            return headerCount == 1 ? (firstHeader, lines.Length) : (-1, -1);
        }

        private static int FindRefIdsStart(string[] lines, int start, int end) =>
            SerializeReferenceYaml.FindRefIdsStart(lines, start, end);

        private static int FindEntryEnd(string[] lines, int headerIndex, int end, int entryIndent) =>
            SerializeReferenceYaml.FindEntryEnd(lines, headerIndex, end, entryIndent);

        private static int IndentOf(string line) =>
            SerializeReferenceYaml.IndentOf(line);

        private static bool TryParseInlineType(string body, out ManagedTypeName type) =>
            SerializeReferenceYaml.TryParseInlineType(body, out type);

        // Writes lines back preserving the source's newline style, trailing-newline state and encoding (a UTF-8 BOM
        // stays). Unity writes its YAML with LF on every platform; File.WriteAllLines would re-emit Environment.NewLine
        // (CRLF on Windows) and churn the whole file for a one-line edit. Returns false, with the reason logged, when
        // the asset cannot be made editable; nothing is written then.
        private static bool TryWritePreservingNewlines(string assetPath, IReadOnlyList<string> lines)
        {
            var original = ReadAllText(assetPath, out var encoding);

            // Check out through the version control provider, as Unity's own saves do. Without a provider this returns
            // true even for a read-only file, so the read-only flag is checked on its own.
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

            // A checkout may fetch a newer revision; the edit was computed from the old one, so it must not be applied.
            if (ReadAllText(assetPath, out _) != original)
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

        // The encoding comes from the byte-order mark, UTF-8 without one when there is none.
        private static string ReadAllText(string path, out Encoding encoding)
        {
            using var reader = new StreamReader(path, new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true);
            var text = reader.ReadToEnd();
            encoding = reader.CurrentEncoding;
            return text;
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
