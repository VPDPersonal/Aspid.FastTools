using System;
using System.IO;
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
