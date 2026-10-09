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
        // SessionState, so a domain reload keeps them, and only a save that writes the file uses them up.
        private static List<Note> _notes;

        private static List<Note> Notes => _notes ??= LoadNotes();

        // Drops the loaded notes, as a domain reload does; the next access reads them back from SessionState.
        internal static void ReloadNotes() => _notes = null;

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

                var pending = BeginSave(path, SerializeReferenceHelpers.StoredTypeResolves);
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

            NoteReplaced(assetPath, fileId, rid);
        }

        internal static void NoteReplaced(string assetPath, long fileId, long rid)
        {
            var note = new Note(assetPath, fileId, rid, Undo.GetCurrentGroup());

            Notes.RemoveAll(existing => existing.Matches(assetPath, fileId, rid));
            Notes.Add(note);
            SaveNotes();

            // The replace runs right after this call and may record into a group of its own (a multi-object pick
            // increments the group first): the note follows it to the group it landed in.
            EditorApplication.delayCall += () =>
            {
                var group = Undo.GetCurrentGroup();
                if (note.Undone || group <= note.UndoGroup || !Notes.Contains(note)) return;

                note.UndoGroup = group;
                SaveNotes();
            };
        }

        // Only an undo step takes a replace back. Steps run through the groups in order, so undoing the note's group or
        // an earlier one takes it back, and only redoing that group restores it; any other step leaves the note as is.
        private static void OnUndoRedo(in UndoRedoInfo info)
        {
            var changed = false;

            foreach (var note in Notes)
            {
                var undone = info.isRedo
                    ? note.Undone && info.undoGroup != note.UndoGroup
                    : note.Undone || info.undoGroup <= note.UndoGroup;

                if (undone == note.Undone) continue;

                note.Undone = undone;
                changed = true;
            }

            if (changed) SaveNotes();
        }

        // The state a save is about to overwrite: the lists at risk, the notes they rely on and a stamp of the file. Null
        // when the save puts nothing at risk and uses no note.
        internal static PendingSave BeginSave(string assetPath, Func<ManagedTypeName, bool> resolves)
        {
            var replaced = new HashSet<(long fileId, long rid)>();
            foreach (var note in Notes)
                if (!note.Undone && note.AssetPath == assetPath) replaced.Add((note.FileId, note.Rid));

            var snapshots = SerializeReferenceYamlEditor.SnapshotMissingLists(assetPath, resolves, replaced);
            if (snapshots.Count == 0 && replaced.Count == 0) return null;

            return new PendingSave(snapshots, replaced, Stamp(assetPath));
        }

        // Runs once Unity has written the file.
        internal static void CompleteSave(string assetPath, PendingSave pending)
        {
            if (!ConsumeIfWritten(assetPath, pending) || pending.Snapshots.Count == 0) return;

            // An edit made since the save is only in memory, and the reimport after a restore would drop it.
            var guid = AssetDatabase.GUIDFromAssetPath(assetPath);
            if (!guid.Empty()) AssetDatabase.SaveAssetIfDirty(guid);

            var report = RestoreSnapshots(assetPath, pending.Snapshots);
            if (report.Restored.Count > 0) AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            Log(assetPath, report);
        }

        // A save that left the file as it was used up nothing: its notes wait for the next save.
        internal static bool ConsumeIfWritten(string assetPath, PendingSave pending)
        {
            if (Stamp(assetPath) == pending.Stamp) return false;

            if (Notes.RemoveAll(note => !note.Undone && note.AssetPath == assetPath && pending.Replaced.Contains((note.FileId, note.Rid))) > 0)
                SaveNotes();

            return true;
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

        private static (DateTime writeTimeUtc, long length) Stamp(string assetPath)
        {
            try
            {
                var file = new FileInfo(assetPath);
                return file.Exists ? (file.LastWriteTimeUtc, file.Length) : (default, -1);
            }
            catch (Exception)
            {
                return (default, -1);
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
            public readonly (DateTime writeTimeUtc, long length) Stamp;

            public PendingSave(List<MissingListSnapshot> snapshots, HashSet<(long fileId, long rid)> replaced,
                (DateTime writeTimeUtc, long length) stamp)
            {
                Snapshots = snapshots;
                Replaced = replaced;
                Stamp = stamp;
            }
        }

        private sealed class Note
        {
            public readonly string AssetPath;
            public readonly long FileId;
            public readonly long Rid;

            public int UndoGroup;
            public bool Undone;

            public Note(string assetPath, long fileId, long rid, int undoGroup)
            {
                AssetPath = assetPath;
                FileId = fileId;
                Rid = rid;
                UndoGroup = undoGroup;
            }

            public bool Matches(string assetPath, long fileId, long rid) =>
                AssetPath == assetPath && FileId == fileId && Rid == rid;

            // The path goes last, so a tab in it cannot shift the other fields.
            public string Encode() =>
                string.Join(FieldSeparator.ToString(), FileId, Rid, UndoGroup, Undone ? 1 : 0, AssetPath);

            public static bool TryDecode(string line, out Note note)
            {
                note = null;

                var fields = line.Split(new[] { FieldSeparator }, count: 5);
                if (fields.Length != 5 || string.IsNullOrEmpty(fields[4])) return false;
                if (!long.TryParse(fields[0], out var fileId) || !long.TryParse(fields[1], out var rid)) return false;
                if (!int.TryParse(fields[2], out var undoGroup)) return false;

                note = new Note(fields[4], fileId, rid, undoGroup) { Undone = fields[3] == "1" };
                return true;
            }
        }
    }
}
