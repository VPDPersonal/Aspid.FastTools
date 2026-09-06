using System;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The shared, parser-free YAML-scan toolkit for Unity's RefIds serialization, used by both the repair flow and
    // the graph window. Single-sourcing these primitives keeps the two readers in agreement about quoting,
    // indentation and the document grammar, so a fix to one cannot silently diverge from the other.
    internal static class SerializeReferenceYaml
    {
        // An object document header: the local file id is the YAML anchor, and the class id is a fallback label
        // when the live type name is unavailable.
        public static readonly Regex DocumentHeader = new(@"^--- !u!(?<class>\d+) &(?<id>\d+)", RegexOptions.Compiled);

        public static readonly Regex RefIdsKey = new(@"^\s*RefIds:\s*$", RegexOptions.Compiled);

        // The inline type mapping's body, allowing for the single-quoted class names Unity writes for closed
        // generics.
        public static readonly Regex InlineType = new(
            @"class:\s*(?:'(?<class>(?:[^']|'')*)'|(?<class>[^,}]*?))\s*,\s*ns:\s*(?<ns>[^,}]*?)\s*,\s*asm:\s*(?<asm>[^,}]*?)\s*$",
            RegexOptions.Compiled);

        // The asset extensions whose YAML can host managed references. The scanners layer folder exclusion on top.
        public static readonly string[] ScanExtensions = { ".prefab", ".asset", ".unity" };

        // False for a malformed or empty type body.
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

        // -1 when the document has no managed references.
        public static int FindRefIdsStart(string[] lines, int start, int end)
        {
            for (var i = start; i < end; i++)
            {
                if (RefIdsKey.IsMatch(lines[i]))
                    return i;
            }

            return -1;
        }

        // An entry runs until the next list item at its own indent, or until the block dedents out of it; blank
        // lines are spanned.
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

        // The engine-level, settings-agnostic candidate test; a caller that must honor the user's excluded folders
        // combines it with its own check.
        public static bool IsCandidateAssetPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/", StringComparison.Ordinal))
                return false;

            return ScanExtensions.Any(extension => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
        }
    }
}
