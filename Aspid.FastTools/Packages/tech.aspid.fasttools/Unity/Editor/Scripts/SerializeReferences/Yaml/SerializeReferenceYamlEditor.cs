using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Rewrites the stored type of a managed reference directly in an asset's YAML text. This is the only way
    /// to re-point a <c>[SerializeReference]</c> whose type can no longer be loaded (renamed / moved / deleted),
    /// because Unity drops missing references to <see langword="null"/> through the serialization API and never
    /// exposes them for reassignment. Parser-free: the document and the target <c>RefIds</c> entry are located by
    /// line scanning, and only the inline <c>{ … }</c> on the entry's <c>type:</c> line is replaced.
    /// </summary>
    /// <remarks>
    /// Split across partial files by responsibility:
    /// <see cref="FindMissingReferences"/> / <see cref="FindUnsetRequiredFields"/> (scan, <c>.Scan.cs</c>),
    /// the YAML-mutating repairs (<c>.Rewrite.cs</c>) and the read-only id/type/field probes (<c>.Read.cs</c>).
    /// This file holds the document / <c>RefIds</c> / indentation primitives those parts share.
    /// </remarks>
    internal static partial class SerializeReferenceYamlEditor
    {
        // The null managed-reference id Unity stores for an unassigned [SerializeReference] field
        // (UnityEngine.Serialization.ManagedReferenceUtility.RefIdNull).
        private const long NullRid = -2;

        // The object document header ("--- !u!114 &11400000") and the RefIds-block lookup are single-sourced in
        // SerializeReferenceYaml so this repair flow and the graph scanner read Unity's RefIds shape identically.
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

        // Index of the "RefIds:" key line within [start, end) (see SerializeReferenceYaml.FindRefIdsStart).
        private static int FindRefIdsStart(string[] lines, int start, int end) =>
            SerializeReferenceYaml.FindRefIdsStart(lines, start, end);

        // The exclusive end line of a RefIds entry that begins at headerIndex (see SerializeReferenceYaml.FindEntryEnd).
        private static int FindEntryEnd(string[] lines, int headerIndex, int end, int entryIndent) =>
            SerializeReferenceYaml.FindEntryEnd(lines, headerIndex, end, entryIndent);

        // Leading-indentation width of a line, counting each space or tab as one unit (see SerializeReferenceYaml.IndentOf).
        private static int IndentOf(string line) =>
            SerializeReferenceYaml.IndentOf(line);

        // Parses the inline "class: X, ns: Y, asm: Z" body of a RefIds type mapping (see SerializeReferenceYaml.TryParseInlineType).
        private static bool TryParseInlineType(string body, out ManagedTypeName type) =>
            SerializeReferenceYaml.TryParseInlineType(body, out type);

        /// <summary>
        /// Writes <paramref name="lines"/> back to <paramref name="assetPath"/> preserving the file's original newline
        /// style and trailing-newline state. <see cref="File.WriteAllLines(string,IEnumerable{string})"/> would re-emit
        /// every line with <see cref="Environment.NewLine"/> (CRLF on Windows) — Unity writes its YAML with LF on every
        /// platform, so that would churn the whole file LF→CRLF for a one-line edit. Sniffing the source newline keeps a
        /// surgical edit to just the intended line(s).
        /// </summary>
        private static void WritePreservingNewlines(string assetPath, IReadOnlyList<string> lines)
        {
            var original = File.ReadAllText(assetPath);
            var newline = DominantNewline(original);

            var builder = new StringBuilder(original.Length);
            for (var i = 0; i < lines.Count; i++)
            {
                builder.Append(lines[i]);
                if (i < lines.Count - 1) builder.Append(newline);
            }

            // Re-add the trailing terminator only if the source had one (Unity assets always do).
            if (original.Length > 0 && original[^1] == '\n') builder.Append(newline);

            File.WriteAllText(assetPath, builder.ToString());
        }

        // The newline to re-emit when rewriting an asset: the style that dominates the source by line count, not "any
        // CRLF wins". Picking by majority keeps a one-line edit on a mixed- or lone-CR file from flipping every other
        // line's terminator (a whole-file diff for a one-line change). CRLF wins ties only when it is the strict
        // majority; otherwise LF — Unity's invariant terminator — is used. Counts CRLF and lone-LF (an LF not preceded
        // by CR); a lone CR (classic Mac) maps to LF, since this writer only emits "\r\n" or "\n".
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

        // Whether every line in [start, end) is indented with spaces only — the precondition the block-removing writes
        // verify before touching the file, so an asset with unexpected (tab / mixed) indentation is left untouched.
        private static bool BlockIndentIsTrusted(string[] lines, int start, int end)
        {
            for (var i = Math.Max(start, 0); i < end && i < lines.Length; i++)
            {
                if (!IndentIsSpaceOnly(lines[i])) return false;
            }

            return true;
        }

        // A line whose leading indentation is spaces only — Unity's invariant for serialized YAML. Returns false when
        // the indent begins with a tab or any other whitespace, the case where IndentOf and the "- rid:" \s* regexes
        // can still measure the same nesting differently (a single tab vs a run of spaces). Callers about to delete a
        // bounded block bail on such a line rather than risk a mis-bounded, non-undoable write.
        private static bool IndentIsSpaceOnly(string line)
        {
            // A blank / whitespace-only line carries no indentation to measure — FindEntryEnd spans blank lines inside
            // an entry, so a stray tab in such a line must not abort an otherwise valid (space-indented) block removal.
            if (string.IsNullOrWhiteSpace(line)) return true;

            // The leading run of a non-blank line is its indentation; the first non-space character ends it. A tab (or
            // any non-space whitespace) there means tab / mixed indentation — the case IndentOf and the regexes disagree
            // on — so the block is untrusted. Whitespace after the first content character is not indentation.
            foreach (var character in line)
            {
                if (character == ' ') continue;
                return !char.IsWhiteSpace(character);
            }

            return true;
        }

        // Whether the text looks like a Unity-serialized YAML asset: its directive preamble (everything before the
        // first document "---") must carry Unity's signature "%TAG !u! tag:unity3d.com,2011:" directive. Guards the
        // destructive writes so a hand-authored or foreign YAML file — which this line-scanning parser was never
        // designed for — can never be surgically rewritten.
        private static bool LooksLikeUnityYaml(string[] lines)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                var line = (i == 0 ? StripByteOrderMark(lines[i]) : lines[i]).TrimStart();
                if (line.Length == 0) continue;
                if (line.StartsWith("%TAG !u!", StringComparison.Ordinal)) return true;
                if (line.StartsWith("---", StringComparison.Ordinal)) return false; // first document reached without the %TAG marker
            }

            return false;
        }

        // File.ReadAllLines strips a UTF-8 BOM in practice, but guard the first line defensively so the %TAG sniff is
        // never thrown off by a leading byte-order mark.
        private static string StripByteOrderMark(string line) =>
            line.Length > 0 && line[0] == '\uFEFF' ? line[1..] : line;
    }
}
