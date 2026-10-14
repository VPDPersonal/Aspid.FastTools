using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors.Internal;
using static Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceAuditUI;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceProjectSummary
    {
        // A card builds this many rows, then offers Show more, so thousands of entries stay cheap to build.
        public const int RowsPerPage = 200;

        private const int MaxPreviewedEntries = 8;

        private const string RequiredNotScannedText = "Required fields have not been checked yet — Rescan to include them.";
        private const string RequiredDisabledText =
            "Required fields are not checked while gate severity is Off — set it to Warn or Fail in Project Settings → " +
            "Aspid.FastTools → SerializeReference, then Rescan.";
        private const string TypeNamesNotScannedText = "Type names and required fields have not been checked yet — Rescan to include them.";
        // Type names and required violations are read by Scan Project and refreshed only by this window's own edits, so
        // the tooltips of their rows say they may be out of date; the line under the results header does not repeat it.
        public const string ScanTimeNote = "As of the last Scan Project — Rescan to refresh.";

        // Only a sweep that actually ran the required check can call the project clean. A sweep skipped because
        // severity was Off has not checked anything once severity is on again.
        public static RequiredAuditState GetRequiredAuditState(bool scanned, bool checkDisabled, bool severityOff) =>
            !scanned || (checkDisabled && !severityOff) ? RequiredAuditState.NotScanned
            : checkDisabled ? RequiredAuditState.Disabled
            : RequiredAuditState.Checked;

        public static string BuildRequiredNotCheckedText(RequiredAuditState state) =>
            state == RequiredAuditState.Disabled ? RequiredDisabledText : RequiredNotScannedText;

        // Type names are read only by Scan Project, so a window that shows the index of another feature (Unity Search,
        // Asset References) has not checked them.
        public static string BuildNotCheckedText(RequiredAuditState state, bool typeNamesScanned) =>
            typeNamesScanned ? BuildRequiredNotCheckedText(state) : TypeNamesNotScannedText;

        // Binary files and Git LFS pointers are not text YAML, so no scan reads them; a file another program locks
        // cannot be opened (AssetFileFormat.Unreadable) and is counted with them.
        public static string BuildUnreadFilesText(int count) =>
            (count == 1
                ? "1 file was not checked (binary, a Git LFS pointer or not readable)"
                : $"{count} files were not checked (binary, Git LFS pointers or not readable)") + " — see the Console.";

        // A read-only or locked file logs its reason to the Console; an entry that changed since the scan is left
        // alone without a log, so the text names both causes.
        public static string BuildNotRewrittenText(int count, string verb = "rewritten") =>
            $"{count} could not be {verb} — a file is read-only or locked (see the Console), or the entry changed since the scan.";

        // An in-memory entry fails without a log when its scene or Prefab Mode stage was closed after the scan, or
        // when the live object no longer holds that missing reference.
        public static string BuildNoLongerOpenText(int count) =>
            $"{count} {(count == 1 ? "was" : "were")} no longer open in Prefab Mode or a scene, or changed since the scan — Rescan.";

        // The parts of a summary body that apply, in one paragraph; a part that is null or empty is left out.
        public static string JoinSentences(params string[] sentences) =>
            string.Join(" ", sentences.Where(sentence => !string.IsNullOrEmpty(sentence)));

        // Only a sweep that read every file and checked every kind of problem can call the project clean.
        private static bool IsFullyChecked(RequiredAuditState state, bool typeNamesScanned, int unreadFileCount) =>
            state == RequiredAuditState.Checked && typeNamesScanned && unreadFileCount == 0;

        public static (bool Success, string Title, string Message) BuildNothingFoundState(
            RequiredAuditState state, bool typeNamesScanned = true, int unreadFileCount = 0)
        {
            if (IsFullyChecked(state, typeNamesScanned, unreadFileCount))
                return (true, "Project clean", "No missing managed references, type names or unset required fields found anywhere under Assets/.");

            var found = state == RequiredAuditState.Checked && typeNamesScanned
                ? "No missing managed references, type names or unset required fields"
                : typeNamesScanned
                    ? "No missing managed references or type names"
                    : "No missing managed references";

            // Without Scan Project the number of unread files is unknown, so "anywhere under Assets/" would claim too much.
            var wherever = unreadFileCount > 0 || !typeNamesScanned ? "in the files that could be read" : "anywhere under Assets/";
            var message = $"{found} found {wherever}.";

            if (state != RequiredAuditState.Checked || !typeNamesScanned)
                message += " " + BuildNotCheckedText(state, typeNamesScanned);

            if (unreadFileCount > 0)
                message += " " + BuildUnreadFilesText(unreadFileCount);

            return (false, "No missing references", message);
        }

        public static StatusStyle.Type GetMissingReferencesCleanStatus(
            RequiredAuditState state, bool hasRequiredViolations, bool typeNamesScanned = true, int unreadFileCount = 0) =>
            hasRequiredViolations ? StatusStyle.Type.Warning
            : IsFullyChecked(state, typeNamesScanned, unreadFileCount) ? StatusStyle.Type.Success
            : StatusStyle.Type.Info;

        public static string BuildMissingReferencesCleanHintText(
            RequiredAuditState state, bool hasRequiredViolations, bool typeNamesScanned = true, int unreadFileCount = 0)
        {
            // Required violations are listed under their own heading, which says how to act on them.
            var hint =
                state != RequiredAuditState.Checked || !typeNamesScanned ? BuildNotCheckedText(state, typeNamesScanned)
                : hasRequiredViolations ? string.Empty
                : unreadFileCount > 0 ? "Nothing left to repair in the files that were read."
                : "Nothing left to repair. Rescan to sweep the project again and confirm it's clean.";

            if (unreadFileCount == 0) return hint;

            var unread = BuildUnreadFilesText(unreadFileCount);
            return hint.Length == 0 ? unread : hint + " " + unread;
        }

        public static string BuildResultsHeaderText(int brokenCount, int migrationCount, int requiredCount, int typeNameCount = 0)
        {
            var parts = new List<string>(4);
            if (brokenCount > 0) parts.Add(BuildCountText(brokenCount, "missing reference"));
            if (typeNameCount > 0) parts.Add(BuildCountText(typeNameCount, "missing type name"));
            if (migrationCount > 0) parts.Add(BuildCountText(migrationCount, "pending migration"));
            if (requiredCount > 0) parts.Add(BuildCountText(requiredCount, "required violation"));

            return string.Join(", ", parts);
        }

        // The line under the results header keeps only what needs attention now: a check that did not run, or files
        // that could not be read.
        public static string BuildResultsHintText(RequiredAuditState state, bool typeNamesScanned = true, int unreadFileCount = 0)
        {
            var parts = new List<string>(2);

            if (state != RequiredAuditState.Checked || !typeNamesScanned)
                parts.Add(BuildNotCheckedText(state, typeNamesScanned));

            if (unreadFileCount > 0)
                parts.Add(BuildUnreadFilesText(unreadFileCount));

            return string.Join(" ", parts);
        }

        // How to act on the results. It does not change between scans, so the view puts it in the tooltip of the results
        // header instead of the line under it.
        public static string BuildResultsGuideText(bool hasRequiredViolations, bool hasOverrides = false, bool hasBrokenGroups = true)
        {
            var parts = new List<string>(3);

            if (hasBrokenGroups)
                parts.Add("Each group is a broken stored type — Fix all re-points its every entry to one replacement, or to <None>.");

            if (hasOverrides)
                parts.Add("Prefab instance overrides are fixed on the instance: pick a new type or Revert the override.");

            if (hasRequiredViolations)
                parts.Add("Click a required-violation row to jump to its asset.");

            return string.Join("\n", parts);
        }

        public static string BuildGroupCountText(MissingReferenceGroup group)
        {
            var entries = group.Entries.Count;
            var files = group.FileCount;
            var entryText = entries == 1 ? "1 entry" : $"{entries} entries";
            var fileText = files == 1 ? "1 file" : $"{files} files";
            return $"{entryText} · {fileText}";
        }

        // The rows a card builds when `requested` rows were asked for: the first page at least, every entry at most.
        // The entries of a group, one item per file in the order the files first appear.
        public static IReadOnlyList<FileRids> GroupByFile(IReadOnlyList<MissingReferenceLocation> entries)
        {
            var files = new List<FileRids>();
            var indexByPath = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var entry in entries)
            {
                if (!indexByPath.TryGetValue(entry.AssetPath, out var index))
                {
                    index = files.Count;
                    indexByPath.Add(entry.AssetPath, index);
                    files.Add(new FileRids(entry.AssetPath));
                }

                files[index].Rids.Add(entry.Entry.Rid);
            }

            return files;
        }

        // One rid is named, several are counted, whatever their length, so the rows of a card read alike; the row's
        // tooltip lists them all.
        public static string DescribeRids(IReadOnlyList<long> rids) =>
            rids.Count == 1 ? $"rid {rids[0]}" : $"{rids.Count} rids";

        public static int GetShownRows(int requested, int total) =>
            Math.Min(total, Math.Max(requested, RowsPerPage));

        public static int GetNextShownRows(int shown) => shown + RowsPerPage;

        public static string BuildShowMoreText(int shown, int total) =>
            $"Show more ({shown} of {total} shown)";

        public static string BuildShowMoreTooltip(int shown, int total) =>
            $"Show the next {BuildCountText(Math.Min(RowsPerPage, total - shown), "row")}.";

        public static string BuildTypeNameCountText(MissingTypeNameGroup group)
        {
            var entries = group.Entries.Count;
            var files = group.FileCount;
            var entryText = entries == 1 ? "1 entry" : $"{entries} entries";
            var fileText = files == 1 ? "1 file" : $"{files} files";
            return $"{entryText} · {fileText} · type name";
        }

        public static string BuildTypeNameDiffPreview(IReadOnlyList<MissingTypeNameLocation> entries, string newName)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Changes:");

            for (var i = 0; i < entries.Count && i < MaxPreviewedEntries; i++)
            {
                var entry = entries[i];
                builder.AppendLine($"  {System.IO.Path.GetFileName(entry.AssetPath)} ({DescribeTypeNameField(entry.Entry)}):");
                builder.AppendLine($"    - {entry.Entry.TypeName}");
                builder.AppendLine($"    + {newName}");
            }

            if (entries.Count > MaxPreviewedEntries)
                builder.AppendLine($"  …and {entries.Count - MaxPreviewedEntries} more");

            builder.AppendLine();
            return builder.ToString();
        }

        // "_weapon", "rid 1001 · _arrow", "_weapon · override".
        public static string DescribeTypeNameField(StoredTypeNameEntry entry)
        {
            var field = string.IsNullOrEmpty(entry.FieldPath) ? SerializeReferenceYamlEditor.TypeNameKey : entry.FieldPath;
            if (entry.Rid != 0) field = $"rid {entry.Rid} · {field}";
            return entry.IsOverride ? field + " · override" : field;
        }

        public static string BuildFixAllLabel(MissingReferenceGroup group, bool isMigration) =>
            $"{(isMigration ? "Reassign all" : "Fix all")} ({group.Entries.Count})  ▼";

        public static string BuildDiffPreview(IReadOnlyList<MissingReferenceLocation> entries, ManagedTypeName newType)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Changes:");

            // Failed preview computations must not inflate the hidden-entry remainder.
            var computed = SerializeReferenceBatchEditor.ComputeRewrites(entries, newType);
            var edits = new List<(MissingReferenceLocation entry, RewriteEdit edit)>(entries.Count);
            for (var i = 0; i < entries.Count; i++)
            {
                if (computed[i].IsValid)
                    edits.Add((entries[i], computed[i]));
            }

            for (var i = 0; i < edits.Count && i < MaxPreviewedEntries; i++)
            {
                var (entry, edit) = edits[i];
                builder.AppendLine($"  {System.IO.Path.GetFileName(entry.AssetPath)} (rid {entry.Entry.Rid}):");
                builder.AppendLine($"    - {edit.OldLine.Trim()}");
                builder.AppendLine($"    + {edit.NewLine.Trim()}");
            }

            if (edits.Count > MaxPreviewedEntries)
                builder.AppendLine($"  …and {edits.Count - MaxPreviewedEntries} more");

            var uncomputable = entries.Count - edits.Count;
            if (uncomputable > 0)
                builder.AppendLine($"  ({uncomputable} entr{(uncomputable == 1 ? "y" : "ies")} could not be previewed)");

            builder.AppendLine();
            return builder.ToString();
        }

        public static string BuildClearPreview(IReadOnlyList<MissingReferenceLocation> entries)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Clears:");

            var shown = 0;
            foreach (var entry in entries)
            {
                if (shown >= MaxPreviewedEntries)
                {
                    builder.AppendLine($"  …and {entries.Count - shown} more");
                    break;
                }

                builder.AppendLine($"  {System.IO.Path.GetFileName(entry.AssetPath)} (rid {entry.Entry.Rid})");
                shown++;
            }

            builder.AppendLine();
            return builder.ToString();
        }

        internal readonly struct FileRids
        {
            public readonly string AssetPath;
            public readonly List<long> Rids;

            public FileRids(string assetPath)
            {
                AssetPath = assetPath;
                Rids = new List<long>();
            }
        }
    }

    internal enum RequiredAuditState
    {
        NotScanned,
        Disabled,
        Checked,
    }
}
