using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // A type name stored by a SerializableType or SerializableMonoScript wrapper: its _assemblyQualifiedName scalar in a
    // document's fields, in a managed reference's data, or in a prefab instance override.
    internal readonly struct StoredTypeNameEntry
    {
        // The document that holds the name; for an override, the "--- !u!1001" PrefabInstance document.
        public readonly long FileId;

        // The managed reference whose data holds the wrapper, or 0 outside one.
        public readonly long Rid;

        // The wrapper's property path ("_weapon", "_loadout.Array.data[1]"); within a managed reference, the path in
        // its data; for an override, the modification's path without the backing field.
        public readonly string FieldPath;

        // The stored assembly-qualified name, unfolded and unquoted; empty when no type is stored.
        public readonly string TypeName;

        // The _script reference of a SerializableMonoScript wrapper; an empty guid for a SerializableType.
        public readonly string ScriptGuid;
        public readonly long ScriptFileId;

        // The m_Script of the document, which declares the wrapper field; empty inside a managed reference and in an
        // override.
        public readonly string HostScriptGuid;
        public readonly long HostScriptFileId;

        // The type of the managed reference whose data holds the wrapper; empty outside one.
        public readonly ManagedTypeName HostReferenceType;

        public readonly bool IsOverride;

        // The overridden object in the source prefab; zero and empty unless IsOverride.
        public readonly long TargetFileId;
        public readonly string TargetGuid;

        // Where the scalars are in the lines the entry was read from, for a rewrite of the same lines: [ValueStart,
        // ValueEnd) holds the name, ValueHead is its first line up to the value; ScriptStart is -1 without _script.
        internal readonly int ValueStart;
        internal readonly int ValueEnd;
        internal readonly string ValueHead;
        internal readonly int ScriptStart;
        internal readonly int ScriptEnd;
        internal readonly string ScriptHead;

        public StoredTypeNameEntry(
            long fileId,
            long rid,
            string fieldPath,
            string typeName,
            string scriptGuid,
            long scriptFileId,
            string hostScriptGuid,
            long hostScriptFileId,
            ManagedTypeName hostReferenceType,
            bool isOverride,
            long targetFileId,
            string targetGuid,
            int valueStart,
            int valueEnd,
            string valueHead,
            int scriptStart,
            int scriptEnd,
            string scriptHead)
        {
            FileId = fileId;
            Rid = rid;
            FieldPath = fieldPath ?? string.Empty;
            TypeName = typeName ?? string.Empty;
            ScriptGuid = scriptGuid ?? string.Empty;
            ScriptFileId = scriptFileId;
            HostScriptGuid = hostScriptGuid ?? string.Empty;
            HostScriptFileId = hostScriptFileId;
            HostReferenceType = hostReferenceType;
            IsOverride = isOverride;
            TargetFileId = targetFileId;
            TargetGuid = targetGuid ?? string.Empty;
            ValueStart = valueStart;
            ValueEnd = valueEnd;
            ValueHead = valueHead ?? string.Empty;
            ScriptStart = scriptStart;
            ScriptEnd = scriptEnd;
            ScriptHead = scriptHead ?? string.Empty;
        }

        // Whether _script references a script; a null reference leaves the guid empty.
        public bool HasScript => ScriptGuid.Length > 0;

        // Whether the wrapper writes a _script line, so it is a SerializableMonoScript, even with a null reference.
        public bool HasScriptField => ScriptStart >= 0;

        // The same field as storing `typeName`, for an edit that expects a name written after this entry was read.
        public StoredTypeNameEntry WithTypeName(string typeName) => new(
            FileId, Rid, FieldPath, typeName, ScriptGuid, ScriptFileId, HostScriptGuid, HostScriptFileId, HostReferenceType,
            IsOverride, TargetFileId, TargetGuid, ValueStart, ValueEnd, ValueHead, ScriptStart, ScriptEnd, ScriptHead);

        // Whether both entries are the same field, whatever name it stores now.
        public bool IsSameField(StoredTypeNameEntry other) =>
            FileId == other.FileId && Rid == other.Rid && IsOverride == other.IsOverride &&
            TargetFileId == other.TargetFileId &&
            string.Equals(TargetGuid, other.TargetGuid, StringComparison.Ordinal) &&
            string.Equals(FieldPath, other.FieldPath, StringComparison.Ordinal);
    }
}
