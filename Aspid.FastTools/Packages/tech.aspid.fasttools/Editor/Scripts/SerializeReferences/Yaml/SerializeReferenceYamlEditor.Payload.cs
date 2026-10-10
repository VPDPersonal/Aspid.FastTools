using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static partial class SerializeReferenceYamlEditor
    {
        // A number JSON accepts as is. Unity writes floats in this form too: 1.5, 0.0000999, 3.4e+38.
        private static readonly Regex _jsonNumber = new(@"^-?(0|[1-9]\d*)(\.\d+)?([eE][-+]?\d+)?$");

        // The flat top-level scalars of a missing reference's payload (Unity's serializedData) as JSON values, in
        // payload order. Quoted, escaped and wrapped scalars are decoded. A key that isTextField accepts stays a JSON
        // string even when its scalar reads as a number. Nested mappings, sequences and flow values are skipped.
        public static List<KeyValuePair<string, string>> ReadPayloadScalarsAsJson(string serializedData,
            Func<string, bool> isTextField = null)
        {
            var result = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrEmpty(serializedData)) return result;

            var lines = serializedData.Replace("\r\n", "\n").Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Length == 0 || char.IsWhiteSpace(line[0]) || line[0] == '-') continue;

                var separator = line.IndexOf(':');
                if (separator <= 0) continue;

                var key = line[..separator].Trim();
                var first = line[(separator + 1)..].Trim(' ', '\t');

                // A wrapped scalar continues on the indented lines below its key, empty lines included.
                var last = i;
                while (last + 1 < lines.Length && (lines[last + 1].Length == 0 || char.IsWhiteSpace(lines[last + 1][0])))
                    last++;

                while (last > i && lines[last].Trim(' ', '\t').Length == 0)
                    last--;

                var continuation = new string[last - i];
                Array.Copy(sourceArray: lines, sourceIndex: i + 1, destinationArray: continuation, destinationIndex: 0, length: continuation.Length);

                var next = last + 1 < lines.Length ? lines[last + 1] : string.Empty;
                i = last;

                if (key.Length == 0) continue;

                if (first.Length == 0)
                {
                    // An empty value heads a nested mapping or sequence; alone, it is an empty string.
                    if (continuation.Length == 0 && !next.StartsWith("-", StringComparison.Ordinal))
                        result.Add(new KeyValuePair<string, string>(key, "\"\""));

                    continue;
                }

                if (!TryDecodeScalar(first, continuation, out var text, out var plain)) continue;

                var asNumber = plain && !(isTextField?.Invoke(key) ?? false);
                result.Add(new KeyValuePair<string, string>(key, ToJsonValue(text, asNumber: asNumber)));
            }

            return result;
        }

        public static string BuildJsonObject(IEnumerable<KeyValuePair<string, string>> members)
        {
            var json = new StringBuilder("{");

            foreach (var member in members)
            {
                if (json.Length > 1) json.Append(',');
                json.Append(QuoteJson(member.Key)).Append(':').Append(member.Value);
            }

            return json.Append('}').ToString();
        }

        private static bool TryDecodeScalar(string first, string[] continuation, out string text, out bool plain)
        {
            text = null;
            plain = false;

            if (first[0] is '"' or '\'')
                return TryDecodeQuoted(JoinScalarLines(first, continuation), doubleQuoted: first[0] == '"', out text);

            // Flow collections, block scalars, anchors, aliases and tags are not flat scalars.
            if (first[0] is '{' or '[' or '|' or '>' or '&' or '*' or '!') return false;

            plain = true;
            text = FoldPlain(first, continuation);
            return true;
        }

        // The scalar after its opening quote, with its lines joined by '\n' for the folding rules.
        private static string JoinScalarLines(string first, string[] continuation)
        {
            var raw = new StringBuilder(value: first, startIndex: 1, length: first.Length - 1, capacity: first.Length + 64);
            foreach (var line in continuation) raw.Append('\n').Append(line);
            return raw.ToString();
        }

        // YAML flow folding: a single line break reads as a space, each empty line as '\n', and the white space
        // around a break is dropped. A double-quoted scalar also takes escapes, and '\' before a break removes it.
        private static bool TryDecodeQuoted(string raw, bool doubleQuoted, out string text)
        {
            text = null;

            var builder = new StringBuilder(capacity: raw.Length);
            var whitespaceStart = -1;
            var quote = doubleQuoted ? '"' : '\'';
            var i = 0;

            while (i < raw.Length)
            {
                var c = raw[i];

                if (c == quote)
                {
                    if (doubleQuoted || i + 1 >= raw.Length || raw[i + 1] != '\'')
                    {
                        text = builder.ToString();
                        return true;
                    }

                    builder.Append('\'');
                    whitespaceStart = -1;
                    i += 2;
                    continue;
                }

                if (c == '\n')
                {
                    if (whitespaceStart >= 0) builder.Length = whitespaceStart;
                    whitespaceStart = -1;
                    i = FoldLineBreak(raw, i, builder);
                    continue;
                }

                if (c is ' ' or '\t')
                {
                    if (whitespaceStart < 0) whitespaceStart = builder.Length;
                    builder.Append(c);
                    i++;
                    continue;
                }

                whitespaceStart = -1;

                if (c != '\\' || !doubleQuoted)
                {
                    builder.Append(c);
                    i++;
                    continue;
                }

                if (i + 1 >= raw.Length) return false;

                if (raw[i + 1] == '\n')
                {
                    i += 2;
                    while (i < raw.Length && raw[i] is ' ' or '\t') i++;
                    continue;
                }

                if (!TryAppendEscape(raw, ref i, builder)) return false;
            }

            // No closing quote.
            return false;
        }

        // `i` is on the break; returns the index of the next content character.
        private static int FoldLineBreak(string raw, int i, StringBuilder builder)
        {
            var emptyLines = 0;
            i++;

            while (true)
            {
                while (i < raw.Length && raw[i] is ' ' or '\t') i++;
                if (i >= raw.Length || raw[i] != '\n') break;

                emptyLines++;
                i++;
            }

            if (emptyLines == 0) builder.Append(' ');
            else builder.Append('\n', repeatCount: emptyLines);

            return i;
        }

        // `i` is on the backslash and ends after the escape.
        private static bool TryAppendEscape(string raw, ref int i, StringBuilder builder)
        {
            var escape = raw[i + 1];
            i += 2;

            var digits = escape switch
            {
                'x' => 2,
                'u' => 4,
                'U' => 8,
                _ => 0,
            };

            if (digits > 0) return TryAppendCodePoint(raw, ref i, digits, builder);

            var decoded = escape switch
            {
                '0' => '\0',
                'a' => '\a',
                'b' => '\b',
                't' or '\t' => '\t',
                'n' => '\n',
                'v' => '\v',
                'f' => '\f',
                'r' => '\r',
                'e' => '\u001B',
                ' ' or '"' or '/' or '\\' => escape,
                'N' => '\u0085',
                '_' => '\u00A0',
                'L' => '\u2028',
                'P' => '\u2029',
                _ => (char?)null,
            };

            if (decoded is null) return false;

            builder.Append(decoded.Value);
            return true;
        }

        private static bool TryAppendCodePoint(string raw, ref int i, int digits, StringBuilder builder)
        {
            if (i + digits > raw.Length) return false;

            if (!int.TryParse(raw.Substring(i, digits), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var codePoint) ||
                codePoint < 0)
                return false;

            i += digits;

            // \u escapes may spell the halves of a surrogate pair one by one.
            if (codePoint <= 0xFFFF) builder.Append((char)codePoint);
            else if (codePoint <= 0x10FFFF) builder.Append(char.ConvertFromUtf32(codePoint));
            else return false;

            return true;
        }

        private static string FoldPlain(string first, string[] continuation)
        {
            var builder = new StringBuilder(first);
            var emptyLines = 0;

            foreach (var raw in continuation)
            {
                var line = raw.Trim(' ', '\t');
                if (line.Length == 0)
                {
                    emptyLines++;
                    continue;
                }

                if (emptyLines == 0) builder.Append(' ');
                else builder.Append('\n', repeatCount: emptyLines);

                builder.Append(line);
                emptyLines = 0;
            }

            return builder.ToString();
        }

        // JsonUtility reads Infinity, -Infinity and NaN bare, as Unity writes them; a quoted one would read as 0.
        private static string ToJsonValue(string text, bool asNumber)
        {
            if (asNumber && (_jsonNumber.IsMatch(text) || text is "Infinity" or "-Infinity" or "NaN"))
                return text;

            return QuoteJson(text);
        }

        private static string QuoteJson(string text)
        {
            var json = new StringBuilder(capacity: text.Length + 2).Append('"');

            foreach (var c in text)
            {
                var escape = c switch
                {
                    '"' => "\\\"",
                    '\\' => "\\\\",
                    '\n' => "\\n",
                    '\r' => "\\r",
                    '\t' => "\\t",
                    '\b' => "\\b",
                    '\f' => "\\f",
                    < ' ' => "\\u" + ((int)c).ToString(format: "x4", CultureInfo.InvariantCulture),
                    _ => null,
                };

                if (escape is null) json.Append(c);
                else json.Append(escape);
            }

            return json.Append('"').ToString();
        }
    }
}
