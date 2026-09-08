using System;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceYaml
    {
        public static readonly Regex DocumentHeader = new(@"^--- !u!(?<class>\d+) &(?<id>\d+)", RegexOptions.Compiled);

        public static readonly Regex RefIdsKey = new(@"^\s*RefIds:\s*$", RegexOptions.Compiled);

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
