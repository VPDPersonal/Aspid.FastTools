using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static partial class SerializeReferenceYamlEditor
    {
        // The backing field of every serializable type wrapper (SerializableTypeBase). The scan finds a wrapper by this
        // key alone, so a user field of the same name is read as a wrapper too.
        public const string TypeNameKey = "_assemblyQualifiedName";

        // The editor-only script reference of a SerializableMonoScript.
        private const string TypeNameScriptKey = "_script";

        private const string TypeNameOverrideSuffix = "." + TypeNameKey;
        private const string ScriptOverrideSuffix = "." + TypeNameScriptKey;

        // "  key: value", "  - key: value", "  key:" — a block mapping key with an optional sequence dash. A folded
        // scalar's continuation line has no "key: " shape, as an assembly-qualified name holds no ": ".
        private static readonly Regex _blockKey = new(
            @"^(?<indent> *)(?<dash>- +)?(?<key>[A-Za-z_][^:\s]*):(?: +(?<value>.*?))? *$", RegexOptions.Compiled);

        // "  - {fileID: 0}", "  - 5", "  -": a sequence item that is not a mapping.
        private static readonly Regex _sequenceItem = new(@"^(?<indent> *)-(?: +.*)?$", RegexOptions.Compiled);

        private static readonly Regex _referenceFileId = new(@"\bfileID:\s*(?<id>-?\d+)", RegexOptions.Compiled);
        private static readonly Regex _referenceGuid = new(@"\bguid:\s*(?<guid>[0-9a-fA-F]+)", RegexOptions.Compiled);

        // A cheap probe a sweep may run before FindStoredTypeNames: a file with no such line stores no type name.
        public static bool MayHoldTypeNames(string line) =>
            line.IndexOf(TypeNameKey, StringComparison.Ordinal) >= 0;

        // MayHoldTypeNames for ReadLinesIfContainsAny, which searches the file as bytes.
        public static readonly byte[][] TypeNameMarkers = { Encoding.ASCII.GetBytes(TypeNameKey) };

        public static List<StoredTypeNameEntry> FindStoredTypeNames(string assetPath, bool knownTextYaml = false) =>
            FindStoredTypeNames(SerializeReferenceYaml.ReadLines(assetPath, knownTextYaml));

        // Every stored type name of every document: in the fields of a MonoBehaviour or ScriptableObject, in the data of
        // its managed references, and in the modifications of a PrefabInstance. An empty name is listed as well.
        public static List<StoredTypeNameEntry> FindStoredTypeNames(string[] lines)
        {
            var result = new List<StoredTypeNameEntry>();
            if (lines is null) return result;

            try
            {
                for (var i = 0; i < lines.Length; i++)
                {
                    var header = DocumentHeader.Match(lines[i]);
                    if (!header.Success || !long.TryParse(header.Groups["id"].Value, out var fileId)) continue;

                    var end = NextDocumentStart(lines, i + 1);
                    if (HoldsTypeName(lines, i + 1, end))
                    {
                        if (header.Groups["class"].Value == PrefabInstanceClassId) CollectOverrideTypeNames(lines, i + 1, end, fileId, result);
                        else CollectDocumentTypeNames(lines, i + 1, end, fileId, result);
                    }

                    i = end - 1;
                }
            }
            catch (Exception)
            {
                // Best effort, like FindMissingReferences: a parse failure yields what was read so far.
            }

            return result;
        }

        // Replaces the names of the edited fields in one write. An edit applies only while its field still stores the
        // name the edit expects. Returns how many edits were applied; the caller reimports the asset.
        public static int RewriteTypeNames(string assetPath, IReadOnlyList<TypeNameEdit> edits)
        {
            if (edits is null || edits.Count == 0) return 0;

            try
            {
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return 0;

                var lines = File.ReadAllLines(assetPath);
                if (!LooksLikeUnityYaml(lines)) return 0;

                var applied = 0;
                var replacements = new List<(int Start, int End, string Line)>();
                var current = FindStoredTypeNames(lines);

                foreach (var edit in edits)
                {
                    var found = current.FindIndex(candidate =>
                        candidate.IsSameField(edit.Entry) &&
                        string.Equals(candidate.TypeName, edit.Entry.TypeName, StringComparison.Ordinal));
                    if (found < 0) continue;

                    var entry = current[found];
                    current.RemoveAt(found);
                    applied++;

                    replacements.Add((entry.ValueStart, entry.ValueEnd, entry.ValueHead + ": " + FormatTypeNameScalar(edit.NewName)));

                    if (edit.NewScriptReference is not null && entry.ScriptStart >= 0)
                        replacements.Add((entry.ScriptStart, entry.ScriptEnd, entry.ScriptHead + ": " + edit.NewScriptReference));
                }

                if (applied == 0) return 0;

                // From the bottom up, so a folded scalar that collapses to one line does not shift the next range.
                replacements.Sort((a, b) => b.Start.CompareTo(a.Start));

                var edited = new List<string>(lines);
                foreach (var (start, end, line) in replacements)
                {
                    edited.RemoveRange(start, end - start);
                    edited.Insert(start, line);
                }

                if (!TryWritePreservingNewlines(assetPath, edited)) return 0;

                // Same-tick writes can leave the modification-time key unchanged, so bust the probe cache explicitly.
                SerializeReferenceYamlProbeCache.ClearCache();
                return applied;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[TypeSelector] Failed to rewrite type names in '{assetPath}': {exception}");
                return 0;
            }
        }

        // An inline "{fileID: …, guid: …, type: 3}" mapping for a _script reference, or the null reference.
        public static string FormatScriptReference(long fileId, string guid) =>
            string.IsNullOrEmpty(guid) ? "{fileID: 0}" : $"{{fileID: {fileId}, guid: {guid}, type: 3}}";

        // Unity writes an assembly-qualified name as a plain scalar. One that YAML would read differently is quoted.
        public static string FormatTypeNameScalar(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return string.Empty;

            var plain = (char.IsLetter(typeName[0]) || typeName[0] == '_') &&
                        typeName.Trim().Length == typeName.Length &&
                        typeName.IndexOf(": ", StringComparison.Ordinal) < 0 &&
                        typeName.IndexOf(" #", StringComparison.Ordinal) < 0 &&
                        typeName.IndexOfAny(new[] { '\n', '\r', '\t' }) < 0;

            return plain ? typeName : $"'{typeName.Replace("'", "''")}'";
        }

        private static bool HoldsTypeName(string[] lines, int start, int end)
        {
            for (var i = start; i < end; i++)
                if (MayHoldTypeNames(lines[i])) return true;

            return false;
        }

        private static void CollectDocumentTypeNames(string[] lines, int start, int end, long fileId, List<StoredTypeNameEntry> result)
        {
            TryReadScriptGuid(lines, start, end, out var hostGuid, out _);
            var hostFileId = TryReadScriptFileId(lines, start, end);

            var frames = new List<PathFrame>();

            for (var i = start; i < end; i++)
            {
                var line = lines[i];
                if (line.Trim().Length == 0) continue;

                var match = _blockKey.Match(line);
                if (!match.Success)
                {
                    // A scalar or flow sequence item still counts toward its list's indices.
                    var item = _sequenceItem.Match(line);
                    if (item.Success) PushItem(frames, item.Groups["indent"].Length);

                    i = SkipContinuation(lines, i, IndentOf(line), end);
                    continue;
                }

                var indent = match.Groups["indent"].Length;
                var dash = match.Groups["dash"];
                var contentIndent = indent + (dash.Success ? dash.Length : 0);

                if (dash.Success) PushItem(frames, indent);
                PopFrames(frames, contentIndent);

                var key = match.Groups["key"].Value;
                var value = match.Groups["value"].Success ? match.Groups["value"].Value : string.Empty;

                if (key == TypeNameKey)
                {
                    var valueStart = i;
                    var typeName = ReadScalar(lines, value, contentIndent, ref i, end);
                    var head = line[..(contentIndent + key.Length)];

                    var hasScript = TryFindSiblingScript(lines, i + 1, end, contentIndent,
                        out var scriptStart, out var scriptEnd, out var scriptHead, out var scriptReference);
                    ParseObjectReference(scriptReference, out var scriptFileId, out var scriptGuid);

                    var inReference = TryGetReferenceScope(frames, out var rid, out var referenceType, out var dataDepth);

                    result.Add(new StoredTypeNameEntry(
                        fileId,
                        inReference ? rid : 0,
                        BuildFieldPath(frames, inReference ? dataDepth + 1 : 1),
                        typeName,
                        scriptGuid,
                        scriptFileId,
                        inReference ? string.Empty : hostGuid,
                        inReference ? 0 : hostFileId,
                        inReference ? referenceType : default,
                        isOverride: false,
                        targetFileId: 0,
                        targetGuid: string.Empty,
                        valueStart,
                        i + 1,
                        head,
                        hasScript ? scriptStart : -1,
                        hasScript ? scriptEnd : -1,
                        scriptHead));

                    continue;
                }

                if (value.Length > 0)
                {
                    // A RefIds entry's "rid:" and "type:" name the reference whose data follows.
                    if (frames.Count > 0 && frames[^1] is { IsItem: true } entry && IsRefIdsEntry(frames, frames.Count - 1))
                    {
                        if (key == "rid" && long.TryParse(value.Trim(), out var entryRid)) entry.Rid = entryRid;
                        else if (key == "type" && TryReadInlineTypeBody(value, out var body) && TryParseInlineType(body, out var type))
                            entry.ReferenceType = type;
                    }

                    i = SkipContinuation(lines, i, contentIndent, end);
                    continue;
                }

                frames.Add(PathFrame.Key(contentIndent, key));
            }
        }

        // A sequence item at `indent`: closes the previous item of the same list and opens the next one.
        private static void PushItem(List<PathFrame> frames, int indent)
        {
            while (frames.Count > 0)
            {
                var top = frames[^1];
                if (top.Indent > indent || (top.IsItem && top.Indent == indent)) frames.RemoveAt(frames.Count - 1);
                else break;
            }

            var owner = frames.Count > 0 ? frames[^1] : null;
            var index = owner is null ? 0 : owner.ItemCount++;
            frames.Add(PathFrame.Item(indent, index));
        }

        // A key at `indent` closes every key at its indent or deeper; the item it belongs to stays open.
        private static void PopFrames(List<PathFrame> frames, int indent)
        {
            while (frames.Count > 0 && frames[^1].Indent >= indent)
                frames.RemoveAt(frames.Count - 1);
        }

        // Unity's property path of the frames from `from`: "list.Array.data[2].field". Frame 0 is the document's
        // class key ("MonoBehaviour:"), never part of the path.
        private static string BuildFieldPath(List<PathFrame> frames, int from)
        {
            var builder = new StringBuilder();

            for (var i = Math.Max(1, from); i < frames.Count; i++)
            {
                var frame = frames[i];
                if (frame.IsItem)
                {
                    builder.Append(".Array.data[").Append(frame.Index).Append(']');
                    continue;
                }

                if (builder.Length > 0) builder.Append('.');
                builder.Append(frame.Name);
            }

            return builder.ToString();
        }

        // Inside "references: RefIds: - rid: N … data:", the wrapper belongs to reference N; `dataDepth` is the index
        // of the "data" frame, after which the reference's own field path starts.
        private static bool TryGetReferenceScope(List<PathFrame> frames, out long rid, out ManagedTypeName type, out int dataDepth)
        {
            rid = 0;
            type = default;
            dataDepth = -1;

            for (var i = 0; i + 1 < frames.Count; i++)
            {
                if (!frames[i].IsItem || !IsRefIdsEntry(frames, i)) continue;
                if (frames[i + 1].IsItem || frames[i + 1].Name != "data") continue;

                rid = frames[i].Rid;
                type = frames[i].ReferenceType;
                dataDepth = i + 1;
                return true;
            }

            return false;
        }

        private static bool IsRefIdsEntry(List<PathFrame> frames, int itemIndex) =>
            itemIndex >= 2 &&
            frames[itemIndex - 1] is { IsItem: false, Name: "RefIds" } &&
            frames[itemIndex - 2] is { IsItem: false, Name: "references" };

        // The wrapper's other fields share the name's indent; Unity writes _script right after it.
        private static bool TryFindSiblingScript(
            string[] lines, int from, int end, int indent,
            out int scriptStart, out int scriptEnd, out string scriptHead, out string reference)
        {
            scriptStart = -1;
            scriptEnd = -1;
            scriptHead = string.Empty;
            reference = string.Empty;

            for (var i = from; i < end; i++)
            {
                var line = lines[i];
                if (line.Trim().Length == 0) continue;

                var lineIndent = IndentOf(line);
                if (lineIndent < indent) return false;
                if (lineIndent > indent) continue;

                var match = _blockKey.Match(line);
                if (!match.Success || match.Groups["dash"].Success) return false;
                if (match.Groups["key"].Value != TypeNameScriptKey) continue;

                scriptStart = i;
                scriptHead = line[..(indent + TypeNameScriptKey.Length)];
                reference = ReadScalar(lines, match.Groups["value"].Value, indent, ref i, end);
                scriptEnd = i + 1;
                return true;
            }

            return false;
        }

        private static void CollectOverrideTypeNames(string[] lines, int start, int end, long fileId, List<StoredTypeNameEntry> result)
        {
            var modifications = ReadModifications(lines, start, end);

            foreach (var modification in modifications)
            {
                var path = modification.PropertyPath;
                if (path != TypeNameKey && !path.EndsWith(TypeNameOverrideSuffix, StringComparison.Ordinal)) continue;
                if (modification.ValueLines.Start < 0) continue;

                var fieldPath = path.Length > TypeNameKey.Length ? path[..^TypeNameOverrideSuffix.Length] : string.Empty;
                var script = FindScriptModification(modifications, modification, fieldPath);

                var scriptFileId = 0L;
                var scriptGuid = string.Empty;
                if (script is not null) ParseObjectReference(script.ObjectReference, out scriptFileId, out scriptGuid);

                result.Add(new StoredTypeNameEntry(
                    fileId,
                    rid: 0,
                    fieldPath,
                    modification.Value,
                    scriptGuid,
                    scriptFileId,
                    hostScriptGuid: string.Empty,
                    hostScriptFileId: 0,
                    hostReferenceType: default,
                    isOverride: true,
                    modification.TargetFileId,
                    modification.TargetGuid,
                    modification.ValueLines.Start,
                    modification.ValueLines.End,
                    modification.ValueLines.Head,
                    script?.ObjectReferenceLines.Start ?? -1,
                    script?.ObjectReferenceLines.End ?? -1,
                    script?.ObjectReferenceLines.Head));
            }
        }

        // The "<field>._script" modification of the same target, which a SerializableMonoScript override writes too.
        private static Modification FindScriptModification(List<Modification> modifications, Modification name, string fieldPath)
        {
            var scriptPath = fieldPath.Length == 0 ? TypeNameScriptKey : fieldPath + ScriptOverrideSuffix;

            foreach (var modification in modifications)
            {
                if (modification.TargetFileId != name.TargetFileId ||
                    !string.Equals(modification.TargetGuid, name.TargetGuid, StringComparison.Ordinal)) continue;

                if (string.Equals(modification.PropertyPath, scriptPath, StringComparison.Ordinal) &&
                    modification.ObjectReferenceLines.Start >= 0)
                    return modification;
            }

            return null;
        }

        private static void ParseObjectReference(string reference, out long fileId, out string guid)
        {
            fileId = 0;
            guid = string.Empty;
            if (string.IsNullOrEmpty(reference)) return;

            var id = _referenceFileId.Match(reference);
            if (id.Success) long.TryParse(id.Groups["id"].Value, out fileId);

            // {fileID: 0} is the null reference.
            var match = _referenceGuid.Match(reference);
            if (match.Success && fileId != 0) guid = match.Groups["guid"].Value;
        }

        private static long TryReadScriptFileId(string[] lines, int start, int end)
        {
            for (var i = start; i < end; i++)
            {
                if (!_scriptGuidPattern.IsMatch(lines[i])) continue;

                var id = _referenceFileId.Match(lines[i]);
                return id.Success && long.TryParse(id.Groups["id"].Value, out var fileId) ? fileId : 0;
            }

            return 0;
        }

        private static bool TryReadInlineTypeBody(string value, out string body)
        {
            var trimmed = value.Trim();
            var valid = trimmed.Length >= 2 && trimmed[0] == '{' && trimmed[^1] == '}';
            body = valid ? trimmed[1..^1] : string.Empty;
            return valid;
        }

        // Skips the deeper-indented lines a scalar or flow mapping wraps onto; returns the last line consumed.
        private static int SkipContinuation(string[] lines, int i, int indent, int end)
        {
            while (i + 1 < end)
            {
                var next = lines[i + 1];
                if (next.Trim().Length > 0 && IndentOf(next) <= indent) break;
                i++;
            }

            return i;
        }

        // A level of the path to the current line: a mapping key, or an item of the sequence its parent key owns.
        private sealed class PathFrame
        {
            public int Indent;
            public string Name;
            public bool IsItem;
            public int Index;

            // How many items the sequence under this key has opened so far.
            public int ItemCount;

            // Set on a RefIds entry from its "rid:" and "type:" lines.
            public long Rid;
            public ManagedTypeName ReferenceType;

            public static PathFrame Key(int indent, string name) => new() { Indent = indent, Name = name };

            public static PathFrame Item(int indent, int index) => new() { Indent = indent, Name = string.Empty, IsItem = true, Index = index };
        }

        private readonly struct ScalarLines
        {
            public static readonly ScalarLines None = new(-1, -1, string.Empty);

            public readonly int Start;
            public readonly int End;

            // The first line up to the key's end, without the colon.
            public readonly string Head;

            public ScalarLines(int start, int end, string head)
            {
                Start = start;
                End = end;
                Head = head;
            }
        }
    }
}
