using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Identity of a managed-reference type as it is stored in Unity's serialized YAML
    /// (<c>type: {class: …, ns: …, asm: …}</c>). Used to repair a reference whose type went missing by
    /// rewriting that line directly, since Unity's serialization API cannot reassign a missing type.
    /// </summary>
    internal readonly struct ManagedTypeName
    {
        public readonly string Assembly;
        public readonly string Namespace;
        public readonly string Class;

        public ManagedTypeName(string assembly, string @namespace, string className)
        {
            Assembly = assembly ?? string.Empty;
            Namespace = @namespace ?? string.Empty;
            Class = className ?? string.Empty;
        }

        public bool IsEmpty =>
            string.IsNullOrEmpty(Assembly) && string.IsNullOrEmpty(Namespace) && string.IsNullOrEmpty(Class);

        /// <summary>
        /// Builds the YAML type identity for a resolved <see cref="Type"/>, including the
        /// <c>Name`N[[arg, asm],…]</c> shape Unity uses for closed generics.
        /// </summary>
        public static ManagedTypeName FromType(Type type)
        {
            if (type is null) return default;

            var root = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            return new ManagedTypeName(root.Assembly.GetName().Name, root.Namespace, BuildClassName(type));
        }

        private static string BuildClassName(Type type)
        {
            if (!type.IsGenericType) return type.Name;

            var definition = type.GetGenericTypeDefinition();
            var arguments = type.GetGenericArguments().Select(BuildGenericArgumentName);
            return $"{definition.Name}[[{string.Join("],[", arguments)}]]";
        }

        private static string BuildGenericArgumentName(Type type) =>
            $"{BuildFullClassName(type)}, {type.Assembly.GetName().Name}";

        private static string BuildFullClassName(Type type)
        {
            if (!type.IsGenericType) return type.FullName;

            var definition = type.GetGenericTypeDefinition();
            var prefix = string.IsNullOrEmpty(definition.Namespace) ? string.Empty : $"{definition.Namespace}.";
            return $"{prefix}{BuildClassName(type)}";
        }

        /// <summary>Renders the inline YAML mapping Unity writes for a managed-reference type entry.</summary>
        public string ToYamlType() =>
            $"{{class: {EscapeInline(Class)}, ns: {EscapeInline(Namespace)}, asm: {EscapeInline(Assembly)}}}";

        // A flow-scalar containing any of , [ ] { } would break the inline mapping, so single-quote it
        // (doubling embedded quotes) exactly as Unity does for generic class names like Foo`1[[…]].
        private static string EscapeInline(string value)
        {
            if (string.IsNullOrEmpty(value) || value.IndexOfAny(YamlReservedChars) < 0)
                return value ?? string.Empty;

            return $"'{value.Replace("'", "''")}'";
        }

        private static readonly char[] YamlReservedChars = { ',', '[', ']', '{', '}' };
    }

    /// <summary>
    /// Rewrites the stored type of a managed reference directly in an asset's YAML text. This is the only way
    /// to re-point a <c>[SerializeReference]</c> whose type can no longer be loaded (renamed / moved / deleted),
    /// because Unity drops missing references to <see langword="null"/> through the serialization API and never
    /// exposes them for reassignment. Parser-free: the document and the target <c>RefIds</c> entry are located by
    /// line scanning, and only the inline <c>{ … }</c> on the entry's <c>type:</c> line is replaced.
    /// </summary>
    internal static class SerializeReferenceYamlEditor
    {
        // "--- !u!114 &11400000" — object document header carrying the local file id as its YAML anchor.
        private static readonly Regex DocumentHeader = new(@"^--- !u!\d+ &(\d+)", RegexOptions.Compiled);

        /// <summary>
        /// Replaces the <c>type:</c> mapping of the <c>RefIds</c> entry identified by <paramref name="rid"/> within
        /// the object document anchored at <paramref name="fileId"/>. Returns <see langword="true"/> when the file
        /// was rewritten; the caller is responsible for reimporting the asset.
        /// </summary>
        public static bool TryRewriteType(string assetPath, long fileId, long rid, ManagedTypeName newType)
        {
            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return false;

                var lines = File.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                // Field pointers ("_sidearms:\n  - rid: 1002") share the "- rid:" shape with RefIds entries, so
                // confine the search to the RefIds block — the entries are the only ones with a following type:.
                var refIdsStart = FindRefIdsStart(lines, start, end);
                if (refIdsStart < 0) return false;

                // Match the list item "    - rid: <id>" (the leading dash distinguishes a RefIds entry from a
                // nested data "rid:" scalar).
                var ridPattern = new Regex($@"^\s*-\s+rid:\s*{rid}\s*$");
                var typePattern = new Regex(@"^(?<indent>\s*type:\s*)\{.*\}\s*$");

                for (var i = refIdsStart; i < end; i++)
                {
                    if (!ridPattern.IsMatch(lines[i])) continue;

                    // The type mapping follows the rid line; scan a few lines to tolerate formatting variance.
                    for (var j = i + 1; j < end && j <= i + 4; j++)
                    {
                        var match = typePattern.Match(lines[j]);
                        if (!match.Success) continue;

                        lines[j] = match.Groups["indent"].Value + newType.ToYamlType();
                        File.WriteAllLines(assetPath, lines);
                        return true;
                    }

                    return false;
                }

                return false;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SerializeReferenceSelector] Failed to rewrite managed-reference type in '{assetPath}': {exception}");
                return false;
            }
        }

        /// <summary>
        /// Reads the managed-reference id (<c>rid</c>) stored at <paramref name="propertyPath"/> within the object
        /// document anchored at <paramref name="fileId"/>. Needed because Unity reports an invalid id for a property
        /// whose type is missing — the real id only survives in the YAML. Resolves top-level fields (<c>_weapon</c>)
        /// and top-level sequence elements (<c>_alternates.Array.data[i]</c>); deeper paths return
        /// <see langword="false"/> so the caller can fall back.
        /// </summary>
        public static bool TryReadReferenceId(string assetPath, long fileId, string propertyPath, out long rid)
        {
            rid = 0;
            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return false;

                var lines = File.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                // Field pointers live before the "references:" block; restrict the search to them.
                var fieldsEnd = end;
                var references = new Regex(@"^\s*references:\s*$");
                for (var i = start; i < end; i++)
                    if (references.IsMatch(lines[i])) { fieldsEnd = i; break; }

                // Unity paths look like "_weapon" or "_alternates.Array.data[3]"; normalise to "_weapon" / "_alternates[3]".
                var path = propertyPath.Replace(".Array.data", string.Empty);
                var segment = Regex.Match(path, @"^(?<name>[^\[\]\.]+)(\[(?<idx>\d+)\])?$");
                if (!segment.Success) return false;

                var name = segment.Groups["name"].Value;
                var hasIndex = segment.Groups["idx"].Success;
                var index = hasIndex ? int.Parse(segment.Groups["idx"].Value) : -1;

                var fieldPattern = new Regex($@"^(?<indent>\s*){Regex.Escape(name)}:\s*(?<inline>.*)$");
                var ridScalar = new Regex(@"rid:\s*(-?\d+)");

                for (var i = start; i < fieldsEnd; i++)
                {
                    var field = fieldPattern.Match(lines[i]);
                    if (!field.Success) continue;

                    if (!hasIndex)
                    {
                        var inline = ridScalar.Match(field.Groups["inline"].Value);
                        if (inline.Success) return long.TryParse(inline.Groups[1].Value, out rid);

                        for (var j = i + 1; j < fieldsEnd && j <= i + 3; j++)
                        {
                            var scalar = ridScalar.Match(lines[j]);
                            if (scalar.Success) return long.TryParse(scalar.Groups[1].Value, out rid);
                            if (lines[j].Trim().Length > 0 && !lines[j].TrimStart().StartsWith("rid")) break;
                        }

                        return false;
                    }

                    var count = 0;
                    for (var j = i + 1; j < fieldsEnd; j++)
                    {
                        var trimmed = lines[j].TrimStart();
                        if (trimmed.StartsWith("- "))
                        {
                            if (count == index)
                            {
                                var scalar = ridScalar.Match(trimmed);
                                return scalar.Success && long.TryParse(scalar.Groups[1].Value, out rid);
                            }

                            count++;
                        }
                        else if (trimmed.Length > 0 && !trimmed.StartsWith("-"))
                        {
                            break;
                        }
                    }

                    return false;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Reads the managed-reference id stored at <paramref name="propertyPath"/> and the type recorded for it in
        /// the <c>RefIds</c> block, in a single pass over the asset YAML. This is how a missing reference is found
        /// even when Unity has dropped it from the live object (notably on prefabs / GameObjects): the orphaned
        /// id, type identity and payload all survive in the file.
        /// </summary>
        public static bool TryReadStoredType(string assetPath, long fileId, string propertyPath, out long rid, out ManagedTypeName type)
        {
            rid = 0;
            type = default;

            if (!TryReadReferenceId(assetPath, fileId, propertyPath, out rid)) return false;

            try
            {
                var lines = File.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                var refIdsStart = FindRefIdsStart(lines, start, end);
                if (refIdsStart < 0) return false;

                var ridPattern = new Regex($@"^\s*-\s+rid:\s*{rid}\s*$");
                var typePattern = new Regex(@"^\s*type:\s*\{(?<body>.*)\}\s*$");

                for (var i = refIdsStart; i < end; i++)
                {
                    if (!ridPattern.IsMatch(lines[i])) continue;

                    for (var j = i + 1; j < end && j <= i + 4; j++)
                    {
                        var match = typePattern.Match(lines[j]);
                        if (!match.Success) continue;

                        return TryParseInlineType(match.Groups["body"].Value, out type);
                    }

                    return false;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Parses the inline "class: X, ns: Y, asm: Z" body of a RefIds type mapping, honouring single-quoted values
        // (Unity quotes generic class names such as 'Modifier`1[[…]]').
        private static bool TryParseInlineType(string body, out ManagedTypeName type)
        {
            type = default;

            var match = Regex.Match(body,
                @"class:\s*(?:'(?<class>(?:[^']|'')*)'|(?<class>[^,}]*?))\s*,\s*ns:\s*(?<ns>[^,}]*?)\s*,\s*asm:\s*(?<asm>[^,}]*?)\s*$");
            if (!match.Success) return false;

            var className = match.Groups["class"].Value.Replace("''", "'");
            type = new ManagedTypeName(match.Groups["asm"].Value, match.Groups["ns"].Value, className);
            return !type.IsEmpty;
        }

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

                if (long.TryParse(match.Groups[1].Value, out var anchor) && anchor == fileId)
                    start = i;
            }

            if (start >= 0) return (start, end);
            if (headerCount == 1) return (firstHeader, lines.Length);
            return (-1, -1);
        }

        // Index of the "RefIds:" key line within [start, end), or -1 when the document has no managed references.
        private static int FindRefIdsStart(string[] lines, int start, int end)
        {
            var refIds = new Regex(@"^\s*RefIds:\s*$");
            for (var i = start; i < end; i++)
                if (refIds.IsMatch(lines[i]))
                    return i;

            return -1;
        }
    }
}
