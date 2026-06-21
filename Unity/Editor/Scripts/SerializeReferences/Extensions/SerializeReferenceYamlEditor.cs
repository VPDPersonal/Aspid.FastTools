using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
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
            // Unity stores a nested type's class identity with its declaring types joined by '/' (e.g. Outer/Inner);
            // reflection's Type.Name is only the leaf, so prefix the declaring chain here — the mirror of the read side's
            // '/'->'+' mapping in SerializeReferenceHelpers.StoredTypeResolves. Without this, repairing a reference to a
            // nested type would write `class: Inner`, which Unity cannot resolve (it re-breaks the reference).
            return new ManagedTypeName(root.Assembly.GetName().Name, root.Namespace, NestedPrefix(type) + BuildClassName(type));
        }

        // The "Outer/" (or "Outer/Middle/") prefix Unity prepends to a nested type's class identity; empty for a
        // top-level type. Walks the declaring-type chain from the outermost inward.
        private static string NestedPrefix(Type type)
        {
            if (type.DeclaringType is null) return string.Empty;

            var prefix = string.Empty;
            for (var declaring = type.DeclaringType; declaring is not null; declaring = declaring.DeclaringType)
                prefix = declaring.Name + "/" + prefix;

            return prefix;
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
    /// <summary>
    /// A single orphaned managed-reference entry found in an asset's YAML: the document it lives in
    /// (<see cref="FileId"/>), its <c>RefIds</c> id and the stored (unresolvable) type. Surfaced by the
    /// asset-level repair tool, which finds every such entry regardless of nesting depth or child object.
    /// </summary>
    internal readonly struct MissingReferenceEntry
    {
        public readonly long FileId;
        public readonly long Rid;
        public readonly ManagedTypeName StoredType;

        public MissingReferenceEntry(long fileId, long rid, ManagedTypeName storedType)
        {
            FileId = fileId;
            Rid = rid;
            StoredType = storedType;
        }
    }

    /// <summary>A single computed line change for the bulk-fix diff preview: where it lands and the before/after text.</summary>
    internal readonly struct RewriteEdit
    {
        public readonly string AssetPath;
        public readonly int LineNumber;
        public readonly string OldLine;
        public readonly string NewLine;

        public RewriteEdit(string assetPath, int lineNumber, string oldLine, string newLine)
        {
            AssetPath = assetPath;
            LineNumber = lineNumber;
            OldLine = oldLine;
            NewLine = newLine;
        }

        public bool IsValid => LineNumber >= 0 && !string.IsNullOrEmpty(AssetPath);
    }

    /// <summary>
    /// A serialized field that opts into the <c>[TypeSelector(Required = true)]</c> check, captured for the pure-YAML
    /// scene scan: the YAML field key and whether it is a <c>string</c> type field (vs a <c>[SerializeReference]</c>
    /// managed reference). Produced by reflection in <see cref="SerializeReferenceRequiredGate.GetRequiredFields"/> and
    /// consumed by <see cref="SerializeReferenceYamlEditor.FindUnsetRequiredFields"/>, which stays reflection-free.
    /// </summary>
    internal readonly struct RequiredFieldDescriptor
    {
        public readonly string FieldName;
        public readonly bool IsString;

        public RequiredFieldDescriptor(string fieldName, bool isString)
        {
            FieldName = fieldName;
            IsString = isString;
        }
    }

    /// <summary>
    /// One unset required field found by the pure-YAML scene scan: the owning object document (<see cref="FileId"/>),
    /// the field's YAML key and — for a managed reference — the null id it read (<c>-2</c>); <c>0</c> for a string field.
    /// </summary>
    internal readonly struct RequiredViolationEntry
    {
        public readonly long FileId;
        public readonly string FieldName;
        public readonly long Rid;

        public RequiredViolationEntry(long fileId, string fieldName, long rid)
        {
            FileId = fileId;
            FieldName = fieldName;
            Rid = rid;
        }
    }

    internal static class SerializeReferenceYamlEditor
    {
        // "--- !u!114 &11400000" — object document header carrying the local file id as its YAML anchor.
        private static readonly Regex DocumentHeader = new(@"^--- !u!\d+ &(\d+)", RegexOptions.Compiled);

        // "--- !u!114 &11400000" — a MonoBehaviour document header (class id 114), the only kind that carries m_Script
        // and serialized user fields, so the scene required-field scan iterates these alone.
        private static readonly Regex MonoBehaviourHeader = new(@"^--- !u!114 &(\d+)", RegexOptions.Compiled);

        // "  m_Script: {fileID: 11500000, guid: <guid>, type: 3}" — the script reference whose guid maps to the C# type;
        // its indent is the document's top-level field indent (every direct field of the MonoBehaviour aligns with it).
        private static readonly Regex ScriptGuidPattern =
            new(@"^(?<indent>\s*)m_Script:\s*\{.*\bguid:\s*(?<guid>[0-9a-fA-F]+).*\}", RegexOptions.Compiled);

        // "  references:" — the managed-reference block; the object's own serialized fields all precede it.
        private static readonly Regex ReferencesKey = new(@"^\s*references:\s*$", RegexOptions.Compiled);

        /// <summary>
        /// Scans every object document in the asset and returns each <c>RefIds</c> entry whose stored type fails
        /// the <paramref name="resolves"/> predicate. Because <c>RefIds</c> is a flat per-object list, this finds
        /// missing references at any nesting depth and on any child object — without navigating the Inspector.
        /// </summary>
        public static List<MissingReferenceEntry> FindMissingReferences(string assetPath, Func<ManagedTypeName, bool> resolves)
        {
            var result = new List<MissingReferenceEntry>();

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return result;

                var lines = File.ReadAllLines(assetPath);

                var headers = new List<(long fileId, int start)>();
                for (var i = 0; i < lines.Length; i++)
                {
                    var match = DocumentHeader.Match(lines[i]);
                    if (match.Success && long.TryParse(match.Groups[1].Value, out var fileId))
                        headers.Add((fileId, i));
                }

                var ridPattern = new Regex(@"^\s*-\s+rid:\s*(-?\d+)\s*$");
                var typePattern = new Regex(@"^\s*type:\s*\{(?<body>.*)\}\s*$");

                for (var h = 0; h < headers.Count; h++)
                {
                    var (fileId, start) = headers[h];
                    var end = h + 1 < headers.Count ? headers[h + 1].start : lines.Length;

                    var refIdsStart = FindRefIdsStart(lines, start, end);
                    if (refIdsStart < 0) continue;

                    for (var i = refIdsStart + 1; i < end; i++)
                    {
                        var ridMatch = ridPattern.Match(lines[i]);
                        if (!ridMatch.Success || !long.TryParse(ridMatch.Groups[1].Value, out var rid)) continue;

                        for (var j = i + 1; j < end && j <= i + 4; j++)
                        {
                            var typeMatch = typePattern.Match(lines[j]);
                            if (!typeMatch.Success) continue;

                            if (TryParseInlineType(typeMatch.Groups["body"].Value, out var type) &&
                                !type.IsEmpty && !resolves(type))
                            {
                                result.Add(new MissingReferenceEntry(fileId, rid, type));
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Best effort — a parse failure simply yields no repair candidates.
            }

            return result;
        }

        /// <summary>
        /// Pure-YAML scan for unset <c>[TypeSelector(Required = true)]</c> fields — the scene-safe counterpart of the
        /// object-load required check, which cannot read scene objects (<see cref="SerializeReferenceHelpers.IsScene"/>).
        /// Walks every MonoBehaviour document, resolves its required fields through <paramref name="requiredFieldsForScript"/>
        /// (keyed by the <c>m_Script</c> guid, so this method stays reflection-free), and reports each top-level required
        /// field left unset: a managed reference at the null id (<c>-2</c>) or absent, or an empty/absent string field.
        /// A present managed reference (<c>rid &gt;= 0</c>) counts as set even when its type is missing — that mirrors
        /// <see cref="SerializeReferenceRequiredGate.IsViolation"/>, where a missing type is the missing-type gate's
        /// concern, not a required violation. Required fields nested inside serializable containers are not covered here
        /// (the field keys are read at the document's top level only).
        /// </summary>
        public static List<RequiredViolationEntry> FindUnsetRequiredFields(
            string assetPath, Func<string, IReadOnlyList<RequiredFieldDescriptor>> requiredFieldsForScript)
        {
            var result = new List<RequiredViolationEntry>();
            if (requiredFieldsForScript is null) return result;

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return result;

                // One-shot bulk read like FindMissingReferences — bypass the probe cache so large scene files don't evict
                // the interactive per-property entries (see SerializeReferenceYamlProbeCache remarks).
                var lines = File.ReadAllLines(assetPath);

                for (var i = 0; i < lines.Length; i++)
                {
                    var header = MonoBehaviourHeader.Match(lines[i]);
                    if (!header.Success || !long.TryParse(header.Groups[1].Value, out var fileId)) continue;

                    var docEnd = NextDocumentStart(lines, i + 1);
                    if (!TryReadScriptGuid(lines, i + 1, docEnd, out var guid, out var fieldIndent)) continue;

                    var required = requiredFieldsForScript(guid);
                    if (required is null || required.Count == 0) continue;

                    var fieldsEnd = FindFieldsEnd(lines, i + 1, docEnd);

                    foreach (var descriptor in required)
                    {
                        if (descriptor.IsString)
                        {
                            if (IsStringFieldUnset(lines, i + 1, fieldsEnd, fieldIndent, descriptor.FieldName))
                                result.Add(new RequiredViolationEntry(fileId, descriptor.FieldName, 0));
                        }
                        else if (IsManagedReferenceUnset(lines, i + 1, fieldsEnd, fieldIndent, descriptor.FieldName, out var rid))
                        {
                            result.Add(new RequiredViolationEntry(fileId, descriptor.FieldName, rid));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Best effort — a parse failure simply yields no violations (matches FindMissingReferences).
            }

            return result;
        }

        // The line index of the next "--- " document separator at or after `from`, or the line count when none follows.
        private static int NextDocumentStart(string[] lines, int from)
        {
            for (var i = from; i < lines.Length; i++)
                if (lines[i].StartsWith("--- ", StringComparison.Ordinal))
                    return i;

            return lines.Length;
        }

        // Reads the m_Script guid (and the document's top-level field indent) within [start, end). False for a stripped
        // component or any document carrying no script reference.
        private static bool TryReadScriptGuid(string[] lines, int start, int end, out string guid, out int fieldIndent)
        {
            guid = null;
            fieldIndent = 0;

            for (var i = start; i < end; i++)
            {
                var match = ScriptGuidPattern.Match(lines[i]);
                if (!match.Success) continue;

                guid = match.Groups["guid"].Value;
                fieldIndent = match.Groups["indent"].Length;
                return true;
            }

            return false;
        }

        // The exclusive end of a MonoBehaviour's own serialized fields: the "references:" line, or the document end when
        // the object holds no managed references. Confines field lookups so a key inside a RefIds data block is never read.
        private static int FindFieldsEnd(string[] lines, int start, int end)
        {
            for (var i = start; i < end; i++)
                if (ReferencesKey.IsMatch(lines[i]))
                    return i;

            return end;
        }

        // True when the top-level managed-reference field `name` is unset: its pointer is the null id (-2), or the field
        // is absent / carries no rid. A present rid (>= 0) is set. `rid` returns the read id (or -2 when unset/absent).
        private static bool IsManagedReferenceUnset(string[] lines, int start, int end, int fieldIndent, string name, out long rid)
        {
            rid = NullRid;
            var pattern = new Regex($@"^(?<indent>\s*){Regex.Escape(name)}:\s*(?<inline>.*)$");

            for (var i = start; i < end; i++)
            {
                var field = pattern.Match(lines[i]);
                if (!field.Success || field.Groups["indent"].Length != fieldIndent) continue;

                // Inline pointer (e.g. "_weapon: {rid: 5}") — Unity writes the block form, but tolerate either.
                var inline = Regex.Match(field.Groups["inline"].Value, @"rid:\s*(-?\d+)");
                if (inline.Success && long.TryParse(inline.Groups[1].Value, out var inlineRid))
                {
                    rid = inlineRid;
                    return inlineRid == NullRid;
                }

                // Block form: the rid scalar is the field's first indented child.
                for (var j = i + 1; j < end; j++)
                {
                    if (lines[j].Trim().Length == 0) continue;
                    if (IndentOf(lines[j]) <= fieldIndent) break; // dedented out of the field without a rid

                    var child = Regex.Match(lines[j].Trim(), @"^rid:\s*(-?\d+)$");
                    if (child.Success && long.TryParse(child.Groups[1].Value, out var childRid))
                    {
                        rid = childRid;
                        return childRid == NullRid;
                    }

                    break; // first child is not a rid scalar — not a recognised managed-reference pointer
                }

                return true; // field present but no rid — treat as unset
            }

            return true; // field absent — unset (the issue's "absent = unset")
        }

        // True when the top-level string field `name` is unset: empty, an empty quoted scalar ('' / ""), or absent.
        private static bool IsStringFieldUnset(string[] lines, int start, int end, int fieldIndent, string name)
        {
            var pattern = new Regex($@"^(?<indent>\s*){Regex.Escape(name)}:\s*(?<value>.*)$");

            for (var i = start; i < end; i++)
            {
                var field = pattern.Match(lines[i]);
                if (!field.Success || field.Groups["indent"].Length != fieldIndent) continue;

                var value = field.Groups["value"].Value.Trim();
                return value.Length == 0 || value == "''" || value == "\"\"";
            }

            return true; // field absent — unset
        }

        /// <summary>
        /// Replaces the <c>type:</c> mapping of the <c>RefIds</c> entry identified by <paramref name="rid"/> within
        /// the object document anchored at <paramref name="fileId"/>. Returns <see langword="true"/> when the file
        /// was rewritten; the caller is responsible for reimporting the asset.
        /// </summary>
        public static bool TryRewriteType(string assetPath, long fileId, long rid, ManagedTypeName newType)
        {
            // Single scan shared with the diff preview: compute the edit, then apply exactly that line so the preview
            // and the applied result can never diverge.
            if (!TryComputeRewrite(assetPath, fileId, rid, newType, out var edit)) return false;

            try
            {
                var lines = File.ReadAllLines(assetPath);
                if (edit.LineNumber < 0 || edit.LineNumber >= lines.Length || lines[edit.LineNumber] != edit.OldLine)
                    return false; // the file changed since the edit was computed — abort rather than write a stale line

                lines[edit.LineNumber] = edit.NewLine;
                WritePreservingNewlines(assetPath, lines);
                // Same-tick writes can leave the modification-time key unchanged, so bust the probe cache explicitly.
                SerializeReferenceYamlProbeCache.ClearCache();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[TypeSelector] Failed to rewrite managed-reference type in '{assetPath}': {exception}");
                return false;
            }
        }

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
            var newline = original.IndexOf("\r\n", StringComparison.Ordinal) >= 0 ? "\r\n" : "\n";

            var builder = new StringBuilder(original.Length);
            for (var i = 0; i < lines.Count; i++)
            {
                builder.Append(lines[i]);
                if (i < lines.Count - 1) builder.Append(newline);
            }

            // Re-add the trailing terminator only if the source had one (Unity assets always do).
            if (original.Length > 0 && original[original.Length - 1] == '\n') builder.Append(newline);

            File.WriteAllText(assetPath, builder.ToString());
        }

        /// <summary>
        /// Computes — without writing — the single line change a <see cref="TryRewriteType"/> would make to re-point the
        /// <paramref name="rid"/> entry to <paramref name="newType"/>. Drives the bulk-fix diff preview; the rewrite
        /// applies the returned edit verbatim, so what the preview shows is exactly what is written.
        /// </summary>
        public static bool TryComputeRewrite(string assetPath, long fileId, long rid, ManagedTypeName newType, out RewriteEdit edit)
        {
            edit = default;

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return false;

                var lines = File.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                // Field pointers ("_sidearms:\n  - rid: 1002") share the "- rid:" shape with RefIds entries, so confine
                // the search to the RefIds block — the entries are the only ones with a following type:.
                var refIdsStart = FindRefIdsStart(lines, start, end);
                if (refIdsStart < 0) return false;

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

                        edit = new RewriteEdit(assetPath, j, lines[j], match.Groups["indent"].Value + newType.ToYamlType());
                        return true;
                    }

                    return false;
                }

                return false;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[TypeSelector] Failed to compute managed-reference rewrite in '{assetPath}': {exception}");
                return false;
            }
        }

        /// <summary>
        /// Deletes a whole <c>- rid: N</c> entry (its type and data block) from the <c>RefIds</c> list of the document
        /// anchored at <paramref name="fileId"/>. Used to drop an orphaned managed-reference payload that no field points
        /// at. Confined to the <c>RefIds</c> block so a same-shaped field pointer is never touched. Returns whether an
        /// entry was removed. The edit is not undoable — callers confirm first.
        /// </summary>
        public static bool TryRemoveEntry(string assetPath, long fileId, long rid)
        {
            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return false;

                var lines = File.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                var refIdsStart = FindRefIdsStart(lines, start, end);
                if (refIdsStart < 0) return false;

                var ridPattern = new Regex($@"^(?<indent>\s*)-\s+rid:\s*{rid}\s*$");

                for (var i = refIdsStart; i < end; i++)
                {
                    var match = ridPattern.Match(lines[i]);
                    if (!match.Success) continue;

                    // The entry runs until the next list item at its own indent, or until the block dedents out of it —
                    // the same bounding rule the data-block reader uses.
                    var entryIndent = match.Groups["indent"].Length;
                    var entryEnd = FindEntryEnd(lines, i, end, entryIndent);

                    var remaining = new List<string>(lines.Length - (entryEnd - i));
                    for (var k = 0; k < i; k++) remaining.Add(lines[k]);
                    for (var k = entryEnd; k < lines.Length; k++) remaining.Add(lines[k]);

                    WritePreservingNewlines(assetPath, remaining);
                    SerializeReferenceYamlProbeCache.ClearCache();
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Aspid FastTools] Failed to remove RefIds entry rid {rid} in '{assetPath}': {exception}");
                return false;
            }
        }

        // The null managed-reference id Unity stores for an unassigned [SerializeReference] field
        // (UnityEngine.Serialization.ManagedReferenceUtility.RefIdNull).
        private const long NullRid = -2;

        // The inline type mapping Unity writes for the null sentinel RefIds entry — an empty type identity.
        private const string NullSentinelType = "type: {class: , ns: , asm: }";

        /// <summary>
        /// Nulls a managed reference in the document anchored at <paramref name="fileId"/>: every field / array-element
        /// pointer that holds <paramref name="rid"/> is rewritten to the null id (<c>-2</c>), the now-orphaned
        /// <c>RefIds</c> entry is removed, and — when a null pointer was introduced — the <c>RefIds</c> null sentinel
        /// entry (<c>- rid: -2 / type: {class: , ns: , asm: }</c>) is added if absent. This reproduces exactly what Unity
        /// writes when a <c>[SerializeReference]</c> field is set to <see langword="null"/>: an array element cannot be
        /// dropped, so it must point at <c>-2</c>, and that pointer is only valid when the sentinel entry exists —
        /// without it the load errors "serialized array … is missing entry for Refid -2". Removing the broken entry
        /// clears the object's missing-types flag. Not undoable: the broken payload is discarded. Returns whether the
        /// file was rewritten.
        /// </summary>
        public static bool TryNullReference(string assetPath, long fileId, long rid)
        {
            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return false;

                var lines = File.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                var refIdsStart = FindRefIdsStart(lines, start, end);
                if (refIdsStart < 0) return false;

                // RefIds entry headers sit at the shallowest "- rid:" indent under RefIds; a pointer to the rid lives
                // anywhere else — a field/array element before "references:" or a nested reference inside another entry's
                // data block. The header for this rid is removed; every pointer to it becomes the null id.
                var entryIndent = FindRefIdsEntryIndent(lines, refIdsStart, end);
                if (entryIndent < 0) return false;

                var headerPattern = new Regex($@"^(?<indent>\s*)-\s+rid:\s*{rid}\s*$");
                var pointerToken = new Regex($@"\brid:\s*{rid}\b");

                var headerIndex = -1;
                var pointerNulled = false;

                for (var i = start; i < end; i++)
                {
                    // This rid's own RefIds entry header (a "- rid: N" under RefIds at the entry indent) is removed
                    // below, not nulled — skip it so it isn't rewritten to the null id.
                    if (headerIndex < 0 && i > refIdsStart)
                    {
                        var header = headerPattern.Match(lines[i]);
                        if (header.Success && header.Groups["indent"].Length == entryIndent)
                        {
                            headerIndex = i;
                            continue;
                        }
                    }

                    // Null every pointer to the rid — a "- rid: N" array element, a "rid: N" scalar field or an inline
                    // "{rid: N}" — so no dangling pointer survives the entry's removal (which errors on array fields).
                    if (pointerToken.IsMatch(lines[i]))
                    {
                        lines[i] = pointerToken.Replace(lines[i], $"rid: {NullRid}");
                        pointerNulled = true;
                    }
                }

                // Nothing referenced or stored this rid — leave the file untouched. (When an entry exists but is already
                // unreferenced this still drops it; when only a dangling pointer remains this still nulls it.)
                if (headerIndex < 0 && !pointerNulled) return false;

                var blockStart = headerIndex;
                var blockEnd = headerIndex >= 0 ? FindEntryEnd(lines, headerIndex, end, entryIndent) : -1;

                // A "- rid: -2" pointer is valid only while the RefIds list carries Unity's null sentinel entry; add it
                // when we just introduced a null pointer and the document does not already have one (a shared singleton).
                var needsNullEntry = pointerNulled && !HasNullSentinelEntry(lines, refIdsStart, end, entryIndent);
                var dash = new string(' ', entryIndent);
                var typeIndent = new string(' ', entryIndent + 2);

                var result = new List<string>(lines.Length + 2);
                for (var i = 0; i < lines.Length; i++)
                {
                    if (headerIndex >= 0 && i >= blockStart && i < blockEnd) continue; // drop the broken entry block

                    result.Add(lines[i]);

                    // Insert the sentinel as the RefIds list's first entry, mirroring where Unity writes it.
                    if (needsNullEntry && i == refIdsStart)
                    {
                        result.Add($"{dash}- rid: {NullRid}");
                        result.Add($"{typeIndent}{NullSentinelType}");
                    }
                }

                WritePreservingNewlines(assetPath, result);
                SerializeReferenceYamlProbeCache.ClearCache();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Aspid FastTools] Failed to null managed-reference rid {rid} in '{assetPath}': {exception}");
                return false;
            }
        }

        // Whether the RefIds list already carries Unity's null sentinel entry ("- rid: -2"). The sentinel is a shared
        // singleton — at most one per object — so a second null pointer reuses it rather than adding another.
        private static bool HasNullSentinelEntry(string[] lines, int refIdsStart, int end, int entryIndent)
        {
            var sentinel = new Regex($@"^(?<indent>\s*)-\s+rid:\s*{NullRid}\s*$");
            for (var i = refIdsStart + 1; i < end; i++)
            {
                var match = sentinel.Match(lines[i]);
                if (match.Success && match.Groups["indent"].Length == entryIndent) return true;
            }

            return false;
        }

        // The exclusive end line of a RefIds entry that begins at headerIndex: the entry runs until the next list item at
        // its own indent, or until the block dedents out of it (blank lines are spanned). Shared by the entry removers.
        private static int FindEntryEnd(string[] lines, int headerIndex, int end, int entryIndent)
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

        // The indent of the RefIds list's entry headers: the first "- rid:" line under RefIds. Entries sit at this
        // shallowest dash indent; nested reference pointers inside their data blocks are deeper. -1 when the block is empty.
        private static int FindRefIdsEntryIndent(string[] lines, int refIdsStart, int end)
        {
            var entry = new Regex(@"^(?<indent>\s*)-\s+rid:\s*-?\d+\s*$");
            for (var i = refIdsStart + 1; i < end; i++)
            {
                var match = entry.Match(lines[i]);
                if (match.Success) return match.Groups["indent"].Length;
            }

            return -1;
        }

        /// <summary>
        /// Reads the managed-reference id (<c>rid</c>) stored at <paramref name="propertyPath"/> within the object
        /// document anchored at <paramref name="fileId"/>. Needed because Unity reports an invalid id for a property
        /// whose type is missing — the real id only survives in the YAML. Resolves the path at any depth, walking each
        /// segment either into a managed reference's <c>RefIds</c> data block (a <c>rid:</c> pointer) or down through a
        /// plain serializable container (a nested struct/class mapping or a <c>List&lt;T&gt;</c> of them), so paths
        /// such as <c>_weapon._chargeEffect</c>, <c>_config._weapon</c> and <c>_slots.Array.data[0]._weapon</c> all
        /// resolve. An unresolvable segment returns <see langword="false"/> so the caller can fall back.
        /// </summary>
        public static bool TryReadReferenceId(string assetPath, long fileId, string propertyPath, out long rid)
        {
            rid = 0;
            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return false;

                var lines = SerializeReferenceYamlProbeCache.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return false;

                // Field pointers (and the object's inline serializable data) live before the "references:" block; the
                // RefIds entries and the nested data each managed reference stores live after it.
                var fieldsEnd = end;
                var references = new Regex(@"^\s*references:\s*$");
                for (var i = start; i < end; i++)
                    if (references.IsMatch(lines[i])) { fieldsEnd = i; break; }

                var segments = ParsePathSegments(propertyPath.Replace(".Array.data", string.Empty));
                if (segments is null) return false;

                var refIdsStart = FindRefIdsStart(lines, start, end);

                // Cursor over the lines the current segment is resolved against. It starts on the object's own field
                // block, then for each segment either descends into a plain serializable container (a nested mapping
                // or sequence item, by indent) or jumps into a managed reference's RefIds data block (by rid).
                var cursorStart = start;
                var cursorEnd = fieldsEnd;
                var cursorIndent = -1; // the object's top-level fields: match at any indent

                for (var s = 0; s < segments.Count; s++)
                {
                    var kind = ResolveSegment(lines, cursorStart, cursorEnd, cursorIndent, segments[s],
                        out var segmentRid, out var valueStart, out var valueEnd, out var valueIndent);

                    if (kind == SegmentKind.NotFound) return false;

                    if (s == segments.Count - 1)
                    {
                        if (kind != SegmentKind.Reference) return false;
                        rid = segmentRid;
                        return true;
                    }

                    if (kind == SegmentKind.Reference)
                    {
                        if (refIdsStart < 0) return false;
                        if (!TryGetDataBlockRange(lines, refIdsStart, end, segmentRid, out cursorStart, out cursorEnd, out cursorIndent))
                            return false;
                    }
                    else
                    {
                        cursorStart = valueStart;
                        cursorEnd = valueEnd;
                        cursorIndent = valueIndent;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // A single managed-reference path segment: a field name with an optional sequence index ("_alternates[3]").
        private readonly struct PathSegment
        {
            public readonly string Name;
            public readonly bool HasIndex;
            public readonly int Index;

            public PathSegment(string name, bool hasIndex, int index)
            {
                Name = name;
                HasIndex = hasIndex;
                Index = index;
            }
        }

        // Splits a normalised property path ("_weapon._chargeEffect", "_alternates[3]") into ordered segments.
        private static List<PathSegment> ParsePathSegments(string path)
        {
            var result = new List<PathSegment>();
            foreach (var raw in path.Split('.'))
            {
                var match = Regex.Match(raw, @"^(?<name>[^\[\]\.]+)(\[(?<idx>\d+)\])?$");
                if (!match.Success) return null;

                var hasIndex = match.Groups["idx"].Success;
                result.Add(new PathSegment(match.Groups["name"].Value, hasIndex, hasIndex ? int.Parse(match.Groups["idx"].Value) : -1));
            }

            return result.Count > 0 ? result : null;
        }

        // What a resolved segment points at: a managed reference (a "rid:" pointer) or a plain serializable container
        // (a nested mapping/sequence to descend into), or nothing.
        private enum SegmentKind { NotFound, Reference, Container }

        // Resolves one path segment within [rangeStart, rangeEnd). For a plain field the value is either a managed
        // reference (its sole child is a "rid:" scalar) or a container to descend into; for an indexed field it is the
        // segment.Index-th sequence item, itself either a "- rid:" reference or a "- field:" mapping container. When
        // requiredIndent is non-negative only a field at exactly that effective indent matches (a leading "- " counts
        // toward the indent so a sequence-of-mappings item's fields align with their dashless siblings), which keeps
        // resolution on a block's direct children instead of descending into a deeper object that reuses the name.
        private static SegmentKind ResolveSegment(string[] lines, int rangeStart, int rangeEnd, int requiredIndent,
            PathSegment segment, out long rid, out int valueStart, out int valueEnd, out int valueIndent)
        {
            rid = 0;
            valueStart = valueEnd = valueIndent = -1;

            var fieldPattern = new Regex($@"^(?<lead>\s*)(?<dash>-\s+)?{Regex.Escape(segment.Name)}:\s*(?<inline>.*)$");

            for (var i = rangeStart; i < rangeEnd; i++)
            {
                var field = fieldPattern.Match(lines[i]);
                if (!field.Success) continue;

                var fieldIndent = field.Groups["lead"].Length + field.Groups["dash"].Length;
                if (requiredIndent >= 0 && fieldIndent != requiredIndent) continue;

                return segment.HasIndex
                    ? ResolveSequenceItem(lines, i + 1, rangeEnd, segment.Index, out rid, out valueStart, out valueEnd, out valueIndent)
                    : ClassifyValue(lines, i, fieldIndent, field.Groups["inline"].Value, rangeEnd, out rid, out valueStart, out valueEnd, out valueIndent);
            }

            return SegmentKind.NotFound;
        }

        // Classifies the value of a plain (non-indexed) field at line i with effective indent fieldIndent: a managed
        // reference when the value is a lone "rid:" scalar (inline or as the only following child), otherwise the
        // indented mapping block to descend into.
        private static SegmentKind ClassifyValue(string[] lines, int i, int fieldIndent, string inline, int rangeEnd,
            out long rid, out int valueStart, out int valueEnd, out int valueIndent)
        {
            rid = 0;
            valueStart = valueEnd = valueIndent = -1;

            var inlineMatch = Regex.Match(inline, @"rid:\s*(-?\d+)");
            if (inlineMatch.Success)
                return long.TryParse(inlineMatch.Groups[1].Value, out rid) ? SegmentKind.Reference : SegmentKind.NotFound;

            // Gather the indented value block (lines more indented than the field).
            var blockStart = i + 1;
            var blockEnd = rangeEnd;
            var firstChild = -1;
            for (var j = blockStart; j < rangeEnd; j++)
            {
                if (lines[j].Trim().Length == 0) continue;
                if (IndentOf(lines[j]) <= fieldIndent) { blockEnd = j; break; }
                if (firstChild < 0) firstChild = j;
            }

            if (firstChild < 0) return SegmentKind.NotFound; // scalar/empty field: no managed reference here

            // A managed reference's value block is exactly a "rid:" scalar; anything else is a container.
            var ridScalar = Regex.Match(lines[firstChild].Trim(), @"^rid:\s*(-?\d+)$");
            if (ridScalar.Success)
                return long.TryParse(ridScalar.Groups[1].Value, out rid) ? SegmentKind.Reference : SegmentKind.NotFound;

            valueStart = blockStart;
            valueEnd = blockEnd;
            valueIndent = IndentOf(lines[firstChild]);
            return SegmentKind.Container;
        }

        // Locates the index-th "- " item of a sequence whose items begin at [itemsStart, rangeEnd). A "- rid: N" item
        // is a managed reference; a "- field: …" item is a mapping container whose fields begin on the dash line.
        private static SegmentKind ResolveSequenceItem(string[] lines, int itemsStart, int rangeEnd, int index,
            out long rid, out int valueStart, out int valueEnd, out int valueIndent)
        {
            rid = 0;
            valueStart = valueEnd = valueIndent = -1;

            var itemPattern = new Regex(@"^(?<lead>\s*)-\s");
            var itemIndent = -1;
            var count = 0;

            for (var j = itemsStart; j < rangeEnd; j++)
            {
                if (lines[j].Trim().Length == 0) continue;

                var item = itemPattern.Match(lines[j]);
                if (!item.Success)
                {
                    if (itemIndent >= 0 && IndentOf(lines[j]) <= itemIndent) break; // dedented out of the sequence
                    continue;
                }

                var indent = item.Groups["lead"].Length;
                if (itemIndent < 0) itemIndent = indent;
                else if (indent < itemIndent) break;    // dedented out of the sequence
                else if (indent > itemIndent) continue; // item of a nested sequence, not ours

                if (count == index)
                {
                    var ridMatch = Regex.Match(lines[j].TrimStart(), @"^-\s+rid:\s*(-?\d+)\s*$");
                    if (ridMatch.Success)
                        return long.TryParse(ridMatch.Groups[1].Value, out rid) ? SegmentKind.Reference : SegmentKind.NotFound;

                    // Mapping item: it runs until the next sibling "- " or a dedent; its fields start one "- " past
                    // the item indent.
                    var itemEnd = rangeEnd;
                    for (var k = j + 1; k < rangeEnd; k++)
                    {
                        if (lines[k].Trim().Length == 0) continue;
                        var ind = IndentOf(lines[k]);
                        if (ind < itemIndent || (ind == itemIndent && itemPattern.IsMatch(lines[k]))) { itemEnd = k; break; }
                    }

                    valueStart = j;
                    valueEnd = itemEnd;
                    valueIndent = itemIndent + 2;
                    return SegmentKind.Container;
                }

                count++;
            }

            return SegmentKind.NotFound;
        }

        // Locates the RefIds entry for rid and returns the line range of its "data:" block plus the indent of that
        // block's direct children, so a nested segment can be resolved within the right scope.
        private static bool TryGetDataBlockRange(string[] lines, int refIdsStart, int docEnd, long rid, out int blockStart, out int blockEnd, out int childIndent)
        {
            blockStart = blockEnd = childIndent = -1;
            var ridPattern = new Regex($@"^(?<indent>\s*)-\s+rid:\s*{rid}\s*$");
            var dataPattern = new Regex(@"^\s*data:\s*$");

            for (var i = refIdsStart; i < docEnd; i++)
            {
                var match = ridPattern.Match(lines[i]);
                if (!match.Success) continue;

                // The entry runs until the next list item at its own indent, or until the block dedents out of it.
                var entryIndent = match.Groups["indent"].Length;
                var entryEnd = docEnd;
                for (var j = i + 1; j < docEnd; j++)
                {
                    if (lines[j].Trim().Length == 0) continue;
                    var indent = IndentOf(lines[j]);
                    if (indent < entryIndent || (indent == entryIndent && lines[j].TrimStart().StartsWith("- "))) { entryEnd = j; break; }
                }

                for (var j = i + 1; j < entryEnd; j++)
                {
                    if (!dataPattern.IsMatch(lines[j])) continue;

                    blockStart = j + 1;
                    blockEnd = entryEnd;
                    for (var k = blockStart; k < blockEnd; k++)
                        if (lines[k].Trim().Length > 0) { childIndent = IndentOf(lines[k]); break; }

                    return blockStart < blockEnd && childIndent >= 0;
                }

                return false;
            }

            return false;
        }

        private static int IndentOf(string line)
        {
            var count = 0;
            while (count < line.Length && line[count] == ' ') count++;
            return count;
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
                var lines = SerializeReferenceYamlProbeCache.ReadAllLines(assetPath);
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

        /// <summary>
        /// Reads the top-level serialized field names recorded for the managed reference <paramref name="rid"/> within
        /// the object document anchored at <paramref name="fileId"/> — the direct keys of the entry's <c>data:</c>
        /// block. Used by the Smart Fix suggestion's field-shape heuristic to compare the orphaned payload's shape
        /// against a candidate type's serialized fields. Nested mappings and sequences are reported by their key only.
        /// </summary>
        public static List<string> GetReferenceFieldNames(string assetPath, long fileId, long rid)
        {
            var result = new List<string>();

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return result;

                var lines = SerializeReferenceYamlProbeCache.ReadAllLines(assetPath);
                var (start, end) = FindDocumentRange(lines, fileId);
                if (start < 0) return result;

                var refIdsStart = FindRefIdsStart(lines, start, end);
                if (refIdsStart < 0) return result;

                if (!TryGetDataBlockRange(lines, refIdsStart, end, rid, out var blockStart, out var blockEnd, out var childIndent))
                    return result;

                CollectTopLevelKeys(lines, blockStart, blockEnd, childIndent, result);
            }
            catch (Exception)
            {
                // Best effort — an unreadable block simply yields no field-shape signal.
            }

            return result;
        }

        // Collects the keys of "name: …" entries at exactly childIndent within [blockStart, blockEnd), skipping the
        // deeper lines of nested mappings/sequences so only the block's own top-level fields are reported.
        private static void CollectTopLevelKeys(string[] lines, int blockStart, int blockEnd, int childIndent, List<string> result)
        {
            var keyPattern = new Regex(@"^(?<indent>\s*)(?<key>[^\s:][^:]*):(\s.*|\s*)$");

            for (var i = blockStart; i < blockEnd; i++)
            {
                if (lines[i].Trim().Length == 0) continue;
                if (IndentOf(lines[i]) != childIndent) continue; // a nested line, not a direct field of this block

                var match = keyPattern.Match(lines[i]);
                if (!match.Success) continue;
                if (match.Groups["indent"].Length != childIndent) continue;

                var key = match.Groups["key"].Value.Trim();
                if (key.Length > 0 && key != "-") result.Add(key);
            }
        }

        /// <summary>
        /// Parses the top-level field names from the flat YAML payload Unity exposes for an in-memory missing reference
        /// (<see cref="UnityEditor.SerializationUtility.GetManagedReferencesWithMissingTypes"/>
        /// <c>serializedData</c>). Mirrors the in-memory data-recovery parser: only top-level <c>key: value</c> scalars
        /// (and mapping/sequence headers) are reported; indented and sequence-item lines are skipped.
        /// </summary>
        public static List<string> ParseTopLevelFieldNames(string serializedData)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(serializedData)) return result;

            foreach (var raw in serializedData.Split('\n'))
            {
                var line = raw.TrimEnd('\r');
                if (line.Length == 0 || char.IsWhiteSpace(line[0]) || line[0] == '-') continue;

                var separator = line.IndexOf(':');
                if (separator <= 0) continue;

                var key = line[..separator].Trim();
                if (key.Length > 0) result.Add(key);
            }

            return result;
        }
    }
}
