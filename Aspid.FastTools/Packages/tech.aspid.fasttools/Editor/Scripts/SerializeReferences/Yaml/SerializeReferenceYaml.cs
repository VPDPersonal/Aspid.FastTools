using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
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

        // A flow scalar of the inline type mapping: single-quoted (a generic class), double-quoted (a name with non-ASCII
        // characters, which Unity writes as "\uXXXX" escapes) or plain up to the next ',' or '}'.
        private const string FlowScalar = @"'(?:[^']|'')*'|""(?:[^""\\]|\\.)*""|[^,}]*?";

        public static readonly Regex InlineType = new(
            $@"class:\s*(?<class>{FlowScalar})\s*,\s*ns:\s*(?<ns>{FlowScalar})\s*,\s*asm:\s*(?<asm>{FlowScalar})\s*$",
            RegexOptions.Compiled);

        // Unity 2021.2 and newer write the managed reference registry as version 2, with its entries under RefIds: the
        // only layout the scanners read.
        private const string SupportedReferencesVersion = "2";

        private static readonly Regex _referencesKey = new(@"^(?<indent>\s*)references:\s*$", RegexOptions.Compiled);

        private static readonly Regex _referencesVersion = new(@"^(?<indent>\s*)version:\s*(?<version>\d+)\s*$", RegexOptions.Compiled);

        // A Timeline (.playable) stores its tracks and clips, and an Animator Controller (.controller) its
        // StateMachineBehaviours, as MonoBehaviour documents, so they may hold [SerializeReference] fields.
        public static readonly string[] ScanExtensions = { ".prefab", ".asset", ".unity", ".controller", ".playable" };

        private const int FormatSniffLength = 64;

        private const string LfsPointerPrefix = "version https://git-lfs.github.com/spec/";

        // Reads only the first bytes, so a scanner can skip a binary asset (LightingData, NavMesh, anything in a
        // Force Binary / Mixed project) or an LFS pointer without decoding the whole file. A file that cannot be
        // opened counts as Binary: it is just as unreadable to the YAML pass.
        public static AssetFileFormat SniffFileFormat(string path)
        {
            var buffer = new byte[FormatSniffLength];
            var read = 0;

            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                int count;
                while (read < buffer.Length && (count = stream.Read(buffer, read, buffer.Length - read)) > 0)
                    read += count;
            }
            catch (Exception)
            {
                return AssetFileFormat.Binary;
            }

            var offset = read >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF ? 3 : 0;
            var head = Encoding.ASCII.GetString(buffer, offset, read - offset).TrimStart();

            if (head.StartsWith("%YAML", StringComparison.Ordinal) || head.StartsWith("%TAG !u!", StringComparison.Ordinal))
                return AssetFileFormat.TextYaml;

            return head.StartsWith(LfsPointerPrefix, StringComparison.Ordinal)
                ? AssetFileFormat.LfsPointer
                : AssetFileFormat.Binary;
        }

        public static bool IsTextYamlFile(string path) =>
            SniffFileFormat(path) == AssetFileFormat.TextYaml;

        // The one file read of the text scans (graph, missing types, overrides, usage index, scene required fields):
        // the asset's lines, or null when the path is empty, missing, unreadable or not text YAML. The first bytes are
        // sniffed before the full read, so a binary asset or an LFS pointer is never decoded; `knownTextYaml` skips
        // that sniff for a caller that has already made it (the gate's Scan).
        public static string[] ReadLines(string assetPath, bool knownTextYaml = false)
        {
            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return null;
                if (!knownTextYaml && !IsTextYamlFile(assetPath)) return null;

                return File.ReadAllLines(assetPath);
            }
            catch (Exception)
            {
                // Best effort, like the scanners: an unreadable file has nothing to scan.
                return null;
            }
        }

        public static bool TryParseInlineType(string body, out ManagedTypeName type)
        {
            type = default;

            var match = InlineType.Match(body);

            if (!match.Success)
                return false;

            type = new ManagedTypeName(
                UnquoteScalar(match.Groups["asm"].Value),
                UnquoteScalar(match.Groups["ns"].Value),
                UnquoteScalar(match.Groups["class"].Value));

            return !type.IsEmpty;
        }

        // Reads a flow scalar as YAML does: a single-quoted one doubles its quotes, a double-quoted one has escapes, a
        // plain one stays as written.
        public static string UnquoteScalar(string scalar)
        {
            if (scalar.Length >= 2 && scalar[0] == '\'' && scalar[^1] == '\'')
                return scalar[1..^1].Replace("''", "'");

            return scalar.Length >= 2 && scalar[0] == '"' && scalar[^1] == '"'
                ? UnescapeDoubleQuoted(scalar[1..^1])
                : scalar;
        }

        // Unity writes every character outside printable ASCII as "\uXXXX". An unknown or cut escape stays as written.
        private static string UnescapeDoubleQuoted(string body)
        {
            if (body.IndexOf('\\') < 0) return body;

            var builder = new StringBuilder(capacity: body.Length);

            for (var i = 0; i < body.Length; i++)
            {
                if (body[i] != '\\' || i + 1 == body.Length)
                {
                    builder.Append(body[i]);
                    continue;
                }

                var escape = body[i + 1];
                var digits = escape switch { 'x' => 2, 'u' => 4, 'U' => 8, _ => 0 };

                if (digits > 0 && TryReadHex(body, i + 2, digits, out var code))
                {
                    // "\u" may be one half of a surrogate pair, so it is appended as one UTF-16 unit.
                    if (digits < 8) builder.Append((char)code);
                    else builder.Append(char.ConvertFromUtf32(code));

                    i += 1 + digits;
                    continue;
                }

                var decoded = digits > 0 ? null : DecodeEscape(escape);

                if (decoded is null)
                {
                    builder.Append(body[i]);
                    continue;
                }

                builder.Append(decoded);
                i++;
            }

            return builder.ToString();
        }

        private static string DecodeEscape(char escape) => escape switch
        {
            '0' => "\0",
            'a' => "\a",
            'b' => "\b",
            't' or '\t' => "\t",
            'n' => "\n",
            'v' => "\v",
            'f' => "\f",
            'r' => "\r",
            'e' => "\u001B",
            ' ' => " ",
            '"' => "\"",
            '/' => "/",
            '\\' => "\\",
            'N' => "\u0085",
            '_' => "\u00A0",
            'L' => "\u2028",
            'P' => "\u2029",
            _ => null,
        };

        // Reads `length` hex digits at `start`. An 8-digit value must be a Unicode scalar value, not a surrogate.
        private static bool TryReadHex(string text, int start, int length, out int value)
        {
            value = 0;
            if (start + length > text.Length) return false;

            var digits = text.Substring(start, length);
            if (!long.TryParse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var code)) return false;
            if (code > 0x10FFFF || (length == 8 && code is >= 0xD800 and <= 0xDFFF)) return false;

            value = (int)code;
            return true;
        }

        // True when a document stores its managed references in a registry version other than 2: version 1 of an asset
        // last saved before Unity 2021.2 (no RefIds), or a newer format. The scanners would read such a file as clean.
        public static bool HasUnsupportedReferencesVersion(string[] lines)
        {
            if (lines is null) return false;

            for (var i = 0; i < lines.Length; i++)
            {
                var key = _referencesKey.Match(lines[i]);
                if (!key.Success) continue;

                var next = i + 1;
                while (next < lines.Length && lines[next].Trim().Length == 0)
                    next++;

                if (next == lines.Length) return false;

                // The registry opens with its version; a user field named "references" does not.
                var version = _referencesVersion.Match(lines[next]);
                if (!version.Success || version.Groups["indent"].Length <= key.Groups["indent"].Length) continue;

                if (version.Groups["version"].Value != SupportedReferencesVersion) return true;
            }

            return false;
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

        // Parses the rid of a "- rid: N" line at any indent, e.g. the header of an entry block captured out of its file.
        public static bool TryParseEntryHeaderRid(string line, out long rid)
        {
            rid = 0;

            var match = _entryHeader.Match(line);
            return match.Success && long.TryParse(match.Groups["rid"].Value, out rid);
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
