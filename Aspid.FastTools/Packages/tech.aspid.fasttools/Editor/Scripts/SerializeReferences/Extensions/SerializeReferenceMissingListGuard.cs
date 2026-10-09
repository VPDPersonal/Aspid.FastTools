using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class SerializeReferenceMissingListGuard : AssetModificationProcessor
    {
        private const string NotesKey = "Aspid.FastTools.SerializeReference.MissingListGuard.Notes";
        private const char NoteSeparator = '\n';
        private const char FieldSeparator = '\t';

        // Missing elements the user replaced (<None>, another type, a paste), so the next save lets them go. They live in
        // SessionState, so a domain reload keeps them, and only a save that writes the file uses them up. A note holds
        // the stamp of the file it was made on: after a change outside a save, such as a revert in version control, it
        // no longer applies.
        private static List<Note> _notes;

        private static List<Note> Notes => _notes ??= LoadNotes();

        // Grows with every change of the notes, so a cache built from them can tell it is stale.
        internal static int NotesVersion { get; private set; }

        // Drops the loaded notes, as a domain reload does; the next access reads them back from SessionState.
        internal static void ReloadNotes()
        {
            _notes = null;
            NotesVersion++;
        }

        [InitializeOnLoadMethod]
        private static void TrackUndo() => Undo.undoRedoEvent += OnUndoRedo;

        // Fires with the file still in its pre-save state; the returned set is never altered.
        private static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                if (!IsGuarded(path))
                {
                    if (!string.IsNullOrEmpty(path)) ForgetNotes(path);
                    continue;
                }

                var pending = BeginSave(path, StoredTypeLoads);
                if (pending is null) continue;

                // Anchored to the path, not a SerializedObject: the repair re-reads from disk after Unity writes.
                var captured = path;
                EditorApplication.delayCall += () => CompleteSave(captured, pending);
            }

            return paths;
        }

        // A loaded scene or the open Prefab Mode asset keeps its own copy, so a file rewrite under it would diverge
        // from what the editor shows.
        internal static bool IsGuarded(string path) =>
            SerializeReferenceYaml.IsCandidateAssetPath(path) && SerializeReferenceOpenCopyGuard.IsWritable(path);

        // Whether Unity loads the stored type: it resolves, or a single [MovedFrom] rename claims it. Unity keeps such an
        // element on save, so the guard must not take it for missing.
        internal static bool StoredTypeLoads(ManagedTypeName type) =>
            SerializeReferenceHelpers.StoredTypeResolves(type) || SerializeReferenceMovedFromResolver.TryResolve(type, out _);

        // Called before a missing element is replaced, so the next save does not bring it back; a healthy element is
        // not noted.
        public static void NoteReplaced(SerializedProperty property)
        {
            if (property is null) return;

            var serializedObject = property.serializedObject;
            if (!serializedObject.isEditingMultipleObjects)
            {
                NoteReplacedTarget(property);
                return;
            }

            foreach (var target in serializedObject.targetObjects)
            {
                using var single = new SerializedObject(target);
                var targetProperty = single.FindProperty(property.propertyPath);
                if (targetProperty is not null) NoteReplacedTarget(targetProperty);
            }
        }

        private static void NoteReplacedTarget(SerializedProperty property)
        {
            if (!SerializeReferenceHelpers.TryGetRepairLocation(property, out var assetPath, out var fileId, out var inMemory)) return;
            if (inMemory) return; // an open copy is never rewritten by the guard
            if (!SerializeReferenceHelpers.TryGetMissingReferenceId(property, out var rid)) return;

            // A slot of a top-level list is noted too, so the other slots that share the missing reference stay guarded.
            if (SerializeReferenceHelpers.TryGetMissingListSlot(property, out var field, out var index))
                NoteReplaced(assetPath, fileId, rid, field, index);
            else
                NoteReplaced(assetPath, fileId, rid);
        }

        internal static void NoteReplaced(string assetPath, long fileId, long rid) =>
            NoteReplaced(assetPath, fileId, rid, field: null, index: -1);

        // field and index name the list slot replaced, as the file holds it; without them every slot of the rid counts as
        // replaced.
        internal static void NoteReplaced(string assetPath, long fileId, long rid, string field, int index)
        {
            var note = new Note(assetPath, fileId, rid, field, index, Undo.GetCurrentGroup(), Stamp(assetPath));

            Notes.RemoveAll(existing => existing.Matches(note));
            Notes.Add(note);
            SaveNotes();

            // The replace runs right after this call and may record into a group of its own (a multi-object pick
            // increments the group first): the note's range of groups grows to the one current on the next tick.
            EditorApplication.delayCall += () =>
            {
                var group = Undo.GetCurrentGroup();
                if (note.Undone || group <= note.LastGroup || !Notes.Contains(note)) return;

                note.LastGroup = group;
                SaveNotes();
            };
        }

        // Whether the user replaced the missing reference since the last save, so it reads as a null until a save drops it
        // from the file. field and index name a slot of a top-level list in the file, or are null and -1 elsewhere. A note
        // of another slot counts only when that slot no longer holds the rid, as at save.
        internal static bool IsReplaced(string assetPath, long fileId, long rid, string field, int index)
        {
            (long writeTimeTicks, long length)? stamp = null;

            foreach (var note in Notes)
            {
                if (note.Undone || note.Rid != rid || note.FileId != fileId || note.AssetPath != assetPath) continue;

                stamp ??= Stamp(assetPath);
                if (note.Stamp != stamp.Value) continue;

                if (note.Field is null || (note.Field == field && note.Index == index)) return true;
                if (!SlotHoldsRid(note)) return true;
            }

            return false;
        }

        private static bool SlotHoldsRid(Note note) =>
            SerializeReferenceYamlEditor.TryReadListIds(note.AssetPath, note.FileId, SerializeReferenceYamlEditor.NoAnchor,
                note.Field, out var rids) && note.Index >= 0 && note.Index < rids.Length && rids[note.Index] == note.Rid;

        // Only an undo step takes a replace back. Steps run through the groups in order, so undoing a group up to the end
        // of the note's range takes it back, and redoing a group inside the range restores it; any other step leaves the
        // note as is.
        private static void OnUndoRedo(in UndoRedoInfo info)
        {
            var changed = false;

            foreach (var note in Notes)
            {
                var undone = info.isRedo
                    ? note.Undone && (info.undoGroup < note.Group || info.undoGroup > note.LastGroup)
                    : note.Undone || info.undoGroup <= note.LastGroup;

                if (undone == note.Undone) continue;

                note.Undone = undone;
                changed = true;
            }

            if (changed) SaveNotes();
        }

        // The state a save is about to overwrite: the lists at risk, the notes they rely on and a stamp of the file. Null
        // when the save puts nothing at risk and the file has no note.
        internal static PendingSave BeginSave(string assetPath, Func<ManagedTypeName, bool> resolves)
        {
            var stamp = Stamp(assetPath);
            if (Notes.RemoveAll(note => note.AssetPath == assetPath && note.Stamp != stamp) > 0) SaveNotes();

            var hasNotes = false;
            var replaced = new HashSet<(long fileId, long rid)>();
            var replacedRids = new HashSet<(long fileId, long rid)>();
            var replacedSlots = new List<(long fileId, long rid, string field, int index)>();

            foreach (var note in Notes)
            {
                if (note.AssetPath != assetPath) continue;

                hasNotes = true;
                if (note.Undone) continue;

                replaced.Add((note.FileId, note.Rid));
                if (note.Field is null) replacedRids.Add((note.FileId, note.Rid));
                else replacedSlots.Add((note.FileId, note.Rid, note.Field, note.Index));
            }

            var snapshots = SerializeReferenceYamlEditor.SnapshotMissingLists(assetPath, resolves, replacedRids, replacedSlots);
            if (snapshots.Count == 0 && !hasNotes) return null;

            return new PendingSave(snapshots, replaced, stamp);
        }

        // Runs once Unity has written the file.
        internal static void CompleteSave(string assetPath, PendingSave pending)
        {
            if (!ConsumeIfWritten(assetPath, pending) || pending.Snapshots.Count == 0) return;

            // An edit made since the save is only in memory, and the reimport after a restore would drop it.
            var guid = AssetDatabase.GUIDFromAssetPath(assetPath);
            if (!guid.Empty()) AssetDatabase.SaveAssetIfDirty(guid);

            var report = RestoreSnapshots(assetPath, pending.Snapshots);
            if (report.Restored.Count > 0)
            {
                RestampNotes(assetPath);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            Log(assetPath, report);
        }

        // A save that left the file as it was used up nothing: its notes wait for the next save. The notes a save that
        // wrote did not use move on to the file it wrote.
        internal static bool ConsumeIfWritten(string assetPath, PendingSave pending)
        {
            if (Stamp(assetPath) == pending.Stamp) return false;

            Notes.RemoveAll(note =>
                !note.Undone && note.AssetPath == assetPath && pending.Replaced.Contains((note.FileId, note.Rid)));
            RestampNotes(assetPath);

            return true;
        }

        private static void RestampNotes(string assetPath)
        {
            var stamp = Stamp(assetPath);
            foreach (var note in Notes)
                if (note.AssetPath == assetPath) note.Stamp = stamp;

            SaveNotes();
        }

        internal static List<MissingListSnapshot> SnapshotMissingArrayElements(string assetPath, Func<ManagedTypeName, bool> resolves) =>
            BeginSave(assetPath, resolves)?.Snapshots ?? new List<MissingListSnapshot>();

        internal static MissingListReport RestoreSnapshots(string assetPath, List<MissingListSnapshot> snapshots) =>
            SerializeReferenceYamlEditor.RestoreMissingLists(assetPath, snapshots);

        private static void Log(string assetPath, MissingListReport report)
        {
            if (report.Restored.Count > 0)
            {
                Debug.Log($"[Aspid FastTools] Restored {report.Restored.Count} missing list reference(s) that saving dropped in " +
                    $"'{assetPath}': {MissingListReport.Describe(report.Restored, markGuessed: true)}.");
            }

            var dropped = report.Dropped.FindAll(element => element.Guessed);
            if (dropped.Count == 0) return;

            Debug.LogWarning($"[Aspid FastTools] Saving dropped {dropped.Count} missing list reference(s) in '{assetPath}' " +
                $"that no slot was found for (indexes before the save): {MissingListReport.Describe(dropped, markGuessed: false)}. " +
                "If you did not delete them, restore the file from version control.");
        }

        internal static void ForgetNotes(string assetPath)
        {
            if (Notes.RemoveAll(note => note.AssetPath == assetPath) > 0) SaveNotes();
        }

        private static (long writeTimeTicks, long length) Stamp(string assetPath)
        {
            try
            {
                var file = new FileInfo(assetPath);
                return file.Exists ? (file.LastWriteTimeUtc.Ticks, file.Length) : (0, -1);
            }
            catch (Exception)
            {
                return (0, -1);
            }
        }

        private static List<Note> LoadNotes()
        {
            var notes = new List<Note>();

            foreach (var line in SessionState.GetString(NotesKey, string.Empty).Split(NoteSeparator))
            {
                if (Note.TryDecode(line, out var note)) notes.Add(note);
            }

            return notes;
        }

        private static void SaveNotes()
        {
            NotesVersion++;

            if (Notes.Count == 0)
            {
                SessionState.EraseString(NotesKey);
                return;
            }

            var builder = new StringBuilder();
            foreach (var note in Notes)
            {
                if (builder.Length > 0) builder.Append(NoteSeparator);
                builder.Append(note.Encode());
            }

            SessionState.SetString(NotesKey, builder.ToString());
        }

        internal sealed class PendingSave
        {
            public readonly List<MissingListSnapshot> Snapshots;
            public readonly HashSet<(long fileId, long rid)> Replaced;
            public readonly (long writeTimeTicks, long length) Stamp;

            public PendingSave(List<MissingListSnapshot> snapshots, HashSet<(long fileId, long rid)> replaced,
                (long writeTimeTicks, long length) stamp)
            {
                Snapshots = snapshots;
                Replaced = replaced;
                Stamp = stamp;
            }
        }

        private sealed class Note
        {
            private const int FieldCount = 10;

            public readonly string AssetPath;
            public readonly long FileId;
            public readonly long Rid;

            // The list slot replaced, or null and -1 when every slot of the rid counts.
            public readonly string Field;
            public readonly int Index;

            // The Undo group current when the note was made, and the last group the replace may have recorded into.
            public readonly int Group;
            public int LastGroup;

            public bool Undone;
            public (long writeTimeTicks, long length) Stamp;

            public Note(string assetPath, long fileId, long rid, string field, int index, int group,
                (long writeTimeTicks, long length) stamp)
            {
                AssetPath = assetPath;
                FileId = fileId;
                Rid = rid;
                Field = field;
                Index = field is null ? -1 : index;
                Group = group;
                LastGroup = group;
                Stamp = stamp;
            }

            public bool Matches(Note other) =>
                AssetPath == other.AssetPath && FileId == other.FileId && Rid == other.Rid
                && Field == other.Field && Index == other.Index;

            // The path goes last, so a tab in it cannot shift the other fields.
            public string Encode() =>
                string.Join(FieldSeparator.ToString(), FileId, Rid, Group, LastGroup, Undone ? 1 : 0, Stamp.writeTimeTicks,
                    Stamp.length, Index, Field ?? string.Empty, AssetPath);

            public static bool TryDecode(string line, out Note note)
            {
                note = null;

                var fields = line.Split(new[] { FieldSeparator }, count: FieldCount);
                if (fields.Length != FieldCount || string.IsNullOrEmpty(fields[9])) return false;
                if (!long.TryParse(fields[0], out var fileId) || !long.TryParse(fields[1], out var rid)) return false;
                if (!int.TryParse(fields[2], out var group) || !int.TryParse(fields[3], out var lastGroup)) return false;
                if (!long.TryParse(fields[5], out var ticks) || !long.TryParse(fields[6], out var length)) return false;
                if (!int.TryParse(fields[7], out var index)) return false;

                var field = fields[8].Length == 0 ? null : fields[8];
                note = new Note(fields[9], fileId, rid, field, index, group, (ticks, length))
                {
                    LastGroup = lastGroup,
                    Undone = fields[4] == "1",
                };

                return true;
            }
        }
    }
}
