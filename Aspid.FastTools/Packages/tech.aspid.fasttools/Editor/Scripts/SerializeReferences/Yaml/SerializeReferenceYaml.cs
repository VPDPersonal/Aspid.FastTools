using System;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceYaml
    {
        // "--- !u!114 &11400000". The anchor is a signed 64-bit fileID: sub-assets (AddObjectToAsset) and prefab
        // components are often written with a negative one ("&-4472597160913118672").
        public static readonly Regex DocumentHeader = new(@"^--- !u!(?<class>\d+) &(?<id>-?\d+)", RegexOptions.Compiled);

        public static readonly Regex RefIdsKey = new(@"^\s*RefIds:\s*$", RegexOptions.Compiled);

        private static readonly Regex _entryHeader = new(@"^(?<indent>\s*)-\s+rid:\s*(?<rid>-?\d+)\s*$", RegexOptions.Compiled);

        private static readonly Regex _typeLine = new(@"^\s*type:\s*\{.*\}\s*$", RegexOptions.Compiled);

        public static readonly Regex InlineType = new(
            @"class:\s*(?:'(?<class>(?:[^']|'')*)'|(?<class>[^,}]*?))\s*,\s*ns:\s*(?<ns>[^,}]*?)\s*,\s*asm:\s*(?<asm>[^,}]*?)\s*$",
            RegexOptions.Compiled);

        public static readonly string[] ScanExtensions = { ".prefab", ".asset", ".unity" };

        public static bool TryParseInlineType(string body, out ManagedTypeName type)
        {
            type = default;

            var match = InlineType.Match(body);

            if (!match.Success)
                return false;

            var className = match.Groups["class"].Value.Replace("''", "'");
            type = new ManagedTypeName(match.Groups["asm"].Value, match.Groups["ns"].Value, className);

            return !type.IsEmpty;
        }

        public static int FindRefIdsStart(string[] lines, int start, int end)
        {
            for (var i = start; i < end; i++)
            {
                if (RefIdsKey.IsMatch(lines[i]))
                    return i;
            }

            return -1;
        }

        // Every "--- " line starts a document, whether or not DocumentHeader can parse its anchor, so a document never
        // runs into the next one.
        public static bool IsDocumentStart(string line) =>
            line.StartsWith("--- ", StringComparison.Ordinal);

        public static int FindDocumentEnd(string[] lines, int from)
        {
            for (var i = from; i < lines.Length; i++)
            {
                if (IsDocumentStart(lines[i]))
                    return i;
            }

            return lines.Length;
        }

        // RefIds entry headers sit at the indent of the first "- rid:" under RefIds. A deeper "- rid:" is an element
        // of a [SerializeReference] list inside another entry's data block, not an entry.
        public static int FindRefIdsEntryIndent(string[] lines, int refIdsStart, int end)
        {
            for (var i = refIdsStart + 1; i < end; i++)
            {
                var match = _entryHeader.Match(lines[i]);
                if (match.Success) return match.Groups["indent"].Length;
            }

            return -1;
        }

        public static bool TryMatchEntryHeader(string line, int entryIndent, out long rid)
        {
            rid = 0;

            var match = _entryHeader.Match(line);
            return match.Success
                && match.Groups["indent"].Length == entryIndent
                && long.TryParse(match.Groups["rid"].Value, out rid);
        }

        // Returns the line of rid's own RefIds entry header, or -1. A nested "- rid: N" list element in an earlier
        // entry's data block has the same shape and is skipped by its indent.
        public static int FindEntryHeader(string[] lines, int refIdsStart, int end, long rid, out int entryIndent)
        {
            entryIndent = FindRefIdsEntryIndent(lines, refIdsStart, end);
            if (entryIndent < 0) return -1;

            for (var i = refIdsStart + 1; i < end; i++)
            {
                if (TryMatchEntryHeader(lines[i], entryIndent, out var headerRid) && headerRid == rid)
                    return i;
            }

            return -1;
        }

        // Returns the line of the entry's own "type: {…}" mapping within (headerIndex, entryEnd), or -1. Only the
        // entry's direct children are considered, so a same-named field inside its data block is never taken.
        public static int FindEntryTypeLine(string[] lines, int headerIndex, int entryEnd)
        {
            var childIndent = -1;

            for (var i = headerIndex + 1; i < entryEnd; i++)
            {
                if (lines[i].Trim().Length == 0) continue;

                var indent = IndentOf(lines[i]);
                if (childIndent < 0) childIndent = indent;
                if (indent != childIndent) continue;

                if (_typeLine.IsMatch(lines[i])) return i;
            }

            return -1;
        }

        public static int FindEntryEnd(string[] lines, int headerIndex, int end, int entryIndent)
        {
            for (var j = headerIndex + 1; j < end; j++)
            {
                if (lines[j].Trim().Length == 0)
                    continue;

                var indent = IndentOf(lines[j]);
                if (indent < entryIndent || (indent == entryIndent && lines[j].TrimStart().StartsWith("- ")))
                    return j;
            }

            return end;
        }

        // Counts each space or tab as one unit. Unity always indents with spaces, but the entry regexes capture
        // leading whitespace with \s*, so counting tabs here keeps this aligned with them — otherwise a tab-indented
        // line would read as indent 0 while a regex sees it as N and the entry would be mis-bounded.
        public static int IndentOf(string line)
        {
            var count = 0;
            while (count < line.Length && (line[count] == ' ' || line[count] == '\t'))
            {
                count++;
            }

            return count;
        }

        public static bool IsCandidateAssetPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/", StringComparison.Ordinal))
                return false;

            return ScanExtensions.Any(extension => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
        }
    }
}
