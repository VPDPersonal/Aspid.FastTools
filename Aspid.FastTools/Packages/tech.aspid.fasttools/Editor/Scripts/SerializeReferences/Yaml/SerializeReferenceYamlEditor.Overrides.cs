using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static partial class SerializeReferenceYamlEditor
    {
        // A PrefabInstance document: a variant's root, a nested prefab, a prefab instance in a scene.
        private const string PrefabInstanceClassId = "1001";

        private static readonly Regex _modificationsKey =
            new(@"^(?<indent>\s*)m_Modifications:\s*(?<inline>.*)$", RegexOptions.Compiled);

        private static readonly Regex _modificationTarget =
            new(@"^(?<indent>\s*)-\s+target:\s*\{(?<body>.*)\}\s*$", RegexOptions.Compiled);

        private static readonly Regex _modificationField =
            new(@"^(?<indent>\s*)(?<key>propertyPath|value):\s?(?<value>.*)$", RegexOptions.Compiled);

        private static readonly Regex _targetFileId = new(@"\bfileID:\s*(?<id>-?\d+)", RegexOptions.Compiled);
        private static readonly Regex _targetGuid = new(@"\bguid:\s*(?<guid>[0-9a-fA-F]+)", RegexOptions.Compiled);

        // "managedReferences[7288618111259901954]" sets the type of that reference; a deeper path sets one of its fields.
        private static readonly Regex _managedReferenceTypePath =
            new(@"^managedReferences\[(?<rid>-?\d+)\]$", RegexOptions.Compiled);

        public static List<PrefabOverrideReference> FindPrefabOverrideReferences(string assetPath)
        {
            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return new List<PrefabOverrideReference>();

                return CollectPrefabOverrides(File.ReadAllLines(assetPath));
            }
            catch (Exception)
            {
                // Best effort, like FindMissingReferences.
                return new List<PrefabOverrideReference>();
            }
        }

        // For a sweep that already read the file for another pass.
        public static List<PrefabOverrideReference> FindPrefabOverrideReferences(string[] lines)
        {
            try
            {
                return lines is null ? new List<PrefabOverrideReference>() : CollectPrefabOverrides(lines);
            }
            catch (Exception)
            {
                return new List<PrefabOverrideReference>();
            }
        }

        // Every type-setting modification ("managedReferences[rid]" -> "<asm> <ns>.<class>") of every PrefabInstance
        // document. A cleared field writes "managedReferences[-2]" with an empty value, which is not a reference.
        private static List<PrefabOverrideReference> CollectPrefabOverrides(string[] lines)
        {
            var result = new List<PrefabOverrideReference>();

            for (var i = 0; i < lines.Length; i++)
            {
                var header = DocumentHeader.Match(lines[i]);
                if (!header.Success || header.Groups["class"].Value != PrefabInstanceClassId ||
                    !long.TryParse(header.Groups["id"].Value, out var fileId)) continue;

                var end = NextDocumentStart(lines, i + 1);
                var modifications = ReadModifications(lines, i + 1, end);

                foreach (var modification in modifications)
                {
                    var typePath = _managedReferenceTypePath.Match(modification.PropertyPath);
                    if (!typePath.Success || !long.TryParse(typePath.Groups["rid"].Value, out var rid) || rid < 0) continue;
                    if (!TryParseOverrideTypeValue(modification.Value, out var type)) continue;

                    result.Add(new PrefabOverrideReference(fileId, rid, type, modification.TargetFileId,
                        modification.TargetGuid, FindPointerPath(modifications, modification, rid)));
                }

                i = end - 1;
            }

            return result;
        }

        // The modification of the same target whose value is the rid is the field that points at the reference.
        private static string FindPointerPath(List<Modification> modifications, Modification typeModification, long rid)
        {
            var ridText = rid.ToString();
            var ownPrefix = typeModification.PropertyPath + ".";

            foreach (var modification in modifications)
            {
                if (modification.TargetFileId != typeModification.TargetFileId ||
                    !string.Equals(modification.TargetGuid, typeModification.TargetGuid, StringComparison.Ordinal)) continue;

                if (!string.Equals(modification.Value, ridText, StringComparison.Ordinal)) continue;
                if (modification.PropertyPath.StartsWith(ownPrefix, StringComparison.Ordinal)) continue;
                if (modification.PropertyPath.EndsWith(".Array.size", StringComparison.Ordinal)) continue;

                return modification.PropertyPath;
            }

            return string.Empty;
        }

        // "<asm> <ns>.<class>": the assembly is everything before the first space. The namespace ends at the last '.'
        // before the first nested-type '/' or generic-argument '[', so "Ns.Outer/Inner" and "Ns.Box`1[[Ns.T, Asm]]"
        // split the way the RefIds "type:" line stores them.
        public static bool TryParseOverrideTypeValue(string value, out ManagedTypeName type)
        {
            type = default;
            if (string.IsNullOrWhiteSpace(value)) return false;

            var trimmed = value.Trim();
            var space = trimmed.IndexOf(' ');
            if (space <= 0 || space == trimmed.Length - 1) return false;

            var assembly = trimmed[..space];
            var fullName = trimmed[(space + 1)..].Trim();

            var cut = fullName.Length;
            var nested = fullName.IndexOf('/');
            var generic = fullName.IndexOf('[');
            if (nested >= 0) cut = Math.Min(cut, nested);
            if (generic >= 0) cut = Math.Min(cut, generic);

            var dot = cut > 0 ? fullName.LastIndexOf('.', cut - 1) : -1;
            var @namespace = dot > 0 ? fullName[..dot] : string.Empty;
            var className = dot > 0 ? fullName[(dot + 1)..] : fullName;
            if (className.Length == 0) return false;

            type = new ManagedTypeName(assembly, @namespace, className);
            return true;
        }

        private static List<Modification> ReadModifications(string[] lines, int start, int end)
        {
            var result = new List<Modification>();

            var keyLine = -1;
            var keyIndent = 0;
            for (var i = start; i < end; i++)
            {
                var key = _modificationsKey.Match(lines[i]);
                if (!key.Success) continue;

                // "m_Modifications: []" carries nothing.
                if (key.Groups["inline"].Value.Trim().Length > 0) return result;

                keyLine = i;
                keyIndent = key.Groups["indent"].Length;
                break;
            }

            if (keyLine < 0) return result;

            Modification current = null;
            for (var i = keyLine + 1; i < end; i++)
            {
                var line = lines[i];
                if (line.Trim().Length == 0) continue;

                // Unity writes the sequence dashes at the key's own indent; the next sibling key ends the block.
                var indent = IndentOf(line);
                var isItem = line.TrimStart().StartsWith("- ", StringComparison.Ordinal);
                if (indent < keyIndent || (indent == keyIndent && !isItem)) break;

                var target = _modificationTarget.Match(line);
                if (target.Success && target.Groups["indent"].Length == keyIndent)
                {
                    current = new Modification(ParseTargetFileId(target.Groups["body"].Value),
                        ParseTargetGuid(target.Groups["body"].Value));
                    result.Add(current);
                    continue;
                }

                if (current is null) continue;

                var field = _modificationField.Match(line);
                if (!field.Success) continue;

                var fieldIndent = field.Groups["indent"].Length;
                var scalar = ReadScalar(lines, field.Groups["value"].Value, fieldIndent, ref i, end);

                if (field.Groups["key"].Value == "propertyPath") current.PropertyPath = scalar;
                else current.Value = scalar;
            }

            return result;
        }

        // A plain or quoted scalar, with the deeper-indented continuation lines a long value wraps onto folded back
        // with single spaces. `i` ends on the last line consumed.
        private static string ReadScalar(string[] lines, string first, int fieldIndent, ref int i, int end)
        {
            var builder = new StringBuilder(first.Trim());

            while (i + 1 < end)
            {
                var next = lines[i + 1];
                if (next.Trim().Length == 0 || IndentOf(next) <= fieldIndent) break;

                builder.Append(' ').Append(next.Trim());
                i++;
            }

            return Unquote(builder.ToString());
        }

        private static string Unquote(string scalar)
        {
            if (scalar.Length >= 2 && scalar[0] == '\'' && scalar[^1] == '\'')
                return scalar[1..^1].Replace("''", "'");

            if (scalar.Length >= 2 && scalar[0] == '"' && scalar[^1] == '"')
                return scalar[1..^1].Replace("\\\"", "\"").Replace("\\\\", "\\");

            return scalar;
        }

        private static long ParseTargetFileId(string body)
        {
            var match = _targetFileId.Match(body);
            return match.Success && long.TryParse(match.Groups["id"].Value, out var id) ? id : 0;
        }

        private static string ParseTargetGuid(string body)
        {
            var match = _targetGuid.Match(body);
            return match.Success ? match.Groups["guid"].Value : string.Empty;
        }

        private sealed class Modification
        {
            public readonly long TargetFileId;
            public readonly string TargetGuid;

            public string PropertyPath = string.Empty;
            public string Value = string.Empty;

            public Modification(long targetFileId, string targetGuid)
            {
                TargetFileId = targetFileId;
                TargetGuid = targetGuid;
            }
        }
    }
}
