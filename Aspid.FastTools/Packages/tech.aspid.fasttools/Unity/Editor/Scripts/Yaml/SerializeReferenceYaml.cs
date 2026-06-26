using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The shared, parser-free YAML-scan toolkit for Unity's managed-reference (<c>RefIds</c>) serialization, used by
    /// both the repair flow (<see cref="SerializeReferenceYamlEditor"/>) and the graph window
    /// (<see cref="SerializeReferenceGraphScanner"/>). Single-sourcing these primitives keeps the two readers in
    /// agreement about Unity's RefIds shape — quoting, indentation and the document/inline-type grammar — so a fix to
    /// one cannot silently diverge from the other.
    /// </summary>
    internal static class SerializeReferenceYaml
    {
        // "--- !u!114 &11400000" — object document header carrying the local file id as its YAML anchor and the
        // class id ("!u!114") used as a best-effort fallback label when the live type name is unavailable.
        public static readonly Regex DocumentHeader = new(@"^--- !u!(?<class>\d+) &(?<id>\d+)", RegexOptions.Compiled);

        // "  RefIds:" — the managed-reference id list of an object document.
        public static readonly Regex RefIdsKey = new(@"^\s*RefIds:\s*$", RegexOptions.Compiled);

        // The inline "class: X, ns: Y, asm: Z" body of a RefIds type mapping, honouring the single-quoted class names
        // Unity writes for closed generics (e.g. 'Modifier`1[[…]]').
        public static readonly Regex InlineType = new(
            @"class:\s*(?:'(?<class>(?:[^']|'')*)'|(?<class>[^,}]*?))\s*,\s*ns:\s*(?<ns>[^,}]*?)\s*,\s*asm:\s*(?<asm>[^,}]*?)\s*$",
            RegexOptions.Compiled);

        /// <summary>
        /// Parses the inline <c>class: X, ns: Y, asm: Z</c> body of a <c>RefIds</c> type mapping into a
        /// <see cref="ManagedTypeName"/>, honouring the single-quoted class names Unity writes for closed generics
        /// (e.g. <c>'Modifier`1[[…]]'</c>). Returns <see langword="false"/> for a malformed or empty type body.
        /// </summary>
        public static bool TryParseInlineType(string body, out ManagedTypeName type)
        {
            type = default;

            var match = InlineType.Match(body);
            if (!match.Success) return false;

            var className = match.Groups["class"].Value.Replace("''", "'");
            type = new ManagedTypeName(match.Groups["asm"].Value, match.Groups["ns"].Value, className);
            return !type.IsEmpty;
        }

        /// <summary>
        /// Index of the <c>RefIds:</c> key line within <c>[start, end)</c>, or <c>-1</c> when the document has no
        /// managed references.
        /// </summary>
        public static int FindRefIdsStart(string[] lines, int start, int end)
        {
            for (var i = start; i < end; i++)
                if (RefIdsKey.IsMatch(lines[i]))
                    return i;

            return -1;
        }

        /// <summary>
        /// The exclusive end line of a <c>RefIds</c> entry that begins at <paramref name="headerIndex"/>: the entry runs
        /// until the next list item at its own indent, or until the block dedents out of it (blank lines are spanned).
        /// </summary>
        public static int FindEntryEnd(string[] lines, int headerIndex, int end, int entryIndent)
        {
            for (var j = headerIndex + 1; j < end; j++)
            {
                if (lines[j].Trim().Length == 0) continue;

                var indent = IndentOf(lines[j]);
                if (indent < entryIndent || (indent == entryIndent && lines[j].TrimStart().StartsWith("- ")))
                    return j;
            }

            return end;
        }

        /// <summary>
        /// Leading-indentation width of a line, counting each space or tab as one unit. Unity always indents its YAML
        /// with spaces, but the <c>"- rid:"</c> / <c>"type:"</c> indent regexes capture leading whitespace with
        /// <c>\s*</c> (which counts tabs too). Counting tabs here keeps this measure aligned with those regexes: a
        /// tab-indented line would otherwise read as indent 0 here while a regex sees it as N, and
        /// <see cref="FindEntryEnd"/> would mis-bound the entry. Alignment, not visual tab width, is what matters —
        /// both measures count one unit per character.
        /// </summary>
        public static int IndentOf(string line)
        {
            var count = 0;
            while (count < line.Length && (line[count] == ' ' || line[count] == '\t')) count++;
            return count;
        }
    }
}
